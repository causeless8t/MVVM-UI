using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class TMPInputFieldBinder : DataBinder<TMP_InputField>,
        IDataBinder<string>, IDataBinder<bool>, IDataBinder<float>, IDataBinder<TMP_Text>, IDataBinder<int>, ICommandBinder<string>, IUIEventBinder
    {
        public enum eTMP_InputFieldProperty
        {
            Enable,
            Text,
            IsFocused,
            CharacterLimit,
            TextComponent,
            FontSize,
            SetWithoutNotify,
            OnValueChanged,
            OnSubmit,
            OnSelect,
            OnDeselect
        }

        [Serializable]
        public struct BindInfoTMP_InputField
        {
            public string Key;
            public eTMP_InputFieldProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoTMP_InputField> _bindInfos = new();
        private Dictionary<string, eTMP_InputFieldProperty> _bindInfoDic; 
        private event Action<TMP_InputField, string> OnValueChangedAction;
        private event Action<TMP_InputField, string> OnSubmitAction;
        private event Action<TMP_InputField, string> OnSelectAction;
        private event Action<TMP_InputField, string> OnDeselectAction;

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
                if (info.PropertyType == eTMP_InputFieldProperty.OnValueChanged ||
                    info.PropertyType == eTMP_InputFieldProperty.OnSubmit ||
                    info.PropertyType == eTMP_InputFieldProperty.OnSelect ||
                    info.PropertyType == eTMP_InputFieldProperty.OnDeselect)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onSubmit.RemoveListener(OnSubmit);
            Target.onSelect.RemoveListener(OnSelect);
            Target.onDeselect.RemoveListener(OnDeselect);
            Target.onValueChanged.AddListener(OnValueChanged);
            Target.onSubmit.AddListener(OnSubmit);
            Target.onSelect.AddListener(OnSelect);
            Target.onDeselect.AddListener(OnDeselect);
        }

        protected override void OnDestroy()
        {
            Target.onValueChanged.RemoveListener(OnValueChanged);
            Target.onSubmit.RemoveListener(OnSubmit);
            Target.onSelect.RemoveListener(OnSelect);
            Target.onDeselect.RemoveListener(OnDeselect);
            base.OnDestroy();
        }

        public void SetProperty(string key, string value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.Text: Target.text = value; break;
            }
        }

        public void SetProperty(string key, float value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.FontSize: Target.pointSize = value; break;
            }
        }

        public void SetProperty(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.Enable: Target.interactable = value; break;
            }
        }

        public void SetProperty(string key, TMP_Text value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.TextComponent: Target.textComponent = value; break;
            }
        }

        public void SetProperty(string key, int value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.CharacterLimit: Target.characterLimit = value; break;
            }
        }

        int IDataBinder<int>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_InputFieldProperty.CharacterLimit: return Target.characterLimit;
            }
            return default;
        }

        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;
        
        TMP_Text IDataBinder<TMP_Text>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_InputFieldProperty.TextComponent: return Target.textComponent;
            }
            return default;
        }

        bool IDataBinder<bool>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_InputFieldProperty.IsFocused: return Target.isFocused;
                case eTMP_InputFieldProperty.Enable: return Target.interactable;
            }
            return default;
        }

        float IDataBinder<float>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_InputFieldProperty.FontSize: return Target.pointSize;
            }
            return default;
        }

        string IDataBinder<string>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_InputFieldProperty.Text: return Target.text;
            }
            return default;
        }
        
        private void OnValueChanged(string value)
        {
            OnValueChangedAction?.Invoke(Target, value);
        }
        
        private void OnSubmit(string value)
        {
            OnSubmitAction?.Invoke(Target, value);
        }
        
        private void OnSelect(string value)
        {
            OnSelectAction?.Invoke(Target, value);
        }
        
        private void OnDeselect(string value)
        {
            OnDeselectAction?.Invoke(Target, value);
        }

        public void InvokeMethod(string key, string value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_InputField>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_InputFieldProperty.SetWithoutNotify: Target.SetTextWithoutNotify(value); break;
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
                case eTMP_InputFieldProperty.OnValueChanged: OnValueChangedAction += action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnSubmit: OnSubmitAction += action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnSelect: OnSelectAction += action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnDeselect: OnDeselectAction += action as Action<TMP_InputField, string>; break;
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
                case eTMP_InputFieldProperty.OnValueChanged: OnValueChangedAction -= action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnSubmit: OnSubmitAction -= action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnSelect: OnSelectAction -= action as Action<TMP_InputField, string>; break;
                case eTMP_InputFieldProperty.OnDeselect: OnDeselectAction -= action as Action<TMP_InputField, string>; break;
            }
        }
    }
}

