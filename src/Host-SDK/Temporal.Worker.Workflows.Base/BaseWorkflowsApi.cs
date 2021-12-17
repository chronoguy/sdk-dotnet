using System;
using System.Threading.Tasks;

using Temporal.CommonDataModel;

namespace Temporal.Worker.Workflows.Base
{
    public class BaseWorkflowsApi
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
}
