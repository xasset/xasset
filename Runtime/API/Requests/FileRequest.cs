using System;
using System.Collections.Generic;

namespace xasset
{
    public enum FileReadMode
    {
        String = 0,
        Bytes = 1,
        Json = 2,
    }

    public class FileAsset
    {
        public string error;
        public byte[] bytes;
        public string content;

        public void Dispose()
        {
            bytes = null;
            content = string.Empty;
        }
    }

    public class FileRequest : LoadRequest, IReloadable
    {
        private static readonly Queue<FileRequest> Unused = new Queue<FileRequest>();

        internal static readonly Dictionary<string, FileRequest> Loaded = new Dictionary<string, FileRequest>();

        public FileReadMode readMode { get; set; }

        public IFileHandler handler { get; } = CreateHandler();

        public FileAsset asset { get; set; }

        public ManifestAsset info { get; internal set; }

        public override int priority => 1;

        public static Func<IFileHandler> CreateHandler { get; set; } = RuntimeFileHandler.CreateInstance;

        public Action reloaded { get; set; }

        protected override void OnStart()
        {
            handler.OnStart(this);
        }

        protected override void OnUpdated()
        {
            handler.Update(this);
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion(this);
        }

        public void ReloadAsync()
        {
            status = Status.Processing;
            handler.OnReload(this);
        }

        public void OnReloaded()
        {
            reloaded?.Invoke();
            reloaded = null;
        }

        public bool IsReloaded()
        {
            OnUpdated();
            return isDone;
        }

        protected override void OnDispose()
        {
            Remove(this);
            handler.Dispose(this);
            asset.Dispose();
            asset = null;
        }

        private static void Remove(FileRequest request)
        {
            Loaded.Remove(request.path);
        }

        internal static FileRequest Load(string path, FileReadMode readMode)
        {
            if (!Files.TryGetFile(ref path, out var info))
            {
                Logger.E($"File not found {path}");
                return null;
            }

            var key = path;
            if (!Loaded.TryGetValue(key, out var request))
            {
                request = Unused.Count > 0 ? Unused.Dequeue() : new FileRequest();
                request.Reset();
                request.info = info;
                request.path = path;
                request.readMode = readMode;
                Loaded[key] = request;
            }

            request.LoadAsync();
            return request;
        }
    }
}