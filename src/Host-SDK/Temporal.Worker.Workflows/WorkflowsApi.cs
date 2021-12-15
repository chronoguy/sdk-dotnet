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
            // In the actual implementation, we need to make sure to log the error (include payload?) and to propagate an appropriate
            // kind of failure to the client.
            throw new NotSupportedException($"Query \"{queryName}\" cannot be handled by this workflow {GetWorkflowImplementationDescription()}.");
        }

        public virtual Task HandleSignalAsync(string signalName, PayloadsCollection input, WorkflowContext workflowCtx)
        {
            // Signals are fire-and-forget from the client's perspecive. So there is no error we can return.
            // The actual implementation needs to make sure that the error is logged (include payload?) and not retried
            // (subsequent invocations of the same signal will, or course, be allowed).
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

        Task<TResult> ExecuteAsync<TResult>(string activityName) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, CancellationToken cancelToken) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, IActivityInvocationConfiguration invocationConfig) where TResult : IDataValue;
        Task<TResult> ExecuteAsync<TResult>(string activityName, CancellationToken cancelToken, IActivityInvocationConfiguration invocationConfig) where TResult : IDataValue;

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

    public abstract class DynamicWorkflowBase<TInput, TResult> : BasicWorkflowBase
                where TInput : IDataValue
                where TResult : IDataValue
    {
        public sealed override async Task<PayloadsCollection> RunAsync(WorkflowContext workflowCtx)
        {
            TInput input = (typeof(TInput) == typeof(IDataValue.Void))
                    ? (TInput) (IDataValue) IDataValue.Void.Instance
                    : workflowCtx.GetSerializer(workflowCtx.CurrentRun.Input).Deserialize<TInput>(workflowCtx.CurrentRun.Input);
            
            DynamicWorkflowContext dynamicCtx = new DynamicWorkflowContext(workflowCtx);

            TResult result = await RunAsync(input, dynamicCtx);

            PayloadsCollection serializedResult = (result == null || result.GetType() == typeof(IDataValue.Void))
                    ? PayloadsCollection.Empty
                    : workflowCtx.WorkflowImplementationConfig.DefaultPayloadSerializer.Serialize<TResult>(result);

            return serializedResult;
        }

        public abstract Task<TResult> RunAsync(TInput input, DynamicWorkflowContext workflowCtx);
    }

    public abstract class DynamicWorkflowBase : DynamicWorkflowBase<IDataValue.Void, IDataValue.Void>    
    {
        public sealed override async Task<IDataValue.Void> RunAsync(IDataValue.Void _, DynamicWorkflowContext workflowCtx)
        {
            await RunAsync(workflowCtx);
            return null;
        }

        public abstract Task RunAsync(DynamicWorkflowContext workflowCtx);
    }


    public abstract class DynamicWorkflowBase<TResult>: DynamicWorkflowBase<IDataValue.Void, TResult>
            where TResult : IDataValue
    {
        public sealed override async Task<TResult> RunAsync(IDataValue.Void _, DynamicWorkflowContext workflowCtx)
        {
            TResult result = await RunAsync(workflowCtx); // no await required, but using for consistency with non-generic DynamicWorkflowBase.
            return result;
        }
        
        public abstract Task<TResult> RunAsync(DynamicWorkflowContext workflowCtx);
    }

    //public abstract class DynamicWorkflowBase<TInput, TResult> : DynamicWorkflowInternalBase<TInput, TResult>
    //        where TInput : IDataValue where TResult : IDataValue
    //{
    //    internal protected override Task<TResult> RunIternalAsync(TInput input, WorkflowContext workflowCtx)
    //    {
    //        return RunDynamicAsync(input, workflowCtx);
    //    }
    //    public abstract Task<TResult> RunDynamicAsync(TInput input, WorkflowContext workflowCtx);
    //}

    // ---

    public class DynamicWorkflowContext : WorkflowContext
    {
        internal DynamicWorkflowContext(WorkflowContext baseContext) { }
        public IDynamicWorkflowController DynamicControl { get; }
    }

    /// <summary>SignalHandlers are specified as a triple (priority, matcherRegex, handlerDelegate).
    /// (Queries are handled in equivalent manner, including default policy.)
    /// When a signal with a name 'SignalName' is received, we evaluate all respective triples in the order of priority, until
    /// SignalName is completely matched by the respective matcherRegex. Then the handlerDelegate is invoked to process the 
    /// signal.
    /// If no matcherRegex matches the received SignalName, we exaluate whether the current DefaultSignalHandlerPolicy applies.
    /// Such policy usually comes with its own matcherRegex and applies to all signals that are not matched by any handlers,
    /// but are matched by the policy's matcherRegex. It may spacify catch-all behaviours such as "cache the signal an process
    /// it if and when a matching signal handler is configures", "ignore the signal" or other
    /// (<see cref="DefaultSignalHandlerPolicy"/>).
    /// If SignalName is not matched by the default policy's matcherRegex, the policy does not apply.
    /// In that case, the signal will fall through to the hardcoded fafault provided by
    /// <see cref="BasicWorkflowBase.HandleSignalAsync(String, PayloadsCollection, WorkflowContext)"/>.
    /// This will likely be ignoring the signal logging an error.</summary>
    public interface IDynamicWorkflowController
    {
        IHandlerCollection<Action<string, IDataValue, DynamicWorkflowContext>> SignalHandlers { get; }
        IHandlerCollection<Func<string, IDataValue, DynamicWorkflowContext, Task<IDataValue>>> QueryHandlers { get; }

        SignalHandlingOrderPolicy SignalHandlingOrderPolicy { get; set; }

        SignalHandlerDefaultPolicy SignalHandlerDefaultPolicy { get; set; }
        QueryHandlerDefaultPolicy QueryHandlerDefaultPolicy { get; set; }
    }

    public interface IHandlerCollection<THandler> where THandler : class
    {
        int Count { get; }
        void Clear() { }
        void GetAt(int index, out string matcherRegex, out THandler handler);
        void RemoveAt(int index);
        void RemoveAt(int index, out string matcherRegex, out THandler handler);
        bool TryAdd(string matcherRegex, THandler handler);
        bool TryInsert(int index, string matcherRegex, THandler handler);
        bool TryUpdateAt(int index, string matcherRegex, THandler handler);
        bool TryGetByRegexPattern(string matcherRegex, out int index);
        bool TryGetByRegexPattern(string matcherRegex, out int index, out THandler handler);
        bool TryFindFirstMatch(string itemToMatch, out int index);
        bool TryFindFirstMatch(string itemToMatch, out int index, out string matcherRegex, out THandler handler);        
    }

    /// <summary>This setting affects the processing order of signals. It is independent of SignalHandlerDefaultPolicy.
    /// However, unless <seealso cref="SignalHandlerDefaultPolicy" /> any of these settings result in the same
    /// semantic behavior. Should this me a parameter to
    /// <seealso cref="SignalHandlerDefaultPolicy.CacheAndProcessWhenHandlerIsSet(String)" />?
    /// <br />
    /// Need to design when a a cached signal is handled if a matching handler is added:
    ///  - immediately?
    ///  - workflow should call some kind of HandleNewlyEnabledSingals() API?
    ///  - at the end of the current workflow task (at the start there was no handler yet)?
    ///  - at the satart of next workflow task, before additional signals that arrive in the meantime?
    ///  - something else?
    /// This choice may simplify some of the <c>SignalHandlingOrderPolicy</c>-options.</summary>
    public class SignalHandlingOrderPolicy
    {
        /// <summary>Handle signal as soon as it arrives, *if* a matching handler is configured.
        /// If a handler is added for cached signals, handle them immediately.</summary>
        public static SignalHandlingOrderPolicy AsSoonAsPossible() { return null; }

        /// <summary>Handle signals strictly in the order of arrival. A cached signal is considered no-yet-handled.
        /// Thus, if there any signals in the cache, all arriving signals are cached, even if a matching handlers exists.
        /// If a matching handler is added for a cached signal, but there are earlier signals that have no matching handlers,
        /// such signal will not be handled until all earlier signals are handled.
        /// Adding a matching handler for the earliest not-yet-handled signal can trigger several signals queueued up in this
        /// manner to be handled immadiately.</summary>
        public static SignalHandlingOrderPolicy Strict() { return null; }

        /// <summary>Handle signal as soon as it arrives, *if* a matching handler is configured.
        /// If a handler is added for a cached signals, handle them immediately in first-in-last-out order.
        /// No strict ordering, i.e. if a cached signal does not have a matching handler, it will not affect
        /// other cached signals, regardless of whether they arrived before or after.</summary>
        public static SignalHandlingOrderPolicy OnArrivalOrReversedFromCache() { return null; }

        /// <summary>Handle signal as soon as it arrives, *if* a matching handler is configured.
        /// If a handler is added for cached signals, handle them strictly in first-in-first-out order.
        /// I.e., there is strict ordering (similar to <seealso cref="SignalHandlingOrderPolicy.Strict" />) only
        /// for signals that are readily in the cache.</summary>
        public static SignalHandlingOrderPolicy OnArrivalOrStrictFromCache() { return null; }

        /// <summary>Handle signal as soon as it arrives, *if* a matching handler is configured.
        /// If a handler is added for cached signals, handle them strictly in first-in-last-out order.
        /// I.e., there is strict reversed ordering (similar to <seealso cref="SignalHandlingOrderPolicy.Strict" />) only
        /// for signals that are readily in the cache.</summary>
        public static SignalHandlingOrderPolicy OnArrivalOrStrictReversedFromCache() { return null; }
    }

    public class SignalHandlerDefaultPolicy
    {
        public static SignalHandlerDefaultPolicy CacheAndProcessWhenHandlerIsSet(string matcherRegex) { return null; }
        public static SignalHandlerDefaultPolicy CustomHandler(string matcherRegex, Action<string, IDataValue, DynamicWorkflowContext> handler) { return null; }
        public static SignalHandlerDefaultPolicy Ignore(string matcherRegex) { return null; }
        public static SignalHandlerDefaultPolicy None() { return null; }

        public string Name { get; }
        public string MatcherRegex { get; }
        public bool IsMatch(string signalName) { return false; }

        public bool TryClearCache() { return false; }
    }

    public class QueryHandlerDefaultPolicy
    {
        public static QueryHandlerDefaultPolicy ConstantResultValue(string matcherRegex, IDataValue resultValue) { return null; }
        public static QueryHandlerDefaultPolicy CustomHandler(string matcherRegex, Func<string, IDataValue, DynamicWorkflowContext, Task<IDataValue>> handler) { return null; }
        public static QueryHandlerDefaultPolicy CustomError(string matcherRegex, Func<string, IDataValue, Exception> errorFactory) { return null; }
        public static QueryHandlerDefaultPolicy None() { return null; }

        public string Name { get; }
        public string MatcherRegex { get; }
        public bool IsMatch(string queryName) { return false; }
    }

    // ---

}
