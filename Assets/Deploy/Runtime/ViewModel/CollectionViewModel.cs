using System.Collections.Generic;
using System.Linq;

namespace Causeless3t.UI.MVVM
{
    public sealed class CollectionViewModel : IBindableCollection 
    {
        private readonly List<ICollectionItem> _viewModelList = new();
        private readonly string _propertyName;

        public CollectionViewModel(string name)
        {
            _propertyName = name;
        }

        public ICollectionItem this[int i]
        {
            get => _viewModelList[i];
            set => _viewModelList[i] = value;
        }
        
        public void AddItem(ICollectionItem item)
        {
            _viewModelList.Add(item);
            BinderManager.Instance.BroadcastAddItem(BindKey, _viewModelList.Count-1, item);
        }

        public void AddItems(IEnumerable<ICollectionItem> collection)
        {
            var baseViewModels = collection as ICollectionItem[] ?? collection.ToArray();
            _viewModelList.AddRange(baseViewModels);
            BinderManager.Instance.BroadcastAddItems(BindKey, baseViewModels);
        }
        
        public void InsertItem(int index, ICollectionItem item)
        {
            _viewModelList.Insert(index, item);
            BinderManager.Instance.BroadcastInsertItem(BindKey, index, item);
        }

        public void InsertItems(int index, IEnumerable<ICollectionItem> collection)
        {
            var baseViewModels = collection as ICollectionItem[] ?? collection.ToArray();
            _viewModelList.InsertRange(index, baseViewModels);
            BinderManager.Instance.BroadcastInsertItems(BindKey, index, baseViewModels);
        }
        
        public void RemoveAt(int index)
        {
            _viewModelList.RemoveAt(index);
            BinderManager.Instance.BroadcastRemoveAt(BindKey, index);
        }

        public void Clear()
        {
            _viewModelList.Clear();
            BinderManager.Instance.BroadcastClearItems(BindKey);
        }

        #region IBindableCollection

        public string BindKey => $"{GetType().FullName}/{_propertyName}";

        #endregion IBindableCollection
    }
}
