using UnityEditor;

namespace xasset.editor
{
    public struct AssetRequestHandlerSimulation : AssetRequestHandler
    {
        private AssetRequest request { get; set; }

        public void OnStart()
        {
            Load();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            request = null;
        }

        public void WaitForCompletion()
        {
        }

        private void Load()
        {
            if (request.isAll)
            {
                request.assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(request.info.path);
                if (request.assets == null)
                {
                    request.SetResult(Request.Result.Failed, "subAssets == null");
                    return;
                }
            }
            else
            {
                request.asset = AssetDatabase.LoadAssetAtPath(request.info.path, request.type);
                if (request.asset == null)
                {
                    request.SetResult(Request.Result.Failed, "asset == null");
                    return;
                }
            }

            request.progress = 1;
            request.SetResult(Request.Result.Success);
        }

        public static AssetRequestHandler CreateInstance(AssetRequest request)
        {
            return new AssetRequestHandlerSimulation {request = request};
        }
    }
}