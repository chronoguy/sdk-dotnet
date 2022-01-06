using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    public sealed class WorkflowSynchronizationContext : SynchronizationContext
    {
        public static TResult ExecuteInContext<TArg, TResult>(Func<TArg, TResult> action, TArg state, SynchronizationContext targetSyncCtx)
        {
            Program.WriteLine($"--ExecuteInContext(..): 1");

            if (action == null)
            {
                return default(TResult);
            }
            
            if (targetSyncCtx == null)
            {
                return action(state);
            }

            SynchronizationContext prevSyncCtx = SynchronizationContext.Current;
            bool installSyncCtx = (prevSyncCtx != targetSyncCtx);

            Program.WriteLine($"--ExecuteInContext(..): 2; installSyncCtx={installSyncCtx}.");

            if (installSyncCtx)
            {
                SynchronizationContext.SetSynchronizationContext(targetSyncCtx);
            }

            TResult result;
            try
            {                
                Program.WriteLine($"--ExecuteInContext(..): 3 (start callback)");
                result = action(state);
                Program.WriteLine($"--ExecuteInContext(..): 4 (end callback)");
            }
            finally
            {
                if (installSyncCtx)
                {
                    SynchronizationContext.SetSynchronizationContext(prevSyncCtx);
                }
            }

            Program.WriteLine($"--ExecuteInContext(..): End");
            return result;
        }

        private static int s_lastId = 0;
        
        private readonly Queue<UserCallback> _actions = new Queue<UserCallback>();

        public WorkflowSynchronizationContext()
        {
            Id = GetNextId();
            OriginalId = Id;
        }

        private WorkflowSynchronizationContext(int originalId)
        {
            Id = GetNextId();
            OriginalId = originalId;
        }

        public int Id { get; }

        public int OriginalId { get; }

        public override string ToString()
        {
            return $"{nameof(WorkflowSynchronizationContext)}(Id={Id}, OriginalId={OriginalId})";
        }

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

        public override SynchronizationContext CreateCopy()
        {
            return new WorkflowSynchronizationContext();
        }
       
        public void InvokeAllPosted()
        {
            ThreadPoolInvocationState invocationState = new(this);
            object errorInfo = null;

            while(_actions.TryDequeue(out UserCallback callbackItem))
            {
                invocationState.Reset();

                ThreadPool.QueueUserWorkItem(callbackItem.InvokeDelegate, invocationState);

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
        }

        private bool TryDequeueAction(out UserCallback callbackItem)
        {
            lock (_actions)
            {
                return _actions.TryDequeue(out callbackItem);                
            }
        }

        private void EqueueAction(UserCallback callbackItem)
        {
            lock (_actions)
            {
                _actions.Enqueue(callbackItem);
            }
        }

        // Dispatches an asynchronous message.
        public override void Post(SendOrPostCallback callbackDelegate, object state)
        {
            Program.WriteLine($"**{this.ToString()}.POST(..): 1");

            if (callbackDelegate == null)
            {
                return;
            }

            UserCallback callback = new UserCallback(callbackDelegate.Invoke, state);
            EqueueAction(callback);

            Program.WriteLine($"**{this.ToString()}.POST(..): End");
        }

        // Dispatches a synchronous message.
        public override void Send(SendOrPostCallback workItem, object state)
        {
            throw new NotSupportedException($"{nameof(WorkflowSynchronizationContext)}.{nameof(Send)}(..) is not supported.");
        }

        private record UserCallback(Action<object> CallbackDelegate, object State)
        {
            private WaitCallback _invokeDelegate = null;
            private Func<object, object> _callbackDelegateAsFunc = null;

            public WaitCallback InvokeDelegate
            {
                get
                {
                    if (_invokeDelegate == null)
                    {
                        _invokeDelegate = InvokeManaged;
                    }

                    return _invokeDelegate;
                }
            }

            public void InvokeDirectly()
            {
                CallbackDelegate(State);
            }

            private object CallbackDelegateAsFuncWrapper(object state)
            {
                CallbackDelegate(state);
                return null;
            }

            public void InvokeManaged(object invocationStateObject)
            {
                if (invocationStateObject == null)
                {
                    throw new ArgumentNullException(nameof(invocationStateObject));
                }

                if (! (invocationStateObject is ThreadPoolInvocationState invocationState))
                {
                    throw new ArgumentException($"The specified {nameof(invocationStateObject)} was expected to be of type"
                                              + $" {nameof(ThreadPoolInvocationState)}, but the actual type was {invocationStateObject.GetType().Name}.");
                }

                try
                {                    
                    if (_callbackDelegateAsFunc == null)
                    {
                        _callbackDelegateAsFunc = CallbackDelegateAsFuncWrapper;
                    }

                    ExecuteInContext(_callbackDelegateAsFunc, State, invocationState.TargetSyncCtx);
                    invocationState.TrySetStatusSucceeded();
                }
                catch (Exception ex)
                {
                    invocationState.TrySetStatusFailed(ex);
                }
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

            private Exception _exception = null;
            private int _status = ExecutionStatus.NotStarted;
            private ManualResetEventSlim _completionSignal = new(initialState: false);

            public ThreadPoolInvocationState(SynchronizationContext targetSyncCtx)
            {
                TargetSyncCtx = targetSyncCtx;
            }

            public SynchronizationContext TargetSyncCtx { get; }

            public int Status
            {
                get { return _status; }
            }

            public bool TrySetStatusSucceeded()
            {
                if (ExecutionStatus.NotStarted != Interlocked.CompareExchange(ref _status, ExecutionStatus.Succeeded, ExecutionStatus.NotStarted))
                {
                    return false;
                }

                _completionSignal.Set();
                return true;                
            }

            public bool TrySetStatusFailed(Exception exception)
            {
                if (ExecutionStatus.NotStarted != Interlocked.CompareExchange(ref _status, ExecutionStatus.Failed, ExecutionStatus.NotStarted))
                {
                    return false;
                }
                
                _exception = exception;
                _completionSignal.Set();
                return true;                
            }

            public void WaitForCompletion()
            {
                _completionSignal.Wait();
            }

            public bool TryGetException(out Exception exception)
            {
                exception = _exception;
                return (exception != null);
            }

            public void Reset()
            {
                _exception = null;
                _status = ExecutionStatus.NotStarted;
                _completionSignal.Reset();
            }

            public void Dispose()
            {
                Reset();
                ManualResetEventSlim completionSignal = Interlocked.Exchange(ref _completionSignal, null);
                completionSignal.Dispose();
            }
        }
    }
}
