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
        public string error;

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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.I($"Start build job with {parameters.build}.");
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
                    Logger.E(e.Message);
                    error = e.Message;
                }

                sw.Stop();
                Logger.I($"{step.GetType().Name} for {parameters.build} {(string.IsNullOrEmpty(error) ? "success" : "failed")} with {sw.ElapsedMilliseconds / 1000f}s.");
                if (!string.IsNullOrEmpty(error)) break;
            }

            stopwatch.Stop();
            Logger.I($"Complete build job for {parameters.build} {(string.IsNullOrEmpty(error) ? "success" : $"failed({error})")} with {stopwatch.ElapsedMilliseconds / 1000f}s.");
        }

        public void TreatError(string e)
        {
            error = e;
            Logger.E($"{error}");
        }
    }
}