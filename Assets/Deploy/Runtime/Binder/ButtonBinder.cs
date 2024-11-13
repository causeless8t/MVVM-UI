using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonBinder : DataBinder<Button>, IDataBinder<bool>, IUIEventBinder
    {
        public enum eButtonProperty
        {
            Enable,
            OnClick
        }

        [Serializable]
        public struct BindInfoButton
        {
            public string Key;
            public eButtonProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoButton> _bindInfos = new();
        private Dictionary<string, eButtonProperty> _bindInfoDic;
        private event Action<Button> OnClickAction;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Target.onClick.RemoveListener(OnClick);
            Target.onClick.AddListener(OnClick);
        }

        protected override void OnDestroy()
        {
            Target.onClick.RemoveListener(OnClick);
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
                if (info.PropertyType == eButtonProperty.OnClick)
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
            Target ??= GetComponent<Button>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eButtonProperty.Enable: Target.interactable = value; break;
            }
        }

        public new bool GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Button>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eButtonProperty.Enable: return Target.interactable;
            }
            return default;
        }

        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        private void OnClick()
        {
            OnClickAction?.Invoke(Target);
        }

        public void AddListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eButtonProperty.OnClick: OnClickAction += action as Action<Button>; break;
            }
        }

        public void RemoveListener(string key, Delegate action)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic.TryGetValue(key, out var type)) return;
            switch (type)
            {
                case eButtonProperty.OnClick: OnClickAction -= action as Action<Button>; break;
            }
        }
    }
}

