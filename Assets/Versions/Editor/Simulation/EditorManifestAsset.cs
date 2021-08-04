using UnityEditor;

namespace VEngine.Editor.Simulation
{
    public class EditorManifestAsset : ManifestAsset
    {
        private int assetIndex;
        private int groupIndex;

        private string[] groups;

        protected override void OnLoad()
        {
            base.OnLoad();
            groupIndex = 0;
            assetIndex = 0;
            pathOrURL = name;
            groups = AssetDatabase.GetAllAssetBundleNames();
            status = LoadableStatus.Loading;
        }

        public override void Override()
        {
            Versions.Override(asset);
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.Loading)
            {
                while (groupIndex < groups.Length)
                {
                    var group = groups[groupIndex];
                    var assets = AssetDatabase.GetAssetPathsFromAssetBundle(group);
                    while (assetIndex < assets.Length)
                    {
                        asset.AddAsset(assets[assetIndex]);
                        assetIndex++;
                    }

                    assetIndex = 0;
                    groupIndex++;
                }

                Finish();
            }
        }

        public static EditorManifestAsset Create(string name, bool builtin)
        {
            var asset = new EditorManifestAsset
            {
                name = name
            };
            return asset;
        }
    }
}