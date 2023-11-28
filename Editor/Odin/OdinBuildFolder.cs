using System.Collections.Generic;
using com.regina.fUnityTools.Editor;

namespace xasset.editor.Odin
{
    public class OdinBuildFolder
    {
        public BuildEntry buildEntry;

        public OdinBuildGroup odinBuildGroup;

        public OdinBuildFolder(BuildEntry entry, OdinBuildGroup group)
        {
            list = new List<BuildEntry>();
            this.buildEntry = entry;
            this.odinBuildGroup = group;
            CollectEntries();
        }

        public string GetMenuName()
        {
            return odinBuildGroup.GetMenuName(buildEntry);
        }

        private List<BuildEntry> list;

        public void CollectEntries()
        {
            list.Clear();
            EditorAsset[] subAssets = EditorFileUtils.GetTopUnityAssets(buildEntry.asset);
            for (int i = 0; i < subAssets.Length; i++)
            {
                string subAssetPath = subAssets[i].assetPath;
                BuildEntry subBuildEntry =
                    OdinExtension.GetBuildEntryByAssetPathAndParentEntry(subAssetPath, buildEntry);
                if (IsExistedBuildGroup(subBuildEntry)) continue;
                list.Add(subBuildEntry);
            }
        }

        public BuildEntry[] GetEntries()
        {
            return list.ToArray();
        }

        public void AddBuildEntryToGroup(BuildEntry target)
        {
            odinBuildGroup.AddBuildEntry(target);
        }
        
        private bool IsExistedBuildGroup(BuildEntry target)
        {
            return odinBuildGroup.IsExistedBuildEntry(target);
        }
    }
}