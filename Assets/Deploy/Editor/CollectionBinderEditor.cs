using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Causeless3t.UI.MVVM.Editor
{
    [CustomEditor(typeof(CollectionBinder))]
    public sealed class CollectionBinderEditor : UnityEditor.Editor
    {
        private BinderInfo _sourceInfo;

        private static Type[] _currentTypes;

        private readonly List<Type> _sourceOwnerList = new();
        private readonly List<string> _sourceOwnerInspectorList = new();
        private int _sourceOwnerIdx = -1;
        private readonly List<PropertyInfo> _sourcePropList = new();
        private readonly List<string> _sourcePropInspectorList = new();
        private int _sourcePropertyIdx = -1;

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            
            _sourceInfo = serializedObject.FindProperty("_sourceInfo").managedReferenceValue as BinderInfo;

            if (_currentTypes == null)
            {
                var assemblyTypeList = new List<Type>();
                var defaultAssembly = Assembly.Load("Assembly-CSharp");
                assemblyTypeList.AddRange(defaultAssembly.GetTypes());
                foreach (var assemblyName in defaultAssembly.GetReferencedAssemblies())
                {
                    if (assemblyName.Name.Contains("netstandard")) continue;
                    assemblyTypeList.AddRange(Assembly.Load(assemblyName).GetTypes());
                }
                _currentTypes = assemblyTypeList.ToArray();
            }

            UpdateOwnerLists();
            _sourcePropList.Clear();
            _sourcePropInspectorList.Clear();
            if (_sourceOwnerList.Count > 0)
                UpdatePropertyList(Activator.CreateInstance(_sourceOwnerList[_sourceOwnerIdx]));
            
            if (_sourceInfo == null)
            {
                _sourceInfo = new BinderInfo();
                ApplyBinderOwnerInfo();
                ApplyBinderPropertyInfo();
            }
        }

        private void UpdateOwnerLists()
        {
            _sourceOwnerList.Clear();
            _sourceOwnerInspectorList.Clear();

            foreach (var type in _currentTypes)
            {
                if (!typeof(BaseViewModel).IsAssignableFrom(type)) continue;
                if (type.IsInterface || type.IsAbstract) continue;
                _sourceOwnerList.Add(type);
                _sourceOwnerInspectorList.Add(type.ToString());
            }
            _sourceOwnerIdx = _sourceInfo == null ? -1 : _sourceOwnerList.FindIndex((t) => t == _sourceInfo.Owner);
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying) return;

            EditorGUILayout.LabelField("Source");
            EditorGUILayout.Separator();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Owner");
            _sourceOwnerIdx = EditorGUILayout.Popup(_sourceOwnerIdx, _sourceOwnerInspectorList.ToArray());
            if (_sourceOwnerIdx > -1 && EditorGUI.EndChangeCheck())
            {
                UpdatePropertyList(Activator.CreateInstance(_sourceOwnerList[_sourceOwnerIdx]));
                ApplyBinderOwnerInfo();
                ApplyBinderPropertyInfo();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Property");
            _sourcePropertyIdx = EditorGUILayout.Popup(_sourcePropertyIdx, _sourcePropInspectorList.ToArray());
            if (_sourcePropertyIdx > -1 && EditorGUI.EndChangeCheck())
                ApplyBinderPropertyInfo();

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdatePropertyList<T>(T owner)
        {
            _sourcePropList.Clear();
            _sourcePropInspectorList.Clear();

            if (owner == null) return;
            var properties = owner.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (!typeof(IBindableCollection).IsAssignableFrom(property.PropertyType)) continue;
                if (property.GetCustomAttribute(typeof(ObsoleteAttribute)) != null) continue;
                if (property.CanRead && property.CanWrite)
                    _sourcePropInspectorList.Add($"{property.PropertyType} {property.Name}");
                else if (property.CanRead)
                    _sourcePropInspectorList.Add($"{property.PropertyType} {property.Name} (R)");
                else continue;
                _sourcePropList.Add(property);
            }

            _sourcePropertyIdx = _sourceInfo == null ? -1 : _sourcePropList.FindIndex((p) => p == _sourceInfo!.PInfo);
        }

        private void ApplyBinderOwnerInfo()
        {
            if (_sourceOwnerIdx < 0) return;
            _sourceInfo.Owner = _sourceOwnerList[_sourceOwnerIdx];
            serializedObject.FindProperty("_sourceInfo").managedReferenceValue = _sourceInfo;
        }
        
        private void ApplyBinderPropertyInfo()
        {
            if (_sourcePropList.Count == 0 || _sourcePropertyIdx < 0) return;
            var propInfo = _sourcePropList[_sourcePropertyIdx];
            BinderInfo.eBindRange bindType = propInfo.CanRead switch
            {
                true when propInfo.CanWrite => BinderInfo.eBindRange.GetNSet,
                true => BinderInfo.eBindRange.Get,
                _ => BinderInfo.eBindRange.Set
            };
            _sourceInfo.SetPropertyInfo(bindType, propInfo);
            serializedObject.FindProperty("_sourceInfo").managedReferenceValue = _sourceInfo;
        }
    }
}
