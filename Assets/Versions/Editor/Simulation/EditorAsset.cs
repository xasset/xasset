using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor.Simulation
{
    public class EditorAsset : Asset
    {
        protected override void OnLoad()
        {
            if (mustCompleteOnNextFrame) OnLoaded();
        }

        private void OnLoaded()
        {
            if (string.IsNullOrEmpty(error))
            {
                asset = AssetDatabase.LoadAssetAtPath(pathOrURL, type);
                if (asset == null)
                {
                    Finish("asset not found.");
                    return;
                }
            }

            Finish();
        }

        protected override void OnUnload()
        {
            if (asset == null) return;

            if (!(asset is GameObject)) Resources.UnloadAsset(asset);

            asset = null;
        }

        protected override void OnUpdate()
        {
            if (status != LoadableStatus.Loading) return;

            OnLoaded();
        }

        public override void LoadImmediate()
        {
            OnLoaded();
        }

        internal static EditorAsset Create(string path, Type type)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            return new EditorAsset
            {
                pathOrURL = path,
                type = type
            };
        }
    }
}