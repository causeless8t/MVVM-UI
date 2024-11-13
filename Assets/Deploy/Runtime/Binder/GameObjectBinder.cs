using Causeless3t.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Causeless3t.UI
{
    public sealed class GameObjectBinder : DataBinder<GameObject>, IDataBinder<bool>, ICommandBinder<bool>
    {
        public enum eGameObjectProperty
        {
            IsActive,
            SetActive
        }

        [Serializable]
        public struct BindInfoGameObject
        {
            public string Key;
            public eGameObjectProperty PropertyType;
        }

        [SerializeField]
        private List<BindInfoGameObject> _bindInfos = new();
        private Dictionary<string, eGameObjectProperty> _bindInfoDic; 

        protected override void Awake()
        {
            base.Awake();
            Target = gameObject;
        }

        protected override void LoadData()
        {
            if (_bindInfos.Count == 0) return;
            _bindInfoDic = _bindInfos.ToDictionary(info => info.Key, info => info.PropertyType);
        }
        
        public void SetProperty(string key, bool value)
        {
            // Only Getter
        }

        public new bool GetProperty(string key)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return default;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return default;
            Target ??= gameObject;
            if (Target.IsUnityNull()) return default;
            switch (type)
            { 
                case eGameObjectProperty.IsActive: return Target.activeSelf;
            }
            return default;
        }
        
        public bool HasKey(string key) => _bindInfoDic?.ContainsKey(key) ?? false;

        public void InvokeMethod(string key, bool value)
        {
            if (_bindInfoDic == null)
                LoadData();
            if (_bindInfoDic == null) return;
            if (!_bindInfoDic!.TryGetValue(key, out var type)) return;
            Target ??= gameObject;
            if (Target.IsUnityNull()) return;
            switch (type)
            {
                case eGameObjectProperty.SetActive: Target.SetActive(value); break;
            }
        }
    }
}

