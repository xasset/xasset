using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace xasset.editor
{
    public interface IBuildJobStep
    {
        void Start(BuildJob job);
    }

    public class BuildJob
    {
        public readonly List<BuildAsset> bundledAssets = new List<BuildAsset>();
        public readonly List<BuildBundle> bundles = new List<BuildBundle>();
        public readonly List<string> changes = new List<string>();
        public string error { get; set; }

        public BuildJob(BuildParameters buildParameters)
        {
            parameters = buildParameters;
        }

        public BuildParameters parameters { get; }

        public void AddAsset(BuildAsset asset)
        {
            bundledAssets.Add(asset);
        }

        public static BuildJob StartNew(BuildParameters parameters, params IBuildJobStep[] steps)
        {
            var task = new BuildJob(parameters);
            task.Start(steps);
            return task;
        }

        public void Start(params IBuildJobStep[] steps)
        {
            foreach (var step in steps)
            {
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    step.Start(this);
                }
                catch (Exception e)
                {
                    Logger.E($"{e.Message}:{e.StackTrace}");
                    error = e.Message;
                }

                sw.Stop();
                Logger.I($"{step.GetType().Name} for {parameters.name} {(string.IsNullOrEmpty(error) ? "success" : "failed")} with {sw.ElapsedMilliseconds / 1000f}s.");
                if (!string.IsNullOrEmpty(error)) break;
            }
        }

        public void TreatError(string e)
        {
            error = e;
            Logger.E($"{error}");
        }
    }
}