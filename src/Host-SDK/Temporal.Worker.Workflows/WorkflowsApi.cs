using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Temporal.CommonDataModel;
using Temporal.Serialization;
using Temporal.Worker.Hosting;

namespace Temporal.Worker.Workflows
{
    public class WorkflowsApi
    {
    }

    // =========== Basic workflow abstractions ===========
    // The most basic abstraction of a workflow that a user can see is IBasicWorkflow.
    // We do not guide there and it is rare that users will be exposed to this abstraction.
    // However, if a user wants to customise how higher level code abstractions use to Temporal concepts,
    // they will need to implement IBasicWorkflow or by subclassing BasicWorkflowBase.

    public interface IBasicWorkflow
    {
        string WorkflowTypeName { get; }

        Task<PayloadsCollection> RunAsync(WorkflowContext workflowCtx);

        Task HandleSignalAsync(string signalName, PayloadsCollection input, WorkflowContext workflowCtx);

        Task<PayloadsCollection> HandleQueryAsync(string signalName, PayloadsCollection input, WorkflowContext workflowCtx);
    }

    public abstract class BasicWorkflowBase : IBasicWorkflow
    {
        private string _workflowImplementationDescription = null;

        public virtual string WorkflowTypeName
        {
            get { return this.GetType().Name; }
        }

        public abstract Task<PayloadsCollection> RunAsync(WorkflowContext workflowCtx);        

        public virtual Task<PayloadsCollection> HandleQueryAsync(string queryName, PayloadsCollection input, WorkflowContext workflowCtx)
        {
            // We need to make sure that this is not retried. Different exception type?
            throw new NotSupportedException($"Query \"{queryName}\" cannot be handled by this workflow {GetWorkflowImplementationDescription()}.");
        }

        public virtual Task HandleSignalAsync(string signalName, PayloadsCollection input, WorkflowContext workflowCtx)
        {
            // We need to make sure that this is not retried. Different exception type?
            throw new NotSupportedException($"Signal \"{signalName}\" cannot be handled by this workflow {GetWorkflowImplementationDescription()}.");
        }

        protected virtual string GetWorkflowImplementationDescription()
        {
            if (_workflowImplementationDescription == null)
            {
                _workflowImplementationDescription = $"{{Temporal_WorkflowTypeName=\"{WorkflowTypeName}\";"
                                                   + $" Clr_WorkflowImplementationType=\"{this.GetType().FullName}\"}}";
            }

            return _workflowImplementationDescription;
        }
    }

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

        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, CancellationToken cancelToken) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TArg, TResult>(string activityName, TArg activityArguments, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig) where TArg : IDataValue where TResult : IDataValue;
    }

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


    // =========== Basic activity abstractions ===========

    public interface IBasicActivity
    {
        string ActivityTypeName { get; }

        Task<PayloadsCollection> RunAsync(PayloadsCollection input, ActivityContext workflowCtx);        
    }

    public abstract class BasicActivityBase : IBasicActivity
    {
        private string _activityTypeName = null;

        public virtual string ActivityTypeName
        {
            get
            {
                if (_activityTypeName == null)
                {
                    string implTypeName = this.GetType().Name;
                    _activityTypeName = implTypeName.EndsWith("Activity") ? implTypeName.Substring(implTypeName.Length - "Activity".Length) : implTypeName;
                }

                return _activityTypeName;
            }
        }

        public abstract Task<PayloadsCollection> RunAsync(PayloadsCollection input, ActivityContext activityCtx);
    }

    public class ActivityContext
    {
        public string ActivityTypeName { get; }

        /// <summary>Get the serializer for the specified payload.
        /// If metadata specifies an available serializer - get that one;
        /// If metadata specifies an unavailable serializer - throw;
        /// If metadata specified nothing - get the default form the config.</summary>        
        public IPayloadSerializer GetSerializer(PayloadsCollection payloads) { return null; }
    }


    // =========== Dynamic workflow abstractions ===========
    // When users want to dynamically (at runtime) map implementations to signals / queries,
    // they will use these abstractions.

    public interface IDataValue
    {
    }

    // ---

    public class DynamicWorkflowContext : WorkflowContext
    {
        public IDynamicWorkflowController DynamicControl { get; }
    }

    public interface IDynamicWorkflowController
    {
        IHandlerCollection<Action<string, IDataValue, IDynamicWorkflowController>> SignalHandlers { get; }
        IHandlerCollection<Func<string, IDataValue, IDynamicWorkflowController, Task<IDataValue>>> QueryHandlers { get; }

        // Signals & queries that are not matched to the default policy will fall through to the ballback processing in BasicWorkflowBase.
        DefaultSignalHandlerPolicy DefaultSignalHandlerPolicy { get; set; }
        DefaultQueryHandlerPolicy DefaultQueryHandlerPolicy { get; set; }
    }

    public interface IHandlerCollection<THandler> where THandler : class
    {
        int Count { get; }        
        void Get(int index, out string matcherRegex, out THandler handler);
        void Remove(int index, out string matcherRegex, out THandler handler);
        bool TryInsert(int index, string matcherRegex, THandler handler);
        bool TryUpdate(int index, string matcherRegex, THandler handler);        
        bool TryGetByMatcherRegex(string matcherRegex, out int index, out THandler handler);        
        bool TryFindFirstMatch(string itemToMatch, out int index, out string matcherRegex, out THandler handler);
    }

    public class DefaultSignalHandlerPolicy
    {
        public static DefaultSignalHandlerPolicy CacheAndProcessWhenHandlerIsSet(string matcherRegex) { return null; }
        public static DefaultSignalHandlerPolicy CustomHandler(string matcherRegex, Action<string, IDataValue, IDynamicWorkflowController> handler) { return null; }
        public static DefaultSignalHandlerPolicy Ignore(string matcherRegex) { return null; }
        public static DefaultSignalHandlerPolicy None() { return null; }

        public string Name { get; }
        public string MatcherRegex { get; }
        public bool IsMatch(string signalName) { return false; }
    }

    public class DefaultQueryHandlerPolicy
    {
        public static DefaultSignalHandlerPolicy ConstantResultValue(string matcherRegex, IDataValue resultValue) { return null; }
        public static DefaultSignalHandlerPolicy CustomHandler(string matcherRegex, Func<string, IDataValue, IDynamicWorkflowController, Task<IDataValue>> handler) { return null; }
        public static DefaultQueryHandlerPolicy CustomError(string matcherRegex, Func<string, IDataValue, Exception> errorFactory) { return null; }
        public static DefaultQueryHandlerPolicy None() { return null; }

        public string Name { get; }
        public string MatcherRegex { get; }
        public bool IsMatch(string queryName) { return false; }
    }

    // ---

}
