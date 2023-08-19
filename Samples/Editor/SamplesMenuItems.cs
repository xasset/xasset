using UnityEditor;
using UnityEditor.SceneManagement;

namespace xasset.samples.editor
{
    public static class SamplesMenuItems
    {
        [MenuItem("xasset/Open/Startup Scene", false, 100)]
        public static void OpenStartupScene()
        {
            EditorSceneManager.OpenScene("Assets/xasset/Samples/Startup.unity");
        }
    }
}