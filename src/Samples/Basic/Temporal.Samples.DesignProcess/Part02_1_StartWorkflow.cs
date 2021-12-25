using System;
using System.Threading.Tasks;

using Temporal.Common.WorkflowConfiguration;
using Temporal.WorkflowClient;

namespace Temporal.Sdk.BasicSamples
{
    public class Part02_1_StartWorkflow
    {
        
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();            
        }

        public static async Task MainAsync(string[] _)
        {
            var serviceConfig = new TemporalServiceClientConfiguration();
            var serviceClient = new TemporalServiceClient(serviceConfig);

            WorkflowRun workflowRun1 = await serviceClient.StartNewWorkflowRunAsync("namespace", "workflowTypeName", "taskQueueMoniker");

            WorkflowRun workflowRun2 = await serviceClient.StartNewWorkflowRunAsync("namespace", "workflowTypeName", "workflowId", "taskQueueMoniker");


            WorkflowClient workflowClient = serviceClient.CreateNewWorkflowClient("namespace", "workflowTypeName", "workflowId");

            WorkflowRun workflowRun10 = await workflowClient.StartNewRunAsync("taskQueueMoniker");

            WorkflowRun workflowRun11 = await workflowClient.StartNewRunAsync((IWorkflowExecutionConfiguration) null);


            WorkflowRun workflowRun12 = await workflowClient.GetOrStartRunAsync("workflowRunId", (IWorkflowExecutionConfiguration) null);

            WorkflowRun workflowRun13 = await workflowClient.GetRunAsync("workflowRunId");

            WorkflowRun workflowRun14 = await workflowClient.GetRunAsync();
        }
    }

    public class TemporalServiceClient
    {
        static private TemporalServiceClientConfiguration CreateDefaultConfiguration() { return null; }
        public TemporalServiceClient() : this(CreateDefaultConfiguration()) { }
        public TemporalServiceClient(TemporalServiceClientConfiguration config) { }

        // Misc general APIs:
        public Task<NeedsDesign> GetClusterInfoAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> DescribeTaskQueueAsync(NeedsDesign args) { return null; }               

        // Namespace control APIs:
        public Task<NeedsDesign> RegisterNamespaceAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> DescribeNamespaceAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> ListNamespacesAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> UpdateNamespaceAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> DeprecateNamespaceAsync(NeedsDesign args) { return null; }

        // Workflow exploration APIs:
        public Task<NeedsDesign> ListOpenWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> ListClosedWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> ListWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> ListArchivedWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> ScanWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> CountWorkflowExecutionsAsync(NeedsDesign args) { return null; }
        public Task<NeedsDesign> GetSearchAttributesAsync(NeedsDesign args) { return null; }

        // 
        public Task<WorkflowClient> GetWorkflowAsync(string workflowNamespace, string workflowTypeName, string workflowId) { return null; }
        public Task<TryGetResult<WorkflowClient>> TryGetWorkflowAsync(string workflowNamespace, string workflowTypeName, string workflowId) { return null; }

        public Task<StartNewWorkflowResult> StartNewWorkflowAsync(string workflowNamespace, string workflowTypeName, string taskQueueMoniker) { return null; }
        public Task<StartNewWorkflowResult> StartNewWorkflowAsync(string workflowNamespace, string workflowTypeName, IWorkflowExecutionConfiguration workflowConfiguration) { return null; }
    }

    public class WorkflowClient
    {
        public Task<WorkflowRun> StartNewRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> StartNewRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }

        public Task<WorkflowRun> GetOrStartRunAsync(string taskQueueMoniker) { return null; }
        public Task<WorkflowRun> GetOrStartRunAsync(IWorkflowExecutionConfiguration workflowConfiguration) { return null; }

        public Task<WorkflowRun> GetRunAsync(string workflowRunId) { return null; }

        public Task<WorkflowRun> GetRunAsync() { return null; }
    }

    public class WorkflowRun
    {

    }

    public sealed class StartNewWorkflowResult
    {
        public WorkflowClient Workflow { get; }
        public WorkflowRun WorkflowRun { get; }
        private StartNewWorkflowResult() { throw new NotSupportedException("Use a different ctor overload."); }
        public StartNewWorkflowResult(WorkflowClient workflow, WorkflowRun workflowRun) { Workflow = workflow; WorkflowRun = workflowRun; }        
    }



    public sealed class TryGetResult<T>
    {
        public bool IsFound { get; }
        public T Item { get; }
        internal TryGetResult() : this(false, default(T)) { }
        internal TryGetResult(bool isFound, T item) { IsFound = isFound; Item = item; }
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
