using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.samples
{
    public class LoadAsset : MonoBehaviour
    {
        public Image image;

        private readonly List<LoadRequest> _requests = new List<LoadRequest>();

        public void Unload()
        {
            foreach (var request in _requests) request.Release();
            _requests.Clear();
        }

        public void Load()
        {
            // 模拟单帧频繁加载卸载
            var asset = Asset.Load("PPJT.png", typeof(Sprite));
            asset.Release();
            asset.Retain();
            asset.Release();
            asset.Retain();
            SetSprite(asset);
            _requests.Add(asset);
        }

        private IEnumerator _Loading()
        {
            const string path = "biubiux2.png";
            var asset = Asset.LoadAsync(path, typeof(Sprite));
            yield return asset;
            asset.Release();
            asset.Retain();
            _requests.Add(asset);
            SetSprite(asset);
        }

        public void LoadAsync()
        {
            StartCoroutine(_Loading());
        }

        private void SetSprite(AssetRequest asset)
        {
            if (asset.asset != null) image.sprite = asset.asset as Sprite;
            if (asset.assets != null) image.sprite = asset.assets[0] as Sprite;

            image.SetNativeSize();
        }

        public void LoadAllAsync()
        {
            var request = Asset.LoadAllAsync("igg.png", typeof(Sprite));
            request.WaitForCompletion();
            _requests.Add(request);
            SetSprite(request);
        }

        public void InstantiateAsync()
        {
            _requests.Add(Asset.InstantiateAsync("CycleReferences2.prefab"));
            _requests.Add(Asset.InstantiateAsync("CycleReferences1.prefab"));
        }
    }
}