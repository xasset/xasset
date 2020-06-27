using UnityEditor;
using UnityEngine;

namespace libx
{
    public static class EditorRuntimeInitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var settings = BuildScript.GetSettings(); 
            Assets.basePath = System.Environment.CurrentDirectory;
            Assets.runtimeMode = settings.runtimeMode;
            Assets.loadDelegate = AssetDatabase.LoadAssetAtPath;  
            Menu.SetChecked(MenuItems.KRuntimeMode, settings.runtimeMode); 
        }

        [InitializeOnLoadMethod]
        private static void OnEditorInitialize()
        {
            EditorUtility.ClearProgressBar(); 
            BuildScript.GetManifest();
            BuildScript.GetSettings();
            BuildScript.GetBuildRules(); 
        }
    }
}
