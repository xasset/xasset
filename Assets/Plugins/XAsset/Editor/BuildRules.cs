//
// BuildRules.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
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
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace libx
{
    [Serializable]
    public class BuildRule
    {
        [Tooltip("搜索路径")]
        public string searchPath;

        [Tooltip("搜索通配符，多个之间请用,(逗号)隔开")]
        public string searchPattern;

        [Tooltip("使用固定的名称生成AssetBundle？不是请留空，是请配置")]
        public string assetBundleName;

        [Tooltip("是否仅针对目录做处理")]
        public bool searchDirOnly;

        [Tooltip("是否可以被处理为公共资源")]
        public bool unshared;

        [Tooltip("递归处理子目录或者只处理顶层目录")]
        public SearchOption searchOption = SearchOption.AllDirectories;
    }

    public class BuildRules : ScriptableObject
    {
        public string searchPatternText = "*.txt,*.bytes,*.json,*.csv,*.xml,*htm,*.html,*.yaml,*.fnt";
        public string searchPatternPrefab = "*.prefab";
        public string searchPatternMaterial = "*.mat";
        public string searchPatternController = "*.controller";
        public string searchPatternPNG = "*.png";
        public string searchPatternAsset = "*.asset";
        public string searchPatternScene = "*.unity";
        public string searchPatternDir = "*";

        [Tooltip("不被处理为公共资源的目录，也可以在规则配置中定义")]
        public List<string> unshared = new List<string>();

        public BuildRule[] rules = new BuildRule[0] {};

        public Dictionary<string, string> asset2bundles = new Dictionary<string, string>();

        public Dictionary<string, HashSet<string>> tracker = new Dictionary<string, HashSet<string>>();

        public Dictionary<string, string[]> conflicted = new Dictionary<string, string[]>();

        [NonSerialized]
        public List<string> duplicated = new List<string>();

        public bool ValidateAsset(string asset)
        {
            if (!asset.StartsWith("Assets/"))
            {
                return false;
            }
            string ext = Path.GetExtension(asset);
            if (ext == ".dll" || ext == ".cs" || ext == ".meta" || ext == ".js" || ext == ".boo")
                return false;
            return true;
        }

        public static bool IsScene(string asset)
        {
            return asset.EndsWith(".unity");
        }

        public void Track(string asset, string bundle)
        {
            if (!tracker.ContainsKey(asset))
            {
                HashSet<string> hashSet = new HashSet<string>();
                hashSet.Add(bundle);
                tracker.Add(asset, hashSet);
            }
            tracker[asset].Add(bundle);
            var abn = AssetDatabase.GetImplicitAssetBundleName(asset);
            if (tracker[asset].Count > 1 && string.IsNullOrEmpty(abn))
            {
                duplicated.Add(asset);
            }
        }

        public IEnumerable<string> CheckTracker(string asset)
        {
            if (tracker.ContainsKey(asset))
            {
                return tracker[asset];
            }
            return new HashSet<string>();
        }

        public Dictionary<string, string> Apply()
        {
            tracker.Clear();
            duplicated.Clear();
            conflicted.Clear();
            asset2bundles.Clear();

            for (int i = 0, max = rules.Length; i < max; i++)
            {
                BuildRule rule = rules[i];
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("执行规则{0}/{1}", i, max), rule.searchPath, i / (float)max))
                {
                    break;
                }
                if (rule.unshared)
                {
                    if (!unshared.Contains(rule.searchPath))
                    {
                        unshared.Add(rule.searchPath);
                    }
                }
                ApplyRule(rule);
            }

            {
                int i = 0, max = asset2bundles.Count;
                foreach (var item in asset2bundles)
                {
                    string bundle = item.Value;
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("设置包名{0}/{1}", i, max), bundle, i / (float)max))
                    {
                        break;
                    }
                    var ai = AssetImporter.GetAtPath(item.Key);
                    if (ai != null && !bundle.Equals(ai.assetBundleName))
                    {
                        ai.assetBundleName = bundle;
                    }
                    i++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();

            string[] bundles = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0, max = bundles.Length; i < max; i++)
            {
                string bundle = bundles[i];
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("分析依赖{0}/{1}", i, max), bundle, i / (float)max))
                {
                    break;
                }

                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
                if (Array.Exists(assetPaths, IsScene) && !Array.TrueForAll(assetPaths, IsScene))
                {
                    conflicted.Add(bundle, assetPaths);
                }

                string[] dependencies = AssetDatabase.GetDependencies(assetPaths, true);
                foreach (string asset in dependencies)
                {
                    if (ValidateAsset(asset) && !unshared.Exists((s) => asset.Contains(s)))
                    {
                        Track(asset, bundle);
                    }
                }
            }

            {
                int i = 0, max = conflicted.Count;
                foreach (var item in conflicted)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("解决冲突{0}/{1}", i, max), item.Key, i / (float)max))
                    {
                        break;
                    }
                    string[] assets = item.Value;
                    foreach (string asset in assets)
                    {
                        if (!IsScene(asset))
                        {
                            duplicated.Add(asset);
                        }
                    }
                    i++;
                }
            }

            for (int i = 0, max = duplicated.Count; i < max; i++)
            {
                string item = duplicated[i];
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("解决冗余{0}/{1}", i, max), item, i / (float)max))
                {
                    break;
                }
                OptimazieAsset(item);
            }
            duplicated.Clear();
            EditorUtility.ClearProgressBar();
            return asset2bundles;
        }

        public void ClearAssetBundles()
        {
            BuildScript.ClearAssetBundles();
        }

        private void OptimazieAsset(string asset)
        {
            if (asset.EndsWith(".shader"))
            {
                asset2bundles[asset] = "shaders";
            }
            else
            {
                string text = Path.GetDirectoryName(asset) + "/" + Path.GetFileNameWithoutExtension(asset);
                text = text.Replace("\\", "/").ToLower();
                asset2bundles[asset] = text;
            }
            var ai = AssetImporter.GetAtPath(asset);
            if (ai != null && string.IsNullOrEmpty(ai.assetBundleName))
            {
                ai.assetBundleName = asset2bundles[asset];
            }
        }

        private void ApplyRule(BuildRule rule)
        {
            var path = rule.searchPath;
            var patterns = rule.searchPattern.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var searchOption = rule.searchOption;
            var searchDir = rule.searchDirOnly;
            var assetBundleName = rule.assetBundleName;
            int startIndex = path.LastIndexOf('/') + 1;
            var suffix = searchDir ? "_x" : "";

            if (! Directory.Exists(path))
            {
                Debug.LogWarning("Rule searchPath not exsit:" + path);
                return;
            }

            if (searchDir)
            {
                var asset = path.Replace("\\", "/");
                if (string.IsNullOrEmpty(assetBundleName))
                {
                    string assetName = Path.GetFileNameWithoutExtension(asset);
                    string str = asset.Substring(startIndex);
                    asset2bundles[asset] = (str + "/" + assetName).Replace("\\", "/").ToLower() + suffix; // 防止目录名称和 ab 名称冲突
                }
                else
                {
                    asset2bundles[asset] = assetBundleName;
                }
            }

            foreach (var item in patterns)
            {
                string[] files = searchDir ? Directory.GetDirectories(path, item, searchOption) : Directory.GetFiles(path, item, searchOption);
                foreach (string file in files)
                {
                    if (!searchDir && Directory.Exists(file))
                    {
                        continue;
                    }

                    var asset = file.Replace("\\", "/");
                    if (string.IsNullOrEmpty(assetBundleName))
                    {
                        string assetName = Path.GetFileNameWithoutExtension(asset);
                        string directoryName = Path.GetDirectoryName(asset);
                        string str = directoryName.Substring(startIndex);
                        asset2bundles[asset] = (str + "/" + assetName).Replace("\\", "/").ToLower() + suffix; // 防止目录名称和 ab 名称冲突
                    }
                    else
                    {
                        asset2bundles[asset] = assetBundleName;
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(BuildRules))]
    public class BuildRulesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BuildRules rules = target as BuildRules;
            EditorGUILayout.HelpBox("【操作提示】" +
                "\n    - 编辑器菜单中 Assets/Apply Rule 可以针对 Project 视图中选中的文件夹快速创建目标规则：" +
                "\n     - Text：" + rules.searchPatternText +
                "\n     - Prefab" + rules.searchPatternPrefab +
                "\n     - PNG" + rules.searchPatternPNG +
                "\n     - Material" + rules.searchPatternMaterial +
                "\n     - Controller" + rules.searchPatternController +
                "\n     - Asset" + rules.searchPatternAsset +
                "\n     - Scene" + rules.searchPatternScene +
                "\n【注意事项】：" +
                "\n    - 所有shader放到一个包" +
                "\n    - 场景文件不可以和非场景文件放到一个包" +
                "\n    - 预知体通常单个文件一个包" +
                "\n    - 资源冗余可以自行集成 AssetBundle-Browser 查看" +
                "\n作者:fjy" +
                "\n邮箱:xasset@qq.com", MessageType.None);

            using (var h = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                if (GUILayout.Button("清理"))
                {
                    if (EditorUtility.DisplayDialog("提示", "是否确定清理？", "确定"))
                    {
                        rules.rules = new BuildRule[0];
                    }
                }

                if (GUILayout.Button("执行"))
                {
                    rules.Apply();
                }
                GUILayout.Space(16);
            }
            base.OnInspectorGUI();
        }
    }
}