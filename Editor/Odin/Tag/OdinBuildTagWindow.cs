using System;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

namespace xasset.editor.Odin
{
    public class OdinBuildTagWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Odin Tags Window")]
        private static void OpenWindow()
        {
            OdinBuildTagWindow window = GetWindow<OdinBuildTagWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(200, 400);
            window.titleContent = new GUIContent("ROX Tags");
            string[] temp = Enum.GetNames(typeof(TagEnum));
            if (temp[0] == "none")
            {
                window.tags = new string[temp.Length - 1];
                for (int i = 1; i < temp.Length; i++)
                {
                    window.tags[i - 1] = temp[i];
                }
            }

            for (int i = 0; i < window.tags.Length; i++)
            {
                Debug.Log(window.tags[i]);
            }
        }

        [Button]
        private void UpdateBuildTagConfig()
        {
            OdinBuildTagEditorUtil.WriteOdinBuildTagClassFile(tags);
        }

        [HideLabel] [ShowInInspector] private string[] tags;
    }
}