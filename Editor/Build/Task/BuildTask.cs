using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace xasset.editor
{
    public interface IBuildStep
    {
        void Start(BuildTask task);
    }

    public class BuildTask
    {
        private readonly Dictionary<string, BuildEntry> _assets = new Dictionary<string, BuildEntry>();
        public readonly List<BuildEntry> assets = new List<BuildEntry>();
        public readonly List<BuildBundle> bundles = new List<BuildBundle>();
        public readonly List<string> changes = new List<string>();
        public long buildLastTimeTime { get; }
        private BuildTask(Build build)
        {
            parameters = build.parameters;
            groups = build.groups;
            var file = AssetDatabase.GetAssetPath(build);
            buildLastTimeTime = Settings.GetLastWriteTime(file);
        }

        public BuildGroup[] groups { get; }

        public string error { get; set; }
        public bool nothingToBuild { get; set; }

        public BuildParameters parameters { get; }

        public bool AddAsset(BuildEntry entry)
        {
            if (_assets.ContainsKey(entry.asset))
            {
                Logger.W($"asset {entry.asset} already exist in build {parameters.name}.");
                return false;
            } 
            _assets[entry.asset] = entry;
            assets.Add(entry);
            return true;
        }

        public BuildEntry GetAsset(string path)
        {
            return _assets.TryGetValue(path, out var value) ? value : null;
        }

        public static BuildTask StartNew(Build build, params IBuildStep[] steps)
        {
            var task = new BuildTask(build);
            task.Start(steps);
            return task;
        }

        private void Start(params IBuildStep[] jobs)
        {
            foreach (var job in jobs)
            {
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    job.Start(this);
                }
                catch (Exception e)
                {
                    Logger.E($"{e.Message}:{e.StackTrace}");
                    error = e.Message;
                }

                sw.Stop();
                if (!string.IsNullOrEmpty(error))
                {
                    Logger.I(
                        $"{job.GetType().Name} for {parameters.name} failed({error}) with {sw.ElapsedMilliseconds / 1000f}s.");
                    break;
                }

                Logger.I(
                    $"{job.GetType().Name} for {parameters.name} success with {sw.ElapsedMilliseconds / 1000f}s.");
            }
        }

        public void TreatError(string e)
        {
            error = e;
            Logger.E($"{error}");
        }
    }
}