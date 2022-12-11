using System.Collections;
using UnityEngine;

namespace xasset.example
{
    [DisallowMultipleComponent]
    public class Startup : MonoBehaviour
    {
        public ExampleScene startWithScene;
        private IEnumerator Start()
        {
            var initializeAsync = Assets.InitializeAsync();
            yield return initializeAsync;
            Scene.LoadAsync(startWithScene.ToString());
        }
    }
}