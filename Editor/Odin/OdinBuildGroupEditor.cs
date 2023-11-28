using System;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace xasset.editor.Odin
{
    public class OdinBuildGroupEditor
    {
        public OdinBuildGroup odinBuildGroup;

        private Dictionary<BuildEntry, OdinBuildFolderEditor> subFolderEditorDic;

        public OdinBuildGroupEditor(OdinBuildGroup group)
        {
            subFolderEditorDic = new Dictionary<BuildEntry, OdinBuildFolderEditor>();
            this.odinBuildGroup = group;
            deliveryMode = group.buildGroup.deliveryMode;
            desc = group.buildGroup.desc;
            Refresh();
        }

        public void AddOdinBuildFolderEditor(BuildEntry buildEntry, OdinBuildFolderEditor folderEditor)
        {
            subFolderEditorDic.Add(buildEntry, folderEditor);
        }

        public void Refresh()
        {
            List<OdinBuildGroupEntry> list = new List<OdinBuildGroupEntry>();
            BuildEntry[] entries = odinBuildGroup.GetEntries();
            for (int i = 0; i < entries.Length; i++)
            {
                BuildEntry entry = entries[i];
                OdinBuildGroupEntry odinEntry = new OdinBuildGroupEntry(entry);
                odinEntry.onDeleteBuildEntry = DeleteBuildEntry;
                list.Add(odinEntry);
            }

            groupEntries = list;
        }

        private void RefreshSubFolder()
        {
            foreach (var item in subFolderEditorDic)
            {
                item.Value.buildFolder.CollectEntries();
                item.Value.Refresh();
            }
        }

        [HorizontalGroup("Groups/Settings", Width = 250), HideLabel]
        [VerticalGroup("Groups/Settings/Left"), ShowInInspector, EnumPaging]
        private DeliveryMode deliveryMode;

        [HorizontalGroup("Groups/Settings", Width = 250)]
        [TextArea(2, 2)]
        [ShowInInspector]
        [VerticalGroup("Groups/Settings/Left"), HideLabel]
        private string desc;

        [HorizontalGroup("Groups/Settings", 400)]
        [ShowInInspector]
        [HideLabel]
        [TextArea(4, 4)]
        [BoxGroup("Groups/Settings/Middle", ShowLabel = false)]
        [HorizontalGroup("Groups/Settings/Middle/Horizontal")]
        private string search;

        [HorizontalGroup("Groups/Settings", 400)]
        [BoxGroup("Groups/Settings/Middle", ShowLabel = false)]
        [HorizontalGroup("Groups/Settings/Middle/Horizontal")]
        [ShowInInspector, Button("搜索", 70)]
        private void SearchAssets()
        {
            Debug.Log("Search Assets");
        }

        [HorizontalGroup("Groups/Settings"), ShowInInspector]
        [BoxGroup("Groups/Settings/Right", ShowLabel = false)]
        [VerticalGroup("Groups/Settings/Right/Button"), Button("保存", 35)]
        private void SaveModified()
        {
            OdinExtension.SaveChanges();
        }

        [HorizontalGroup("Groups/Settings"), ShowInInspector]
        [BoxGroup("Groups/Settings/Right", ShowLabel = false)]
        [VerticalGroup("Groups/Settings/Right/Button"), Button("重置", 35)]
        private void ClearModified()
        {
            EditorWindow.GetWindow<OdinBuildWindow>().Close();
            OdinBuildWindow.OpenReginaWindow();
        }

        [VerticalGroup("Groups"), HideLabel, ShowInInspector]
        [TableList(IsReadOnly = true, ShowIndexLabels = true, AlwaysExpanded = true, ShowPaging = true,
            NumberOfItemsPerPage = 30)]
        public List<OdinBuildGroupEntry> groupEntries;

        [HorizontalGroup("Groups/NewEntry"), HideLabel, ShowInInspector]
        [InfoBox("请添加")]
        public UnityEngine.Object newEntry
        {
            get { return null; }
            set
            {
                string assetPath = AssetDatabase.GetAssetPath((UnityEngine.Object) value);
                BuildEntry newBuildEntry = OdinExtension.CreateBuildEntry(assetPath);
                odinBuildGroup.AddBuildEntry(newBuildEntry);
                Refresh();
                RefreshSubFolder();
            }
        }

        private void DeleteBuildEntry(BuildEntry target)
        {
            odinBuildGroup.DeleteBuildEntry(target);
            Refresh();
        }

        public class OdinBuildGroupEntry
        {
            private BuildEntry entry;

            public OdinBuildGroupEntry(BuildEntry entry)
            {
                this.entry = entry;
                asset = entry.asset;
                bundleMode = entry.bundleMode;
                addressMode = entry.addressMode;
                tag = (TagEnum) Enum.ToObject(typeof(TagEnum), entry.tag);
            }

            [HorizontalGroup("Assets/Item", Width = 150), VerticalGroup("Assets")]
            [ReadOnly, HideLabel, ObjectReference, ShowInInspector]
            private string asset;

            [HorizontalGroup("Assets/Item", Width = 200)]
            [HideLabel, ShowInInspector]
            public BundleMode bundleMode
            {
                get { return entry.bundleMode; }
                set { entry.bundleMode = value; }
            }

            [HorizontalGroup("Assets/Item", Width = 210)]
            [HideLabel, ShowInInspector]
            public AddressMode addressMode
            {
                get { return entry.addressMode; }
                set { entry.addressMode = value; }
            }

            [HorizontalGroup("Assets/Item", Width = 100)]
            [HideLabel, ShowInInspector]
            public TagEnum tag
            {
                get { return (TagEnum) Enum.ToObject(typeof(TagEnum), entry.tag); }
                set { entry.tag = (ulong) value; }
            }

            [HorizontalGroup("Assets/Item", MinWidth = 400), HideLabel, ReadOnly, ShowInInspector]
            public string path => entry.asset;

            [HideInInspector] public Action<BuildEntry> onDeleteBuildEntry;

            [HorizontalGroup("Assets/Item", Width = 25), HideLabel, Button(SdfIconType.X), ShowInInspector]
            private void DeleteBuildEntry()
            {
                if (onDeleteBuildEntry != null) onDeleteBuildEntry(entry);
            }
        }
    }
}