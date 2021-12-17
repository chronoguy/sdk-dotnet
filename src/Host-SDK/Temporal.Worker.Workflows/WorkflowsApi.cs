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
        public OrchestrationService Orchestrator { get; }
        public WorkflowRunContext CurrentRun { get; }
        public WorkflowPreviousRunContext  LastRun { get; }

        /// <summary>Get the serializer for the specified payload.
        /// If metadata specifies an available serializer - get that one;
        /// If metadata specifies an unavailable serializer - throw;
        /// If metadata specified nothing - get the default form the config.</summary>        
        public IPayloadSerializer GetSerializer(PayloadsCollection payloads) { return null; }
    }

    // ----------- -----------

    public sealed class OrchestrationService
    {
        public IActivityOrchestrationService Activities { get; }        
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
}
