using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMPTextBinder : DataBinder<TMP_Text>,
        IDataBinder<string>, IDataBinder<Color>, IDataBinder<float>
    {
        public enum eTMP_TextProperty
        {
            Text,
            Color,
            Alpha,
            Size
        }

        [Serializable]
        public struct BindInfoTMP_Text
        {
            public string Key;
            public eTMP_TextProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoTMP_Text> _bindInfos = new();
        private Dictionary<string, eTMP_TextProperty> _bindInfoDic; 

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, string value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_TextProperty.Text: Target.SetText(value); break;
            }
        }

        public void SetProperty(string key, float value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_TextProperty.Alpha: Target.alpha = value; break;
                case eTMP_TextProperty.Size: Target.fontSize = value; break;
            }
        }

        public void SetProperty(string key, Color value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTMP_TextProperty.Color: Target.color = value; break;
            }
        }

        public override bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        Color IDataBinder<Color>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_TextProperty.Color: return Target.color;
            }
            return default;
        }

        float IDataBinder<float>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_TextProperty.Alpha: return Target.alpha;
                case eTMP_TextProperty.Size: return Target.fontSize;
            }
            return default;
        }

        string IDataBinder<string>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<TMP_Text>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTMP_TextProperty.Text: return Target.text;
            }
            return default;
        }
    }
}

