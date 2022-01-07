using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    public sealed class WorkflowSynchronizationContext : SynchronizationContext
    {
        #region Static Id management

        private static int s_lastId = 0;

        private static int GetNextId()
        {
            int id = Interlocked.Increment(ref s_lastId);
            while (id > (Int32.MaxValue / 2))
            {
                Interlocked.Exchange(ref s_lastId, 0);
                id = Interlocked.Increment(ref s_lastId);
            }

            return id;
        }

        #endregion Static Id management

        public const TaskCreationOptions DefaultTaskCreationOptions = TaskCreationOptions.PreferFairness
                                                                    | TaskCreationOptions.DenyChildAttach
                                                                    | TaskCreationOptions.RunContinuationsAsynchronously;

        public const TaskContinuationOptions DefaultTaskContinuationOptions = TaskContinuationOptions.PreferFairness
                                                                            | TaskContinuationOptions.DenyChildAttach
                                                                            | TaskContinuationOptions.LazyCancellation;

        private readonly Queue<UserWorkAction> _workActions = new Queue<UserWorkAction>();
        private readonly WaitCallback _executeWorkActionDelegate;
        private readonly WorkflowSynchronizationContextTaskScheduler _taskScheduler;

        public WorkflowSynchronizationContext()                        
            : this(originalId: null)
        {            
        }

        private WorkflowSynchronizationContext(int? originalId)
        {
            Id = GetNextId();
            OriginalId = originalId ?? Id;
            _executeWorkActionDelegate = ExecuteWorkAction;
            _taskScheduler = new WorkflowSynchronizationContextTaskScheduler(this);
        }

        public int Id { get; }

        public int OriginalId { get; }

        public TaskScheduler TaskScheduler
        {
            get { return _taskScheduler; }
        }

        public override string ToString()
        {
            return $"{nameof(WorkflowSynchronizationContext)}(Id={Id}, OriginalId={OriginalId})";
        }

        public TaskFactory CreateNewTaskFactory(CancellationToken cancelToken)
        {
            return new TaskFactory(cancelToken, DefaultTaskCreationOptions, DefaultTaskContinuationOptions, TaskScheduler);
        }

        public TaskFactory<TResult> CreateNewTaskFactory<TResult>(CancellationToken cancelToken)
        {
            return new TaskFactory<TResult>(cancelToken, DefaultTaskCreationOptions, DefaultTaskContinuationOptions, TaskScheduler);
        }

        public override SynchronizationContext CreateCopy()
        {
            return new WorkflowSynchronizationContext(OriginalId);
        }

        /// <summary>Dispatches a synchronous message. NOT Supported - will throw <c>NotSupportedException</c>!</summary>        
        public override void Send(SendOrPostCallback workAction, object state)
        {
            throw new NotSupportedException($"{nameof(WorkflowSynchronizationContext)}.{nameof(Send)}(..) is not supported.");
        }

        /// <summary>Dispatches an asynchronous message.</summary>   
        public override void Post(SendOrPostCallback workAction, object state)
        {
            Post(workAction, state, representedTask: null);
        }

        /// <summary>Dispatches an asynchronous message.</summary>
        public void Post(SendOrPostCallback workAction, object state, Task representedTask)
        {
            Program.WriteLine($"**{this.ToString()}.POST(.., representedTask={(representedTask?.Id.ToString() ?? "null")}): 1");

            if (workAction == null)
            {
                return;
            }

            UserWorkAction callback = new UserWorkAction(workAction.Invoke, state, representedTask);
            EqueueWorkAction(callback);

            Program.WriteLine($"**{this.ToString()}.POST(.., representedTask={(representedTask?.Id.ToString() ?? "null")}): End");
        }

        public void InvokeAllPostedWorkActions()
        {
            Program.WriteLine($"**{this.ToString()}.InvokeAllPostedWorkActions(): 1  (wrkActsCnt={_workActions.Count})");

            using ThreadPoolInvocationState invocationState = new();
            object errorInfo = null;

            while(TryDequeueWorkAction(out UserWorkAction workAction))
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

            Program.WriteLine($"**{this.ToString()}.InvokeAllPostedWorkActions(): End");
        }

        private bool TryDequeueWorkAction(out UserWorkAction workAction)
        {
            lock (_workActions)
            {
                return _workActions.TryDequeue(out workAction);                
            }
        }

        private void EqueueWorkAction(UserWorkAction workAction)
        {
            lock (_workActions)
            {
                _workActions.Enqueue(workAction);
            }
        }

        /// <summary>
        /// This method backs <see cref="WorkflowSynchronizationContextTaskScheduler.GetScheduledTasks" />.
        /// As described for any override of <see cref="TaskScheduler.GetScheduledTasks" /> this API is
        /// intended for integration with debuggers. It will only be invoked when a debugger requests the data.
        /// The returned tasks will be used by debugging tools to access the currently queued tasks, in order to
        /// provide a representation of this information in the UI.
        /// It is important to note that, when this method is called, all other threads in the process will
        /// be frozen. Therefore, it's important to avoid synchronization. Thus, we do not lock on <c>_workActions</c>
        /// and instead catch and gracefully give up in case of concurrent access errors.
        /// </summary>
        /// <returns></returns>
        internal IReadOnlyCollection<Task> GetAllPostedTasks()
        {
            List<Task> postedTasks = new(capacity: _workActions.Count);

            try
            {
                foreach (UserWorkAction workAction in _workActions)
                {
                    if (workAction.TryGetRepresentedTask(out Task workActionTask))
                    {
                        postedTasks.Add(workActionTask);
                    }
                }
            }
            catch
            {
                // If we could not access all workActionTask, just return what we have.
            }

            return postedTasks;
        }

        private void ExecuteWorkAction(object invocationStateObject)
        {
            if (invocationStateObject == null)
            {
                throw new ArgumentNullException(nameof(invocationStateObject));
            }

            if (! (invocationStateObject is ThreadPoolInvocationState invocationState))
            {
                throw new ArgumentException($"The specified {nameof(invocationStateObject)} was expected to be of type"
                                          + $" {nameof(ThreadPoolInvocationState)}, but the actual type"
                                          + $" was {invocationStateObject.GetType().Name}.");
            }

            try
            {
                ExecuteWorkActionInContext(invocationState.WorkAction);
                invocationState.TrySetStatusSucceeded();
            }
            catch (Exception ex)
            {
                invocationState.TrySetStatusFailed(ex);
            }
        }

        private void ExecuteWorkActionInContext(UserWorkAction workAction)
        {
            Program.WriteLine($"**{this.ToString()}.ExecuteWorkActionInContext(workAction.TaskId={workAction.RepresentedTask?.Id}): 1");

            SynchronizationContext prevSyncCtx = SynchronizationContext.Current;

            bool installSyncCtx = (prevSyncCtx != this);
            if (installSyncCtx)
            {
                SynchronizationContext.SetSynchronizationContext(this);
            }

            try
            {
                workAction.Invoke();
            }
            finally
            {
                if (installSyncCtx)
                {
                    SynchronizationContext.SetSynchronizationContext(prevSyncCtx);
                }
            }

            Program.WriteLine($"**{this.ToString()}.ExecuteWorkActionInContext(workAction.TaskId={workAction.RepresentedTask?.Id}): End");
        }

        #region Inner types

        private record UserWorkAction(Action<object> Action, object State, Task RepresentedTask)
        {
            public void Invoke()
            {
                Action(State);
            }

            public bool TryGetRepresentedTask(out Task representedTask)
            {
                representedTask = RepresentedTask;
                return (representedTask != null);
            }
        }

        private sealed class ThreadPoolInvocationState : IDisposable
        {
            public static class ExecutionStatus
            {
                public const int NotStarted = 0;
                public const int Succeeded = 2;
                public const int Failed = 3;
            }

            private UserWorkAction _workAction = null;
            private Exception _exception = null;
            private int _status = ExecutionStatus.NotStarted;
            private ManualResetEventSlim _completionSignal = new(initialState: false);

            public UserWorkAction WorkAction
            {
                get { return _workAction; }
            }

            public int Status
            {
                get { return _status; }
            }

            private ManualResetEventSlim GetCompletionSignal()
            {
                ManualResetEventSlim completionSignal = _completionSignal;
                if (completionSignal == null)
                {
                    throw new ObjectDisposedException($"This {nameof(ThreadPoolInvocationState)} instance is already disposed.");
                }

                return completionSignal;
            }

            public bool TrySetStatusSucceeded()
            {
                if (ExecutionStatus.NotStarted != Interlocked.CompareExchange(ref _status, ExecutionStatus.Succeeded, ExecutionStatus.NotStarted))
                {
                    return false;
                }

                GetCompletionSignal().Set();
                return true;                
            }

            public bool TrySetStatusFailed(Exception exception)
            {
                if (ExecutionStatus.NotStarted != Interlocked.CompareExchange(ref _status, ExecutionStatus.Failed, ExecutionStatus.NotStarted))
                {
                    return false;
                }
                
                _exception = exception;
                GetCompletionSignal().Set();
                return true;                
            }

            public void WaitForCompletion()
            {
                GetCompletionSignal().Wait();
            }

            public bool TryGetException(out Exception exception)
            {
                exception = _exception;
                return (exception != null);
            }

            public void Reset(UserWorkAction workAction)
            {
                if (workAction == null)
                {
                    throw new ArgumentNullException(nameof(workAction));
                }

                ResetCore(workAction);
            }

            public void Dispose()
            {
                ResetCore(workAction: null);
                ManualResetEventSlim completionSignal = Interlocked.Exchange(ref _completionSignal, null);
                completionSignal?.Dispose();
            }

            private void ResetCore(UserWorkAction workAction)
            {
                _workAction = workAction;
                _exception = null;
                _status = ExecutionStatus.NotStarted;
                GetCompletionSignal().Reset();
            }
        }
        
        #endregion Inner types
    }
}
