using UnityEditor;
using UnityEngine;
using Versions.Editor.Builds;

namespace Versions.Example.Editor
{
    [InitializeOnLoad]
    public static class ExampleBuildProcessor
    {
        static ExampleBuildProcessor()
        {
            BuildScript.postprocessBuildBundles += PostprocessBuildBundles;
            BuildScript.preprocessBuildBundles += PreprocessBuildBundles;
        }


        private static void PreprocessBuildBundles(BuildTask task)
        {
            Debug.LogFormat("PreprocessBuildBundles {0}", task.name);
        }

        private static void PostprocessBuildBundles(BuildTask task)
        {
            Debug.LogFormat("PostprocessBuildBundles {0}", task.name);

            var record = task.record;
            if (record == null)
            {
                return;
            }

            foreach (var file in record.files)
            {
                Debug.Log(file);
            }
        }
    }
}