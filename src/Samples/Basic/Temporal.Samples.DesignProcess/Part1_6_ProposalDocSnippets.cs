using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Temporal.Common.DataModel;
using Temporal.Worker.Activities;
using Temporal.Worker.Hosting;
using Temporal.Worker.Workflows;

namespace Temporal.Sdk.BasicSamples
{
    public class Part1_6_ProposalDocSnippets
    {
        [Workflow(runMethod: nameof(MainAsync))]
        public class SomeWorkflow
        {
            private bool _isConditionMet = false;

            public Task MainAsync(WorkflowContext workflowCtx)
            {
                while (! _isConditionMet)
                {
                    Task someActivity = workflowCtx.Orchestrator.Activities.ExecuteAsync("SomeActivity");
                    //PerformSome
                }

                return null;
            }

            [WorkflowSignalHandler]
            public void NotifyConditionMetAsync()
            {
                _isConditionMet = true;
            }
        }

        /// <summary>
        /// Parameters to workflow APIs (main method, signal & query parameters) and to activities must implement <see cref="IDataValue" />.
        /// In some specialized cases where it is not possible, the raw (non-deserialized) payload may be accessed
        /// directly (e.g. <see cref="Part4_1_BasicWorkflowUsage" /> and <see cref="Part4_2_BasicWorkflowUsage_MultipleWorkers" />).
        /// </summary>
        public class SpeechRequest : IDataValue
        {
            public SpeechRequest(string text)
            {
                Text = text;
            }

            public string Text
            {
                get; set;
            }
        }


        /// <summary>A sample activity implementation.</summary>
        public static class Speak
        {
            public static Task GreetingAsync(SpeechRequest input, WorkflowActivityContext activityCtx)
            {
                Console.WriteLine($"[{activityCtx.ActivityTypeName}] {input.Text}");
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
                                .Configure(temporalWorkerConfig =>
                                {
                                    temporalWorkerConfig.TaskQueueMoniker = "Some Queue";
                                });

                        serviceCollection.AddWorkflowWithAttributes<SomeWorkflow>();

                        serviceCollection.AddActivity<SpeechRequest>("SpeakAGreeting", Speak.GreetingAsync);
                    })
                    .Build();

            appHost.Run();
        }
    }
}
