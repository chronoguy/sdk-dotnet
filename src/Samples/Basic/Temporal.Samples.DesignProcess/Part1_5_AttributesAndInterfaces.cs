using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Temporal.Async;
using Temporal.Common.DataModel;
using Temporal.Worker.Hosting;
using Temporal.Worker.Workflows;
using Temporal.WorkflowClient;

namespace Temporal.Sdk.BasicSamples
{
    /// <summary>
    /// This sample goes together with <see cref="Part2_2_IfaceBasedClient" /.>
    /// </summary>
    public class Part1_5_AttributesAndInterfaces
    {
        // Entities:

        public record MoneyAmount(int Dollars, int Cents) : IDataValue
        {
            public int TotalCents
            {
                get { return Dollars * 100 + Cents; }
            }
            
            public static MoneyAmount operator +(MoneyAmount amountA, MoneyAmount amountB)
            {
                int sum = amountA.TotalCents + amountB.TotalCents;
                return new MoneyAmount(sum / 100, sum % 100);                
            }

            public static bool operator >=(MoneyAmount amountA, MoneyAmount amountB)
            {
                return amountA.TotalCents >= amountB.TotalCents;
            }

            public static bool operator <=(MoneyAmount amountA, MoneyAmount amountB)
            {
                return amountA.TotalCents <= amountB.TotalCents;
            }
        }

        public record OrderConfirmation(DateTime TimestampUtc, Guid OrderId) : IDataValue;

        public record Product(string Name, MoneyAmount Price) : IDataValue;

        public record Products(IReadOnlyCollection<Product> Collection) : IDataValue;

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
            Task<Products> GetProductsAsync();

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

        public static class RemoteApiNames
        {
            public static class Activities
            {
                public const string DisplayRemainingSteps = "DisplayRemainingSteps";
                public const string DisplayCompletion = "DisplayCompletion";
            }

            public static class CountdownTimerWorkflow
            {                
                public static class Signals
                {
                    public const string UpdateTargetTime = "UpdateTargetTime";
                }
            }
        }

        [Workflow(runMethod: "ShopAsync")]
        public class ShoppingCart : IShoppingCart
        {
            private User _owner = null;
            private List<Product> _products = new List<Product>();
            private MoneyAmount _appliedPayment = new MoneyAmount(0, 0);
            private TaskCompletionSource<OrderConfirmation> _orderCompletion = new();
            private DeliveryInfo _deliveryInfo = null;

            [WorkflowSignalHandler]
            public Task AddProductAsync(Product product)
            {
                if (product != null && !_orderCompletion.Task.IsCompleted)
                {
                    _products.Add(product);
                }

                return Task.CompletedTask;
            }

            Task IShoppingCart.ApplyPaymentAsync(MoneyAmount amount)
            {
                throw new NotSupportedException($"This {nameof(IShoppingCart)} implementation does not support this SignalStub-method."
                                              + $" The supported handler for this signal type"
                                              + $" is {nameof(ApplyPayment)}({nameof(MoneyAmount)}, {nameof(WorkflowContext)}).");
            }

            [WorkflowSignalHandler(SignalTypeName = "Pay")]
            public void ApplyPayment(MoneyAmount amount, WorkflowContext workflowContext)
            {
                _appliedPayment = _appliedPayment + amount;

                if (TryGetTotalWithTax().IsSuccess(out MoneyAmount totalPrice))
                {
                    if (_appliedPayment >= totalPrice)
                    {
                        _orderCompletion.TrySetResult(new OrderConfirmation(workflowContext.DeterministicApi.DateTimeUtcNow,
                                                                            workflowContext.DeterministicApi.CreateNewGuid()));
                    }
                }
            }
            
            Task<User> IShoppingCart.GetOwnerAsync()
            {
                return Task.FromResult(GetOwner());
            }

            [WorkflowQueryHandler]
            public User GetOwner()
            {
                return _owner;
            }

            Task<Products> IProductList.GetProductsAsync()
            {
                return Task.FromResult(GetProducts());
            }

            [WorkflowQueryHandler]
            public Products GetProducts()
            {
                return null;
            }

            Task<MoneyAmount> IProductList.GetTotalAsync()
            {
                return Task.FromResult(GetTotal());
            }

            [WorkflowQueryHandler(QueryTypeName = "GetTotalWithoutTax")]
            public MoneyAmount GetTotal()
            {
                MoneyAmount total = new(0, 0);
                foreach(Product p in _products)
                {
                    total = total + p.Price;
                }

                return total;
            }

            Task IShoppingCart.SetDeliveryInfoAsync(DeliveryInfo deliveryInfo)
            {
                SetDeliveryInfo(deliveryInfo);
                return Task.CompletedTask;
            }

            [WorkflowSignalHandler]
            public void SetDeliveryInfo(DeliveryInfo deliveryInfo)
            {
                _deliveryInfo = deliveryInfo;
            }

            Task<TryGetResult<MoneyAmount>> IShoppingCart.TryGetTotalWithTaxAsync()
            {
                return Task.FromResult(TryGetTotalWithTax());
            }

            [WorkflowQueryHandler]
            public TryGetResult<MoneyAmount> TryGetTotalWithTax()
            {
                if (_deliveryInfo == null)
                {
                    return new TryGetResult<MoneyAmount>();
                }

                MoneyAmount preTax = GetTotal();

                // No tax on zero:
                if (preTax.TotalCents == 0)
                {
                    return new TryGetResult<MoneyAmount>(preTax);
                }

                // Mock tax calculation uses fixed tax:
                MoneyAmount tax = _deliveryInfo.Address.Contains("Seattle")
                        ? new MoneyAmount(1, 0)
                        : new MoneyAmount(0, 50);

                return new TryGetResult<MoneyAmount>(preTax + tax);
            }

            Task<OrderConfirmation> IShoppingCart.ContinueShoppingAsync()
            {
                throw new NotSupportedException($"This {nameof(IShoppingCart)} implementation does not support this RunMethod-Stub."
                                              + $" The workflow run method is {nameof(ShopAsync)}({nameof(User)}, {nameof(WorkflowContext)}).");
            }

            Task<OrderConfirmation> IShoppingCart.ShopAsync(User shopper)
            {
                throw new NotSupportedException($"This {nameof(IShoppingCart)} implementation does not support this RunMethod-Stub."
                                              + $" The workflow run method is {nameof(ShopAsync)}({nameof(User)}, {nameof(WorkflowContext)}).");
            }

            public async Task<OrderConfirmation> ShopAsync(User shopper, WorkflowContext workflowCtx)
            {
                _owner = shopper;

                OrderConfirmation confirmation = await _orderCompletion.Task;
                return confirmation;
            }
        }

        public static void Main(string[] args)
        {
            IHost appHost = Host.CreateDefaultBuilder(args)
                    .UseTemporalWorkerHost()
                    .ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddTemporalWorker()
                                .Configure(temporalWorkerConfig =>
                                {
                                    temporalWorkerConfig.TaskQueueMoniker = "taskQueueMoniker";
                                });

                        serviceCollection.AddWorkflowWithAttributes<ShoppingCart>();
                    })
                    .Build();

            appHost.Run();
        }
    }
}
