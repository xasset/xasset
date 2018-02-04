using UnityEngine;

namespace XAsset
{
    public class ReleaseAssetOnDestroy : MonoBehaviour
    { 
        public Asset asset;

        public static ReleaseAssetOnDestroy Register(GameObject go, Asset asset)
        {
            ReleaseAssetOnDestroy component = go.GetComponent<ReleaseAssetOnDestroy>();
            if (component == null)
            {
                component = go.AddComponent<ReleaseAssetOnDestroy>();
            }
            component.asset = asset; 
            return component;
        }

        private void OnDestroy()
        {
            if (asset != null)
            {
                asset.Release();
                asset = null;
            } 
        }
    }
}
