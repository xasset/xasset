using System;
using System.Collections;
using UnityEngine;
using libx;

public class LoadAsset : MonoBehaviour
{
    public string assetPath;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        /// 初始化
        var init =  Assets.Initialize();
        yield return init;
        if (! string.IsNullOrEmpty(init.error))
        {
            Debug.LogError(init.error);
            yield break;
        } 

        if (assetPath.EndsWith(".prefab", StringComparison.CurrentCulture))
        {
            Assets.LoadAssetAsync(assetPath, typeof(UnityEngine.Object)).completed += delegate(AssetRequest request)
            {
                if (! string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError(request.error);
                    return;
                } 
                var go = Instantiate(request.asset);
                go.name = request.asset.name;
                /// 设置关注对象，当关注对象销毁时，回收资源
                request.Require(go);
                Destroy(go, 3);
                /// 设置关注对象后，只需要释放一次 
                /// 这里如果之前没有调用 Require，下一帧这个资源就会被回收
                request.Release();
            }; 
        }
        else if (assetPath.EndsWith(".unity", StringComparison.CurrentCulture))
        {
            yield return LoadSceneAsync();
        }
    } 

    IEnumerator LoadSceneAsync()
    {
        var sceneAsset = Assets.LoadSceneAsync(assetPath, true);
        while (!sceneAsset.isDone)
        {
            Debug.Log(sceneAsset.progress);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        sceneAsset.Release();
    }
}
