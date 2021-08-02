using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Versions.Example
{
    public class Async2Sync : MonoBehaviour
    {
        public Image image;
        private readonly List<Asset> _assets = new List<Asset>();

        // Use this for initialization
        private void Start()
        {
            const string assetPath = Res.Texture2D_Logo;
            var assetType = typeof(Sprite);
            _assets.Add(Asset.LoadAsync(assetPath, assetType));
            var asset = Asset.Load(assetPath, assetType);
            image.sprite = asset.Get<Sprite>();
            image.SetNativeSize();
            _assets.Add(asset);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete)) ReleaseAssets();
        }

        private void OnDestroy()
        {
            ReleaseAssets();
        }

        private void ReleaseAssets()
        {
            foreach (var item in _assets) item.Release();

            _assets.Clear();
        }
    }
}