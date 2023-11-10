using System;
using System.Collections.Generic;
using System.Reflection;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public sealed class BinderManager : Singleton<BinderManager>
    {
        private struct BinderTuple : IEquatable<BinderTuple>
        {
            public IBindable Owner;
            public Component ComponentOwner;
            public PropertyInfo PInfo;

            public bool Equals(BinderTuple other)
            {
                return Equals(Owner, other.Owner) && Equals(ComponentOwner, other.ComponentOwner) && Equals(PInfo, other.PInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is BinderTuple other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Owner, ComponentOwner, PInfo);
            }
        }

        private static readonly Dictionary<string, List<BinderTuple>> BinderDictionary = new();

        public void Bind(string key, IBindable owner, PropertyInfo info, Component component = null)
        {
            if (!BinderDictionary.TryGetValue(key, out var tuples))
            {
                tuples = new List<BinderTuple>();
                BinderDictionary.Add(key, tuples);
            }
            var newBinder = new BinderTuple
            {
                Owner = owner,
                PInfo = info,
                ComponentOwner = component
            };
            if (!tuples.Contains(newBinder))
                tuples.Add(newBinder);
        }

        public void UnBind(string key)
        {
            if (!BinderDictionary.ContainsKey(key)) return;
            BinderDictionary.Remove(key);
        }

        public void BroadcastValue(string key, object value)
        {
            if (!BinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                tuple.Owner.SetPropertyLockFlag(key);
                tuple.PInfo.SetValue(tuple.ComponentOwner == null ? tuple.Owner : tuple.ComponentOwner, value);
            });
        }
    }
}
