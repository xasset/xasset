using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XAsset
{
    public sealed class Assets : MonoBehaviour
    {
        private static Assets instance = null;

#if UNITY_EDITOR
        static int activeBundleMode = -1;

        const string kActiveBundleMode = "ActiveBundleMode";

        public static bool ActiveBundleMode
        {
            get
            {
                if (activeBundleMode == -1)
                    activeBundleMode = UnityEditor.EditorPrefs.GetBool(kActiveBundleMode, true) ? 1 : 0;
                return activeBundleMode != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != activeBundleMode)
                {
                    activeBundleMode = newValue;
                    UnityEditor.EditorPrefs.SetBool(kActiveBundleMode, value);
                }
            }
        }

        public static void BuildManifest(string path, List<AssetBundleBuild> builds, bool forceRebuild = false)
        {
            manifest.Build(path, builds, forceRebuild);
        }
#endif

        private static Manifest manifest = new Manifest();
        public static string[] allAssetNames { get { return manifest.allAssets; } }
        public static string[] allBundleNames { get { return manifest.allBundles; } }
        public static string GetBundleName(string assetPath) { return manifest.GetBundleName(assetPath); }
        public static string GetAssetName(string assetPath) { return manifest.GetAssetName(assetPath); }

        public static bool Initialize()
        {
            if (instance == null)
            {
                var go = new GameObject("Assets");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<Assets>();
            }

#if UNITY_EDITOR
            if (ActiveBundleMode)
            {
                return InitializeBundle();
            }
            else
            {
                return true;
            }
#else
			return InitializeBundle();
#endif
        }

        public static Asset Load<T>(string path) where T : Object
        {
            return Load(path, typeof(T));
        }

        public static Asset Load (string path, System.Type type)
        {
            return LoadInternal(path, type, false);
        }

        public static Asset LoadAsync<T>(string path)
        {
            return LoadAsync(path, typeof(T));
        }

        public static Asset LoadAsync (string path, System.Type type)
        {
            return LoadInternal(path, type, true);
        }

        public static void Unload(Asset asset)
        {
            asset.Unload();
        }

        private static bool InitializeBundle()
        {
            string relativePath = Path.Combine(Utility.AssetBundlesOutputPath, Utility.GetPlatformName());
            var url =
#if UNITY_EDITOR
                relativePath + "/";
#else
				Path.Combine(Application.streamingAssetsPath, relativePath) + "/"; 
#endif
            if (Bundles.Initialize(url))
            {
                var bundle = Bundles.Load("manifest");
                if (bundle != null)
                {
                    var asset = bundle.LoadAsset<TextAsset>("Manifest.txt");
                    if (asset != null)
                    {
                        using (var reader = new StringReader(asset.text))
                        {
                            manifest.Load(reader);
                            reader.Close();
                        }
                        bundle.Unload();
                        Resources.UnloadAsset(asset);
                        asset = null;
                    }
                    return true;
                }
                else
                {
                    throw new FileNotFoundException("assets manifest not exist.");
                }
            }
            else
            {
                throw new FileNotFoundException("bundle manifest not exist.");
            }
        }

        private static Asset CreateAssetRuntime(string path, System.Type type, bool asyncMode)
        {
            if (asyncMode)
                return new BundleAssetAsync(path, type);
            else
                return new BundleAsset(path, type);
        }

        private static Asset LoadInternal(string path, System.Type type, bool asyncMode)
        {
            Asset asset = assets.Find(obj => { return obj.assetPath == path; });
            if (asset == null)
            {
#if UNITY_EDITOR
                if (Assets.ActiveBundleMode)
                {
                    asset = CreateAssetRuntime(path, type, asyncMode);
                }
                else
                {
                    asset = new Asset(path, type);
                }
#else
				asset = CreateAssetRuntime (path, type, asyncMode);
#endif
                assets.Add(asset);
            }
            asset.Load();
            return asset;
        }

        private static readonly List<Asset> assets = new List<Asset>();

        private void Update()
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.isDone && asset.references <= 0)
                {
                    asset.Dispose();
                    asset = null;
                    assets.RemoveAt(i);
                    i--;
                }
            }

            Bundles.Update();
        }
    }
}