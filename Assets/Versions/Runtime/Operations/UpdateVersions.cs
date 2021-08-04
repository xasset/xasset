namespace VEngine
{
    public sealed class UpdateVersions : Operation
    {
        public ManifestAsset asset;
        public string file;

        public string version { get; private set; }

        public bool changed { get; private set; }

        public override void Start()
        {
            base.Start();
            if (Versions.OfflineMode)
            {
                Finish();
                return;
            }

            asset = ManifestAsset.LoadAsync(file);
        }

        public void Override()
        {
            if (Versions.OfflineMode) return;

            asset.Override();
        }

        public void Dispose()
        {
            if (asset == null) return;

            if (asset.status != LoadableStatus.Unloaded) asset.Release();
        }

        protected override void Update()
        {
            if (status == OperationStatus.Processing)
            {
                if (!asset.isDone) return;

                version = asset.assetVersion.version.ToString();
                changed = asset.changed;
                Finish(asset.error);
            }
        }
    }
}