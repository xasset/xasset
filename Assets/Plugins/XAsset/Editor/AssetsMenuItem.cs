//
// AssetsMenuItem.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Plugins.XAsset.Editor
{
    public static class AssetsMenuItem
    {
        private const string KMarkAssetsWithDir = "Assets/AssetBundles/按目录标记";
        private const string KMarkAssetsWithFile = "Assets/AssetBundles/按文件标记";
        private const string KMarkAssetsWithName = "Assets/AssetBundles/按名称标记"; 
        private const string KBuildManifest = "Assets/AssetBundles/生成配置";
        private const string KBuildAssetBundles = "Assets/AssetBundles/生成資源包";
        private const string KBuildPlayer = "Assets/AssetBundles/生成播放器";
        private const string KCopyPath = "Assets/复制路径";
        private const string KMarkAssets = "标记资源";

        public static string assetRootPath;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var settings = BuildScript.GetSettings();
            if (settings.localServer)
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (!isRunning)
                {
                    LaunchLocalServer.Run();
                }
            }
            else
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (isRunning)
                {
                    LaunchLocalServer.KillRunningAssetBundleServer();
                }
            }
			Utility.dataPath = System.Environment.CurrentDirectory;
            Utility.downloadURL = BuildScript.GetManifest().downloadURL;
            Utility.assetBundleMode = settings.runtimeMode;
            Utility.getPlatformDelegate = BuildScript.GetPlatformName;
            Utility.loadDelegate = AssetDatabase.LoadAssetAtPath;
            assetRootPath = settings.assetRootPath;
        }

        public static string TrimedAssetBundleName(string assetBundleName)
        {
            return assetBundleName.Replace(assetRootPath, "");
        }

        [MenuItem(KMarkAssetsWithDir)]
        private static void MarkAssetsWithDir()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break; 
                var assetBundleName = Path.GetDirectoryName(path) + "_g";
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem(KMarkAssetsWithFile)]
        private static void MarkAssetsWithFile()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break;  
                
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (dir == null)
                    continue;
                if (name == null)
                    continue;

                var assetBundleName = TrimedAssetBundleName(Path.Combine(dir, name));
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null); 
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem(KMarkAssetsWithName)]
        private static void MarkAssetsWithName()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                if (Directory.Exists(path))
                    continue;
                if (EditorUtility.DisplayCancelableProgressBar(KMarkAssets, path, i * 1f / assets.Length))
                    break; 
                var assetBundleName = Path.GetFileNameWithoutExtension(path);
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null);  
            } 

            EditorUtility.ClearProgressBar();
        } 

        [MenuItem(KBuildManifest)]
        private static void BuildManifest()
        {
            BuildScript.BuildManifest();
        }

        [MenuItem(KBuildAssetBundles)]
        private static void BuildAssetBundles()
        {
            BuildScript.BuildAssetBundles();
        }

        [MenuItem(KBuildPlayer)]
        private static void BuildStandalonePlayer()
        {
            BuildScript.BuildStandalonePlayer();
        }

        [MenuItem(KCopyPath)]
        private static void CopyPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log(assetPath);
        }
    }
}