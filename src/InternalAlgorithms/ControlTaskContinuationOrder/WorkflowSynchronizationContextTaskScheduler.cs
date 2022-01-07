using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    /// <summary>
    /// A TaskScheduler implementation that executes all tasks queued to it through a call to
    /// <see cref="System.Threading.SynchronizationContext.Post"/> on the <see cref="System.Threading.SynchronizationContext"/>
    /// that its associated with.
    /// The default constructor for this class binds to the current <see cref="System.Threading.SynchronizationContext"/>
    /// The implementation is copied from the .NET sources and modified to prevent in-line task execution, forcing in-line execution
    /// requests to run asynchronously.
    /// </summary>
    internal class WorkflowSynchronizationContextTaskScheduler : TaskScheduler
    {
        public const bool PermitInlineExecution = false;

        // SynchronizationContext associated with this scheduler:
        private readonly WorkflowSynchronizationContext _syncCtx;

        // Preallocated SendOrPostCallback delegate:
        private readonly SendOrPostCallback _tryExecuteTaskWrapper;

        public WorkflowSynchronizationContextTaskScheduler(WorkflowSynchronizationContext syncCtx)
        {
            if (syncCtx == null)
            {
                throw new ArgumentNullException(nameof(syncCtx));
            }

            _syncCtx = syncCtx;
            _tryExecuteTaskWrapper = (taskObject) => TryExecuteTask((Task) taskObject);
        }

        /// <summary>
        /// Implements the <see cref="System.Threading.Tasks.TaskScheduler.MaximumConcurrencyLevel"/> property this scheduler.
        /// Returns 1, because a <see cref="WorkflowSynchronizationContext"/> runs items sequentially.        
        /// </summary>
        public override int MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _syncCtx.GetAllPostedTasks();
        }

        /// <summary>
        /// Implementation of <see cref="System.Threading.Tasks.TaskScheduler.QueueTask"/> for this scheduler class.        
        /// Simply posts the tasks to be executed on the associated <see cref="System.Threading.SynchronizationContext"/>.
        /// </summary>
        protected override void QueueTask(Task task)
        {
            _syncCtx.Post(_tryExecuteTaskWrapper, (object) task, task);
        }

        /// <summary>
        /// Implementation of <see cref="System.Threading.Tasks.TaskScheduler.TryExecuteTaskInline"/> for this scheduler class.
        /// The task will be executed in-line only the compile time setting <see cref="PermitInlineExecution" /> is
        /// set to <c>true</c> AND if the call happens within the associated <see cref="SynchronizationContext" />.
        /// </summary>        
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (PermitInlineExecution && (SynchronizationContext.Current == _syncCtx))
            {
                return TryExecuteTask(task);
            }
            else
            {
                return false;
            }
        }
    }
}
