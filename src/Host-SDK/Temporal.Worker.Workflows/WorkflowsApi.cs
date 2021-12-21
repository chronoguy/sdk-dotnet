using System;
using System.Threading;
using System.Threading.Tasks;

using Temporal.CommonDataModel;
using Temporal.Serialization;

namespace Temporal.Worker.Workflows
{
    public class WorkflowsApi
    {
    }

    // ----------- -----------

    public class WorkflowContext
    {        
        public IWorkflowExecutionConfiguration WorkflowExecutionConfig { get; }
        public IWorkflowImplementationConfiguration WorkflowImplementationConfig { get; }
        public IOrchestrationService Orchestrator { get; }
        public WorkflowRunContext CurrentRun { get; }
        public WorkflowPreviousRunContext  LastRun { get; }
        public IDeterministicApi DeterministicApi { get; set; }

        /// <summary>Get the serializer for the specified payload.
        /// If metadata specifies an available serializer - get that one;
        /// If metadata specifies an unavailable serializer - throw;
        /// If metadata specified nothing - get the default form the config.</summary>        
        public IPayloadSerializer GetSerializer(PayloadsCollection payloads) { return null; }
    }

    // ----------- -----------

    public interface IOrchestrationService
    {
        IActivityOrchestrationService Activities { get; }
        void ConfigureContinueAsNew(bool startNewRunAfterReturn, IDataValue newRunInput);
        void ConfigureContinueAsNew(bool startNewRunAfterReturn);
        Task SleepAsync(TimeSpan timeSpan);
        Task<bool> SleepAsync(TimeSpan timeSpan, CancellationToken cancelToken);
        Task SleepUntilAsync(DateTime sleepEndUtc);
        Task<bool> SleepUntilAsync(DateTime sleepEndUtc, CancellationToken cancelToken);
    }

    public sealed class OrchestrationService : IOrchestrationService
    {        
        public IActivityOrchestrationService Activities { get; }
        public void ConfigureContinueAsNew(bool startNewRunAfterReturn, IDataValue newRunInput) { }
        public void ConfigureContinueAsNew(bool startNewRunAfterReturn) { }
        public Task SleepAsync(TimeSpan timeSpan) { return null; }
        public Task<bool> SleepAsync(TimeSpan timeSpan, CancellationToken cancelToken) { return null; }
        public Task SleepUntilAsync(DateTime sleepEndUtc) { return null; }
        public Task<bool> SleepUntilAsync(DateTime sleepEndUtc, CancellationToken cancelToken) { return null; }
    }

    public interface IActivityOrchestrationService
    {
        Task<PayloadsCollection> ExecuteAsync(string activityName, PayloadsCollection activityArguments);
        Task<PayloadsCollection> ExecuteAsync(string activityName, PayloadsCollection activityArguments, CancellationToken cancelToken);
        Task<PayloadsCollection> ExecuteAsync(string activityName, PayloadsCollection activityArguments, IActivityInvocationConfiguration invocationConfig);
        Task<PayloadsCollection> ExecuteAsync(string activityName, PayloadsCollection activityArguments, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig);
        
        Task ExecuteAsync<TArg>(string activityName, TArg activityArguments) where TArg : IDataValue;
        Task ExecuteAsync<TArg>(string activityName, TArg activityArguments, CancellationToken cancelToken) where TArg : IDataValue;
        Task ExecuteAsync<TArg>(string activityName, TArg activityArguments, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue;
        Task ExecuteAsync<TArg>(string activityName, TArg activityArguments, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue;

        Task<TResult> ExecuteAsync<TResult>(string activityName) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, CancellationToken cancelToken) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, IActivityInvocationConfiguration invocationConfig) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig) where TResult : IDataValue;

        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, CancellationToken cancelToken) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue where TResult : IDataValue;
    }

    // ----------- -----------

    public interface IDeterministicApi
    {
        Random CreateNewRandom();
        Guid CreateNewGuid();
    }

    public class DeterministicApi : IDeterministicApi
    {
        public Random CreateNewRandom() { return null; }

        public Guid CreateNewGuid() { return default(Guid); }
    }

    // ----------- -----------

    public class WorkflowRunContext
    {
        public CancellationToken CancelToken { get; }
        public string RunId { get; }
        public PayloadsCollection Input { get; }
        
    }

    public class WorkflowPreviousRunContext
    {
        public bool IsAvailable { get; }
        public Task<TResult> TryGetCompletion<TResult>() { return null; }
        public Task TryGetCompletion() { return null; }
        public Task<PayloadsCollection> GetCompletion() { return null; }
    }

    // ----------- -----------

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

    // ----------- -----------

    public class RetryPolicy
    {
    }

    // ----------- -----------

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
    public class WorkflowAttribute : Attribute
    {
        public string RunMethod { get; }

        public string WorkflowTypeName { get; set; }
        
        public WorkflowAttribute(string runMethod)
        {
            RunMethod = runMethod;
        }

        public override bool IsDefaultAttribute()
        {
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class WorkflowSignalAttribute : Attribute
    {
        public string SignalTypeName { get; }

        public WorkflowSignalAttribute()
            : this(String.Empty)
        {
        }

        public WorkflowSignalAttribute(string signalTypeName)
        {
            SignalTypeName = signalTypeName;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class WorkflowQueryAttribute : Attribute
    {
        public string QueryTypeName { get; }

        public WorkflowQueryAttribute()
            : this(String.Empty)
        {
        }

        public WorkflowQueryAttribute(string queryTypeName)
        {
            QueryTypeName = queryTypeName;
        }
    }

    // ----------- -----------
}
