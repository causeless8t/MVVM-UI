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
        private int _targetOwnerIdx = 0;
        private readonly List<PropertyInfo> _targetPropList = new();
        private readonly List<string> _targetPropInspectorList = new();
        private int _targetPropertyIdx = 0;
        private int _targetUpdateCycleIdx = 0;

        private readonly List<Type> _sourceOwnerList = new();
        private readonly List<string> _sourceOwnerInspectorList = new();
        private int _sourceOwnerIdx = 0;
        private readonly List<PropertyInfo> _sourcePropList = new();
        private readonly List<string> _sourcePropInspectorList = new();
        private int _sourcePropertyIdx = 0;

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
            UpdatePropertyList(dataBinder!.GetComponent(_targetOwnerList[_targetOwnerIdx]), true);
            if (_sourceOwnerList.Count > 0)
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
            _targetOwnerIdx = _targetInfo == null ? 0 : Mathf.Max(_targetOwnerList.FindIndex((t) => t == _targetInfo.Owner), 0);

            foreach (var type in _currentTypes)
            {
                if (!typeof(BaseViewModel).IsAssignableFrom(type)) continue;
                if (type.IsInterface || type.IsAbstract) continue;
                _sourceOwnerList.Add(type);
                _sourceOwnerInspectorList.Add(type.ToString());
            }
            _sourceOwnerIdx = _sourceInfo == null ? 0 : Mathf.Max(_sourceOwnerList.FindIndex((t) => t == _sourceInfo.Owner), 0);
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying) return;

            EditorGUILayout.LabelField("Target");
            EditorGUILayout.Separator();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Owner");
            _targetOwnerIdx = EditorGUILayout.Popup(_targetOwnerIdx, _targetOwnerInspectorList.ToArray());
            if (EditorGUI.EndChangeCheck())
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
            if (EditorGUI.EndChangeCheck())
                ApplyBinderPropertyInfo(true);
            EditorGUILayout.Separator();
            if (_targetInfo.Range is BinderInfo.eBindRange.GetNSet)
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
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePropertyList(Activator.CreateInstance(_sourceOwnerList[_sourceOwnerIdx]), false);
                ApplyBinderOwnerInfo(false);
                ApplyBinderPropertyInfo(false);
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Property");
            _sourcePropertyIdx = EditorGUILayout.Popup(_sourcePropertyIdx, _sourcePropInspectorList.ToArray());
            if (EditorGUI.EndChangeCheck())
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
                _targetPropertyIdx = binderInfo == null ? 0 : Mathf.Max(propList.FindIndex((p) => p == binderInfo!.PInfo), 0);
            else
                _sourcePropertyIdx = binderInfo == null ? 0 : Mathf.Max(propList.FindIndex((p) => p == binderInfo!.PInfo), 0);
        }

        private void ApplyBinderOwnerInfo(bool isTarget)
        {
            var binderInfo = isTarget ? _targetInfo : _sourceInfo;
            binderInfo.Owner = isTarget ? _targetOwnerList[_targetOwnerIdx] : _sourceOwnerList[_sourceOwnerIdx];
            if (isTarget)
                serializedObject.FindProperty("_targetInfo").managedReferenceValue = binderInfo;
            else
                serializedObject.FindProperty("_sourceInfo").managedReferenceValue = binderInfo;
        }
        
        private void ApplyBinderPropertyInfo(bool isTarget)
        {
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
