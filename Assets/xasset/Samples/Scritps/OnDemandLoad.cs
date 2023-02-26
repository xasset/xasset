using UnityEngine;

namespace xasset.samples
{ 
    public class OnDemandLoad : MonoBehaviour
    {
        public SamplesScene scene;  

        public void Run()
        {
            Scene.LoadAsync(scene.ToString());
        } 
    }
}