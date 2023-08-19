using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    [DisallowMultipleComponent]
    public class Downloader : MonoBehaviour
    {
        internal static readonly Queue<DownloadRequest> Queue = new Queue<DownloadRequest>();
        private static readonly List<DownloadRequest> Progressing = new List<DownloadRequest>();
        private static readonly Queue<DownloadRequest> Unused = new Queue<DownloadRequest>();

        public static Func<DownloadRequest, IDownloadHandler> CreateHandler { get; set; } =
            request => new DownloadHandlerUWR(request);

        public static bool IsDownloading => Queue.Count > 0 || Progressing.Count > 0;
        public static bool Paused { get; private set; }


        private void Update()
        {
            UpdateAll();
        }

        private void OnDestroy()
        {
            CancelAll();
        }


        public static DownloadRequest DownloadAsync(DownloadContent content)
        {
            DownloadRequest request;
            if (Unused.Count > 0)
            {
                request = Unused.Dequeue();
                request.Reset();
            }
            else
            {
                request = new DownloadRequest();
            }

            request.content = content;
            request.SendRequest();
            request.handler = Assets.IsWebGLPlatform
                ? new DownloadHandlerUWR(request)
                : CreateHandler(request);

            return request;
        }

        public static void Pause()
        {
            if (Paused) return;
            foreach (var request in Progressing) request.Pause();
            Paused = true;
        }

        public static void UnPause()
        {
            if (!Paused) return;
            foreach (var request in Progressing) request.UnPause();
            Paused = false;
        }

        private static void UpdateAll()
        {
            if (Paused) return;

            while (Queue.Count > 0 && (Progressing.Count < Assets.MaxDownloads || Assets.MaxDownloads == 0))
            {
                var item = Queue.Dequeue();
                if (item.status == DownloadRequestBase.Status.Wait) item.Start();

                Progressing.Add(item);
            }

            for (var index = 0; index < Progressing.Count; index++)
            {
                var item = Progressing[index];
                if (item.isDone)
                {
                    Progressing.RemoveAt(index);
                    index--;
                    Complete(item);
                    continue;
                }

                item.Update();
            }

            DownloadRequestBatch.UpdateAll();
        }

        private static void Complete(DownloadRequest request)
        {
            request.Complete();
            switch (request.result)
            {
                case DownloadRequestBase.Result.Success:
                    Unused.Enqueue(request);
                    break;
                case DownloadRequestBase.Result.Cancelled:
                    Unused.Enqueue(request);
                    break;
                case DownloadRequestBase.Result.Failed:
                    break;
                default:
                    throw new Exception($"Invalid download status {request.status}");
            }
        }

        private static void CancelAll()
        {
            foreach (var request in Progressing) request.Cancel();

            Progressing.Clear();
            Queue.Clear();

            DownloadRequestBatch.CancelAll();
        }
    }
}