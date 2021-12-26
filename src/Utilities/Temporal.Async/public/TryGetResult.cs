using System;

namespace Temporal.Async
{
    public sealed class TryGetResult<T>
    {
        private readonly bool _isSuccess;
        private readonly T _result;

        internal TryGetResult()
            : this(false, default(T))
        {
        }

        internal TryGetResult(bool isSuccess, T result)
        {
            _isSuccess = isSuccess;
            _result = result;
        }

        public bool IsSuccess()
        {
            return IsSuccess(out _);
        }

        public bool IsSuccess(out T result)
        {
            result = _result;
            return _isSuccess;
        }
    }
}
