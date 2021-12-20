using System;
using Microsoft.Extensions.Hosting;
using Temporal.Worker.Hosting;

namespace Temporal.Sdk.BasicSamples
{
    public class Part01_1_TypicalWorkflow
    {

        public static void Main(string[] args)
        {
            // 'UseTemporalWorkerHost' will configure all the temporal defaults
            // and also apply all the file-based application config files.
            // Configuraton will automatically be persisted via side affects as appropriate before being passed to the workflow implmentation.
            // Config files are treated elsewhere.
            // Previous and past examples show how to *optionally* tweak configuration through in-line code at different scopes.

            IHost appHost = Host.CreateDefaultBuilder(args)
                    .UseTemporalWorkerHost()
                    .ConfigureServices(serviceCollection =>
                    {
                        serviceCollection.AddTemporalWorker()
                                .Configure(temporalWorkerConfig =>
                                {
                                    temporalWorkerConfig.TaskQueueMoniker = "Some Queue";
                                });

                        //serviceCollection.AddWorkflow<SayHelloWorkflow>();

                        //serviceCollection.AddActivity<SpeakAGreetingActivity>();
                    })
                    .Build();

            appHost.Run();
        }
    }
}
