using System;
using System.Collections.Generic;
using System.Reflection;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public sealed class BinderManager : Singleton<BinderManager>
    {
        #region PropertyBinder

        private struct PropertyBinder : IEquatable<PropertyBinder>
        {
            public IBindableProperty Owner;
            public Component ComponentOwner;
            public PropertyInfo PInfo;

            public bool Equals(PropertyBinder other)
            {
                return Equals(Owner, other.Owner) && Equals(ComponentOwner, other.ComponentOwner) && Equals(PInfo, other.PInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is PropertyBinder other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Owner, ComponentOwner, PInfo);
            }
        }
        
        private static readonly Dictionary<string, List<PropertyBinder>> PropertyBinderDictionary = new();
        
        public void BindProperty(string key, IBindableProperty owner, PropertyInfo info, Component component = null)
        {
            if (!PropertyBinderDictionary.TryGetValue(key, out var tuples))
            {
                tuples = new List<PropertyBinder>();
                PropertyBinderDictionary.Add(key, tuples);
            }
            var newBinder = new PropertyBinder
            {
                Owner = owner,
                PInfo = info,
                ComponentOwner = component
            };
            if (!tuples.Contains(newBinder))
                tuples.Add(newBinder);
        }

        public void UnBindProperty(string key)
        {
            if (!PropertyBinderDictionary.ContainsKey(key)) return;
            PropertyBinderDictionary.Remove(key);
        }

        public void BroadcastValue(string key, object value)
        {
            if (!PropertyBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                tuple.Owner.SetPropertyLockFlag(key);
                tuple.PInfo.SetValue(tuple.ComponentOwner == null ? tuple.Owner : tuple.ComponentOwner, value);
            });
        }

        #endregion PropertyBinder

        #region MethodBinder

        private struct MethodBinder : IEquatable<MethodBinder>
        {
            public IBindableMethod Owner;
            public Component ComponentOwner;
            public MethodInfo MInfo;

            public bool Equals(MethodBinder other)
            {
                return Equals(Owner, other.Owner) && Equals(ComponentOwner, other.ComponentOwner) && Equals(MInfo, other.MInfo);
            }

            public override bool Equals(object obj)
            {
                return obj is MethodBinder other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Owner, ComponentOwner, MInfo);
            }
        }

        #endregion MethodBinder

        #region CollectionBinder

        private struct CollectionBinderInfo : IEquatable<CollectionBinderInfo>
        {
            public CollectionViewModel ViewModel;
            public ReusableScrollView ComponentOwner;

            public bool Equals(CollectionBinderInfo other)
            {
                return Equals(ViewModel, other.ViewModel) && Equals(ComponentOwner, other.ComponentOwner);
            }

            public override bool Equals(object obj)
            {
                return obj is CollectionBinderInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ViewModel, ComponentOwner);
            }
        }
        
        private static readonly Dictionary<string, List<CollectionBinderInfo>> CollectionBinderDictionary = new();

        public void BindCollection(CollectionViewModel viewModel, ReusableScrollView scrollView)
        {
            if (!CollectionBinderDictionary.TryGetValue(viewModel.BindKey, out var tuples))
            {
                tuples = new List<CollectionBinderInfo>();
                CollectionBinderDictionary.Add(viewModel.BindKey, tuples);
            }
            var newBinder = new CollectionBinderInfo
            {
                ViewModel = viewModel,
                ComponentOwner = scrollView
            };
            if (!tuples.Contains(newBinder))
                tuples.Add(newBinder);
        }
        
        public void UnBindCollection(string key)
        {
            if (!CollectionBinderDictionary.ContainsKey(key)) return;
            CollectionBinderDictionary.Remove(key);
        }

        private void BindItem(CollectionBinderInfo tuple, int index)
        {
            var bindTrans = tuple.ComponentOwner.content.transform.Find(index.ToString());
            if (bindTrans == null) return;
            var viewModel = tuple.ViewModel[index] as BaseViewModel;
            foreach (var binder in bindTrans.GetComponentsInChildren<DataBinder>())
                binder.Bind(viewModel);
        }
        
        private void BindItem(CollectionBinderInfo tuple)
        {
            var contentTrans = tuple.ComponentOwner.content.transform;
            foreach (Transform child in contentTrans)
            {
                if (!int.TryParse(child.name, out var index)) continue;
                var viewModel = tuple.ViewModel[index] as BaseViewModel;
                foreach (var binder in child.GetComponentsInChildren<DataBinder>())
                    binder.Bind(viewModel);
            }
        }

        private void UnBindItem(CollectionBinderInfo tuple, int index)
        {
            var bindTrans = tuple.ComponentOwner.content.transform.Find(index.ToString());
            if (bindTrans == null) return;
            foreach (var binder in bindTrans.GetComponentsInChildren<DataBinder>())
                binder.UnBind();
        }
        
        private void UnBindItem(CollectionBinderInfo tuple)
        {
            var contentTrans = tuple.ComponentOwner.content.transform;
            foreach (var binder in contentTrans.GetComponentsInChildren<DataBinder>())
                binder.UnBind();
        }

        public void BroadcastAddItem(string key, int index, ICollectionItem item)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                tuple.ComponentOwner.AddItem(item);
                BindItem(tuple, index);
                tuple.ComponentOwner.UpdateChildView(item);
            });
        }
        
        public void BroadcastAddItems(string key, IEnumerable<ICollectionItem> collection)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                tuple.ComponentOwner.AddItems(collection);
                BindItem(tuple);
                tuple.ComponentOwner.UpdateChildrenViews();
            });
        }

        public void BroadcastInsertItem(string key, int index, ICollectionItem item)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                tuple.ComponentOwner.InsertItem(index, item);
                BindItem(tuple, index);
                tuple.ComponentOwner.UpdateChildView(item);
            });
        }
        
        public void BroadcastInsertItems(string key, int index, IEnumerable<ICollectionItem> collection)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                tuple.ComponentOwner.InsertItems(index, collection);
                BindItem(tuple);
                tuple.ComponentOwner.UpdateChildrenViews();
            });
        }
        
        public void BroadcastRemoveAt(string key, int index)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                UnBindItem(tuple, index);
                tuple.ComponentOwner.RemoveAt(index);
                tuple.ComponentOwner.UpdateChildrenViews();
            });
        }
        
        public void BroadcastClearItems(string key)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                tuple.ComponentOwner.ClearList();
                UnBindItem(tuple);
                tuple.ComponentOwner.UpdateChildrenViews();
            });
        }

        public void UpdateItems(string key, ReusableScrollView scrollView, IEnumerable<ICollectionItem> collection)
        {
            if (!CollectionBinderDictionary.TryGetValue(key, out var tuples)) return;
            tuples.ForEach((tuple) =>
            {
                if (tuple.ComponentOwner != scrollView) return;
                if (!tuple.ComponentOwner.IsInitialized)
                    tuple.ComponentOwner.InitView(tuple.ViewModel);
                BindItem(tuple);
                tuple.ComponentOwner.UpdateChildrenViews();
            });
        }

        #endregion CollectionBinder
    }
}
