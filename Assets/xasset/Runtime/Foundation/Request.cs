using System;
using System.Collections;

namespace xasset
{
    public class Request : IEnumerator
    {
        public enum Result
        {
            Default,
            Success,
            Failed,
            Cancelled
        }

        public enum Status
        {
            Wait,
            Processing,
            Complete
        }

        public Action<Request> completed;

        public Result result { get; protected set; } = Result.Default;
        public Status status { get; protected set; } = Status.Wait;
        public bool isDone => status == Status.Complete;
        public float progress { get; set; }
        public string error { get; protected set; }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
            completed = null;
            status = Status.Wait;
        }

        public object Current => null;

        public bool Update()
        {
            if (isDone) return false;
            OnUpdated();
            return true;
        }

        public void SetResult(Result value, string msg = null)
        {
            progress = 1;
            result = value;
            status = result == Result.Default ? Status.Wait : Status.Complete;
            error = msg;
        }

        public void Start()
        {
            if (status != Status.Wait) return;
            status = Status.Processing;
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdated()
        {
        }

        protected virtual void OnCompleted()
        {
        }

        public void SendRequest()
        {
            Scheduler.Enqueue(this);
        }

        public void Complete()
        {
            OnCompleted();
            var saved = completed;
            completed?.Invoke(this);
            completed -= saved;
        }

        public void Cancel()
        {
            SetResult(Result.Cancelled);
        }
    }
}