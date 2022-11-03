using UnityEngine;

namespace xasset.editor
{
    public enum BundleMode
    {
        PackTogether,
        PackByEntry,
        PackByFile,
        PackByFolder,
        PackByTopSubFolder,
        PackByCustom
    }

    [CreateAssetMenu(menuName = "xasset/" + nameof(Group), fileName = nameof(Group))]
    public class Group : ScriptableObject
    {
        public BundleMode bundleMode = BundleMode.PackByFile;
        public Object[] entries;
        public string filter;
        public string build { get; set; }
    }
}