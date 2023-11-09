using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    [Serializable]
    public sealed class BinderInfo : IEquatable<BinderInfo>
    {
        public enum eBindRange
        {
            None = 0,
            Get,
            Set,
            GetNSet
        }

        [SerializeField]
        private string _ownerType;
        public Type Owner
        {
            get => string.IsNullOrEmpty(_ownerType) ? null : Type.GetType(_ownerType);
            set => _ownerType = value.AssemblyQualifiedName;
        }

        public eBindRange Range;
        [SerializeField]
        private string _propName;
        [SerializeField]
        private string _returnType;
        public PropertyInfo PInfo
        {
            get => string.IsNullOrEmpty(_returnType) ? null : Owner?.GetProperty(_propName, Type.GetType(_returnType), new Type[]{});
            set
            {
                _propName = value.Name;
                _returnType = value.PropertyType.AssemblyQualifiedName;
            }
        }
        public string GetKey() => $"{_ownerType}/{PInfo.Name}";

        public void SetPropertyInfo(eBindRange range, PropertyInfo info)
        {
            Range = range;
            PInfo = info;
        }

        public bool Equals(BinderInfo other) => GetKey().Equals(other?.GetKey());
    }
}
