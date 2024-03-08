using UnityEditor;

namespace xasset.editor
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        private const string helpContent = "Please run xasset>Build Player Assets before the editor enter in playmode.";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var settings = target as Settings;
            if (settings == null) return;
            Assets.MaxRetryTimes = settings.player.maxRetryTimes;
            Assets.MaxDownloads = settings.player.maxDownloads;
            Scheduler.MaxRequests = settings.player.maxRequests;
            Scheduler.Autoslicing = settings.player.autoslicing;
            Scheduler.AutoslicingTimestep = settings.player.autoslicingTimestep;
            Recycler.AutoreleaseTimestep = settings.player.autoreleaseTimestep;
            Logger.LogLevel = settings.player.logLevel;
            EditorGUILayout.HelpBox(helpContent, MessageType.Info);
        }
    }
}