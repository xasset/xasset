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

            OnLoaded(AssetDatabase.LoadAssetAtPath(pathOrURL, type));
        }

        public override void LoadImmediate()
        {
            OnLoaded(AssetDatabase.LoadAssetAtPath(pathOrURL, type));
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