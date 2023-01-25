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


    [CreateAssetMenu(menuName = "xasset/" + nameof(BuildGroup), fileName = nameof(BuildGroup))]
    public class BuildGroup : ScriptableObject
    {
        public BundleMode bundleMode = BundleMode.PackByFile;
        public AddressMode addressMode = AddressMode.LoadByPath;
        public Object[] entries;
        public string filter;
        public string build { get; set; }
    }
}