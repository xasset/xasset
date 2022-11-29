using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class BuildRecord
    {
        public string name;
        public string platform;
        public string[] changes;
        public ulong size;
        public long timestamp;
    }

    public class BuildRecords : ScriptableObject, ISerializationCallbackReceiver
    {
        public static string Filename = "BuildRecords.json";
        public List<BuildRecord> data = new List<BuildRecord>();

        private Dictionary<string, BuildRecord> _data = new Dictionary<string, BuildRecord>();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _data.Clear();
            foreach (var record in data)
            {
                _data[record.name] = record;
            }
        }

        public void Set(string file, string[] changes, ulong size)
        {
            if (!_data.TryGetValue(file, out var value))
            {
                value = new BuildRecord()
                {
                    name = file,
                    changes = changes,
                    size = size,
                    timestamp = DateTime.Now.ToFileTime(),
                };
                _data[file] = value;
                data.Add(value);
            }
            else
            {
                Logger.W($"Record {file} Exist.");
            }
        }

        public bool TryGetValue(string file, out BuildRecord value)
        {
            return _data.TryGetValue(file, out value);
        }
    }
}