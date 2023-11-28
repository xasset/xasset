using System.Linq;
using UnityEditor;
using System.Collections.Generic;
using com.regina.fUnityTools.Editor;

namespace xasset.editor.Odin
{
    public class OdinBuildGroup
    {
        public BuildGroup buildGroup;

        private Dictionary<string, string> groupMenuDic;

        private string groupMenu => $"{buildGroup.owner.name}/{buildGroup.name}";

        public OdinBuildGroup(BuildGroup group)
        {
            groupMenuDic = new Dictionary<string, string>();
            this.buildGroup = group;
            _modifies = group.assets.ToList();
            UpdateGroupMenu();
        }

        private void UpdateGroupMenu()
        {
            groupMenuDic.Clear();
            for (int i = 0; i < _modifies.Count; i++)
            {
                string assetPath = _modifies[i].asset;
                int lastIndex = assetPath.LastIndexOf('/');
                string buildEntryMenu = assetPath.Substring(lastIndex + 1);
                groupMenuDic.Add(assetPath, buildEntryMenu);
            }
        }

        public string GetMenuName(BuildEntry folderBuildEntry)
        {
            int lastIndex = folderBuildEntry.asset.LastIndexOf('/');
            string subBuildEntryMenu = folderBuildEntry.asset.Substring(lastIndex + 1); //AssetName
            foreach (var item in groupMenuDic)
            {
                if (folderBuildEntry.asset.Contains(item.Key))
                {
                    subBuildEntryMenu = item.Value + folderBuildEntry.asset.Substring(item.Key.Length);
                    break;
                }
            }

            return $"{groupMenu}/{subBuildEntryMenu}";
        }

        private Dictionary<BuildEntry, OdinBuildFolder> subOdinBuildFolders;

        public void AddSubOinBuildFolder(BuildEntry buildEntry, OdinBuildFolder folder)
        {
            if (subOdinBuildFolders == null) subOdinBuildFolders = new Dictionary<BuildEntry, OdinBuildFolder>();
            if (subOdinBuildFolders.ContainsKey(buildEntry)) return;
            subOdinBuildFolders.Add(buildEntry, folder);
        }

        public List<BuildEntry> Modifies => _modifies;

        private List<BuildEntry> _modifies;

        public BuildEntry[] GetEntries()
        {
            return _modifies.ToArray();
        }

        public void AddBuildEntry(BuildEntry target)
        {
            if (IsExistedBuildEntry(target)) return;
            _modifies.Add(target);
            if (!EditorFileUtils.IsUnityDirectory(target.asset)) return;
            UpdateGroupMenu();
            string oldMenuName = GetMenuName(target);
            OdinBuildFolder odinFolderBuildFolder =
                new OdinBuildFolder(target, this);
            AddSubOinBuildFolder(target, odinFolderBuildFolder);
            OdinBuildFolderEditor menuFolder = new OdinBuildFolderEditor(odinFolderBuildFolder);
            EditorWindow.GetWindow<OdinBuildWindow>().DeleteBuildMenu(oldMenuName);
            string newMenuName = $"{groupMenu}/" + OdinExtension.GetBuildEntryName(target);
            EditorWindow.GetWindow<OdinBuildWindow>().AddMenuName(newMenuName, menuFolder);
            RefreshSubFolders(target);
            EditorWindow.GetWindow<OdinBuildWindow>().RefreshMenu();
        }

        public void DeleteBuildEntry(BuildEntry target)
        {
            if (!IsExistedBuildEntry(target)) return;
            _modifies.Remove(target);
            if (!EditorFileUtils.IsUnityDirectory(target.asset)) return;
            UpdateGroupMenu();
            string menuName = GetMenuName(target);
            EditorWindow.GetWindow<OdinBuildWindow>().DeleteBuildMenu(menuName);
            RefreshSubFolders(target);
            EditorWindow.GetWindow<OdinBuildWindow>().RefreshMenu();
        }

        public bool IsExistedBuildEntry(BuildEntry target)
        {
            for (int j = 0; j < _modifies.Count; j++)
            {
                BuildEntry groupEntry = _modifies[j];
                if (groupEntry.asset == target.asset)
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshSubFolders(BuildEntry target)
        {
            if (subOdinBuildFolders.ContainsKey(target))
            {
                OdinBuildFolder folder = subOdinBuildFolders[target];
                folder.CollectEntries();
            }
            else
            {
                foreach (var item in subOdinBuildFolders)
                {
                    if (target.asset.Contains(item.Key.asset))
                    {
                        item.Value.CollectEntries();
                    }
                }
            }
        }

        public void SaveModifies()
        {
            buildGroup.assets = _modifies.ToArray();
        }
    }
}