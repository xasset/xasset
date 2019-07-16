//
// AssetManifestABDataSource.cs
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

using System.Collections;
using System.Collections.Generic;
using AssetBundleBrowser.AssetBundleDataSource;
using UnityEditor;
using UnityEngine;
using System;

namespace Plugins.XAsset.Editor
{
    internal class AssetManifestABDataSource : ABDataSource
    {
        public static List<ABDataSource> CreateDataSources()
        {
            var op = new AssetManifestABDataSource();
            var retList = new List<ABDataSource>();
            retList.Add(op);
            return retList;
        }

        public string Name
        {
            get
            {
                return "AssetManifest";
            }
        }

        public string ProviderName
        {
            get
            {
                return "XAsset";
            }
        }

        public string[] GetAssetPathsFromAssetBundle(string assetBundleName)
        {
            var manifest = BuildScript.GetManifest();
            List<string> assetPaths = new List<string>(); 
            var assetBundleNames = manifest.bundles;
            var dirs = manifest.dirs;

            foreach (var item in manifest.assets)
            {
                if (assetBundleNames[item.bundle] == assetBundleName)
                {
                    var assetPath = dirs[item.dir] + "/" + item.name;
                    assetPaths.Add(assetPath);
                }
            } 
            return assetPaths.ToArray();
        }

        public string GetAssetBundleName(string assetPath)
        {
            var manifest = BuildScript.GetManifest(); 
            var assetBundleNames = manifest.bundles;
            var dirs = manifest.dirs;

            foreach (var item in manifest.assets)
            {
                var path = dirs[item.dir] + "/" + item.name; 
                if (path == assetPath)
                {
                    return assetBundleNames[item.bundle];
                }
            } 
            return string.Empty;
        }

        public string GetImplicitAssetBundleName(string assetPath)
        {
            return GetAssetBundleName(assetPath);
        }

        public string[] GetAllAssetBundleNames()
        {
            return BuildScript.GetManifest().bundles;
        }

        public bool IsReadOnly()
        {
            return false;
        }

        public void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variantName)
        {
            BuildScript.SetAssetBundleNameAndVariant(assetPath, bundleName, variantName);
        }

        public void RemoveUnusedAssetBundleNames()
        {
            BuildScript.RemoveUnusedAssetBundleNames();
        }

        public bool CanSpecifyBuildTarget
        { 
            get { return true; } 
        }

        public bool CanSpecifyBuildOutputDirectory
        { 
            get { return true; } 
        }

        public bool CanSpecifyBuildOptions
        { 
            get { return true; } 
        }

        public bool BuildAssetBundles(ABBuildInfo info)
        {
            if (info == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            var manifest = BuildScript.GetManifest();
            BuildScript.RemoveUnusedAssetBundleNames();

            var assets = manifest.assets; 
            var assetBundleNames = manifest.bundles;
            var dirs = manifest.dirs;

            var map = new Dictionary<string, List<string>>(); 

            foreach (var item in manifest.bundles)
            {
                map[item] = new List<string>();
            } 

            foreach (var item in assets)
            {
                var assetPath = dirs[item.dir] + "/" + item.name;
                map[assetBundleNames[item.bundle]].Add(assetPath);  
            }

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (var item in map)
            {
                builds.Add(new AssetBundleBuild()
                    {
                        assetBundleName = item.Key,
                        assetNames = item.Value.ToArray()
                    });
            }

            var buildManifest = BuildPipeline.BuildAssetBundles(info.outputDirectory, builds.ToArray(), info.options, info.buildTarget);
            if (buildManifest == null)
            {
                Debug.Log("Error in build");
                return false;
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                if (info.onBuild != null)
                {
                    info.onBuild(assetBundleName);
                }
            }
            return true;
        }
    }
}
