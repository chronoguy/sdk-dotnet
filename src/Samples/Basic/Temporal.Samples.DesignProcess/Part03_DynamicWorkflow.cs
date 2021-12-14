using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Temporal.CommonDataModel;
using Temporal.Serialization;
using Temporal.Worker.Hosting;
using Temporal.Worker.Workflows;

namespace Temporal.Sdk.BasicSamples
{
    /// <summary>
    /// This is the lowest level of a workflow abstraction available to the users.
    /// We will not guide them here in any prominent way.
    /// However, users who wish to realize advanced scenarios, may want to use this low-level abstraction.
    /// Examples:
    ///  - a different way to map attributed interfaces or classes to temporal messages
    ///  - a different appoach to implementing a dynamic workflow to the one offered by our framework
    /// </summary>
    public class Part01_BasicDynamicWorkflow
    {       
        public class SayHelloWorkflow : BasicWorkflowBase
        {
            private const string AddresseeNameDefault = "Boss";

            private string _addresseeName = null;

            public override async Task<PayloadsCollection> RunAsync(PayloadsCollection input, WorkflowContext workflowCtx)
            {
                string greeting = $"Hello, {_addresseeName ?? AddresseeNameDefault}.";
                await SayHelloAsync(greeting);

                return PayloadsCollection.Empty;
            }

            public override Task HandleSignalAsync(string signalName, PayloadsCollection input, WorkflowContext workflowCtx)
            {
                if (signalName.Equals("SetAddressee", StringComparison.OrdinalIgnoreCase))
                {
                    // ToDo: Look into the data converter fx in Java to see 
                    string addresseeName = workflowCtx.PayloadSerializer.Deserialize<string>(input);
                    
                    _addresseeName = addresseeName;
                    return Task.CompletedTask;
                }

                return base.HandleSignalAsync(signalName, input, workflowCtx);                
            }

            private Task SayHelloAsync(string greetingText)
            {
                // ToDo: invoke activity.
                return Task.CompletedTask;
            }
        }

        public static void Main(string[] args)
        {
            IHost appHost = Host.CreateDefaultBuilder(args)
                    .UseTemporalWorkerHost()
                    .ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddTemporalWorker()
                                .Configure(temporalWorkerConfiguration =>
                                {
                                    temporalWorkerConfiguration.TaskQueueMoniker = "Some Queue";
                                });

                        serviceCollection.AddWorkflow<SayHelloWorkflow>();
                    })
                    .Build();
            appHost.Run();
        }

        public static void Main_UseWorkflowExecutionConfiguration(string[] args)
        {
            IHost appHost = Host.CreateDefaultBuilder(args)
                    .UseTemporalWorkerHost()
                    .ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddTemporalWorker()
                                .Configure(temporalWorkerConfiguration =>
                                {
                                    temporalWorkerConfiguration.TaskQueueMoniker = "Some Queue";
                                });

                        serviceCollection.AddWorkflow<SayHelloWorkflow>()
                                .ConfigureExecution(workflowExecutionConfiguration =>
                                {
                                    workflowExecutionConfiguration.WorkflowTaskTimeoutMillisec = 5_000;
                                })
                                .ConfigureImplementation(workflowImplementationConfiguration =>
                                {
                                    workflowImplementationConfiguration.PayloadSerializer = new JsonPayloadSerializer();
                                });
                    })
                    .Build();
            appHost.Run();
        }
        
        public static void Main_UseCustomWorkerConfig(string[] args)
        {
            IHost appHost = Host.CreateDefaultBuilder(args)
                    .UseTemporalWorkerHost(temporalServiceConfiguration =>
                    {
                        temporalServiceConfiguration.Namespace = "MyNamespace";
                        temporalServiceConfiguration.OrchestratorServiceUrl = "http://api.endpoint.com:12345";
                    })
                    .ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddTemporalWorker()
                                .Configure(temporalWorkerConfiguration =>
                                {
                                    temporalWorkerConfiguration.TaskQueueMoniker = "Some Queue";
 
                                    temporalWorkerConfiguration.EnablePollForActivities = true;
                                    temporalWorkerConfiguration.CachedStickyWorkflowsMax = 0;
 
                                    temporalWorkerConfiguration.NonStickyQueue.ConcurrentWorkflowTaskPollsMax = 1;
                                    temporalWorkerConfiguration.NonStickyQueue.ConcurrentActivityTaskPollsMax = 1;
 
                                    temporalWorkerConfiguration.StickyQueue.ScheduleToStartTimeoutMillisecs = 10_000;
                                    temporalWorkerConfiguration.StickyQueue.ConcurrentWorkflowTaskPollsMax = 5;
                                    temporalWorkerConfiguration.StickyQueue.ConcurrentActivityTaskPollsMax = 5;
                                });

                        serviceCollection.AddWorkflow<SayHelloWorkflow>();
                    })
                    .Build();
            appHost.Run();
        }
    }
}
