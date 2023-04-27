using UnityEditor;

namespace xasset.editor
{
    public struct EditorAssetHandler : IAssetHandler
    {
        public void OnStart(AssetRequest request)
        {
            Load(request);
        }

        public void Update(AssetRequest request)
        {
        }

        public void Dispose(AssetRequest request)
        {
        }

        public void WaitForCompletion(AssetRequest request)
        {
        }

        public void OnReload(AssetRequest request)
        {
        }

        private void Load(AssetRequest request)
        {
            if (request.isAll)
            {
                request.assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(request.path);
                if (request.assets == null)
                {
                    request.SetResult(Request.Result.Failed, "subAssets == null");
                    return;
                }
            }
            else
            {
                request.asset = AssetDatabase.LoadAssetAtPath(request.path, request.type);
                if (request.asset == null)
                {
                    request.SetResult(Request.Result.Failed, "asset == null");
                    return;
                }
            }

            request.progress = 1;
            request.SetResult(Request.Result.Success);
        }

        public static IAssetHandler CreateInstance()
        {
            return new EditorAssetHandler();
        }
    }
}