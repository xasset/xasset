using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xasset
{
    public struct RuntimeSceneHandler : ISceneHandler
    {
        private Dependencies _dependencies;

        public void OnStart(SceneRequest request)
        {
            _dependencies = Dependencies.LoadAsync(request.info);
        }

        public void Update(SceneRequest request)
        {
            if (_dependencies.isDone) return;
            request.progress = _dependencies.progress * progressRate;
            _dependencies.Update();
        }

        public void Release(SceneRequest request)
        {
            _dependencies.Release();
        }

        public AsyncOperation LoadSceneAsync(SceneRequest request)
        {
            var loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            return SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(request.path), loadSceneMode);
        }

        public bool IsReady(SceneRequest request)
        {
            return _dependencies.isDone;
        }

        public void WaitForCompletion(SceneRequest request)
        {
            _dependencies.WaitForCompletion();
            if (!_dependencies.CheckResult(request, out _))
                return;

            var loadSceneMode = request.withAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            SceneManager.LoadScene(Path.GetFileNameWithoutExtension(request.path), loadSceneMode);
            request.SetResult(Request.Result.Success);
        }

        public float progressRate => 0.5f;

        public static ISceneHandler CreateInstance()
        {
            return new RuntimeSceneHandler();
        }
    }

    public interface ISceneHandler
    {
        float progressRate { get; }
        void OnStart(SceneRequest request);
        void Update(SceneRequest request);
        void Release(SceneRequest request);
        AsyncOperation LoadSceneAsync(SceneRequest request);
        bool IsReady(SceneRequest request);
        void WaitForCompletion(SceneRequest request);
    }
}