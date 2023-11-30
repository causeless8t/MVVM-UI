using System.Collections.Generic;
using Causeless3t.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    public sealed class ReusableScrollView : ScrollRect
    {
        [SerializeField]
        private GameObject _itemPrefab;
        [SerializeField]
        private int _columnCount = 1;
        [SerializeField] 
        private Vector2 _itemSpace;
        
        private int _viewableItemCount = 0;
        private readonly List<RectTransform> _itemList = new();
        private float _diffPreFramePosition = 0;
        private int _currentItemNo = 0;

        private readonly List<ICollectionItem> _listData = new();
        private CollectionViewModel _parentViewModel;
        
        private readonly List<ICollectionItem> _changedItems = new();

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onValueChanged.RemoveListener(OnValueChanged);
        }

        #endregion MonoBehaviour

        public bool IsInitialized => _parentViewModel != null;
        public void InitView(CollectionViewModel viewModel) => _parentViewModel = viewModel;
        
        public void AddItem(ICollectionItem item)
        {
            _listData.Add(item);
            ResetView();
        }

        public void AddItems(IEnumerable<ICollectionItem> collection)
        {
            _listData.AddRange(collection);
            ResetView();
        }

        public void RemoveAt(int index)
        {
            _listData.RemoveAt(index);
            ResetView();
        }

        public void InsertItem(int index, ICollectionItem item)
        {
            _listData.Insert(index, item);
            ResetView();
        }
        
        public void InsertItems(int index, IEnumerable<ICollectionItem> collection)
        {
            _listData.InsertRange(index, collection);
            ResetView();
        }

        private void ResetView()
        {
            if (_listData == null) return;
            if (_itemPrefab == null) return;
            _columnCount = Mathf.Max(1, _columnCount);
            
            UpdateViewSize();
        }

        public void ClearList()
        {
            _itemList.ForEach((trans) => DestroyImmediate(trans.gameObject));
            _itemList.Clear();
            _listData.Clear();
            ResetView();
        }

        private void UpdateViewSize()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            var itemRect = _itemPrefab.GetComponent<RectTransform>();
            if (itemRect == null) return;
            
            if (_listData.Count < _viewableItemCount)
                _viewableItemCount = _listData.Count;
            else
                _viewableItemCount = _columnCount * (vertical ? Mathf.RoundToInt (rectTransform.sizeDelta.y / itemRect.sizeDelta.y) + 2 : Mathf.RoundToInt (rectTransform.sizeDelta.x / itemRect.sizeDelta.x) + 2);

            if (vertical)
                content.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (itemRect.sizeDelta.y + _itemSpace.y) * Mathf.CeilToInt(_listData.Count / (float)_columnCount) - _itemSpace.y);
            else
                content.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, (itemRect.sizeDelta.x + _itemSpace.x) * Mathf.CeilToInt(_listData.Count / (float)_columnCount) - _itemSpace.x);

            var columnIndex = _itemList.Count % _columnCount;
            for (int i = _itemList.Count; i < _viewableItemCount; i++) 
            {
                itemRect = Instantiate (_itemPrefab).GetComponent<RectTransform>();
                itemRect.SetParent (content, false);
                itemRect.name = i.ToString ();
                itemRect.anchorMin = itemRect.anchorMax = itemRect.pivot = new Vector2(0, 1);
                var sizeDelta = itemRect.sizeDelta;
                itemRect.anchoredPosition = vertical ? 
                    new Vector2 ((sizeDelta.x + _itemSpace.x) * columnIndex, -(sizeDelta.y + _itemSpace.y) * (i / _columnCount)) : 
                    new Vector2 ((sizeDelta.x + _itemSpace.x) * (i / _columnCount), -(sizeDelta.y + _itemSpace.y) * columnIndex);
                _itemList.Add (itemRect);
                itemRect.gameObject.SetActive (true);
                columnIndex = columnIndex + 1 >= _columnCount ? 0 : columnIndex + 1;
            }
        }

        public void UpdateChildrenViews()
        {
            for (int i = 0; i < _itemList.Count; ++i)
            {
                if (!_itemList[i].gameObject.activeSelf) continue;
                if (!int.TryParse(_itemList[i].name, out var index)) continue;
                if (index < 0 || index >= _listData.Count) continue;
                _listData[index].UpdateItem();
            }
        }

        public void UpdateChildView(ICollectionItem item)
        {
            if (!_listData.Contains(item)) return;
            if (_itemList.FindIndex((view) =>
                    view.gameObject.activeSelf && int.TryParse(view.name, out var index) && index == item.Index) ==
                -1) return;
            item.UpdateItem();
        }
        
        private void OnValueChanged(Vector2 value)
        {
            if (_itemList.Count == 0) return;
            var itemRect = _itemPrefab.GetComponent<RectTransform>();
            if (itemRect == null) return;
            var itemScale = vertical ? itemRect.sizeDelta.y : itemRect.sizeDelta.x;
            var itemSpace = vertical ? _itemSpace.y : _itemSpace.x;

            _changedItems.Clear();
            // scroll up, item attach bottom  or  right
            var anchoredPosition = vertical ? -content.anchoredPosition.y : content.anchoredPosition.x;
            while (anchoredPosition - _diffPreFramePosition < -(itemScale + itemSpace) * 2) 
            {
                _diffPreFramePosition -= (itemScale + itemSpace);

                for (int i = 0; i < _columnCount; ++i)
                {
                    var item = _itemList[0];
                    _itemList.RemoveAt (0);
                    _itemList.Add (item);
                    
                    var pos = (itemScale + itemSpace) * (_viewableItemCount / _columnCount) + (itemScale + itemSpace) * _currentItemNo;
                    item.anchoredPosition = vertical ? new Vector2 ((item.sizeDelta.x + itemSpace) * i, -pos) : new Vector2 (pos, -(item.sizeDelta.y + itemSpace) * i);

                    item.gameObject.SetActive(true);
                    if (_viewableItemCount + _currentItemNo * _columnCount + i < _listData.Count)
                    {
                        var index = _viewableItemCount + _currentItemNo * _columnCount + i;
                        item.name = index.ToString();
                        _changedItems.Add(_parentViewModel[index]);
                    }
                    else
                        item.gameObject.SetActive(false);
                }
                _currentItemNo++;
            }

            // scroll down, item attach top  or  left
            while (anchoredPosition - _diffPreFramePosition > -(itemScale + itemSpace) * 2) 
            {
                _diffPreFramePosition += (itemScale + itemSpace);

                for (int i = 0; i < _columnCount; ++i)
                {
                    var itemListLastCount = _viewableItemCount - 1;
                    var item = _itemList[itemListLastCount];
                    _itemList.RemoveAt(itemListLastCount);
                    _itemList.Insert(0, item);

                    var pos = (itemScale + itemSpace) * _currentItemNo;
                    item.anchoredPosition = vertical ? new Vector2((item.sizeDelta.x + itemSpace) * i, -pos) : new Vector2(pos, -(item.sizeDelta.y + itemSpace) * i);

                    item.gameObject.SetActive(true);
                    if (_currentItemNo * _columnCount + i > -1)
                    {
                        var index = _currentItemNo * _columnCount + i;
                        item.name = index.ToString();
                        _changedItems.Add(_parentViewModel[index]);
                    }
                    else
                        item.gameObject.SetActive(false);
                }
                _currentItemNo--;
            }
            
            if (IsInitialized && _changedItems.Count > 0)
                BinderManager.Instance.UpdateItems(_parentViewModel.BindKey, this, _changedItems);
        }
    }
}

