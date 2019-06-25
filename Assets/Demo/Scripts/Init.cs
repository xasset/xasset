using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plugins.XAsset;
using System;

public class Init : MonoBehaviour
{
    public string assetPath;

    // Start is called before the first frame update
    void Start()
    {
        Assets.Initialize(delegate {
            var asset = Assets.Load(assetPath, typeof(UnityEngine.Object));
            asset.completed += OnAssetLoaded;
        }, delegate(string error){
            Debug.Log(error);
        }); 
    }

    private void OnAssetLoaded(Asset asset)
    {
        if (asset.name.EndsWith(".prefab", StringComparison.CurrentCulture))
        {
            var go = Instantiate(asset.asset);
            go.name = asset.asset.name;
            asset.Require(go);
            Destroy(go, 3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
