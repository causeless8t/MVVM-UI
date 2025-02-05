using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class RectTransformBinder : DataBinder<RectTransform>,
        IDataBinder<Vector2>, IDataBinder<Vector3>, IDataBinder<Quaternion>, IDataBinder<Rect>
    {
        public enum eRectTransformProperty
        {
            AnchorPosition,
            Position,
            Rotation,
            Scale,
            Size,
            Rect
        }

        [Serializable]
        public struct BindInfoRectTransform
        {
            public string Key;
            public eRectTransformProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoRectTransform> _bindInfos = new();
        private Dictionary<string, eRectTransformProperty> _bindInfoDic; 

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, Vector2 value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eRectTransformProperty.Size: Target.sizeDelta = value; break;
                case eRectTransformProperty.AnchorPosition: Target.anchoredPosition = value; break;
            }
        }

        public void SetProperty(string key, Vector3 value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eRectTransformProperty.Position: Target.position = value; break;
                case eRectTransformProperty.Scale: Target.localScale = value; break;
            }
        }

        public void SetProperty(string key, Quaternion value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eRectTransformProperty.Rotation: Target.rotation = value; break;
            }
        }

        public void SetProperty(string key, Rect value)
        {
            // Only Getter
        }

        Rect IDataBinder<Rect>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eRectTransformProperty.Rect: return Target.rect;
            }
            return default;
        }

        public override bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        Quaternion IDataBinder<Quaternion>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eRectTransformProperty.Rotation: return Target.rotation;
            }
            return default;
        }

        Vector3 IDataBinder<Vector3>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eRectTransformProperty.Position: return Target.position;
                case eRectTransformProperty.Scale: return Target.localScale;
            }
            return default;
        }

        Vector2 IDataBinder<Vector2>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= GetComponent<RectTransform>();
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eRectTransformProperty.AnchorPosition: return Target.anchoredPosition;
                case eRectTransformProperty.Size: return Target.sizeDelta;
            }
            return default;
        }
    }
}

