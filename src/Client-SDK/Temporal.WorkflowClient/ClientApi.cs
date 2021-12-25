using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Temporal.WorkflowClient
{
    public class ClientApi
    {
    }



    public sealed class TryGetAsyncResult<T>
    {
        private readonly TaskCompletionSource<bool> _isFoundCompletion = new TaskCompletionSource<bool>();
        private readonly TaskCompletionSource<T> _getCompletion = new TaskCompletionSource<T>();

        internal bool TrySetNotFound()
        {
            lock (_getCompletion)
            {
                _isFoundCompletion.TrySetResult(false);
                return _getCompletion.TrySetResult(default(T));
            }
        }

        internal bool TrySetFound(T item)
        {
            lock (_getCompletion)
            {
                _isFoundCompletion.TrySetResult(true);
                return _getCompletion.TrySetResult(item);
            }
        }

        internal bool TrySetCanceled(CancellationToken cancelToken)
        {
            lock (_getCompletion)
            {
                _isFoundCompletion.TrySetCanceled(cancelToken);
                return _getCompletion.TrySetCanceled(cancelToken);
            }
        }

        internal bool TrySetException(Exception exception)
        {
            lock (_getCompletion)
            {
                _isFoundCompletion.TrySetException(exception);
                return _getCompletion.TrySetException(exception);
            }
        }

        public Task<bool> IsFoundAsync()
        {
            return _isFoundCompletion.Task;
        }

        public Task<T> GetAsync()
        {
            return _getCompletion.Task;
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return _getCompletion.Task.GetAwaiter();
        }
    }
}
