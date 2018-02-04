using System.Collections;
using UnityEngine;
using XAsset;

public class AssetsTest : MonoBehaviour
{ 
    void Start()
    {
        if (!Assets.Initialize())
        {
            Debug.LogError("Assets.Initialize falied.");
        }
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        string assetPath = "Assets/SampleAssets/Logo.prefab";
        Debug.Log("------------------ Assets.Load ------------------");
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForEndOfFrame();
            var asset = Assets.Load<GameObject>(assetPath);
            if (asset.asset != null)
            {
                var go = Instantiate(asset.asset) as GameObject;
                ReleaseAssetOnDestroy.Register(go, asset);
                DestroyImmediate(go);
            } 
        }
        yield return new WaitForEndOfFrame();
        Debug.Log("------------------ Assets.LoadAsync ------------------");
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForEndOfFrame();
            var asset = Assets.LoadAsync<GameObject>(assetPath);
            if (asset != null)
            {
                yield return asset;
                var prefab = asset.asset;
                if (prefab != null)
                {
                    var go = Instantiate(prefab) as GameObject;
                    ReleaseAssetOnDestroy.Register(go, asset);
                    DestroyImmediate(go);
                    go = null; 
                } 
                yield return new WaitForEndOfFrame();
            }
        } 
    }
}
