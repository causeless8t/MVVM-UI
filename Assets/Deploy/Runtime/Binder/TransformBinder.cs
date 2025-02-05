using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class TransformBinder : DataBinder<Transform>, IDataBinder<Vector3>, IDataBinder<Quaternion>
    {
        public enum eTransformProperty
        {
            Position,
            Rotation,
            Scale,
        }

        [Serializable]
        public struct BindInfoTransform
        {
            public string Key;
            public eTransformProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoTransform> _bindInfos = new();
        private Dictionary<string, eTransformProperty> _bindInfoDic; 

        protected override void Awake()
        {
            base.Awake();
            Target = transform;
        }
        
        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }

        public void SetProperty(string key, Vector3 value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= transform;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTransformProperty.Position: Target.position = value; break;
                case eTransformProperty.Scale: Target.localScale = value; break;
            }
        }

        public void SetProperty(string key, Quaternion value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= transform;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eTransformProperty.Rotation: Target.rotation = value; break;
            }
        }

        public override bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        Quaternion IDataBinder<Quaternion>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= transform;
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTransformProperty.Rotation: return Target.rotation;
            }
            return default;
        }

        Vector3 IDataBinder<Vector3>.GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= transform;
            if (Target.IsUnityNull()) return default;
            switch (type)
            {
                case eTransformProperty.Position: return Target.position;
                case eTransformProperty.Scale: return Target.localScale;
            }
            return default;
        }
    }
}

