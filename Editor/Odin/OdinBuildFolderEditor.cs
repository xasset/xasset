#if UNITY_EDITOR
namespace xasset.editor.Odin
{
    using System;
    using UnityEngine;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;

    public class OdinBuildFolderEditor
    {
        public OdinBuildFolder buildFolder;

        public OdinBuildFolderEditor(OdinBuildFolder folder)
        {
            buildFolder = folder;
            Refresh();
        }

        public void Refresh()
        {
            List<OdinBuildFolderEntry> list = new List<OdinBuildFolderEntry>();
            BuildEntry[] entries = buildFolder.GetEntries();
            for (int i = 0; i < entries.Length; i++)
            {
                BuildEntry entry = entries[i];
                OdinBuildFolderEntry folderEntry = new OdinBuildFolderEntry(entry);
                folderEntry.onAddToBuildGroup = ReCollectEntries;
                list.Add(folderEntry);
            }

            assets = list;
        }

        [VerticalGroup("$groupName"), HideLabel, ShowInInspector]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
            NumberOfItemsPerPage = 35)]
        private List<OdinBuildFolderEntry> assets;

        private void ReCollectEntries(BuildEntry target)
        {
            buildFolder.AddBuildEntryToGroup(target);
            Refresh();
        }

        public class OdinBuildFolderEntry
        {
            private BuildEntry entry;

            public BuildEntry Entry => entry;

            public OdinBuildFolderEntry(BuildEntry entry)
            {
                this.entry = entry;
                asset = entry.asset;
            }

            [HideInInspector] public Action<BuildEntry> onAddToBuildGroup;

            [HorizontalGroup("Assets/Item", Width = 150), VerticalGroup("Assets")]
            [ReadOnly, HideLabel, ObjectReference, ShowInInspector]
            private string asset;

            [HorizontalGroup("Assets/Item", Width = 200)]
            [ReadOnly, HideLabel, ShowInInspector]
            public BundleMode bundleMode => entry.bundleMode;

            [HorizontalGroup("Assets/Item", Width = 210)]
            [ReadOnly, HideLabel, ShowInInspector]
            public AddressMode addressMode => entry.addressMode;

            [HorizontalGroup("Assets/Item", Width = 100)]
            [ReadOnly, HideLabel, ShowInInspector]
            public TagEnum tag => (TagEnum) Enum.ToObject(typeof(TagEnum), entry.tag);

            [HorizontalGroup("Assets/Item", MinWidth = 400)]
            [ReadOnly, HideLabel, ShowInInspector]
            public string path => entry.asset;

            [HorizontalGroup("Assets/Item", MinWidth = 50), Button, ShowInInspector]
            private void AddToBuildGroup()
            {
                if (onAddToBuildGroup != null) onAddToBuildGroup(entry);
            }
        }
    }
}
#endif