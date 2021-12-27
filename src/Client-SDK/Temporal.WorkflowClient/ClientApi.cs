using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Temporal.Common.DataModel;
using Temporal.Common.WorkflowConfiguration;

using Temporal.Async;
using Temporal.Collections;

namespace Temporal.WorkflowClient
{
    public class ClientApi
    {
    }

    public class TemporalServiceClient
    {
        static private TemporalServiceClientConfiguration CreateDefaultConfiguration() { return null; }
        public TemporalServiceClient() : this(CreateDefaultConfiguration()) { }
        public TemporalServiceClient(TemporalServiceClientConfiguration config) { }

        public Task<NeedsDesign> GetClusterInfoAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> GetSearchAttributesAsync(NeedsDesign oneOrMoreArgs) { return null; }

        public Task<NeedsDesign> RegisterNamespaceAsync(NeedsDesign oneOrMoreArgs) { return null; }       
        public Task<NeedsDesign> ListNamespacesAsync(NeedsDesign oneOrMoreArgs) { return null; }

        public Task<bool> IsNamespaceAccessibleAsync(string @namespace) { return null; }
        public Task<bool> IsNamespaceAccessibleAsync(string @namespace, CancellationToken cancelToken) { return null; }

        public Task<TemporalServiceNamespaceClient> GetNamespaceClientAsync() { return null; }
        public Task<TemporalServiceNamespaceClient> GetNamespaceClientAsync(CancellationToken cancelToken) { return null; }
        public Task<TemporalServiceNamespaceClient> GetNamespaceClientAsync(string @namespace) { return null; }
        public Task<TemporalServiceNamespaceClient> GetNamespaceClientAsync(string @namespace, CancellationToken cancelToken) { return null; }
        public Task<TryGetResult<TemporalServiceNamespaceClient>> TryGetNamespaceClientAsync(string @namespace) { return null; }
        public Task<TryGetResult<TemporalServiceNamespaceClient>> TryGetNamespaceClientAsync(string @namespace, CancellationToken cancelToken) { return null; }
    }

    public class TemporalServiceNamespaceClient
    {
        public string Namespace { get; }

        // -- Misc general APIs: --

        public Task<NeedsDesign> DescribeTaskQueueAsync(NeedsDesign oneOrMoreArgs) { return null; }

        // -- Namespace APIs: --

        public Task<NeedsDesign> DescribeNamespaceAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> UpdateNamespaceAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> DeprecateNamespaceAsync(NeedsDesign oneOrMoreArgs) { return null; } // Depricated in proto. Need it?

        // -- Workflow exploration APIs (based on GRPC capabilities. Needs review): --

        // Should the APIs right below use 'WorkflowRuns' rather than 'WorkflowExecutions' for consistency with the terminology used here?
        public Task<NeedsDesign> ListOpenWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> ListClosedWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> ListWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> ListArchivedWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; }
        public Task<NeedsDesign> ScanWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; } // Difference ListWorkflowsAsync vs. ScanWorkflowsAsync
        public Task<NeedsDesign> CountWorkflowExecutionsAsync(NeedsDesign oneOrMoreArgs) { return null; } // What exactly does the GRPC API count?

        // -- Workflow access and control APIs: --

        // List workflows (not workflow executions):
        public Task<IPaginatedReadOnlyCollectionPage<Workflow>> ListWorkflowsAsync(NeedsDesign oneOrMoreArgs) { return null; }

        // Get a client for the specified 'workflowTypeName' while generating a random GUID-based 'workflowId':
        public Workflow GetNewWorkflow(string workflowTypeName) { return null; }        

        // Get a client for the specified 'workflowId' while fetching 'workflowTypeName' from the server:
        public Task<Workflow> GetExistingWorkflowAsync(string workflowId) { return null; }
        public Task<Workflow> GetExistingWorkflowAsync(string workflowId, CancellationToken cancelToken) { return null; }

        // Get a client for the specified 'workflowId' and 'workflowTypeName'.
        // If a workflow with the specified 'workflowId' already exists, but has a different 'workflowTypeName' - fail.
        public Task<Workflow> GetWorkflowAsync(string workflowTypeName, string workflowId) { return null; }
        public Task<Workflow> GetWorkflowAsync(string workflowTypeName, string workflowId, CancellationToken cancelToken) { return null; }

        // Provide TryXxx pattern alternative to above equivalents:
        public Task<TryGetResult<Workflow>> TryGetExistingWorkflowAsync(string workflowId) { return null; }
        public Task<TryGetResult<Workflow>> TryGetExistingWorkflowAsync(string workflowId, CancellationToken cancelToken) { return null; }        
        public Task<TryGetResult<Workflow>> TryGetWorkflowAsync(string workflowTypeName, string workflowId) { return null; }
        public Task<TryGetResult<Workflow>> TryGetWorkflowAsync(string workflowTypeName, string workflowId, CancellationToken cancelToken) { return null; }
    }

    public class Workflow
    {
        public string Namespace { get; }
        public string WorkflowTypeName { get; }
        public string WorkflowId { get; }

        public Task<bool> HasRunsAsync() { return null; }

        public Task<IPaginatedReadOnlyCollectionPage<WorkflowRun>> ListRunsAsync(NeedsDesign oneOrMoreArgs) { return null; }

        #region TerminateAsync(..)
        public Task TerminateAsync(string reason) { return null; }
        public Task TerminateAsync(string reason, PayloadsCollection details, CancellationToken cancelToken) { return null; }
        public Task TerminateAsync(string reason, IDataValue details, CancellationToken cancelToken) { return null; }
        #endregion TerminateAsync(..)


        #region StartNewRunAsync(..)
        public Task<WorkflowRun> StartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> StartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }
        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion StartNewRunAsync(..)


        #region TryStartNewRunAsync(..)
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion TryStartNewRunAsync(..)


        #region StartNewRunWithSignalAsync(..)
        public Task<WorkflowRun> StartNewRunWithSignalAsync(string taskQueueMoniker, string signalName) { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync(string taskQueueMoniker, string signalName, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(string taskQueueMoniker, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(string taskQueueMoniker, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, string signalName) { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection workflowArgs, string signalName, PayloadsCollection signalArgs, CancellationToken cancelToken) { return null; }
        #endregion StartNewRunWithSignalAsync(..)


        #region TryStartNewRunWithSignalAsync(..)
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(string taskQueueMoniker, string signalName) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(string taskQueueMoniker, string signalName, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(string taskQueueMoniker, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(string taskQueueMoniker, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(string taskQueueMoniker, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, string signalName) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowConfiguration, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection workflowArgs, string signalName, PayloadsCollection signalArgs, CancellationToken cancelToken) { return null; }
        #endregion TryStartNewRunWithSignalAsync(..)


        #region GetActiveOrStartNewRunAsync(..)
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion GetActiveOrStartNewRunAsync(..)


        #region TryGetActiveOrStartNewRunAsync(..)
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowConfiguration, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion TryGetActiveOrStartNewRunAsync(..)


        #region GetXxxxRunAsync(..)
        // Get the run with the specified run-id.
        public Task<WorkflowRun> GetRunAsync(string workflowRunId) { return null; }
        public Task<WorkflowRun> GetRunAsync(string workflowRunId, CancellationToken cancelToken) { return null; }

        // Get the most recent run (if there any at all for the workflow).
        public Task<WorkflowRun> GetLatestRunAsync() { return null; }
        public Task<WorkflowRun> GetLatestRunAsync(CancellationToken cancelToken) { return null; }

        // Get the very last run ONCE it has started and is known to be final (no continue-as-new runs will follow).
        public Task<WorkflowRun> GetFinalRunAsync() { return null; }
        public Task<WorkflowRun> GetFinalRunAsync(CancellationToken cancelToken) { return null; }

        // Get the very last run IF it has already started and is already known to be final (no continue-as-new runs will follow).
        public Task<WorkflowRun> GetFinalRunIfAvailableAsync() { return null; }
        public Task<WorkflowRun> GetFinalRunIfAvailableAsync(CancellationToken cancelToken) { return null; }
        #endregion GetXxxxRunAsync(..)


        #region TryGetXxxxRunAsync(..)
        // TryXxx pattern alternatives for above equivalents:
        public Task<TryGetResult<WorkflowRun>> TryGetRunAsync(string workflowRunId) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetRunAsync(string workflowRunId, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetLatestRunAsync() { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetLatestRunAsync(CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetFinalRunAsync() { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetFinalRunAsync(CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetFinalRunIfAvailableAsync() { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetFinalRunIfAvailableAsync(CancellationToken cancelToken) { return null; }
        #endregion TryGetXxxxRunAsync(..)
    }

    public class WorkflowRun
    {
        public static WorkflowRun CreateNew(object workflowStub) { return null; }
        public static bool TryCreateNew(object workflowStub, out WorkflowRun workflowRun) { workflowRun = null; return false; }

        public string Namespace { get { return Workflow.Namespace; } }
        public string WorkflowTypeName { get { return Workflow.WorkflowTypeName; } }
        public string WorkflowId { get { return Workflow.WorkflowId; } }

        public string WorkflowRunId { get; }
        public Workflow Workflow { get; }

        public Task<bool> IsActiveAsync() { return null; }

        public Task<WorkflowRunInfo> GetInfoAsync() { return null; }

        public Task<bool> IsContinuedAsNewAsync() { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetContinuedAsNewAsync() { return null; }

        public TStub GetStub<TStub>() { return default(TStub); }

        /// <summary>Get result if run has finished. Otherwise return False. No long poll.</summary>
        public Task<TryGetResult<WorkflowRunResult>> TryGetResultIfAvailableAync() { return null; }
        public Task<TryGetResult<WorkflowRunResult>> TryGetResultIfAvailableAync(CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRunResult<TResult>>> TryGetResultIfAvailableAync<TResult>() where TResult : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRunResult<TResult>>> TryGetResultIfAvailableAync<TResult>(CancellationToken cancelToken) where TResult : IDataValue { return null; }

        /// <summary>The returned task completes when this run finishes. Performs long poll.</summary>
        public Task<WorkflowRunResult> GetResultAsync() { return null; }
        public Task<WorkflowRunResult> GetResultAsync(CancellationToken cancelToken) { return null; }

        public Task<WorkflowRunResult<TResult>> GetResultAsync<TResult>() where TResult : IDataValue { return null; }
        public Task<WorkflowRunResult<TResult>> GetResultAsync<TResult>(CancellationToken cancelToken) where TResult : IDataValue { return null; }

        /// <summary>Get result of the entire workflow, if it finished (otherwise, return False). May not be of this run it's continued. No long poll.</summary>
        public Task<TryGetResult<WorkflowRunResult>> TryGetFinalWorkflowResultIfAvailableAync() { return null; }
        public Task<TryGetResult<WorkflowRunResult>> TryGetFinalWorkflowResultIfAvailableAync(CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRunResult<TResult>>> TryGetFinalWorkflowResultIfAvailableAync<TResult>() where TResult : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRunResult<TResult>>> TryGetFinalWorkflowResultIfAvailableAync<TResult>(CancellationToken cancelToken) where TResult : IDataValue { return null; }

        /// <summary>Get result of the entire workflow. May not be of this run it's continued. Performs long poll.</summary>
        public Task<WorkflowRunResult> GetFinalWorkflowResultAsync() { return null; }
        public Task<WorkflowRunResult> GetFinalWorkflowResultAsync(CancellationToken cancelToken) { return null; }
        public Task<WorkflowRunResult<TResult>> GetFinalWorkflowResultAsync<TResult>() where TResult : IDataValue { return null; }
        public Task<WorkflowRunResult<TResult>> GetFinalWorkflowResultAsync<TResult>(CancellationToken cancelToken) where TResult : IDataValue { return null; }

        public Task SignalAsync(string signalName) { return null; }
        public Task SignalAsync(string signalName, CancellationToken cancelToken) { return null; }
        public Task SignalAsync<TArg>(string signalName, TArg arg) where TArg : IDataValue { return null; }
        public Task SignalAsync<TArg>(string signalName, TArg arg, CancellationToken cancelToken) where TArg : IDataValue { return null; }
        public Task SignalAsync(string signalName, PayloadsCollection arg, CancellationToken cancelToken) { return null; }

        public Task<TResult> QueryAsync<TResult>(string queryName) where TResult : IDataValue { return null; }
        public Task<TResult> QueryAsync<TResult>(string queryName, CancellationToken cancelToken) where TResult : IDataValue { return null; }
        public Task<TResult> QueryAsync<TArg, TResult>(string queryName, TArg args) where TArg : IDataValue where TResult : IDataValue { return null; }
        public Task<TResult> QueryAsync<TArg, TResult>(string queryName, TArg args, CancellationToken cancelToken) where TArg : IDataValue where TResult : IDataValue { return null; }
        public Task<PayloadsCollection> QueryAsync(string queryName, PayloadsCollection args, CancellationToken cancelToken) { return null; }

        public Task RequestCancellationAsync() { return null; }
        public Task RequestCancellationAsync(CancellationToken cancelToken) { return null; }

        public Task TerminateAsync(string reason) { return null; }
        public Task TerminateAsync(string reason, PayloadsCollection details, CancellationToken cancelToken) { return null; }
        public Task TerminateAsync(string reason, IDataValue details, CancellationToken cancelToken) { return null; }
    }

    public sealed class WorkflowRunInfo
    {
        // @ToDo. Roughly corresponds to DescribeWorkflowExecutionResponse.
    }

    public class WorkflowRunResult<TResult> where TResult : IDataValue
    {
        public bool IsCompletedNormally { get; }
        public bool IsFailed { get; }
        public bool IsCancelled { get; }
        public bool IsTerminated { get; }
        public bool IsTimedOut { get; }
        public bool IsContinuedAsNew { get; }

        public PayloadsCollection ResultPayload { get; }

        public Exception Failure { get; }
        public IReadOnlyCollection<Exception> Failures { get; }

        public WorkflowExecutionStatus Status { get; }

        public TResult GetValue() { return default(TResult); }
    }

    public class WorkflowRunResult : WorkflowRunResult<IDataValue.Void>
    {
    }

    public class TemporalServiceClientConfiguration
    {
        public String ServiceUrl { get; set; }
        public bool IsHttpsEnabled { get; set; }
        // . . .
    }

    public class NeedsDesign
    {
        // Placeholder. @ToDo.
    }
}
