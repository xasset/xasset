using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    [DisallowMultipleComponent]
    public class Downloader : MonoBehaviour
    {
        internal static readonly Queue<DownloadContentRequest> Queue = new Queue<DownloadContentRequest>();
        private static readonly List<DownloadContentRequest> Progressing = new List<DownloadContentRequest>();
        private static readonly Queue<DownloadContentRequest> Unused = new Queue<DownloadContentRequest>();

        [Range(1, 10)] [SerializeField] private byte maxRequests = 5;
        [SerializeField] private bool simulationMode;

        public static byte MaxRequests { get; set; } = 5;
        public static bool IsDownloading => Queue.Count > 0 || Progressing.Count > 0;
        public static bool SimulationMode { get; set; }
        public static bool Paused { get; private set; }

        private void Awake()
        {
            if (Application.isEditor) SimulationMode = simulationMode;
        }

        private void Start()
        {
            MaxRequests = maxRequests;
        }

        private void Update()
        {
            UpdateAll();
        }

        private void OnDestroy()
        {
            CancelAll();
        }


        public static DownloadContentRequest DownloadAsync(DownloadContent content)
        {
            DownloadContentRequest request;
            if (Unused.Count > 0)
            {
                request = Unused.Dequeue();
                request.Reset();
            }
            else
            {
                request = new DownloadContentRequest();
            }

            request.content = content;
            request.SendRequest();
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

            while (Queue.Count > 0 && (Progressing.Count < MaxRequests || MaxRequests == 0))
            {
                var item = Queue.Dequeue();
                if (item.status == DownloadRequest.Status.Wait) item.Start();

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

            DownloadContentRequestBatch.UpdateAll();
        }

        private static void Complete(DownloadContentRequest request)
        {
            request.Complete();
            switch (request.result)
            {
                case DownloadRequest.Result.Success:
                    Unused.Enqueue(request);
                    break;
                case DownloadRequest.Result.Cancelled:
                    Unused.Enqueue(request);
                    break;
                case DownloadRequest.Result.Failed:
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

            DownloadContentRequestBatch.CancelAll();
        }
    }
}