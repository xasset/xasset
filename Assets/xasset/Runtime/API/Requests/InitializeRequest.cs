using System;

namespace xasset
{
    public class InitializeRequest : Request
    {
        private readonly InitializeRequestHandler _handler;

        public InitializeRequest()
        {
            _handler = CreateHandler(this);
        }

        public static Func<InitializeRequest, InitializeRequestHandler> CreateHandler { get; set; } = InitializeRequestHandlerRuntime.CreateInstance;

        protected override void OnUpdated()
        {
            _handler.OnUpdated();
        }

        protected override void OnStart()
        {
            _handler.OnStart();
        }

        protected override void OnCompleted()
        {
            Logger.D($"Initialize with: {result}.");
            Logger.D($"API Version:{Assets.APIVersion}");
            Logger.D($"Simulation Mode: {Assets.SimulationMode}");
            Logger.D($"Versions: {Assets.Versions}");
            Logger.D($"Platform: {Assets.Platform}");
        }
    }
}