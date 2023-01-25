using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset.editor
{
    public struct EditorInitializeHandler : IInitializeHandler
    {
        public void OnUpdated(InitializeRequest request)
        {
            while (_queue.Count > 0)
            {
                var group = _queue.Dequeue();
                switch (group.addressMode)
                {
                    case AddressMode.LoadByDependencies:
                    case AddressMode.LoadByPath:
                        break;
                    case AddressMode.LoadByName:
                    {
                        var assets = Settings.Collect(group);
                        foreach (var asset in assets)
                        {
                            Assets.SetAddress(asset.path, Path.GetFileName(asset.path));
                        }
                    }
                        break;
                    case AddressMode.LoadByNameWithoutExtension:
                    {
                        var assets = Settings.Collect(group);
                        foreach (var asset in assets)
                        {
                            Assets.SetAddress(asset.path, Path.GetFileNameWithoutExtension(asset.path));
                        }
                    }
                        break;
                }

                request.progress = (max - _queue.Count) * 1f / max;
                if (Scheduler.Busy) return;
            }

            request.progress = 1;
            request.SetResult(Request.Result.Success);
        }

        private Queue<BuildGroup> _queue;

        private int max;

        public void OnStart(InitializeRequest request)
        {
            _queue = new Queue<BuildGroup>();
            Assets.Versions = ScriptableObject.CreateInstance<Versions>();
            Assets.PlayerAssets = ScriptableObject.CreateInstance<PlayerAssets>();
            var assets = Settings.FindAssets<BuildGroup>();
            max = assets.Length;
            foreach (var asset in assets)
                _queue.Enqueue(asset); 
        }

        public static IInitializeHandler CreateInstance()
        {
            return new EditorInitializeHandler();
        }
    }
}