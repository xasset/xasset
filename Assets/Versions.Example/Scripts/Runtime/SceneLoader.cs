using UnityEngine;

namespace Versions.Example
{
    public class SceneLoader : MonoBehaviour
    {
        public string scene;
        public float delay;

        public bool loadOnAwake = true;
        public bool showProgress;

        private Scene loading;

        private void Start()
        {
            if (loadOnAwake)
            {
                LoadScene();
            }
        }

        public void LoadScene()
        {
            if (delay > 0)
            {
                Invoke("Loading", 3);
                return;
            }

            Loading();
        }

        private void Loading()
        {
            if (loading != null)
            {
                return;
            }

            loading = Scene.LoadAsync(Res.GetScene(scene));
            if (showProgress)
            {
                PreloadManager.Instance.ShowProgress(loading);
            }
        }
    }
}