using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class SliderBinder : DataBinder<Slider>, IDataBinder<float>, IDataBinder<bool>, IUIEventBinder
    {
        public enum eSliderProperty
        {
            Value,
            SetWithoutNotify,
            Enable,
            OnValueChanged
        }

        [Serializable]
        public struct BindInfoSlider
        {
            public string Key;
            public eSliderProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoSlider> _bindInfos = new();
        private Dictionary<string, eSliderProperty> _bindInfoDic; 
        private event Action<Slider, float> OnValueChangedAction;

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
                if (info.PropertyType == eSliderProperty.OnValueChanged)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        private void OnValueChanged(float value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }

        public void SetProperty(string key, float value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eSliderProperty.Value: Target.value = value; break;
                case eSliderProperty.SetWithoutNotify: Target.SetValueWithoutNotify(value); break;
            }
        }

        public void SetProperty(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eSliderProperty.Enable: Target.interactable = value; break;
            }
        }

        bool IDataBinder<bool>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eSliderProperty.Enable: return Target.interactable;
            }
            return default;
        }

        public override bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        float IDataBinder<float>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Slider>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eSliderProperty.Value: return Target.value;
            }
            return default;
        }

        public void AddListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eSliderProperty.OnValueChanged: OnValueChangedAction += action as Action<Slider, float>; break;
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
                case eSliderProperty.OnValueChanged: OnValueChangedAction -= action as Action<Slider, float>; break;
            }
        }
    }
}

