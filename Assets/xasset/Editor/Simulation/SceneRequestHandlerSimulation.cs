using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xasset.editor
{
    public struct SceneRequestHandlerSimulation : SceneRequestHandler
    {
        public void OnStart()
        {
        }

        public void Update()
        {
        }

        public void Release()
        {
        }

        public AsyncOperation LoadSceneAsync()
        {
            var parameters = new LoadSceneParameters
                {loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single};
            return EditorSceneManager.LoadSceneAsyncInPlayMode(request.info.path, parameters);
        }

        public bool IsReady()
        {
            return true;
        }

        public void WaitForCompletion()
        {
            var parameters = new LoadSceneParameters
                {loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single};
            EditorSceneManager.LoadSceneInPlayMode(request.info.path, parameters);
            request.SetResult(Request.Result.Success);
        }

        private SceneRequest request { get; set; }
        public float progressRate => 1;

        public static SceneRequestHandler CreateInstance(SceneRequest request)
        {
            return new SceneRequestHandlerSimulation {request = request};
        }
    }
}