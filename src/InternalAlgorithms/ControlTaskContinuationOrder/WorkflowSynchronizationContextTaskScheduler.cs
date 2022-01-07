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

        private readonly SendOrPostCallback _tryExecuteTaskWrapper;

        private readonly Queue<Task> _scheduledTasks = new Queue<Task>();

        public WorkflowSynchronizationContextTaskScheduler(WorkflowSynchronizationContext syncCtx)
            : base()
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

        public override string ToString()
        {
            return nameof(WorkflowSynchronizationContextTaskScheduler) + $"(Id={Id}, SyncCtx.Id={_syncCtx.Id})";
        }

        /// <summary>
        /// Implementation of <see cref="System.Threading.Tasks.TaskScheduler.QueueTask"/> for this scheduler class.        
        /// Simply posts the tasks to be executed on the associated <see cref="System.Threading.SynchronizationContext"/>.
        /// </summary>
        protected override void QueueTask(Task task)
        {
            Program.WriteLine($"**{this.ToString()}.QueueTask(task.Id={task?.Id}): 1");

            if (task != null)
            {
                EqueueTask(task);
            }

            Program.WriteLine($"**{this.ToString()}.QueueTask(task.Id={task?.Id}): End");
        }

        /// <summary>
        /// Implementation of <see cref="System.Threading.Tasks.TaskScheduler.TryExecuteTaskInline"/> for this scheduler class.
        /// The task will be executed in-line only the compile time setting <see cref="PermitInlineExecution" /> is
        /// set to <c>true</c> AND if the call happens within the associated <see cref="SynchronizationContext" />.
        /// </summary>        
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            Program.WriteLine($"**{this.ToString()}.TryExecuteTaskInline(task.Id={task?.Id}, previouslyQueued={taskWasPreviouslyQueued}).");

            if (PermitInlineExecution && !taskWasPreviouslyQueued && (SynchronizationContext.Current == _syncCtx))
            {
                return TryExecuteTask(task);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This method overrides <see cref="TaskScheduler.GetScheduledTasks" />. As described for any such override,
        /// this API is intended for integration with debuggers. It will only be invoked when a debugger requests the
        /// data. The returned tasks will be used by debugging tools to access the currently queued tasks, in order to
        /// provide a representation of this information in the UI.
        /// It is important to note that, when this method is called, all other threads in the process will
        /// be frozen. Therefore, it's important to avoid synchronization. Thus, we do not lock on <c>_scheduledTasks</c>
        /// and instead catch and gracefully give up in case of concurrent access errors.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            List<Task> scheduledTasks = new(capacity: _scheduledTasks.Count);

            try
            {
                foreach (Task task in _scheduledTasks)
                {
                    scheduledTasks.Add(task);
                }
            }
            catch
            {
                // If we could not access all _scheduledTasks, just return what we have.
            }

            return scheduledTasks;
        }

        public void ExecuteAllScheduledTasks()
        {
            Program.WriteLine($"**{this.ToString()}.ExecuteAllScheduledTasks(): 1  (scheduledCount={_scheduledTasks.Count})");

            using ThreadPoolInvocationState invocationState = new();
            object errorInfo = null;

            while (TryDequeueWorkAction(out UserWorkAction workAction))
            {
                invocationState.Reset(workAction);

                ThreadPool.QueueUserWorkItem(_executeWorkActionDelegate, invocationState);

                invocationState.WaitForCompletion();
                if (invocationState.TryGetException(out Exception ex))
                {
                    if (errorInfo == null)
                    {
                        errorInfo = ex;
                    }
                    else if (errorInfo is List<Exception> errorList)
                    {
                        errorList.Add(ex);
                    }
                    else
                    {
                        errorList = new List<Exception>();
                        errorList.Add((Exception) errorInfo);
                        errorList.Add(ex);
                    }
                }
            }

            if (errorInfo != null)
            {
                if (errorInfo is Exception exception)
                {
                    ExceptionDispatchInfo.Capture(exception).Throw();
                }
                else
                {
                    throw new AggregateException((List<Exception>) errorInfo);
                }
            }

            Program.WriteLine($"**{this.ToString()}.ExecuteAllScheduledTasks(): End");
        }

        private bool TryDequeueTaskn(out Task task)
        {
            lock (_scheduledTasks)
            {
                return _scheduledTasks.TryDequeue(out task);
            }
        }

        private void EqueueTask(Task task)
        {
            lock (_scheduledTasks)
            {
                _scheduledTasks.Enqueue(task);
            }
        }



        private bool TryExecuteTaskInContext(Task task)
        {
            Program.WriteLine($"**{this.ToString()}.TryExecuteTaskInContext(task.Id={task.Id}): 1");

            SynchronizationContext prevSyncCtx = SynchronizationContext.Current;

            bool installSyncCtx = (prevSyncCtx != _syncCtx);
            if (installSyncCtx)
            {
                SynchronizationContext.SetSynchronizationContext(_syncCtx);
            }

            bool canExecute;
            try
            {
                canExecute = TryExecuteTask(task);
            }
            finally
            {
                if (installSyncCtx)
                {
                    SynchronizationContext.SetSynchronizationContext(prevSyncCtx);
                }
            }

            Program.WriteLine($"**{this.ToString()}.TryExecuteTaskInContext(task.Id={task.Id}): End  (canExecute={canExecute})");
            return canExecute;
        }
    }
}
