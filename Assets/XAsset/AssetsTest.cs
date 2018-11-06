using System.Collections;
using UnityEngine;
using XAsset;

public class AssetsTest : MonoBehaviour
{
    [SerializeField] string assetPath = "Assets/SampleAssets/Logo.prefab";

    void Start()
    {
#if UNITY_WEBGL
        Assets.InitializeAsync(() =>
        {
            StartCoroutine(Load());
        });
#else
        if (!Assets.Initialize())
        {
            Debug.LogError("Assets.Initialize falied.");
        }
        StartCoroutine(Load());
#endif
    }

    IEnumerator Load()
    {
        var asset = Assets.LoadAsync<GameObject>(assetPath);
        if (asset != null)
        {
            yield return asset;
            var prefab = asset.asset;
            if (prefab != null)
            {
                var go = Instantiate(prefab) as GameObject;
                ReleaseAssetOnDestroy.Register(go, asset);
                GameObject.Destroy(go, 10);
            }
        }

        yield return new WaitForSeconds(11);

        asset = Assets.Load<GameObject>(assetPath);
        if (asset != null)
        {
            var prefab = asset.asset;
            if (prefab != null)
            {
                var go = Instantiate(prefab) as GameObject;
                ReleaseAssetOnDestroy.Register(go, asset);
                GameObject.Destroy(go, 3);
            }
        }
    }
}
