using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Temporal.CommonDataModel;
using Temporal.Serialization;
using Temporal.Worker.Workflows;

namespace Temporal.Worker.Hosting
{
    public class HostingApi
    {
    }

    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseTemporalWorkerHost(this IHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        public static IHostBuilder UseTemporalWorkerHost(this IHostBuilder hostBuilder,
                                                         Action<TemporalServiceConfiguration> configureTemporalService)
        {
            return hostBuilder;
        }

        public static IHostBuilder UseTemporalWorkerHost(this IHostBuilder hostBuilder,
                                                         Action<HostBuilderContext, TemporalServiceConfiguration> configureTemporalService)
        {
            return hostBuilder;
        }

        public static IHostBuilder UseTemporalWorkerHost(this IHostBuilder hostBuilder,
                                                         Action<TemporalServiceConfiguration,
                                                                TemporalWorkerConfiguration,
                                                                WorkflowExecutionConfiguration,
                                                                WorkflowImplementationConfiguration> configureDefaults)
        {
            return hostBuilder;
        }

        public static IHostBuilder UseTemporalWorkerHost(this IHostBuilder hostBuilder,
                                                        Action<HostBuilderContext,
                                                               TemporalServiceConfiguration,
                                                               TemporalWorkerConfiguration,
                                                               WorkflowExecutionConfiguration,
                                                               WorkflowImplementationConfiguration> configureDefaults)
        {
            return hostBuilder;
        }

    }

    public static class ServiceCollectionExtensions
    {
        public static WorkerRegistration AddTemporalWorker(this IServiceCollection serviceCollection)
        { return null; }        

        public static WorkflowRegistration AddWorkflow<TWorkflowImplementation>(this IServiceCollection serviceCollection)
                where TWorkflowImplementation : class, IBasicWorkflow
        { return null; }

        public static ActivityRegistration AddActivity<TActivityImplementation>(this IServiceCollection serviceCollection)
                where TActivityImplementation : class, IBasicActivity
        { return null; }

        public static ActivityRegistration AddActivity<TArg>(this IServiceCollection serviceCollection,
                                                             string activityTypeName,
                                                             Func<TArg, ActivityContext, Task> activityImplementation)
                                            where TArg : IDataValue
        { return null; }

        public static ActivityRegistration AddActivity<TArg, TResult>(this IServiceCollection serviceCollection,
                                                                      string activityTypeName,
                                                                      Func<TArg, ActivityContext, Task<TResult>> activityImplementation)
                                            where TArg : IDataValue where TResult : IDataValue
        { return null; }        
    }

    public class WorkerRegistration
    {
        public WorkerRegistration Configure(Action<TemporalWorkerConfiguration> configurator) { return this; }
        public WorkerRegistration Configure(Action<IServiceProvider, TemporalWorkerConfiguration> configurator) { return this; }
    }

    public class WorkflowRegistration
    {
        public WorkflowRegistration ConfigureExecution(Action<WorkflowExecutionConfiguration> configurator) { return this; }
        public WorkflowRegistration ConfigureExecution(Action<IServiceProvider, WorkflowExecutionConfiguration> configurator) { return this; }
        public WorkflowRegistration ConfigureImplementation(Action<WorkflowImplementationConfiguration> configurator) { return this; }
        public WorkflowRegistration ConfigureImplementation(Action<IServiceProvider, WorkflowImplementationConfiguration> configurator) { return this; }
        public WorkflowRegistration AssignWorker(WorkerRegistration workerRegistration) { return this; }
    }

    public class ActivityRegistration
    {        
        public ActivityRegistration AssignWorker(WorkerRegistration workerRegistration) { return this; }
    }

    // -----------

    public interface ITemporalCoreEngine
    {
    }

    public class TemporalCoreEngine : ITemporalCoreEngine
    {
        public TemporalCoreEngine(TemporalServiceConfiguration config) { }
    }

    public interface ITemporalServiceConfiguration
    {
        string OrchestratorServiceUrl { get; }
        string Namespace { get; }
    }

    public class TemporalServiceConfiguration : ITemporalServiceConfiguration
    {
        public string OrchestratorServiceUrl { get; set; }
        public string Namespace { get; set; }
    }

    // -----------

    public interface ITemporalWorker
    {

    }

    public class TemporalWorker : ITemporalWorker
    {
        public TemporalWorker(TemporalWorkerConfiguration config) { }
    }

    public interface ITemporalWorkerConfiguration
    {
        string TaskQueueMoniker { get; }
        int CachedStickyWorkflowsMax { get; }
        bool EnablePollForActivities { get; }
        IQueuePollingConfiguration NonStickyQueue { get; }
        IStickyQueuePollingConfiguration StickyQueue { get; }        
    }

    public class TemporalWorkerConfiguration : ITemporalWorkerConfiguration
    {
        public string TaskQueueMoniker { get; set; }
        public int CachedStickyWorkflowsMax { get; set; }
        public bool EnablePollForActivities { get; set; }
        public QueuePollingConfiguration NonStickyQueue { get; set; }
        public StickyQueuePollingConfiguration StickyQueue { get; set; }
        IQueuePollingConfiguration ITemporalWorkerConfiguration.NonStickyQueue { get { return this.NonStickyQueue; } }
        IStickyQueuePollingConfiguration ITemporalWorkerConfiguration.StickyQueue { get { return this.StickyQueue; } }


    }

    public interface IQueuePollingConfiguration
    {
        int ConcurrentWorkflowTaskPollsMax { get; }
        int ConcurrentActivityTaskPollsMax { get; }
    }

    public class QueuePollingConfiguration : IQueuePollingConfiguration
    {
        public int ConcurrentWorkflowTaskPollsMax { get; set; }
        public int ConcurrentActivityTaskPollsMax { get; set; }
    }

    public interface IStickyQueuePollingConfiguration : IQueuePollingConfiguration
    {
        int ScheduleToStartTimeoutMillisecs { get; }
    }

    public class StickyQueuePollingConfiguration : QueuePollingConfiguration, IStickyQueuePollingConfiguration
    {
        public int ScheduleToStartTimeoutMillisecs { get; set; }
    }

    // -----------

    /// <summary>
    /// Per-workflow settings related to the execution container of a workflow.
    /// Must not affect the business logic.
    /// Think: if the same workflow was run on a different host, these may be different.
    /// Example: Timeouts.
    /// </summary>
    public interface IWorkflowExecutionConfiguration
    {
        int WorkflowTaskTimeoutMillisec { get; }
    }

    public class WorkflowExecutionConfiguration : IWorkflowExecutionConfiguration
    {
        public int WorkflowTaskTimeoutMillisec { get; set; }
    }

    /// <summary>
    /// Per-workflow settings related to the business logic of a workflow.
    /// May affect the business logic.
    /// Think: if the same workflow was run on a different host, these must be the same.
    /// Example: Serializer.
    /// </summary>
    public interface IWorkflowImplementationConfiguration
    {
        IPayloadSerializer DefaultPayloadSerializer { get; }
        IActivityInvocationConfiguration DefaultActivityInvocationConfig { get; }
    }

    public class WorkflowImplementationConfiguration : IWorkflowImplementationConfiguration
    {
        public IPayloadSerializer DefaultPayloadSerializer { get; set; }
        public ActivityInvocationConfiguration DefaultActivityInvocationConfig { get; set; }
        IActivityInvocationConfiguration IWorkflowImplementationConfiguration.DefaultActivityInvocationConfig { get { return this.DefaultActivityInvocationConfig; } }
    }

    // -----------

    public interface IActivityInvocationConfiguration
    {
        string TaskQueueMoniker { get; }
        int ScheduleToStartTimeoutMillisecs { get; }
        int ScheduleToCloseTimeoutMillisecs { get; }
        int StartToCloseTimeoutMillisecs { get; }
        int HeartbeatTimeoutMillisecs { get; }
        RetryPolicy RetryPolicy { get; }
    }

    public class ActivityInvocationConfiguration : IActivityInvocationConfiguration
    {
        public string TaskQueueMoniker { get; set; }
        public int ScheduleToStartTimeoutMillisecs { get; set; }
        public int ScheduleToCloseTimeoutMillisecs { get; set; }
        public int StartToCloseTimeoutMillisecs { get; set; }
        public int HeartbeatTimeoutMillisecs { get; set; }
        public RetryPolicy RetryPolicy { get; set; }
    }

    // -----------

    public class RetryPolicy
    {
    }

    // -----------
}
