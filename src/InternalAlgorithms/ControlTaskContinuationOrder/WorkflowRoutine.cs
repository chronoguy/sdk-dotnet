using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    public static class WorkflowRoutine
    {
        public struct Void
        {
            public static readonly Void Instance = default(Void);
            public static readonly Task<Void> CompletedTask = Task.FromResult(Instance);

            public override string ToString()
            {
                return nameof(Void);
            }
        }

        private record TaskExecutionContext<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> RoutineFunc,
                                                           TArg UserState,
                                                           CancellationToken CancelToken,
                                                           SynchronizationContext RoutineSyncCtx);

        private static class RoutineTaskExecutorFuncCache<TArg, TResult>
        {
            private static readonly Func<object, Task<TResult>> s_func = Execute<TArg, TResult>;

            internal static Func<object, Task<TResult>> Func
            {
                get { return s_func; }
            }

        }

        private static void ValidateRoutineTaskNotNull(Task routineTask, string routineFuncParamName)
        {
            if (routineTask == null)
            {
                throw new InvalidOperationException($"The {routineFuncParamName} specified for this {nameof(WorkflowRoutine)}"
                                                  + $" returned a null Task. A {routineFuncParamName} specified for a"
                                                  + $" {nameof(WorkflowRoutine)} must return a valid Task instance.");
            }
        }

        private static async Task<Void> ExecuteAndReturnVoidResult(Func<Task> routineFunc)
        {
            Task routineTask = routineFunc();

            ValidateRoutineTaskNotNull(routineTask, nameof(routineFunc));
            await routineTask;

            return Void.Instance;
        }

        private static async Task<Void> ExecuteAndReturnVoidResult<TArg>(Func<TArg, Task> routineFunc, TArg state)
        {
            Task routineTask = routineFunc(state);

            ValidateRoutineTaskNotNull(routineTask, nameof(routineFunc));
            await routineTask;

            return Void.Instance;
        }

        private static async Task<Void> ExecuteAndReturnVoidResult<TArg>(Func<TArg, CancellationToken, Task> routineFunc, TArg state, CancellationToken cancelToken)
        {
            Task routineTask = routineFunc(state, cancelToken);

            ValidateRoutineTaskNotNull(routineTask, nameof(routineFunc));
            await routineTask;

            return Void.Instance;
        }

        public static WorkflowRoutine<Void> Start(Action routineFunc)
        {
            return Start<Void, Void>( (_, _) => { routineFunc(); return Void.CompletedTask; }, Void.Instance, CancellationToken.None);
        }

        public static WorkflowRoutine<Void> Start(Func<Task> routineFunc)
        {
            return Start<Void, Void>( (_, _) => ExecuteAndReturnVoidResult(routineFunc), Void.Instance, CancellationToken.None);
        }

        public static WorkflowRoutine<Void> Start<TArg>(Func<TArg, Task> routineFunc, TArg state)
        {
            return Start<TArg, Void>( (s, _) => ExecuteAndReturnVoidResult(routineFunc, s), state, CancellationToken.None);
        }

        public static WorkflowRoutine<TResult> Start<TResult>(Func<Task<TResult>> routineFunc)
        {
            return Start<Void, TResult>( (_, _) => routineFunc(), Void.Instance, CancellationToken.None);
        }

        public static WorkflowRoutine<TResult> Start<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> routineFunc,
                                                                    TArg state,
                                                                    CancellationToken cancelToken)
        {
            WorkflowSynchronizationContext routineSyncCtx = new();
            Task<TResult> routineTask = StartAsNewTaskAsync(routineFunc, state, cancelToken, routineSyncCtx);
            return new WorkflowRoutine<TResult>(routineTask, routineSyncCtx);
        }

        private static Task<TResult> StartAsNewTaskAsync<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> routineFunc,
                                                                  TArg state,
                                                                  CancellationToken cancelToken,
                                                                  WorkflowSynchronizationContext routineSyncCtx)
        {
            Program.WriteLine($"--{nameof(WorkflowRoutine)}.StartAsNewTaskAsync(.., state={state}, ..): 1");

            if (cancelToken.IsCancellationRequested)
            {
                return Task<TResult>.FromCanceled<TResult>(cancelToken);
            }

            TaskFactory<Task<TResult>> routineTaskFactory = routineSyncCtx.CreateNewTaskFactory<Task<TResult>>(cancelToken);

            try
            {
                // 'routineTaskFactory' uses WorkflowSynchronizationContextTaskScheduler.
                // That will post the work of executing the routine to the WorkflowSynchronizationContext.
                // It will be queued, but not executed until we call InvokeAllPostedWorkActions().

                TaskExecutionContext<TArg, TResult> taskExecCtx = new(routineFunc, state, cancelToken, routineSyncCtx);
                Task<TResult> routineTask = routineTaskFactory.StartNew(RoutineTaskExecutorFuncCache<TArg, TResult>.Func, taskExecCtx)
                                                              .Unwrap();

                Program.WriteLine($"--{nameof(WorkflowRoutine)}.StartAsNewTaskAsync(.., state={state}, ..): End");
                return routineTask;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException ocEx && ocEx.CancellationToken == cancelToken)
                {
                    return Task<TResult>.FromCanceled<TResult>(cancelToken);
                }

                return Task<TResult>.FromException<TResult>(ex);
            }
        }

        private static Task<TResult> Execute<TArg, TResult>(object taskExecutionContextObject)
        {
            if (taskExecutionContextObject == null)
            {
                throw new ArgumentNullException(nameof(taskExecutionContextObject));
            }

            if (!(taskExecutionContextObject is TaskExecutionContext<TArg, TResult> taskExecutionContext))
            {
                throw new ArgumentException($"The specified {nameof(taskExecutionContextObject)} was expected to be of type"
                                          + $" {nameof(TaskExecutionContext<TArg, TResult>)}, but the actual type"
                                          + $" was {taskExecutionContextObject.GetType().FullName}.");
            }

            return Execute<TArg, TResult>(taskExecutionContext);
        }

        private static Task<TResult> Execute<TArg, TResult>(TaskExecutionContext<TArg, TResult> taskExecCtx)
        {
            if (taskExecCtx.CancelToken.IsCancellationRequested)
            {
                return Task<TResult>.FromCanceled<TResult>(taskExecCtx.CancelToken);
            }

            // If 'routineFunc' throws after the first await point, the exception will be embedded into the Task and not thrown
            // until the Task itself is awaited. If it throws before the first await point, the exception will propagate right away.
            // We need to catch iT and wrap it into a Task.
            try
            {
                return Execute<TArg, TResult>(taskExecCtx.RoutineFunc, taskExecCtx.UserState, taskExecCtx.CancelToken, taskExecCtx.RoutineSyncCtx);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException ocEx && ocEx.CancellationToken == taskExecCtx.CancelToken)
                {
                    return Task<TResult>.FromCanceled<TResult>(taskExecCtx.CancelToken);
                }

                return Task<TResult>.FromException<TResult>(ex);
            }
        }

        private static async Task<TResult> Execute<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> routineFunc,
                                                                  TArg state,
                                                                  CancellationToken cancelToken,
                                                                  SynchronizationContext routineSyncCtx)
        {
            Program.WriteLine($"--{nameof(WorkflowRoutine)}.Execute(.., state={state}, ..): 1");

            TResult result;

            SynchronizationContext prevSyncCtx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(routineSyncCtx);

            try
            {                
                Task<TResult> routineTask = routineFunc(state, cancelToken);

                ValidateRoutineTaskNotNull(routineTask, nameof(routineFunc));
                result = await routineTask;                
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevSyncCtx);
            }

            Program.WriteLine($"--{nameof(WorkflowRoutine)}.Execute(.., state={state}, ..): End");
            return result;
        }
    }  // public static class WorkflowRoutine

    public class WorkflowRoutine<TResult>
    {
        private readonly Task<TResult> _routineTask;
        private readonly WorkflowSynchronizationContext _routineSyncCtx;

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

        public void InvokeAllPostedWorkActions()
        {
            _routineSyncCtx.InvokeAllPostedWorkActions();
        }

        public string ToSynchronizationContextString()
        {
            return _routineSyncCtx.ToString();
        }
    }
}
