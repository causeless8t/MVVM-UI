using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Causeless3t.UI.MVVM.Editor
{
    [CustomEditor(typeof(DataBinder))]
    [CanEditMultipleObjects]
    public sealed class DataBinderEditor : UnityEditor.Editor
    {
        private BinderInfo _targetInfo;
        private BinderInfo _sourceInfo;
        private DataBinder.eObserveCycle _observeCycle;

        private static Type[] _currentTypes;

        private readonly List<Type> _targetOwnerList = new();
        private readonly List<string> _targetOwnerInspectorList = new();
        private int _targetOwnerIdx = -1;
        private readonly List<PropertyInfo> _targetPropList = new();
        private readonly List<string> _targetPropInspectorList = new();
        private int _targetPropertyIdx = -1;
        private int _targetUpdateCycleIdx = 0;

        private readonly List<Type> _sourceOwnerList = new();
        private readonly List<string> _sourceOwnerInspectorList = new();
        private int _sourceOwnerIdx = -1;
        private readonly List<PropertyInfo> _sourcePropList = new();
        private readonly List<string> _sourcePropInspectorList = new();
        private int _sourcePropertyIdx = -1;

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            
            _targetInfo = serializedObject.FindProperty("_targetInfo").managedReferenceValue as BinderInfo;
            _sourceInfo = serializedObject.FindProperty("_sourceInfo").managedReferenceValue as BinderInfo;
            _observeCycle = (DataBinder.eObserveCycle)serializedObject.FindProperty("_observeCycle").boxedValue;

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

            var dataBinder = target as DataBinder;
            UpdateOwnerLists(dataBinder);
            _targetPropList.Clear();
            _targetPropInspectorList.Clear();
            _sourcePropList.Clear();
            _sourcePropInspectorList.Clear();
            if (_targetOwnerIdx > -1)
                UpdatePropertyList(dataBinder!.GetComponent(_targetOwnerList[_targetOwnerIdx]), true);
            if (_sourceOwnerList.Count > 0 && _sourceOwnerIdx > -1)
                UpdatePropertyList(Activator.CreateInstance(_sourceOwnerList[_sourceOwnerIdx]), false);
            
            if (_targetInfo == null)
            {
                _targetInfo = new BinderInfo();
                ApplyBinderOwnerInfo(true);
                ApplyBinderPropertyInfo(true);
            }
            if (_sourceInfo == null)
            {
                _sourceInfo = new BinderInfo();
                ApplyBinderOwnerInfo(false);
                ApplyBinderPropertyInfo(false);
            }
        }

        private void UpdateOwnerLists(DataBinder dataBinder)
        {
            _targetOwnerList.Clear();
            _targetOwnerInspectorList.Clear();
            _sourceOwnerList.Clear();
            _sourceOwnerInspectorList.Clear();
            
            var result = new List<Component>();
            dataBinder!.GetComponents(result);
            foreach (var component in result)
            {
                var type = component.GetType();
                _targetOwnerList.Add(type);
                _targetOwnerInspectorList.Add(type.ToString());
            }
            _targetOwnerIdx = _targetInfo == null ? -1 : _targetOwnerList.FindIndex((t) => t == _targetInfo.Owner);

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

            EditorGUILayout.LabelField("Target");
            EditorGUILayout.Separator();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Owner");
            _targetOwnerIdx = EditorGUILayout.Popup(_targetOwnerIdx, _targetOwnerInspectorList.ToArray());
            if (_targetOwnerIdx > -1 && EditorGUI.EndChangeCheck())
            {
                var dataBinder = target as DataBinder;
                var owner = dataBinder!.GetComponent(_targetOwnerList[_targetOwnerIdx]);
                UpdatePropertyList(owner, true);
                ApplyBinderOwnerInfo(true);
                ApplyBinderPropertyInfo(true);
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Property");
            _targetPropertyIdx = EditorGUILayout.Popup(_targetPropertyIdx, _targetPropInspectorList.ToArray());
            if (_targetPropertyIdx > -1 && EditorGUI.EndChangeCheck())
                ApplyBinderPropertyInfo(true);
            EditorGUILayout.Separator();
            if (_targetInfo is {Range: BinderInfo.eBindRange.GetNSet})
            {
                EditorGUI.BeginChangeCheck();
                _observeCycle = (DataBinder.eObserveCycle)EditorGUILayout.EnumPopup("Update Cycle", _observeCycle);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.FindProperty("_observeCycle").boxedValue = _observeCycle;
            }
            else
            {
                _observeCycle = DataBinder.eObserveCycle.None;
                serializedObject.FindProperty("_observeCycle").boxedValue = _observeCycle;
            }
            
            EditorGUILayout.LabelField("Source");
            EditorGUILayout.Separator();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Owner");
            _sourceOwnerIdx = EditorGUILayout.Popup(_sourceOwnerIdx, _sourceOwnerInspectorList.ToArray());
            if (_sourceOwnerIdx > -1 && EditorGUI.EndChangeCheck())
            {
                UpdatePropertyList(Activator.CreateInstance(_sourceOwnerList[_sourceOwnerIdx]), false);
                ApplyBinderOwnerInfo(false);
                ApplyBinderPropertyInfo(false);
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Property");
            _sourcePropertyIdx = EditorGUILayout.Popup(_sourcePropertyIdx, _sourcePropInspectorList.ToArray());
            if (_sourcePropertyIdx > -1 && EditorGUI.EndChangeCheck())
                ApplyBinderPropertyInfo(false);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdatePropertyList<T>(T owner, bool isTarget)
        {
            if (isTarget)
            {
                _targetPropList.Clear();
                _targetPropInspectorList.Clear();
            }
            else
            {
                _sourcePropList.Clear();
                _sourcePropInspectorList.Clear();
            }

            if (owner == null) return;
            var properties = owner.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var resultPropList = isTarget ? _targetPropList : _sourcePropList;
            var resultPropInspectorList = isTarget ? _targetPropInspectorList : _sourcePropInspectorList;
            foreach (var property in properties)
            {
                if (property.DeclaringType == typeof(MonoBehaviour) ||
                    property.DeclaringType == typeof(Component)) continue;
                if (property.GetCustomAttribute(typeof(ObsoleteAttribute)) != null) continue;
                if (property.CanRead && property.CanWrite)
                    resultPropInspectorList.Add($"{property.PropertyType} {property.Name}");
                else if (property.CanRead)
                    resultPropInspectorList.Add($"{property.PropertyType} {property.Name} (R)");
                else continue;
                resultPropList.Add(property);
            }

            BinderInfo binderInfo = isTarget ? _targetInfo : _sourceInfo;
            List<PropertyInfo> propList = isTarget ? _targetPropList : _sourcePropList;
            if (isTarget)
                _targetPropertyIdx = binderInfo == null ? -1 : propList.FindIndex((p) => p == binderInfo!.PInfo);
            else
                _sourcePropertyIdx = binderInfo == null ? -1 : propList.FindIndex((p) => p == binderInfo!.PInfo);
        }

        private void ApplyBinderOwnerInfo(bool isTarget)
        {
            if (isTarget && _targetOwnerIdx < 0) return;
            if (!isTarget && _sourceOwnerIdx < 0) return;
            var binderInfo = isTarget ? _targetInfo : _sourceInfo;
            binderInfo.Owner = isTarget ? _targetOwnerList[_targetOwnerIdx] : _sourceOwnerList[_sourceOwnerIdx];
            if (isTarget)
                serializedObject.FindProperty("_targetInfo").managedReferenceValue = binderInfo;
            else
                serializedObject.FindProperty("_sourceInfo").managedReferenceValue = binderInfo;
        }
        
        private void ApplyBinderPropertyInfo(bool isTarget)
        {
            if (isTarget && (_targetPropList.Count == 0 || _targetPropertyIdx < 0)) return;
            if (!isTarget && (_sourcePropList.Count == 0 || _sourcePropertyIdx < 0)) return;
            var propInfo = isTarget ? _targetPropList[_targetPropertyIdx] : _sourcePropList[_sourcePropertyIdx];
            BinderInfo.eBindRange bindType = propInfo.CanRead switch
            {
                true when propInfo.CanWrite => BinderInfo.eBindRange.GetNSet,
                true => BinderInfo.eBindRange.Get,
                _ => BinderInfo.eBindRange.Set
            };
            var binderInfo = isTarget ? _targetInfo : _sourceInfo;
            binderInfo.SetPropertyInfo(bindType, propInfo);
            if (isTarget)
                serializedObject.FindProperty("_targetInfo").managedReferenceValue = binderInfo;
            else
                serializedObject.FindProperty("_sourceInfo").managedReferenceValue = binderInfo;
        }
    }
}
