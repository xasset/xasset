//
// BuildProcessor.cs
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

using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Plugins.XAsset.Editor
{
    public class BuildProcessor : IPreprocessBuild, IPostprocessBuild
    {
        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS || Environment.OSVersion.Platform != PlatformID.MacOSX) return;
            var searchPath = Path.Combine(Application.dataPath, "Plugins/XAsset/Editor/Shells");
            var shells = Directory.GetFiles(searchPath, "*.sh", SearchOption.AllDirectories);
            foreach (var item in shells)
            {
                if (item == null) continue;
                var newPath = Path.Combine(path, Path.GetFileName(item));
                File.Copy(item, newPath, true);
            }

            var bundleIdentifiers = PlayerSettings.applicationIdentifier.Split('.');
            var appName = bundleIdentifiers[bundleIdentifiers.Length - 1];
            var ipaType = EditorUserBuildSettings.development ? "develop" : "release";
            var ipaName = string.Format("{0}-{1}-{2}",
                appName,
                PlayerSettings.bundleVersion,
                ipaType);

            var configType = EditorUserBuildSettings.iOSBuildConfigType.ToString();
            var openTerminalBash = Path.Combine(path, "OpenTerminal.sh");
            var args = openTerminalBash + " " + path + " " + ipaName + " " + ipaType + " " + appName + " " + configType;
            Process.Start("/bin/bash", args);
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            BuildScript.CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundles));
            var platformName = BuildScript.GetPlatformName();
            var searchPath = Path.Combine(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundles),
                platformName);
            if (!Directory.Exists(searchPath)) return;
            var files = Directory.GetFiles(searchPath, "*.manifest", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                if (info.Exists) info.Delete();
            }

            files = Directory.GetFiles(searchPath, "*.meta", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                var info = new FileInfo(item);
                info.Delete();
            }
        }

        public int callbackOrder
        {
            get { return 0; }
        }
    }
}