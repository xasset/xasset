using System.Collections;
using UnityEngine;

namespace xasset.samples
{
    public class Startup : MonoBehaviour
    {
        public SampleScene startWithScene;
        
        private IEnumerator Start()
        {
            var initializeAsync = Assets.InitializeAsync();
            yield return initializeAsync;
            Scene.LoadAsync(startWithScene.ToString()); 
        }
    }
}