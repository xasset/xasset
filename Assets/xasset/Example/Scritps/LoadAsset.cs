using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace xasset.example
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
            var asset = Asset.Load("Assets/xasset/Example/Textures/PPJT.png", typeof(Sprite));
            SetSprite(asset);
            _requests.Add(asset);
        }

        public void LoadAsync()
        {
            var asset = Asset.LoadAsync("Assets/xasset/Example/Textures/biubiux2.png", typeof(Sprite));
            asset.completed = request => { SetSprite(asset); };
            _requests.Add(asset);
        }

        private void SetSprite(AssetRequest asset)
        {
            if (asset.asset != null) image.sprite = asset.asset as Sprite;

            if (asset.assets != null) image.sprite = asset.assets[0] as Sprite;

            image.SetNativeSize();
        }

        public void LoadAllAsync()
        {
            var request = Asset.LoadAllAsync("Assets/xasset/Example/Textures/igg.png", typeof(Sprite));
            request.WaitForCompletion();
            _requests.Add(request);
            SetSprite(request);
        }

        public void InstantiateAsync()
        {
            _requests.Add(Asset.InstantiateAsync("CycleReferences2"));
            _requests.Add(Asset.InstantiateAsync("CycleReferences1"));
        }
    }
}