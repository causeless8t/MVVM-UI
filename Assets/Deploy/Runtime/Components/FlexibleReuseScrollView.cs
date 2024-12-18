using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Causeless3t.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    public sealed class FlexibleReuseScrollView : ScrollRect
    {
        private static readonly float SizeThreshold = 0.1f;
        
        public class OnAddEvent : UnityEvent<ICollectionItemData> { }
        public class OnRemoveEvent : UnityEvent<ICollectionItemData> { }
        
        [Tooltip("프리팹으로 직접 참조할 프리팹")]
        [SerializeField]
        private GameObject _itemPrefab;
        [Tooltip("아이템 간의 간격")]
        public float ItemSpace;
        
        private int _viewableItemCount = 0;
        private readonly List<ICollectionItemUI> _itemList = new();
        private float _diffPreFramePosition = 0;

        private readonly List<IFlexCollectionItemData> _listData = new();
        public IReadOnlyList<IFlexCollectionItemData> DataList => _listData;

        public bool IsInitialized { get; private set; }
        private bool _isInitView;

        private bool _isInteractable = true;
        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                _isInteractable = value;
                var graphicList = transform.GetComponentsInChildren<Graphic>(true);
                foreach (var graphic in graphicList)
                    graphic.raycastTarget = value;
                var selectableList = transform.GetComponentsInChildren<Selectable>(true);
                foreach (var selectable in selectableList)
                    selectable.interactable = value;
            }
        }

        public int ItemCount => _listData.Count;

        public Vector2 ItemSize { get; private set; }
        
        private OnAddEvent m_OnAddItemEvent = new();
        public OnAddEvent onAddItemEvent 
        { 
            get => m_OnAddItemEvent;
            set => SetClass(ref m_OnAddItemEvent, value);
        }
        
        private OnRemoveEvent m_OnRemoveItemEvent = new();
        public OnRemoveEvent onRemoveItemEvent 
        { 
            get => m_OnRemoveItemEvent;
            set => SetClass(ref m_OnRemoveItemEvent, value);
        }
        
        private void SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
        }

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            _isInitView = false;
            IsInitialized = false;
        }

        protected override void Start()
        {
            base.Start();
            if (!Application.isPlaying) return;
            onValueChanged.AddListener(OnValueChanged);
            InitViewableItemCount();
        }

        protected override void OnDestroy()
        {
            _listData.Clear();
            _itemList.ForEach(itemUI => DestroyImmediate(itemUI.RootRectTransform.gameObject));
            _itemList.Clear();
            onValueChanged.RemoveListener(OnValueChanged);
            base.OnDestroy();
        }

        #endregion MonoBehaviour

        /// <summary>
        /// 아이템 데이터를 추가합니다.
        /// </summary>
        /// <param name="item">추가할 아이템 데이터</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void AddItem(IFlexCollectionItemData item, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.Add(item);
            ResetView();
            Repositioning();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
            m_OnAddItemEvent.Invoke(item);
        }

        /// <summary>
        /// 아이템 데이터를 Collection으로 추가합니다.
        /// </summary>
        /// <param name="collection">추가할 아이템 Collection</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void AddItems(IEnumerable<IFlexCollectionItemData> collection, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.AddRange(collection);
            ResetView();
            Repositioning();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
        }

        /// <summary>
        /// 아이템 데이터를 삭제합니다.
        /// </summary>
        /// <param name="index">삭제할 아이템 데이터 Index</param>
        /// <param name="bKeepPosition">삭제 시 현재 스크롤 포지션 유지 여부</param>
        public void RemoveAt(int index, bool bKeepPosition = false)
        {
            m_OnRemoveItemEvent.Invoke(_listData[index]);
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.RemoveAt(index);
            ResetView();
            Repositioning();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
        }

        /// <summary>
        /// 아이템 데이터를 Index 위치에 추가합니다.
        /// </summary>
        /// <param name="index">추가할 위치 Index</param>
        /// <param name="item">추가할 아이템 데이터</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void InsertItem(int index, IFlexCollectionItemData item, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.Insert(index, item);
            ResetView();
            Repositioning();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
            m_OnAddItemEvent.Invoke(item);
        }
        
        /// <summary>
        /// 아이템 데이터를 Index 위치에 Collection으로 추가합니다.
        /// </summary>
        /// <param name="index">추가할 위치 Index</param>
        /// <param name="collection">추가할 아이템 데이터 Collection</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void InsertItems(int index, IEnumerable<IFlexCollectionItemData> collection, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.InsertRange(index, collection);
            ResetView();
            Repositioning();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
        }

        /// <summary>
        /// Index 위치의 아이템 데이터를 갱신합니다. 
        /// </summary>
        /// <param name="index">갱신할 index</param>
        /// <param name="item">갱신할 데이터</param>
        public void UpdateItem(int index, IFlexCollectionItemData item)
        {
            if (index < 0 || index >= ItemCount) return;
            _listData[index] = item;
            Refresh();
        }

        public ICollectionItemUI GetItemUIByIndex(int index)
        {
            if (_itemList.Count == 0) return default;
            return _itemList.Find((item)=> item.Index == index);
        }

        public IFlexCollectionItemData GetItemDataByIndex(int index)
        {
            if (ItemCount == 0 || index < 0 || index >= ItemCount) return default;
            return _listData[index];
        }

        /// <summary>
        /// 특정 아이템 위치가 맨 위로 오도록 스크롤을 이동합니다.
        /// </summary>
        /// <param name="index">아이템 데이터 Index</param>
        /// <param name="duration">스크롤 이동에 걸리는 시간(sec)</param>
        /// <returns>duration이 0이 아닐때 DoTween, 0이면 null</returns>
        public void ScrollToItemIndex(int index, float duration = 0f)
        {
            if (ItemCount == 0) return;
            index = Mathf.Clamp(index, 0, ItemCount - 1);
            var indexPos = GetPositionByIndex(index);
            indexPos.y *= -1;
            if (index > ItemCount - (_viewableItemCount - 2))
            {
                ScrollToPosition(1.0f, duration);
                return;
            }

            if (Mathf.Approximately(duration, 0f))
            {
                content.anchoredPosition = vertical ? new Vector2(content.anchoredPosition.x, indexPos.y) : new Vector2(indexPos.x, content.anchoredPosition.y);
                return;
            }
            
            if (vertical)
                OnTweenAction(content.anchoredPosition.y, indexPos.y,
                    (from, to, t) => content.anchoredPosition =
                        new Vector2(content.anchoredPosition.x, Mathf.Lerp(from, to, t)), duration).Forget();
            else
                OnTweenAction(content.anchoredPosition.x, indexPos.x,
                    (from, to, t) => content.anchoredPosition =
                        new Vector2(Mathf.Lerp(from, to, t), content.anchoredPosition.y), duration).Forget();
        }

        /// <summary>
        /// 특정 위치로 스크롤을 이동합니다.
        /// </summary>
        /// <param name="pos">이동할 normalized(0~1) 포지션</param>
        /// <param name="duration">스크롤 이동에 걸리는 시간(sec)</param>
        /// <returns>duration이 0이 아닐때 DoTween, 0이면 null</returns>
        public void ScrollToPosition(float pos, float duration = 0f)
        {
            pos = 1f - Mathf.Clamp(pos, 0f, 1f);
            if (Mathf.Approximately(duration, 0f))
            {
                if (vertical)
                    verticalNormalizedPosition = pos;
                else
                    horizontalNormalizedPosition = pos;
                return;
            }
            
            if (vertical)
                OnTweenAction(verticalNormalizedPosition, pos,
                    (from, to, t) => verticalNormalizedPosition = Mathf.Lerp(from, to, t), duration).Forget();
            else
                OnTweenAction(horizontalNormalizedPosition, pos,
                    (from, to, t) => horizontalNormalizedPosition = Mathf.Lerp(from, to, t), duration).Forget();
        }
        
        private CancellationTokenSource _tweenCTS;
        private async UniTask OnTweenAction(float fromValue, float toValue, Action<float, float, float> action, float duration)
        {
            _tweenCTS?.Cancel();
            _tweenCTS = new CancellationTokenSource();
            var timer = 0f;
            while (timer <= duration)
            {
                if (_tweenCTS.IsCancellationRequested) return;
                action?.Invoke(fromValue, toValue, Mathf.Min(timer/duration, 1.0f));
                await UniTask.Yield();
                timer += Time.deltaTime;
            }
            _tweenCTS = null;
        }

        private void ResetView()
        {
            if (_itemPrefab.IsUnityNull()) return;

            if (!_isInitView)
                InitializeView();

            UpdateViewSize();
            Refresh();
        }

        /// <summary>
        /// 아이템 데이터를 모두 삭제합니다.
        /// </summary>
        public void ClearList()
        {
            _listData.Clear();
            ResetView();
        }

        private void InitViewableItemCount()
        {
            var itemRect = _itemPrefab.GetComponent<RectTransform>();
            if (itemRect == null) return;
            var count = vertical
                ? viewport.rect.height / (itemRect.rect.height + ItemSpace / 2)
                : viewport.rect.width / (itemRect.rect.width + ItemSpace / 2);
            count = Mathf.Max(0f, count - SizeThreshold);
            _viewableItemCount = Mathf.CeilToInt(count) + 2;
            InitializeView();
            ResetView();
            IsInitialized = true;
        }

        private void UpdateItem(ICollectionItemUI itemUI, int index)
        {
            var itemRect = itemUI.RootRectTransform;
            if (index >= 0 && index < ItemCount)
            {
                itemRect.gameObject.SetActive(true);
                itemUI.UpdateItem(index, _listData[index]);
            }
            else
            {
                itemUI.UpdateItem(index, null);
                itemRect.gameObject.SetActive(false);
            }
        }

        private void InitializeView()
        {
            if (_viewableItemCount <= 0) return;

            for (int i = -1; i<_viewableItemCount-1; ++i) 
            {
                var dataIndex = i;
                var itemUI = Instantiate(_itemPrefab).GetComponent<ICollectionItemUI>();
                itemUI.Index = dataIndex;
                var itemRect = itemUI.RootRectTransform;
                itemRect.SetParent (content, false);
                var stretchableX = itemRect.anchorMin.x == 0 && Mathf.Approximately(itemRect.anchorMax.x, 1);
                var stretchableY = itemRect.anchorMin.y == 0 && Mathf.Approximately(itemRect.anchorMax.y, 1);
                itemRect.anchorMin = itemRect.anchorMax = new Vector2(0.5f, 1);
                itemRect.pivot = new Vector2(0, 1);
                Vector2 stretchSize = vertical ? 
                    new Vector2(content.rect.width, itemRect.rect.height) : 
                    new Vector2(itemRect.rect.width, content.rect.height);
                itemRect.sizeDelta = new Vector2(stretchableX ? stretchSize.x : itemRect.sizeDelta.x,
                    stretchableY ? stretchSize.y : itemRect.sizeDelta.y);
                ItemSize = itemRect.rect.size;
                itemRect.anchoredPosition = GetPositionByIndex(dataIndex);
                var baseUi = itemRect.GetComponent<BaseUI>();
                if (!baseUi.IsReferenceNull())
                {
                    baseUi.Open();
                }
                _itemList.Add (itemUI);
            }
            _isInitView = true;
        }

        public void Refresh() => _itemList.ForEach(item => UpdateItem(item, item.Index));

        private void Repositioning()
        {
            _itemList.ForEach(item =>
            {
                if (item.Index >= 0 && item.Index < ItemCount)
                    item.RootRectTransform.anchoredPosition = GetPositionByIndex(item.Index);
            });
        }
        
        private void UpdateViewSize()
        {
            float contentSize = 0f;
            foreach (var data in _listData)
                contentSize += data.GetItemSize() + ItemSpace;
            contentSize -= ItemSpace;
            content.SetSizeWithCurrentAnchors(vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal,
                contentSize);
        }

        private Vector2 GetPositionByIndex(int index)
        {
            if (_itemPrefab.IsUnityNull()) return Vector2.zero;
            Vector2 itemSize = vertical ? 
                    new Vector2(ItemSize.x, index >= 0 && index < _listData.Count ? _listData[index].GetItemSize() : ItemSize.y) :
                    new Vector2(index >= 0 && index < _listData.Count ? _listData[index].GetItemSize() : ItemSize.x, ItemSize.y);
            float halfWidth = itemSize.x / 2;
            float contentSize = 0f;
            for (int i=0; i<_listData.Count; ++i)
            {
                if (i == index) break;
                contentSize += _listData[i].GetItemSize() + ItemSpace;
            }
            return vertical ? new Vector2(-halfWidth, -contentSize) : new Vector2(-halfWidth - contentSize, 0); 
        }
        
        private void OnValueChanged(Vector2 value)
        {
            if (_itemList.Count == 0) return;
            
            RectTransform itemRect;
            var lastDataIndex = _itemList[^1].Index;
            var lastItemSize = lastDataIndex >= 0 && lastDataIndex < _listData.Count ? 
                _listData[lastDataIndex].GetItemSize() :
                vertical ? ItemSize.y : ItemSize.x;
            var lastPosition = GetPositionByIndex(lastDataIndex);

            var viewSize = vertical ? viewport.rect.height : viewport.rect.width;

            // scroll up, item attach bottom  or  right
            var anchoredPosition = vertical ? -content.anchoredPosition.y : content.anchoredPosition.x;
            while (anchoredPosition - viewSize < (vertical ? lastPosition.y : lastPosition.x) + (lastItemSize + ItemSpace)) 
            {
                if (lastDataIndex >= ItemCount+1)
                    break;
                
                ++lastDataIndex;
                
                var itemUI = _itemList[0];
                _itemList.RemoveAt (0);
                _itemList.Add (itemUI);
                    
                var dataIndex = lastDataIndex;
                itemRect = itemUI.RootRectTransform;
                itemRect.anchoredPosition = GetPositionByIndex(dataIndex);
                    
                UpdateItem(itemUI, dataIndex);

                lastItemSize = lastDataIndex >= 0 && lastDataIndex < _listData.Count ? 
                    _listData[lastDataIndex].GetItemSize() :
                    vertical ? ItemSize.y : ItemSize.x;
                lastPosition = GetPositionByIndex(lastDataIndex);
            }
            
            var firstDataIndex = _itemList[0].Index;
            var firstItemSize = firstDataIndex >= 0 && firstDataIndex < _listData.Count ? 
                _listData[firstDataIndex].GetItemSize() :
                vertical ? ItemSize.y : ItemSize.x;
            var firstPosition = GetPositionByIndex(firstDataIndex);
            
            // scroll down, item attach top  or  left
            while (anchoredPosition > (vertical ? firstPosition.y : firstPosition.x) - (firstItemSize + ItemSpace)) 
            {
                if (firstDataIndex <= -1)
                    break;

                --firstDataIndex;
               
                var itemUI = _itemList[^1];
                _itemList.RemoveAt(_itemList.Count-1);
                _itemList.Insert(0, itemUI);
                    
                var dataIndex = firstDataIndex;
                itemRect = itemUI.RootRectTransform;
                itemRect.anchoredPosition = GetPositionByIndex(dataIndex);

                UpdateItem(itemUI, dataIndex);

                firstItemSize = firstDataIndex >= 0 && firstDataIndex < _listData.Count ? _listData[firstDataIndex].GetItemSize() :
                    vertical ? ItemSize.y : ItemSize.x;
                firstPosition = GetPositionByIndex(firstDataIndex);
            }
        }
    }
}

