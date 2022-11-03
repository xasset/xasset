namespace xasset
{
    internal struct BundleRequestHandlerFile : BundleRequestHandler
    {
        public BundleRequest request { get; set; }
        public string path;

        public void OnStart()
        {
            request = request;
            request.LoadAssetBundle(path);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void WaitForCompletion()
        {
        }
    }  

    public interface BundleRequestHandler
    {
        void OnStart();
        void Update();
        void Dispose();
        void WaitForCompletion();
    }
}