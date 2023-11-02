namespace xasset
{
    public abstract class LoadRequest : Request, IRecyclable
    {
        protected int refCount { get; private set; }
        public string path { get; protected set; } 

        protected override void OnCompleted()
        {
            Logger.D($"Load {GetType().Name} {path} {result}."); 
        }

        public void Retain()
        {
            refCount++;
        }

        public void Release()
        {
            if (refCount == 0)
            {
                Logger.E($"Release {GetType().Name} {path} too many times {refCount}.");
                return;
            }

            refCount--;
            if (refCount > 0) return;

            Recycler.RecycleAsync(this);
        }

        protected abstract void OnDispose();

        public void WaitForCompletion()
        {
            if (isDone) return;

            if (status == Status.Wait) Start();

            OnWaitForCompletion();
        }

        protected virtual void OnWaitForCompletion()
        {
        }

        // private ActionRequest _completeRequest;

        protected void LoadAsync()
        {
            if (refCount > 0)
            {
                if (isDone) ActionRequest.CallAsync(Complete);
                // {
                //     if (_completeRequest == null)
                //     {
                //         _completeRequest = ActionRequest.Create();
                //         _completeRequest.reuse = false;
                //         
                //     }
                //
                //     _completeRequest.Reset();
                //     _completeRequest.action = Complete;
                //     _completeRequest.SendRequest();
                // }
            }
            else
            {
                SendRequest(); 
            }

            Retain();
        }

        #region IRecyclable

        public void EndRecycle()
        {
            Logger.D($"Unload {GetType().Name} {path}.");
            OnDispose(); 
            // if (_completeRequest == null) return;
            // ActionRequest.Recycle(_completeRequest);
            // _completeRequest = null;
        }

        public virtual bool CanRecycle()
        {
            return isDone;
        }

        public bool IsUnused()
        {
            return refCount == 0;
        }

        public virtual void RecycleAsync()
        {
        }

        public virtual bool Recycling()
        {
            return false;
        }

        #endregion
    }
}