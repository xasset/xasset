using UnityEngine;

namespace xasset.samples
{
    public class LoadSceneAsync : MonoBehaviour
    {
        public SamplesScene scene = SamplesScene.Menu;
        public bool loadWithAdditive;
        public bool loadOnAwake;
        public bool allowSceneActivation = true;
        public bool waitForCompletion;
        public float delayTime;

        private void Start()
        {
            if (!loadOnAwake) return;
            if (delayTime > 0)
            {
                Invoke(nameof(Load), delayTime);
                return;
            }

            Load();
        }


        public void Load()
        {
            if (!loadWithAdditive)
            {
                Scene.LoadAsync(scene.ToString(), loadWithAdditive);
                return;
            }

            var request = Scene.LoadAsync(scene.ToString(), loadWithAdditive);
            request.allowSceneActivation = allowSceneActivation;
            if (waitForCompletion) request.WaitForCompletion();
        }
    }
}