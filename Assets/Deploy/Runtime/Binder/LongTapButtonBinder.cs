using System;
using System.Collections.Generic;
using System.Linq;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(LongTapButton))]
    public sealed class LongTapButtonBinder : DataBinder<LongTapButton>, IDataBinder<bool>, IUIEventBinder
    {
        public enum eLongTapButtonProperty
        {
            Enable,
            OnClick,
            OnLongTap,
            OnVeryLongTap
        }

        [Serializable]
        public struct BindInfoLongTapButton
        {
            public string Key;
            public eLongTapButtonProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoLongTapButton> _bindInfos = new();
        private Dictionary<string, eLongTapButtonProperty> _bindInfoDic; 
        private event Action<LongTapButton> OnClickAction;
        private event Action<LongTapButton> OnLongTapAction;
        private event Action<LongTapButton> OnVeryLongTapAction;

        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onClick.RemoveListener(OnClick);
            Target.onLongTap.RemoveListener(OnLongTap);
            Target.onVeryLongTap.RemoveListener(OnVeryLongTap);
            Target.onClick.AddListener(OnClick);
            Target.onLongTap.AddListener(OnLongTap);
            Target.onVeryLongTap.AddListener(OnVeryLongTap);
        }

        protected override void OnDestroy()
        {
            Target.onClick.RemoveListener(OnClick);
            Target.onLongTap.RemoveListener(OnLongTap);
            Target.onVeryLongTap.RemoveListener(OnVeryLongTap);
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
                if (info.PropertyType != eLongTapButtonProperty.Enable)
                    result.Add(info.Key);
            });
            return result.ToArray();
        }
        
        public void SetProperty(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<LongTapButton>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eLongTapButtonProperty.Enable: Target.interactable = value; break;
            }
        }

        public new bool GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<LongTapButton>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eLongTapButtonProperty.Enable: return Target.interactable;
            }
            return default;
        }

        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;
        
        private void OnClick()
        {
            OnClickAction?.Invoke(Target);
        }
        
        private void OnLongTap()
        {
            OnLongTapAction?.Invoke(Target);
        }
        
        private void OnVeryLongTap()
        {
            OnVeryLongTapAction?.Invoke(Target);
        }

        public void AddListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eLongTapButtonProperty.OnClick: OnClickAction += action as Action<LongTapButton>; break;
                case eLongTapButtonProperty.OnLongTap: OnLongTapAction += action as Action<LongTapButton>; break;
                case eLongTapButtonProperty.OnVeryLongTap: OnVeryLongTapAction += action as Action<LongTapButton>; break;
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
                case eLongTapButtonProperty.OnClick: OnClickAction -= action as Action<LongTapButton>; break;
                case eLongTapButtonProperty.OnLongTap: OnLongTapAction -= action as Action<LongTapButton>; break;
                case eLongTapButtonProperty.OnVeryLongTap: OnVeryLongTapAction -= action as Action<LongTapButton>; break;
            }
        }
    }
}

