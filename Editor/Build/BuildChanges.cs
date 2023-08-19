using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class BuildChange
    {
        public string name;
        public string[] files;
        public ulong size;
        public long timestamp;
    }

    public class BuildChanges : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string Filename = "changes.json";
        public List<BuildChange> data = new List<BuildChange>();

        private readonly Dictionary<string, BuildChange> _data = new Dictionary<string, BuildChange>();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _data.Clear();
            foreach (var record in data) _data[record.name] = record;
        }

        public void Set(string file, string[] changes, ulong size)
        {
            if (_data.TryGetValue(file, out var value)) return;
            value = new BuildChange
            {
                name = file,
                files = changes,
                size = size,
                timestamp = DateTime.Now.ToFileTime()
            };
            _data[file] = value;
            data.Add(value);
        }

        public bool TryGetValue(string file, out BuildChange value)
        {
            return _data.TryGetValue(file, out value);
        }
    }
}