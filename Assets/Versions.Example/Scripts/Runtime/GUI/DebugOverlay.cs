using UnityEngine;

namespace VEngine.Example
{
    public class DebugOverlay : MonoBehaviour
    {
        public Rect windowRect = new Rect(20, 20, 128, 128);

        private void OnGUI()
        {
            windowRect = GUI.Window(0, windowRect, DoMyWindow, nameof(DebugOverlay));
        }

        private static void DoMyWindow(int id)
        {
            GUILayout.Label($"Busy:{Updater.Instance.busy}");
            GUILayout.Label($"Assets:{Asset.Cache.Count}");
            GUILayout.Label($"Bundles:{Bundle.Cache.Count}");
            GUILayout.Label($"Downloads:{Download.Cache.Count}");
            GUI.DragWindow(new Rect(0, 0, 10000, 30));
        }
    }
}