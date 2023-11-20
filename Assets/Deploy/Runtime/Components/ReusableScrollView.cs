using System.Collections.Generic;
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
        
        private int _initItemCount = 0;
        private List<RectTransform> _itemList = new();
        private float _diffPreFramePosition = 0;
        private int _currentItemNo = 0;

        private List<ICollectionItem> _listData;

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

        public void SetListData(IEnumerable<ICollectionItem> collection)
        {
            _listData = new List<ICollectionItem>(collection);
            Init();
        }

        private void Init()
        {
            if (_listData == null) return;
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            if (_itemPrefab == null) return;
            var itemRect = _itemPrefab.GetComponent<RectTransform>();
            if (itemRect == null) return;
            _columnCount = Mathf.Max(1, _columnCount);
            
            if (_listData.Count < _initItemCount)
                _initItemCount = _listData.Count;
            else
                _initItemCount = _columnCount * (vertical ? Mathf.RoundToInt (rectTransform.sizeDelta.y / itemRect.sizeDelta.y) + 2 : Mathf.RoundToInt (rectTransform.sizeDelta.x / itemRect.sizeDelta.x) + 2);

            if (vertical)
                content.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (itemRect.sizeDelta.y + _itemSpace.y) * Mathf.CeilToInt(_listData.Count / (float)_columnCount) - _itemSpace.y);
            else
                content.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, (itemRect.sizeDelta.x + _itemSpace.x) * Mathf.CeilToInt(_listData.Count / (float)_columnCount) - _itemSpace.x);

            var columnIndex = 0;
            for (int i = 0; i < _initItemCount; i++) 
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
                // itemRect.GetComponent<ICollectionItem> ().UpdateItem (i);
                columnIndex = columnIndex + 1 >= _columnCount ? 0 : columnIndex + 1;
            }
        }
        
        private void OnValueChanged(Vector2 value)
        {
            var itemRect = _itemPrefab.GetComponent<RectTransform>();
            if (itemRect == null) return;
            var itemScale = vertical ? itemRect.sizeDelta.y : itemRect.sizeDelta.x;
            var itemSpace = vertical ? _itemSpace.y : _itemSpace.x;
            
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
                    
                    var pos = (itemScale + itemSpace) * (_initItemCount / _columnCount) + (itemScale + itemSpace) * _currentItemNo;
                    item.anchoredPosition = vertical ? new Vector2 ((item.sizeDelta.x + itemSpace) * i, -pos) : new Vector2 (pos, -(item.sizeDelta.y + itemSpace) * i);

                    item.gameObject.SetActive(true);
                    if (_initItemCount + _currentItemNo * _columnCount + i < _listData.Count)
                        // item.GetComponent<ICollectionItem>().UpdateItem(_initItemCount + _currentItemNo * _columnCount + i);
                        item.name = (_initItemCount + _currentItemNo * _columnCount + i).ToString();
                    else
                        // item.GetComponent<ICollectionItem>().UpdateItem(-100);
                        item.gameObject.SetActive(false);
                }
                _currentItemNo++;
            }

            // scroll down, item attach top  or  left
            while (anchoredPosition - _diffPreFramePosition > 0) 
            {
                _diffPreFramePosition += (itemScale + itemSpace);

                for (int i = 0; i < _columnCount; ++i)
                {
                    var itemListLastCount = _initItemCount - 1;
                    var item = _itemList[itemListLastCount];
                    _itemList.RemoveAt(itemListLastCount);
                    _itemList.Insert(0, item);

                    var pos = (itemScale + itemSpace) * _currentItemNo;
                    item.anchoredPosition = vertical ? new Vector2((item.sizeDelta.x + itemSpace) * i, -pos) : new Vector2(pos, -(item.sizeDelta.y + itemSpace) * i);

                    item.gameObject.SetActive(true);
                    if (_currentItemNo * _columnCount + i > -1)
                        // item.GetComponent<ICollectionItem>().UpdateItem(_currentItemNo * _columnCount + i);
                        item.name = (_currentItemNo * _columnCount + i).ToString();
                    else
                        // item.GetComponent<ICollectionItem>().UpdateItem(-100);
                        item.gameObject.SetActive(false);
                }
                _currentItemNo--;
            }
        }
    }
}

