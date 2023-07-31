using System;
using System.Collections;

namespace xasset
{
    public abstract class DownloadRequestBase : IEnumerator
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
            Progressing,
            Paused,
            Completed
        }

        private double _bandwidthSampleTime;
        private ulong _lastDownloadedBytes;

        public Result result { get; private set; } = Result.Default;
        public string error { get; protected internal set; }
        public Status status { get; protected internal set; } = Status.Wait;
        public ulong downloadedBytes { get; protected internal set; }
        public ulong downloadSize { get; set; }
        public float progress { get; protected set; }
        public ulong bandwidth { get; protected internal set; }

        public bool isDone => status == Status.Completed;

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
            downloadedBytes = 0;
            downloadSize = 0;
            SetResult(Result.Default);
        }

        public object Current => null;

        public void SetResult(Result value, string msg = null)
        {
            result = value;
            status = result == Result.Default ? Status.Wait : status = Status.Completed;
            error = msg;
        }

        protected internal void BeganSample()
        {
            bandwidth = 0;
            _lastDownloadedBytes = 0;
            _bandwidthSampleTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        private double GetRealtimeSinceBeganSample()
        {
            return DateTime.Now.TimeOfDay.TotalMilliseconds - _bandwidthSampleTime;
        }

        public void Pause()
        {
            if (status == Status.Paused) return;
            status = Status.Paused;
            bandwidth = 0;
            OnPause(status == Status.Paused);
        }

        public void UnPause()
        {
            if (status != Status.Paused) return;
            status = Status.Progressing;
            OnPause(status == Status.Paused);
        }

        public void Cancel()
        {
            SetResult(Result.Cancelled, DownloadErrors.UserCancel);
            OnCancel();
        }

        protected virtual void OnPause(bool paused)
        {
        }

        protected virtual void OnCancel()
        {
        }

        protected internal void OnReceiveBytes(ulong len)
        {
            _lastDownloadedBytes += len;
            downloadedBytes += len;
            progress = downloadedBytes * 1f / downloadSize;
            var elapsed = GetRealtimeSinceBeganSample();
            if (elapsed > 1000) BeganSample();
            if (elapsed > 0 && _lastDownloadedBytes > 0)
                bandwidth = (ulong)(_lastDownloadedBytes / elapsed) * 1000;
        }

        public virtual void Retry()
        {
        }
    }
}