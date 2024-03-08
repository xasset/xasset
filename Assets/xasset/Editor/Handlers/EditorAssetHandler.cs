using UnityEditor;

namespace xasset.editor
{
    public struct EditorAssetHandler : IAssetHandler
    {
        public void OnStart(AssetRequest request)
        {
            if (request.isAll)
            {
                request.assets = AssetDatabase.LoadAllAssetsAtPath(request.path);
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

                var path = AssetDatabase.GetAssetPath(request.asset);
                if (path != request.path)
                {
                    var message = $"Request asset path {request.path} mismatch to the project asset path {path}.";
                    EditorUtility.DisplayDialog("Error", message, "OK");
                    Logger.E(message);
                }
            }

            request.progress = 1;
            request.SetResult(Request.Result.Success);
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

        public static IAssetHandler CreateInstance()
        {
            return new EditorAssetHandler();
        }
    }
}