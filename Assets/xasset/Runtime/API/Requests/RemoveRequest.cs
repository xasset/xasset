using System.Collections.Generic;
using System.IO;

namespace xasset
{
    public class RemoveRequest : Request
    {
        public readonly List<string> files = new List<string>();
        public int max { get; private set; }
        public int current => max - files.Count;

        protected override void OnStart()
        {
            max = files.Count;
        }

        protected override void OnUpdated()
        {
            if (Downloader.IsDownloading) return;

            progress = current * 1f / max;
            while (files.Count > 0)
            {
                var index = files.Count - 1;
                var file = files[index];
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Logger.D($"Delete:{file}.");
                }

                files.RemoveAt(index);
                if (Scheduler.Busy) return;
            }

            SetResult(Result.Success);
        }
    }
}