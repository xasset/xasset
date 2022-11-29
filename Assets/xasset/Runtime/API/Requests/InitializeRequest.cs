using System;
using UnityEngine;

namespace xasset
{
    public class InitializeRequest : Request
    {
        public static Func<IInitializeHandler> CreateHandler { get; set; } = RuntimeInitializeHandler.CreateInstance;

        public IInitializeHandler handler { get; } = CreateHandler();

        protected override void OnUpdated()
        {
            handler.OnUpdated(this);
        }

        protected override void OnStart()
        {
            handler.OnStart(this);
        }

        protected override void OnCompleted()
        {
            if (!Application.isEditor && Assets.IsWebGLPlatform)
                Assets.DownloadURL = Assets.PlayerDataPath;

            Logger.D($"Initialize with: {result}.");
            Logger.D($"API Version:{Assets.APIVersion}");
            Logger.D($"Simulation Mode: {Assets.SimulationMode}");
            Logger.D($"Offline Mode: {Assets.OfflineMode}");
            Logger.D($"Versions: {Assets.Versions}");
            Logger.D($"Platform: {Assets.Platform}");
        }
    }
}