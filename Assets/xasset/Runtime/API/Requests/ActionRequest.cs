using System;
using System.Collections.Generic;

namespace xasset
{
    public class ActionRequest : Request
    {
        private static readonly Queue<ActionRequest> Unused = new Queue<ActionRequest>();
        private Action action;

        protected override void OnStart()
        {
            action?.Invoke();
            SetResult(Result.Success);
        }

        protected override void OnCompleted()
        {
            Remove(this);
        }

        public static ActionRequest CallAsync(Action action)
        {
            var request = Unused.Count > 0 ? Unused.Dequeue() : new ActionRequest();
            request.Reset();
            request.action = action;
            request.SendRequest();
            return request;
        }

        private static void Remove(ActionRequest request)
        {
            Unused.Enqueue(request);
        }
    }
}