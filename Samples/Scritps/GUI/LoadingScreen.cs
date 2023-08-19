using UnityEngine;

namespace xasset.samples
{
    public class LoadingScreen : MonoBehaviour
    {
        public const string Filename = "LoadingScreen.prefab";

        private static Request request;
        public LoadingBar loadingBar;
        public static LoadingScreen Instance { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void OnDestroy()
        {
            request = null;
        }

        public static Request LoadAsync()
        {
            if (request == null)
                request = Asset.InstantiateAsync(Filename);
            return request;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetProgress(string message, float progress)
        {
            loadingBar.SetProgress(message, progress);
        }
    }
}