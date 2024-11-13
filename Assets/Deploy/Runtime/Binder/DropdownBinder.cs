using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class DropdownBinder : DataBinder<TMP_Dropdown>, IDataBinder<bool>, IDataBinder<int>, IDataBinder<List<TMP_Dropdown.OptionData>>, ICommandBinder<int>, IUIEventBinder
    {
        public enum eDropdownProperty
        {
            Value,
            Enable,
            OptionList,
            OnValueChanged,
            SetWithoutNotify
        }

        [Serializable]
        public struct BindInfoDropdown
        {
            public string Key;
            public eDropdownProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoDropdown> _bindInfos = new();
        private Dictionary<string, eDropdownProperty> _bindInfoDic; 
        private event Action<TMP_Dropdown, int> OnValueChangedAction;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroy()
        {
            Target.onValueChanged.RemoveListener(OnValueChanged);
            base.OnDestroy();
        }

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }
        
        public override string[] GetKeyList()
        {
            List<string> result = new();
            _bindInfos.ForEach((info) =>
            {
                if (info.PropertyType == eDropdownProperty.OnValueChanged)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        private void OnValueChanged(int value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eDropdownProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IDataBinder<bool>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eDropdownProperty.Enable: return Target.interactable;
            }
            return default;
        }
        
        public void SetProperty(string key, int value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eDropdownProperty.Value: Target.value = value; break;
            }
        }

        int IDataBinder<int>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eDropdownProperty.Value: return Target.value;
            }
            return default;
        }
        
        public void SetProperty(string key, List<TMP_Dropdown.OptionData> value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eDropdownProperty.OptionList: Target.options = value; break;
            }
        }

        List<TMP_Dropdown.OptionData> IDataBinder<List<TMP_Dropdown.OptionData>>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            switch (type)
            {
                case eDropdownProperty.OptionList: return Target.options;
            }
            return default;
        }

        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        public void InvokeMethod(string key, int value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Dropdown>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eDropdownProperty.SetWithoutNotify: Target.SetValueWithoutNotify(value); break;
            }
        }

        public void AddListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eDropdownProperty.OnValueChanged: OnValueChangedAction += action as Action<TMP_Dropdown, int>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eDropdownProperty.OnValueChanged: OnValueChangedAction -= action as Action<TMP_Dropdown, int>; break;
            }
        }
    }
}

