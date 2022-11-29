using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    /// <summary>
    ///     支持自动切片的调度器，可以对单帧密集的更新操作进行负载均衡处理。
    /// </summary>
    public class Scheduler : MonoBehaviour
    {
        private static readonly Dictionary<string, RequestQueue> _Queues = new Dictionary<string, RequestQueue>();
        private static readonly List<RequestQueue> Queues = new List<RequestQueue>();
        private static readonly Queue<RequestQueue> Append = new Queue<RequestQueue>();
        private static float _realtimeSinceStartup;

        [SerializeField] [Tooltip("每个队列最大单帧更新数量。")]
        private byte maxRequests = 10;

        [SerializeField] [Tooltip("自动切片时间，值越大处理的请求数量越多，值越小处理请求的数量越小，可以根据目标帧率分配。")]
        private float maxUpdateTimeSlice = 1 / 60f;

        [SerializeField] [Tooltip("是否开启自动切片")] private bool autoSlicing = true;
        public static bool AutoSlicing { get; set; } = true;
        public static bool Working => Queues.Exists(o => o.working);
        public static bool Busy => AutoSlicing && Time.realtimeSinceStartup - _realtimeSinceStartup > MaxUpdateTimeSlice;
        public static float MaxUpdateTimeSlice { get; set; }
        public static byte MaxRequests { get; set; } = 10;

        private void Start()
        {
            AutoSlicing = autoSlicing;
            MaxUpdateTimeSlice = maxUpdateTimeSlice;
            MaxRequests = maxRequests;
        }

        private void Update()
        {
            _realtimeSinceStartup = Time.realtimeSinceStartup;
            while (Append.Count > 0)
            {
                var item = Append.Dequeue();
                Queues.Add(item);
            }

            foreach (var queue in Queues)
                if (!queue.Update())
                    break;
        }

        public static void Enqueue(Request request)
        {
            var key = request.GetType().Name;
            if (!_Queues.TryGetValue(key, out var queue))
            {
                queue = new RequestQueue {key = key, maxRequests = MaxRequests};
                _Queues.Add(key, queue);
                Append.Enqueue(queue);
                // TODO: 这里可以考虑给 Request 加个优先级。
            }

            queue.Enqueue(request);
        }
    }
}