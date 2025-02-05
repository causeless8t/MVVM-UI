using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleBinder : DataBinder<Toggle>, IDataBinder<bool>, ICommandBinder<bool>, IUIEventBinder
    {
        public enum eToggleProperty
        {
            IsOn,
            Enable,
            OnValueChanged,
            SetWithoutNotify
        }

        [Serializable]
        public struct BindInfoSlider
        {
            public string Key;
            public eToggleProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoSlider> _bindInfos = new();
        private Dictionary<string, eToggleProperty> _bindInfoDic; 
        private event Action<Toggle, bool> OnValueChangedAction;

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
                if (info.PropertyType == eToggleProperty.OnValueChanged)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        private void OnValueChanged(bool value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eToggleProperty.Enable: Target.interactable = value; break;
                case eToggleProperty.IsOn: Target.isOn = value; break;
            }
        }

        bool IDataBinder<bool>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eToggleProperty.Enable: return Target.interactable;
                case eToggleProperty.IsOn: return Target.isOn;
            }
            return default;
        }

        public override bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        public void InvokeMethod(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Toggle>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eToggleProperty.SetWithoutNotify: Target.SetIsOnWithoutNotify(value); break;
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
                case eToggleProperty.OnValueChanged: OnValueChangedAction += action as Action<Toggle, bool>; break;
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
                case eToggleProperty.OnValueChanged: OnValueChangedAction -= action as Action<Toggle, bool>; break;
            }
        }
    }
}

