using System.Collections;
using UnityEngine;

namespace xasset.samples
{
    [DisallowMultipleComponent]
    public class Startup : MonoBehaviour
    {
        public SamplesScene startWithScene;
        private IEnumerator Start()
        {
            var initializeAsync = Assets.InitializeAsync();
            yield return initializeAsync;
            Scene.LoadAsync(startWithScene.ToString());
        }
    }
}