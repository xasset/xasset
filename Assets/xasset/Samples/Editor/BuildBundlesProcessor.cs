using System;
using UnityEditor;
using UnityEngine;
using xasset.editor;

namespace xasset.samples.editor
{
    public static class BuildBundlesProcessor
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            Builder.PreprocessBuildBundles = PreprocessBuildBundles;
            Builder.PostprocessBuildBundles = PostprocessBuildBundles;
        }

        private static void PostprocessBuildBundles(BuildTask[] jobs, string[] changes)
        {
            Debug.Log($"PostprocessBuildBundles:\n{string.Join("\n", changes)}.");
        }

        private static void PreprocessBuildBundles(Build[] builds, Settings settings)
        {
            Debug.Log($"PreprocessBuildBundles:\n{string.Join(",", Array.ConvertAll(builds, input => input.name))}.");
        }
    }
}