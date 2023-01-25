using System.Collections.Generic;

namespace xasset.samples
{
    public class PreloadRequest : Request
    {
        private readonly Queue<PreloadItem> queue = new Queue<PreloadItem>();
        public bool loadingBar { get; set; }
        private int size { get; set; }

        public void Enqueue(PreloadItem preloadItem)
        {
            queue.Enqueue(preloadItem);
        }

        protected override void OnStart()
        {
            if (loadingBar) LoadingScreen.Instance.SetVisible(true);
            PreloadAsset.ClearAllAssets();
            size = queue.Count;
        }

        protected override void OnUpdated()
        {
            while (queue.Count > 0)
            {
                var item = queue.Peek();
                if (!item.loading) item.Load();
                if (item.loading && !item.isDone) return;
                queue.Dequeue();
                item.Complete();
                var loaded = size - queue.Count;
                progress = loaded * 1f / size;
                if (loadingBar)
                    LoadingScreen.Instance.SetProgress($"{Constants.Text.Loading}({loaded}/{size})", progress);
            }

            if (loadingBar) LoadingScreen.Instance.SetVisible(false);
            SetResult(Result.Success);
        }
    }
}