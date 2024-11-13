using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class ImageBinder : DataBinder<Image>, IDataBinder<Sprite>, IDataBinder<float>, IDataBinder<Color>
    {
        public enum eImageProperty
        {
            Sprite,
            FillAmount,
            Color
        }

        [Serializable]
        public struct ImageProperty
        {
            public string Key;
            public eImageProperty PropertyType;
        }

        [SerializeField]
        private List<ImageProperty> _bindInfos = new();
        private Dictionary<string, eImageProperty> _bindInfoDic; 

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, Sprite value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eImageProperty.Sprite: Target.sprite = value; break;
            }
        }

        public void SetProperty(string key, float value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eImageProperty.FillAmount: Target.fillAmount = value; break;
            }
        }

        public void SetProperty(string key, Color value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eImageProperty.Color: Target.color = value; break;
            }
        }

        Color IDataBinder<Color>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eImageProperty.Color: return Target.color;
            }
            return default;
        }

        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        float IDataBinder<float>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eImageProperty.FillAmount: return Target.fillAmount;
            }
            return default;
        }

        Sprite IDataBinder<Sprite>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<Image>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eImageProperty.Sprite: return Target.sprite;
            }
            return default;
        }
    }
}

