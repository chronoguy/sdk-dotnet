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

    #region class TemporalServiceNamespaceClient
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
    #endregion class TemporalServiceNamespaceClient

    #region class Workflow
    public class Workflow
    {
        public string Namespace { get; }
        public string WorkflowTypeName { get; }
        public string WorkflowId { get; }

        public Task<bool> HasRunsAsync() { return null; }

        public Task<IPaginatedReadOnlyCollectionPage<WorkflowRun>> ListRunsAsync(NeedsDesign oneOrMoreArgs) { return null; }

        #region StartNewRunAsync(..)
        public Task<WorkflowRun> StartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> StartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig) { return null; }
        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion StartNewRunAsync(..)


        #region TryStartNewRunAsync(..)
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection args, CancellationToken cancelToken) { return null; }
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

        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, string signalName) { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<WorkflowRun> StartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<WorkflowRun> StartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection workflowArgs, string signalName, PayloadsCollection signalArgs, CancellationToken cancelToken) { return null; }
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

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, string signalName) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, TSigArg signalArgs) where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName) where TWfArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, CancellationToken cancelToken) where TWfArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, TSigArg signalArgs) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync<TWfArg, TSigArg>(IWorkflowExecutionConfiguration workflowRunConfig, TWfArg workflowArgs, string signalName, TSigArg signalArgs, CancellationToken cancelToken) where TWfArg : IDataValue where TSigArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryStartNewRunWithSignalAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection workflowArgs, string signalName, PayloadsCollection signalArgs, CancellationToken cancelToken) { return null; }
        #endregion TryStartNewRunWithSignalAsync(..)


        #region GetActiveOrStartNewRunAsync(..)
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig) { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, CancellationToken cancelToken) { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args) where TArg : IDataValue { return null; }
        public Task<WorkflowRun> GetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<WorkflowRun> GetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection args, CancellationToken cancelToken) { return null; }
        #endregion GetActiveOrStartNewRunAsync(..)


        #region TryGetActiveOrStartNewRunAsync(..)
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(string taskQueueMoniker, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(string taskQueueMoniker, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig) { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, CancellationToken cancelToken) { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args) where TArg : IDataValue { return null; }
        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync<TArg>(IWorkflowExecutionConfiguration workflowRunConfig, TArg args, CancellationToken cancelToken) where TArg : IDataValue { return null; }

        public Task<TryGetResult<WorkflowRun>> TryGetActiveOrStartNewRunAsync(IWorkflowExecutionConfiguration workflowRunConfig, PayloadsCollection args, CancellationToken cancelToken) { return null; }
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


        #region GetRunStub<TStub>(..)
        // A stub returned by these methods can always be cast to 'IWorkflowStub'.
        // Initially, the stub is not bound to any workflow.
        // It will be bound when a stub API is called.
        // If a workflow run is active AND binding to active runs is permitted, the stub will be bound to that run.
        // If a workflow run is inactive AND binding to new runs is permitted, then a new run will be attempted to start,
        // and if it succeeds, the stub will be bount to that new run.
        // See docs for 'WorkflowXxxStubAttribute' for more.

        public TStub GetRunStub<TStub>()
        {
            return GetRunStub<TStub>(new WorkflowRunStubConfiguration(canBindToNewRun: false,
                                                                      canBindToExistingActiveRun: true,
                                                                      canBindToExistingFinishedRun: true,
                                                                      mustBindToNewIfContinued: false),
                                     runConfig: null);
        }

        public TStub GetRunStub<TStub>(string taskQueueMoniker)
        {
            return GetRunStub<TStub>(new WorkflowRunStubConfiguration(canBindToNewRun: true,
                                                                      canBindToExistingActiveRun: true,
                                                                      canBindToExistingFinishedRun: true,
                                                                      mustBindToNewIfContinued: false),
                                     new WorkflowExecutionConfiguration(taskQueueMoniker));
        }

        public TStub GetRunStub<TStub>(IWorkflowExecutionConfiguration runConfig)
        {
            return GetRunStub<TStub>(new WorkflowRunStubConfiguration(canBindToNewRun: true,
                                                                      canBindToExistingActiveRun: true,
                                                                      canBindToExistingFinishedRun: true,
                                                                      mustBindToNewIfContinued: false),
                                     runConfig);
        }
        
        public TStub GetRunStub<TStub>(WorkflowRunStubConfiguration stubConfig, IWorkflowExecutionConfiguration runConfig) { return default(TStub); }
        #endregion GetRunStub<TStub>(..)
    }
    #endregion class Workflow


    #region class WorkflowRun
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
    #endregion class WorkflowRun

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

    // ----------- -----------

    public interface IWorkflowRunStub
    {    
        WorkflowRunStubConfiguration Config { get; }
        bool IsBound { get; }
        //bool TryBind(WorkflowRun workflowRun);
        bool TryGetWorkflowRun(out WorkflowRun workflowRun);
    }

    // ----------- -----------
    
    public sealed class WorkflowRunStubConfiguration
    {
        public bool CanBindToNewRun { get; init; }
        public bool CanBindToExistingActiveRun { get; init; }
        public bool CanBindToExistingFinishedRun { get; init; }        
        public bool MustBindToNewIfContinued { get; init; }
        public WorkflowRunStubConfiguration()
            : this(canBindToNewRun: true, canBindToExistingActiveRun: true, canBindToExistingFinishedRun: true, mustBindToNewIfContinued: false) { }
        public WorkflowRunStubConfiguration(bool canBindToNewRun, bool canBindToActiveRun)
            : this(canBindToNewRun, canBindToExistingActiveRun: canBindToActiveRun, canBindToExistingFinishedRun: true, mustBindToNewIfContinued: false) { }

        public WorkflowRunStubConfiguration(bool canBindToNewRun, bool canBindToExistingActiveRun, bool canBindToExistingFinishedRun, bool mustBindToNewIfContinued)
        {
            CanBindToNewRun = canBindToNewRun;
            CanBindToExistingActiveRun = canBindToExistingActiveRun;
            CanBindToExistingFinishedRun = canBindToExistingFinishedRun;
            MustBindToNewIfContinued = mustBindToNewIfContinued;
        }
    }

    /// <summary>
    /// Can be applied to methods with signatures:
    ///     Task SomeMethod()
    ///     Task{TResult} SomeMethod() where TResult : IDataValue
    ///     Task SomeMethod(TArg args) where TArg : IDataValue
    ///     Task{TResult} SomeMethod(TArg args) where TArg : IDataValue where TResult : IDataValue
    ///     Task{PayloadsCollection} SomeMethod(PayloadsCollection args, CancellationToken cancelToken)
    /// otherwise an error during stub generation is thrown.
    /// @ToDo: Cancellation Tokens?
    /// 
    /// The workflow type name is specified to the API that generated the stub. It is not specified here.
    /// 
    /// It is NOT prohibited to have MULTIPLE stub methods point to the main workflow method.
    /// Parameters are not validated by the client and are sent to the workflow as provided.
    /// If no parameters are provided, an empty payload is sent.
    /// 
    /// If a RunMethodStub is invoked on a stub instance that is NOT yet bound to a workflow run, it will attempt to bind the stub
    /// instance to the first option permitted by both, the 'WorkflowRunStubConfiguration' specified to the stab creation method and
    /// the settings of this attribute in the follwing order:
    ///   1) Bind to an existing Active run
    ///   2) Start a New run and bind to it
    ///   3) Bind to an existing Finished run
    /// If it cannot find anything to bind to based on above-mentioned permissions, an error is thrown.
    /// If permissions specify a binding strategy, but the execution fails, the respective error is thrown
    /// and no other binding is attempted. For example, assume that all CanBindXxx settings are True, and there are existing runs,
    /// yet all of them are finished. In that case, based on the above order, it will try to start a New run and bind to it.
    /// If, however, starting new run fails based on the 'WorkflowIdReusePolicy', the failure will be propagated and binding
    /// to an existing Finished run will not be attempted.
    /// 
    /// If CanBindToExistingRun is true, then the RunMethodStub can be called to an active workflow run multiple times.
    /// In that case the returned Task represents the completion of the active run; the run is not started again.
    /// Potential arguments are ignored in such case.
    /// 
    /// If the 'MustBindToNewIfContinued' property of the 'WorkflowRunStubConfiguration' used when the stub instance was created is True,
    /// then the Task returned by a RunMethodStub represents the completion of the ENTIRE workflow, including the as-new continuations
    /// of the current run.
    /// 
    /// This and other 'WorkflowXxxStub' attributes can only be applied to method definitions in intercafes.
    /// They are ignored in classes.
    /// This and other 'WorkflowXxxStub' are interpreted by the workflow client SDK. They do NOT configure how
    /// workflow implementations are hosted by the worker. However, if a worker host loads a workflow that implements interfaces
    /// with 'WorkflowXxxStub'-attributed methods, the host will validate that the workflow implementation has attribute-based
    /// handlers for APIs defined by the 'WorkflowXxxStub' attributes.
    /// 
    /// Note that the signatures required for a 'WorkflowXxxStubAttribute' may be different from the corresponding 'WorkflowXxxHandlerAttribute'.
    /// For examples, queries are async from the client perpective, but must be handled synchronously in the implementation.
    ///     
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class WorkflowRunMethodStubAttribute : Attribute
    {
        public bool CanBindToNewRun { get; set; }
        public bool CanBindToExistingRun { get; set; }
        public WorkflowRunMethodStubAttribute() { CanBindToNewRun = true; CanBindToExistingRun = true; }
    }

    /// <summary>
    /// Can be applied to methods with signatures:
    ///     Task SomeMethod()
    ///     Task SomeMethod(CancellationToken cancelToken)
    ///     
    ///     Task SomeMethod(TArg args) where TArg : IDataValue
    ///     Task SomeMethod(TArg args, CancellationToken cancelToken) where TArg : IDataValue
    ///     
    ///     Task SomeMethod(PayloadsCollection args)
    ///     Task SomeMethod(PayloadsCollection args, CancellationToken cancelToken)
    /// otherwise an error during stub generation is thrown.
    /// 
    /// If 'SignalTypeName' is not specified OR null OR Empty OR WhiteSpaceOnly, then the signal type name is auto-populated
    /// by taking the method name and removing 'Async' from its end if present
    /// (if that would result in an empty string, then 'Async' is not removed).
    /// 
    /// It is NOT prohibited to have multiple stub methods point to the same signal.
    /// Parameters are not validated by the client and are sent to the workflow signal handler as provided.
    /// 
    /// If the stub instance is not bound when a SignalStub-method is invoked, an error is thrown.
    /// 
    /// See <see cref="WorkflowRunMethodStubAttribute" /> for more information on various stub methods and their relation to handler methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class WorkflowSignalStubAttribute : Attribute
    {
        public string SignalTypeName { get; set; }
        public WorkflowSignalStubAttribute() { }
    }

    /// <summary>
    /// Can be applied to methods with signatures:
    ///     Task{TResult} SomeMethod() where TResult : IDataValue
    ///     Task{TResult} SomeMethod(CancellationToken cancelToken) where TResult : IDataValue
    ///     
    ///     Task{TResult} SomeMethod(TArg args) where TArg : IDataValue where TResult : IDataValue
    ///     Task{TResult} SomeMethod(TArg args, CancellationToken cancelToken) where TArg : IDataValue where TResult : IDataValue
    ///     
    ///     Task{PayloadsCollection} SomeMethod(PayloadsCollection args)
    ///     Task{PayloadsCollection} SomeMethod(PayloadsCollection args, CancellationToken cancelToken)
    /// otherwise an error during stub generation is thrown.
    /// 
    /// If 'QueryTypeName' is not specified OR null OR Empty OR WhiteSpaceOnly, then the query type name is auto-populated
    /// by taking the method name and removing 'Async' from its end if present
    /// (if that would result in an empty string, then 'Async' is not removed).
    /// 
    /// It is NOT prohibited to have multiple stub methods point to the same query.
    /// Parameters are not validated by the client and are sent to the workflow query handler as provided.
    /// 
    /// If the stub instance is not bound when a QueryStub-method is invoked, an error is thrown.
    /// 
    /// See <see cref="WorkflowRunMethodStubAttribute" /> for more information on various stub methods and their relation to handler methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class WorkflowQueryStubAttribute : Attribute
    {
        public string QueryTypeName { get; set; }
        public WorkflowQueryStubAttribute() { }
    }

    // ----------- -----------

    public class NeedsDesign
    {
        // Placeholder. @ToDo.
    }

    public class NeedsDesignException : Exception
    {
        // Placeholder. @ToDo.
    }
}
