using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    [CustomPropertyDrawer(typeof(ObjectReferenceAttribute))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, Object> assets = new Dictionary<string, Object>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var key = property.stringValue ?? string.Empty;
            if (!assets.TryGetValue(key, out var value))
            {
                value = string.IsNullOrEmpty(key)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<Object>(key);
                assets[key] = value;
            }

            var asset = EditorGUI.ObjectField(position, label, value, typeof(Object), false);
            property.stringValue = AssetDatabase.GetAssetPath(asset);
            EditorGUI.EndProperty();
        }
    }
}