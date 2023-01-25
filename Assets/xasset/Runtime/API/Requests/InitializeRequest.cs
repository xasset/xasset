using System;
using UnityEngine;

namespace xasset
{
    public class InitializeRequest : Request
    {
        private static InitializeRequest _request;

        private float _startTime;

        public static InitializeRequest InitializeAsync(Action<Request> completed = null)
        {
            if (_request == null)
            {
                _request = new InitializeRequest();
                _request.SendRequest();
                _request.completed = completed;
            }

            if (_request.isDone)
            {
                ActionRequest.CallAsync(_request.Complete);
            }

            return _request;
        }

        public static Func<IInitializeHandler> CreateHandler { get; set; } = RuntimeInitializeHandler.CreateInstance;

        public IInitializeHandler handler { get; } = CreateHandler();

        protected override void OnUpdated()
        {
            handler.OnUpdated(this);
        }

        protected override void OnStart()
        {
            handler.OnStart(this);
            _startTime = Time.realtimeSinceStartup;
        }

        protected override void OnCompleted()
        {
            if (!Application.isEditor && Assets.IsWebGLPlatform)
                Assets.DownloadURL = Assets.PlayerDataPath;

            Logger.D($"Initialize {result} with {Time.realtimeSinceStartup - _startTime} seconds.");
            Logger.D($"API Version:{Assets.APIVersion}");
            Logger.D($"Simulation Mode: {Assets.SimulationMode}");
            Logger.D($"Offline Mode: {Assets.OfflineMode}");
            Logger.D($"Versions: {Assets.Versions}");
            Logger.D($"Platform: {Assets.Platform}");
        }
    }
}