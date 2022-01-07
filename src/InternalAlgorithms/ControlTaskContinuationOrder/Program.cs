using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlTaskContinuationOrder
{
    public class Program
    {
        static void Main(string[] _)
        {
            WriteLine("Hello World!");
            (new Program()).Execute();
        }

        private static object s_writeLineLock = new();
        private static int s_lastWriteLineThreadId = -1;

        public static void WriteLine(string msg = null)
        {
            lock (s_writeLineLock)
            {
                string lnBreak = String.Empty;

                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (s_lastWriteLineThreadId != currentThreadId)
                {
                    lnBreak = Environment.NewLine;
                }

                msg = msg ?? String.Empty;
                Console.WriteLine($"{lnBreak}"
                                + $"[Thread={currentThreadId};"
                                + $" SyncCtx={(SynchronizationContext.Current?.GetType().Name ?? "null")};"
                                + $" TskSched={GetCurrentTaskSchedulerDescription()}]"
                                + $" {msg}");

                s_lastWriteLineThreadId = currentThreadId;
            }
        }

        private static string GetCurrentTaskSchedulerDescription()
        {
            TaskScheduler ts = TaskScheduler.Current;
            if (ts == null)
            {
                return "null";
            }

            return $"{ts.Id}|{ts.GetType().Name}";
        }

        private TaskCompletionSource<string> _signalReceivedCompletion = new TaskCompletionSource<string>();
        private TaskCompletionSource<string> _activityCompletion = null;
        private string _activityName = null;

        public void Execute()
        {
            WriteLine($"Execute: 1");

            //WorkflowSynchronizationContext wfSyncCtx = new();
            //SynchronizationContext wfSyncCtx = new();
            //SynchronizationContext.SetSynchronizationContext(wfSyncCtx);

            WriteLine($"Execute: 2");

            WorkflowRoutine<bool> wfMain = WorkflowRoutine.Start(WorkflowMainAsync);

            WriteLine($"wfMain Sync Ctx: {wfMain.ToSynchronizationContextString()}");
            WriteLine($"Execute: 3");

            wfMain.InvokeAllPostedWorkActions();

            WriteLine($"Execute: 4");

            WorkflowRoutine<WorkflowRoutine.Void> activityCompleter = WorkflowRoutine.Start(CompleteActivityHelper);
            WriteLine($"activityCompleter Sync Ctx: {activityCompleter.ToSynchronizationContextString()}");

            WriteLine($"Execute: 5");

            while (!activityCompleter.IsCompleted)
            {
                activityCompleter.InvokeAllPostedWorkActions();
            }

            WriteLine($"Execute: 6");

            WorkflowRoutine<WorkflowRoutine.Void> signalHandler = WorkflowRoutine.Start(SignalHandler, "Signal A received");
            WriteLine($"signalHandler Sync Ctx: {signalHandler.ToSynchronizationContextString()}");

            WriteLine($"Execute: 7");

            //wfMain.InvokeAllPostedAsyncItems();

            WriteLine($"Execute: 8");

            while (!signalHandler.IsCompleted)
            {
                WriteLine($"Execute: 9.1");

                signalHandler.InvokeAllPostedWorkActions();

                WriteLine($"Execute: 9.2");
            }

            while (!wfMain.IsCompleted)
            {
                WriteLine($"Execute: 10.1");

                wfMain.InvokeAllPostedWorkActions();

                WriteLine($"Execute: 10.2");
            }

            WriteLine($"Execute: 11");

            wfMain.Task.GetAwaiter().GetResult();
            WriteLine($"Execute: End");
        }

        private static bool TryGetTaskFactory<TResult>(out TaskFactory<TResult> taskFactory)
        {
            taskFactory = null;

            SynchronizationContext currSyncCtx = SynchronizationContext.Current;
            if (currSyncCtx == null)
            {
                return false;
            }

            if (! (currSyncCtx is WorkflowSynchronizationContext wfSyncCtx))
            {
                return false;
            }

            taskFactory = wfSyncCtx.CreateNewTaskFactory<TResult>(CancellationToken.None);
            return true;
        }

        private async Task<bool> WorkflowMainAsync()
        {
            WriteLine($"WorkflowMainAsync: 1");

            Task<string> activityTask = ScheduleActivityAsync("Activity 1");

            WriteLine($"WorkflowMainAsync: 2");

            await activityTask;

            WriteLine($"WorkflowMainAsync: 2.5");

            Task<string> completedTask = await Task.WhenAny(activityTask, _signalReceivedCompletion.Task);

            WriteLine($"WorkflowMainAsync: 3");

            string value1 = await completedTask;

            WriteLine($"WorkflowMainAsync: 4");
            WriteLine($"WorkflowMainAsync: value1=\"{value1}\".");

            WriteLine($"WorkflowMainAsync:"
                    + $" activityTask.Status={activityTask.Status};"
                    + $" _signalReceivedCompletion.Task.Status={_signalReceivedCompletion.Task.Status}");

            WriteLine($"WorkflowMainAsync: 5");

            Task<string> scheduledTask;
            if (TryGetTaskFactory(out TaskFactory<Task<string>> taskFactory))
            {
                WriteLine($"WorkflowMainAsync: 5.1");

                scheduledTask = taskFactory.StartNew( async () =>
                    {
                        Program.WriteLine($"WorkflowMainAsync-scheduledTask: 1");
                        await Task.Delay(1000);
                        Program.WriteLine($"WorkflowMainAsync-scheduledTask: End");
                        return "scheduledTask Completed";
                    }).Unwrap();

                WriteLine($"WorkflowMainAsync: 5.2");
            }
            else
            {
                WriteLine($"WorkflowMainAsync: 5.3");
                scheduledTask = Task.FromResult("WorkflowMainAsync-scheduledTask: Could not obtain TaskFactory.");
                WriteLine($"WorkflowMainAsync: 5.4");
            }

            WriteLine($"WorkflowMainAsync: 6");

            string scheduledTaskResult = await scheduledTask;

            WriteLine($"WorkflowMainAsync: 7");
            WriteLine($"WorkflowMainAsync: scheduledTaskResult=\"{scheduledTaskResult}\"");

            string[] value2 = await Task.WhenAll(activityTask, _signalReceivedCompletion.Task);

            WriteLine($"WorkflowMainAsync: 8");
            WriteLine($"WorkflowMainAsync: value2=\"{value2}\".");

            WriteLine($"WorkflowMainAsync:"
                    + $" activityTask.Status={activityTask.Status};"
                    + $" _signalReceivedCompletion.Task.Status={_signalReceivedCompletion.Task.Status}");

            WriteLine($"WorkflowMainAsync: End");
            return true;
        }

        private Task<string> ScheduleActivityAsync(string name)
        {
            WriteLine($"ScheduleActivityAsync(\"{name}\"): 1");

            _activityName = name;
            _activityCompletion = new TaskCompletionSource<string>();
            
            WriteLine($"ScheduleActivityAsync(\"{name}\"): End");

            return _activityCompletion.Task;
        }

        private async Task SignalHandler(string data)
        {
            WriteLine($"SignalHandler(\"{data}\"): 1");

            await Task.Yield();

            WriteLine($"SignalHandler(\"{data}\"): 2");

            _signalReceivedCompletion.TrySetResult(data);

            WriteLine($"SignalHandler(\"{data}\"): 3");

            await Task.Yield();

            WriteLine($"SignalHandler(\"{data}\"): End");
        }

        private void CompleteActivityHelper()
        {
            //WriteLine($"CompleteActivityHelper: 1");

            TaskCompletionSource<string> activityCompletion = Interlocked.Exchange(ref _activityCompletion, null);

            if (activityCompletion == null)
            {
                WriteLine($"CompleteActivityHelper: _activityCompletion was NULL.");
            }
            else
            {
                WriteLine($"CompleteActivityHelper: completING activity named \"{_activityName}\".");
                activityCompletion.TrySetResult(_activityName);
                WriteLine($"CompleteActivityHelper: completED activity named \"{_activityName}\".");
            }

            WriteLine($"CompleteActivityHelper: End");
        }
    }
}
