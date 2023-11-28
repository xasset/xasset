using UnityEditor;
using com.regina.fUnityTools.Editor;

namespace xasset.editor.Odin
{
    public class OdinExtension
    {
        public static void SaveBuildConfig(Build build)
        {
            var array = EditorFileUtils.GetAllAssetsByAssetDirectoryPath("Assets/xasset/Config/Builds");
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].name == build.name)
                {
                    array[i] = build;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                }
            }
        }

        public static BuildEntry CreateBuildEntry(string assetPath)
        {
            BuildEntry entry = new BuildEntry();
            entry.asset = assetPath;
            entry.tag = 0;
            entry.filter = "";
            entry.owner = null;
            entry.addressMode = AddressMode.LoadByPath;
            entry.bundleMode = BundleMode.PackByRaw;
            return entry;
        }

        public static BuildEntry GetBuildEntryByAssetPathAndParentEntry(string assetPath, BuildEntry parent)
        {
            BuildEntry entry = new BuildEntry();
            entry.asset = assetPath;
            entry.tag = parent.tag;
            entry.filter = parent.filter;
            entry.owner = parent.owner;
            entry.addressMode = parent.addressMode;
            entry.bundleMode = parent.bundleMode;
            return entry;
        }

        public static void SaveChanges()
        {
            // if (OdinBuildWindow.cacheBuildDic == null) return;
            // foreach (var item in OdinBuildWindow.cacheBuildDic)
            // {
            //     List<OdinBuildGroup> list = item.Value;
            //     for (int i = 0; i < list.Count; i++)
            //         list[i].SaveModifies();
            //     EditorUtility.SetDirty(item.Key);
            // }

            AssetDatabase.Refresh();
        }

        public static string GetBuildEntryName(BuildEntry buildEntry)
        {
            int lastIndex = buildEntry.asset.LastIndexOf('/');
            return buildEntry.asset.Substring(lastIndex);
        }
        
        public static BuildEntry GetBuildEntryByAssetPath(string assetPath, BuildEntry parentEntry)
        {
            BuildEntry newBuildEntry = new BuildEntry();
            newBuildEntry.asset = assetPath;
            newBuildEntry.addressMode = parentEntry.addressMode;
            newBuildEntry.parent = parentEntry.parent;
            newBuildEntry.bundleMode = parentEntry.bundleMode;
            newBuildEntry.owner = parentEntry.owner;
            newBuildEntry.filter = parentEntry.filter;
            newBuildEntry.bundle = parentEntry.bundle;
            return newBuildEntry;
        }
    }
}