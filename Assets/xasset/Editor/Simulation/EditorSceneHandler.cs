using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xasset.editor
{
    public struct EditorSceneHandler : ISceneHandler
    {
        public void OnStart(SceneRequest request)
        {
        }

        public void Update(SceneRequest request)
        {
        }

        public void Release(SceneRequest request)
        {
        }

        public AsyncOperation LoadSceneAsync(SceneRequest request)
        {
            var parameters = new LoadSceneParameters
                {loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single};
            return EditorSceneManager.LoadSceneAsyncInPlayMode(request.path, parameters);
        }

        public bool IsReady(SceneRequest request1)
        {
            return true;
        }

        public void WaitForCompletion(SceneRequest request)
        {
            var parameters = new LoadSceneParameters
                {loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single};
            EditorSceneManager.LoadSceneInPlayMode(request.path, parameters);
            request.SetResult(Request.Result.Success);
        }

        public float progressRate => 1;

        public static ISceneHandler CreateInstance()
        {
            return new EditorSceneHandler();
        }
    }
}