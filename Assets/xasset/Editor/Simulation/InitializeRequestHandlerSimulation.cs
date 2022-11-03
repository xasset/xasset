using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset.editor
{
    public struct InitializeRequestHandlerSimulation : InitializeRequestHandler
    {
        public void OnUpdated()
        {
            while (_queue.Count > 0)
            {
                var build = _queue.Dequeue();
                var manifest = ScriptableObject.CreateInstance<Manifest>();
                var assets = new List<ManifestAsset>();
                var dirs = new List<string>();
                foreach (var item in build.parameters.groups)
                {
                    if (item == null)
                    {
                        Logger.W($"Group is missing in build {build.name}");
                        continue;
                    }

                    var collect = CollectAssets.Collect(item);
                    foreach (var asset in collect)
                    {
                        var dir = Path.GetDirectoryName(asset.path)?.Replace("\\", "/");
                        var pos = dirs.IndexOf(dir);
                        if (pos == -1)
                        {
                            dirs.Add(dir);
                            pos = dirs.Count - 1;
                        }

                        assets.Add(new ManifestAsset {name = Path.GetFileName(asset.path), dir = pos});
                    }
                }

                manifest.dirs = dirs.ToArray();
                manifest.assets = assets.ToArray();
                manifest.build = build.name;
                manifest.OnAfterDeserialize();
                Assets.Versions.data.Add(new Version
                {
                    build = build.name,
                    ver = 0,
                    manifest = manifest
                });
                request.progress = (max - _queue.Count) * 1f / max;
                if (Scheduler.Busy) return;
            }

            request.progress = 1;
            request.SetResult(Request.Result.Success);
        }

        private InitializeRequest request { get; set; }

        private Queue<Build> _queue;

        private int max;

        public void OnStart()
        {
            Assets.Versions = ScriptableObject.CreateInstance<Versions>();
            Assets.PlayerAssets = ScriptableObject.CreateInstance<PlayerAssets>();
            var builds = Builder.FindAssets<Build>();
            _queue = new Queue<Build>();
            max = builds.Length;
            foreach (var build in builds) _queue.Enqueue(build);
        }

        public static InitializeRequestHandler CreateInstance(InitializeRequest initializeRequest)
        {
            return new InitializeRequestHandlerSimulation {request = initializeRequest};
        }
    }
}