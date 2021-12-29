using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Temporal.Async;
using Temporal.Common.DataModel;
using Temporal.WorkflowClient;

using static Temporal.Sdk.BasicSamples.Part1_4_TimersAndComposition2;

namespace Temporal.Sdk.BasicSamples
{
    public class Part2_2_IfaceBasedClient
    {        
        // Entities:

        public record MoneyAmount(int Dollars, int Cents) : IDataValue;
        public record OrderConfirmation(DateTime TimestampUtc, Guid OrderId) : IDataValue;
        public record Product(string Name, MoneyAmount Price) : IDataValue;
        public record DeliveryInfo(string Address) : IDataValue;
        public record User(string FirstName, string LastName, Guid UserId) : IDataValue
        {
            public string UserKey { get { return UserKey.ToString(); } }
        }

        // Workflow interfaces:

        public interface IProductList
        {
            [WorkflowSignalStub]
            Task AddProductAsync(Product product);

            [WorkflowQueryStub]
            Task<IReadOnlyCollection<Product>> GetProductsAsync();

            [WorkflowQueryStub(QueryTypeName = "GetTotalWithoutTax")]
            Task<MoneyAmount> GetTotalAsync();
        }

        public interface IShoppingCart : IProductList
        {
            [WorkflowQueryStub]
            Task<TryGetResult<MoneyAmount>> TryGetTotalWithTaxAsync();

            [WorkflowQueryStub]
            Task<User> GetOwnerAsync();

            [WorkflowSignalStub]
            Task SetDeliveryInfoAsync(DeliveryInfo deliveryInfo);

            [WorkflowSignalStub(SignalTypeName = "Pay")]
            Task ApplyPaymentAsync(MoneyAmount amount);

            [WorkflowRunMethodStub]
            Task<OrderConfirmation> ShopAsync(User shopper);

            [WorkflowRunMethodStub(CanBindToNewRun = false, CanBindToExistingRun = true)]
            Task<OrderConfirmation> ContinueShoppingAsync();
        }
        
        public static async Task Minimal(string[] _)
        {
            TemporalServiceClientConfiguration serviceConfig = new();
            TemporalServiceNamespaceClient serviceClient = await (new TemporalServiceClient(serviceConfig)).GetNamespaceClientAsync();

            IShoppingCart cart = serviceClient.GetNewWorkflow("ShoppingCart").GetRunStub<IShoppingCart>("taskQueueMoniker");

            Task<OrderConfirmation> order = cart.ShopAsync(new User("Jean-Luc", "Picard", Guid.NewGuid()));
            await cart.AddProductAsync(new Product("Starship Model", new MoneyAmount(42, 99)));
            await cart.SetDeliveryInfoAsync(new DeliveryInfo("11 Headquarters Street, San Francisco"));
            MoneyAmount price = (await cart.TryGetTotalWithTaxAsync()).Result;
            await cart.ApplyPaymentAsync(price);
            OrderConfirmation confirmation = await order;

            Console.WriteLine($" Order \"{confirmation.OrderId}\" was placed at {confirmation.TimestampUtc}.");
        }

        private static async Task<TemporalServiceNamespaceClient> GetShoppingNamespaceClientAsync()
        {
            TemporalServiceClientConfiguration serviceConfig = new();
            TemporalServiceClient serviceClient = new(serviceConfig);
            return await serviceClient.GetNamespaceClientAsync("Shopping");
        }

        public static async Task<bool> AddProductToExistingCart_Main(User shopper, Product product)
        {
            TemporalServiceNamespaceClient client = await GetShoppingNamespaceClientAsync();

            Workflow userVisits = await client.GetWorkflowAsync("ShoppingCart", shopper.UserKey);
            IProductList cart = userVisits.GetRunStub<IProductList>();  // Note that both 'IProductList' and 'IShoppingCart' are valid here.

            return await AddProductToExistingCart_Logic(product, cart);
        }

        private static async Task<bool> AddProductToExistingCart_Logic(Product product, IProductList cart)
        {
            try
            {
                await cart.AddProductAsync(product);  // Will throw if no active run available for binding.
                MoneyAmount total = await cart.GetTotalAsync();

                Console.WriteLine($"Item \"{product.Name}\" added to cart. New total is: ${total.Dollars}.{total.Cents}.");
                return true;
            }
            catch (NeedsDesignException)
            {
                return false;
            }
        }

        public static async Task<bool> TryAddShippingInfoIfUserIsShopping_Main(User shopper, DeliveryInfo shippingInfo)
        {
            TemporalServiceNamespaceClient client = await GetShoppingNamespaceClientAsync();

            Workflow userVisits = await client.GetWorkflowAsync("ShoppingCart", shopper.UserKey);
            IShoppingCart cart = userVisits.GetRunStub<IShoppingCart>();

            return await TryAddShippingInfoIfUserIsShopping_Logic(shippingInfo, cart);
        }

        private static async Task<bool> TryAddShippingInfoIfUserIsShopping_Logic(DeliveryInfo shippingInfo, IShoppingCart cart)
        {
            try
            {
                await cart.SetDeliveryInfoAsync(shippingInfo); // Will throw if no active run available for binding.
                return true;  // Shipping info applied.
            }
            catch (NeedsDesignException)
            {
                return false;  // "Could not apply shipping info. Does user have an active shopping cart?
            }
        }

        public static async Task<bool> PayAndWaitForOrderCompletionIfUserIsShopping_Main(User shopper, MoneyAmount paymentAmount)
        {
            TemporalServiceNamespaceClient client = await GetShoppingNamespaceClientAsync();

            Workflow userVisits = await client.GetWorkflowAsync("ShoppingCart", shopper.UserKey);
            IShoppingCart cart = userVisits.GetRunStub<IShoppingCart>();

            return await PayAndWaitForOrderCompletionIfUserIsShopping_Logic(paymentAmount, cart);
        }

        private static async Task<bool> PayAndWaitForOrderCompletionIfUserIsShopping_Logic(MoneyAmount paymentAmount, IShoppingCart cart)
        {
            Task<OrderConfirmation> order;

            try
            {
                order = cart.ContinueShoppingAsync();                
            }
            catch (NeedsDesignException)
            {
                return false;  // "Could not apply shipping info. Does user have an active shopping cart?
            }

            await cart.ApplyPaymentAsync(paymentAmount);
            
            OrderConfirmation confirmation = await order;
            Console.WriteLine($"Order \"{confirmation.OrderId}\" was placed.");

            return true;
        }

        public static async Task<bool> AddProductToExistingCart2_Main(User shopper, Product product)
        {
            TemporalServiceNamespaceClient client = await GetShoppingNamespaceClientAsync();

            Workflow userVisits = await client.GetWorkflowAsync("ShoppingCart", shopper.UserKey);
            IShoppingCart cart = userVisits.GetRunStub<IShoppingCart>();

            return await AddProductToExistingCart2_Logic(product, cart);
        }

        private static async Task<bool> AddProductToExistingCart2_Logic(Product product, IShoppingCart cart)
        {
            // Force binding to an existing workflow run:
            try
            {
                await cart.ContinueShoppingAsync();
            }
            catch (NeedsDesignException)
            {
                // We could return false here, but let's use another way of checking.
            }

            // Any stub can be cast to 'IWorkflowRunStub'. We can see if the above binding succeeded:
            if (! ((IWorkflowRunStub) cart).IsBound)
            {
                Console.WriteLine("Cart is new. Will not add product.");
                return false;
            }

            Console.WriteLine("Cart is already active. Current items:");
            IReadOnlyCollection<Product> products = await cart.GetProductsAsync();
            foreach(Product p in products)
            {
                Console.WriteLine($"    {p.Name}: ${p.Price.Dollars}.{p.Price.Cents}");
            }

            await cart.AddProductAsync(product);
            MoneyAmount total = await cart.GetTotalAsync();

            Console.WriteLine($"Item \"{product.Name}\" added to cart. New total is: ${total.Dollars}.{total.Cents}.");
            return true;
        }

        public static async Task AddProductToNewOrExistingCart_Main(User shopper, Product product)
        {
            TemporalServiceNamespaceClient client = await GetShoppingNamespaceClientAsync();

            Workflow userVisits = await client.GetWorkflowAsync("ShoppingCart", shopper.UserKey);
            IShoppingCart cart = userVisits.GetRunStub<IShoppingCart>("taskQueueMoniker");

            await AddProductToNewOrExistingCart_Logic(shopper, product, cart);
        }

        private static async Task AddProductToNewOrExistingCart_Logic(User shopper, Product product, IShoppingCart cart)
        {
            await cart.ShopAsync(shopper);  // Start new run or connet to existing.

            Console.WriteLine("Current items:");
            IReadOnlyCollection<Product> products = await cart.GetProductsAsync();
            foreach (Product p in products)
            {
                Console.WriteLine($"    {p.Name}: ${p.Price.Dollars}.{p.Price.Cents}");
            }

            await cart.AddProductAsync(product);
            MoneyAmount total = await cart.GetTotalAsync();

            Console.WriteLine($"Item \"{product.Name}\" added to cart. New total is: ${total.Dollars}.{total.Cents}.");
        }
    }
}
