using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    public static class WorkflowRoutine
    {
        public struct Void
        {
            public static readonly Void Instance = new Void();
            public static readonly Task<Void> CompletedTask = Task.FromResult(Instance);            
        }

        public static WorkflowRoutine<Void> Start(Action routineFunc)
        {
            return WorkflowRoutine<Void>.Start<Void>( (_) => { routineFunc(); return Void.CompletedTask; }, Void.Instance);
        }

        public static WorkflowRoutine<Void> Start(Func<Task> routineFunc)
        {
            return WorkflowRoutine<Void>.Start<Void>( async (_) => { await routineFunc(); return Void.Instance; }, Void.Instance);
        }

        public static WorkflowRoutine<Void> Start<TArg>(Func<TArg, Task> routineFunc, TArg state)
        {
            return WorkflowRoutine<Void>.Start<Void>(async (_) => { await routineFunc(state); return Void.Instance; }, Void.Instance);
        }

        public static WorkflowRoutine<TResult> Start<TResult>(Func<Task<TResult>> routineFunc)
        {
            return WorkflowRoutine<TResult>.Start<Void>( (_) => routineFunc(), Void.Instance);
        }

        public static WorkflowRoutine<TResult> Start<TArg, TResult>(Func<TArg, Task<TResult>> routineFunc, TArg state)
        {
            return WorkflowRoutine<TResult>.Start<TArg>(routineFunc, state);
        }
    }

    public class WorkflowRoutine<TResult>
    {
        private readonly Task<TResult> _routineTask;
        private readonly WorkflowSynchronizationContext _routineSyncCtx;

        public static WorkflowRoutine<TResult> Start<TArg>(Func<TArg, Task<TResult>> routineFunc, TArg state)
        {
            WorkflowSynchronizationContext routineSyncCtx = new();
            Task<TResult> wrappedRoutineTask = Execute(routineFunc, state, routineSyncCtx);
            return new WorkflowRoutine<TResult>(wrappedRoutineTask, routineSyncCtx);
        }

        private static async Task<TResult> Execute<TArg>(Func<TArg, Task<TResult>> routineFunc, TArg state, SynchronizationContext routineSyncCtx)
        {
            SynchronizationContext prevSyncCtx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(routineSyncCtx);

            TResult result;
            try
            {
                Task<TResult> routineTask = routineFunc(state);
                result = await routineTask;                
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevSyncCtx);
            }

            return result;
        }

        public WorkflowRoutine(Task<TResult> routineTask, WorkflowSynchronizationContext routineSyncCtx)
        {
            _routineTask = routineTask;
            _routineSyncCtx = routineSyncCtx;
        }

        public bool IsCompleted
        {
            get { return Task.IsCompleted; }
        }

        public Task<TResult> Task
        {
            get { return _routineTask; }
        }

        public void InvokeAllPostedAsyncItems()
        {
            _routineSyncCtx.InvokeAllPosted();
        }

        public string ToSynchronizationContextString()
        {
            return _routineSyncCtx.ToString();
        }
    }
}
