using UnityEngine.SceneManagement;

namespace VEngine
{
    public class BundledScene : Scene
    {
        protected Dependencies dependencies;

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.DependentLoading)
                UpdateDependencies();
            else if (status == LoadableStatus.Loading) UpdateLoading();
        }

        private void UpdateDependencies()
        {
            if (dependencies == null)
            {
                Finish("dependencies == null");
                return;
            }

            progress = dependencies.progress * 0.5f;
            if (!dependencies.isDone) return;

            var assetBundle = dependencies.assetBundle;
            if (assetBundle == null)
            {
                Finish("assetBundle == null");
                return;
            }

            operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            status = LoadableStatus.Loading;
        }

        protected override void OnUnload()
        {
            if (dependencies != null)
            {
                dependencies.Release();
                dependencies = null;
            }

            base.OnUnload();
        }


        protected override void OnLoad()
        {
            PrepareToLoad();
            dependencies = Dependencies.Load(pathOrURL, mustCompleteOnNextFrame);
            if (mustCompleteOnNextFrame)
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
                Finish();
            }
            else
            {
                status = LoadableStatus.DependentLoading;
            }
        }

        internal static Scene Create(string assetPath, bool additive = false)
        {
            if (!Versions.Contains(assetPath))
                return new Scene
                {
                    pathOrURL = assetPath,
                    loadSceneMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single
                };

            return new BundledScene
            {
                pathOrURL = assetPath,
                loadSceneMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single
            };
        }
    }
}
