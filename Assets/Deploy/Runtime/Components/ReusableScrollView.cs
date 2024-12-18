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
    public sealed class ReusableScrollView : ScrollRect
    {
        private static readonly float SizeThreshold = 0.1f;
        
        public class OnAddEvent : UnityEvent<ICollectionItemData> { }
        public class OnRemoveEvent : UnityEvent<ICollectionItemData> { }
        
        [Tooltip("프리팹으로 직접 참조할 프리팹")]
        [SerializeField]
        private GameObject _itemPrefab;
        [SerializeField]
        private int _columnCount = 1;
        [Tooltip("아이템 간의 간격")]
        public Vector2 ItemSpace;

        private Vector2 _cachedViewportSize;
        
        private int _viewableItemCount = 0;
        private readonly List<ICollectionItemUI> _itemList = new();
        private float _diffPreFramePosition = 0;

        public int ViewableItemCount => Mathf.Max(_viewableItemCount - 2, 0);

        private readonly List<ICollectionItemData> _listData = new();
        public IReadOnlyList<ICollectionItemData> DataList => _listData;

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
        public int ItemRow => Mathf.CeilToInt(_listData.Count / (float)_columnCount);

        private Vector2 _cachedSize;
        public Vector2 ItemSize
        {
            get
            {
                if (_cachedSize.Equals(default))
                    return _itemPrefab.IsUnityNull()
                        ? Vector2.zero
                        : _itemPrefab.GetComponent<RectTransform>().rect.size;
                return _cachedSize;
            }
        }
        
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
        public void AddItem(ICollectionItemData item, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition; 
            _listData.Add(item);
            ResetView();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
            m_OnAddItemEvent.Invoke(item);
        }

        /// <summary>
        /// 아이템 데이터를 Collection으로 추가합니다.
        /// </summary>
        /// <param name="collection">추가할 아이템 Collection</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void AddItems(IEnumerable<ICollectionItemData> collection, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.AddRange(collection);
            ResetView();
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
            if (bKeepPosition)
                ScrollToPosition(prevPos);
        }

        /// <summary>
        /// 아이템 데이터를 Index 위치에 추가합니다.
        /// </summary>
        /// <param name="index">추가할 위치 Index</param>
        /// <param name="item">추가할 아이템 데이터</param>
        /// <param name="bKeepPosition">추가 시 현재 스크롤 포지션 유지 여부</param>
        public void InsertItem(int index, ICollectionItemData item, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.Insert(index, item);
            ResetView();
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
        public void InsertItems(int index, IEnumerable<ICollectionItemData> collection, bool bKeepPosition = false)
        {
            float prevPos = vertical ? verticalNormalizedPosition : horizontalNormalizedPosition;
            _listData.InsertRange(index, collection);
            ResetView();
            if (bKeepPosition)
                ScrollToPosition(prevPos);
        }

        /// <summary>
        /// Index 위치의 아이템 데이터를 갱신합니다. 
        /// </summary>
        /// <param name="index">갱신할 index</param>
        /// <param name="item">갱신할 데이터</param>
        public void UpdateItem(int index, ICollectionItemData item)
        {
            if (index < 0 || index >= _listData.Count) return;
            _listData[index] = item;
            Refresh();
        }

        public ICollectionItemUI GetItemUIByIndex(int index)
        {
            if (_itemList.Count == 0) return default;
            return _itemList.Find((item)=> item.Index == index);
        }

        public ICollectionItemData GetItemDataByIndex(int index)
        {
            if (_listData.Count == 0 || index < 0 || index >= _listData.Count) return default;
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
            if (_listData.Count == 0) return;
            index = Mathf.Clamp(index, 0, _listData.Count - 1);
            var indexPos = GetPositionByIndex(index);
            indexPos.y *= -1;
            if (index / _columnCount == 0)
            {
                ScrollToPosition(0.0f, duration);
                return;
            }
            if (index / _columnCount > _listData.Count / _columnCount - (_viewableItemCount - 2))
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
            if (vertical)
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
            _columnCount = Mathf.Max(1, _columnCount);

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
                ? viewport.rect.height / (itemRect.rect.height + ItemSpace.y / 2)
                : viewport.rect.width / (itemRect.rect.width + ItemSpace.x / 2);
            count = Mathf.Max(0f, count - SizeThreshold);
            _viewableItemCount = Mathf.CeilToInt(count) + 2;
            InitializeView();
            ResetView();
            _cachedViewportSize = viewport.rect.size;
            IsInitialized = true;
        }

        private void UpdateItem(ICollectionItemUI itemUI, int index)
        {
            var itemRect = itemUI.RootRectTransform;
            if (index >= 0 && index < _listData.Count)
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
                for (int j = 0; j<_columnCount; ++j)
                {
                    var dataIndex = i * _columnCount + j;
                    var itemUIObj = Instantiate(_itemPrefab);
                    itemUIObj.SetActive(true); // 최초 Awake 로직 동작
                    var itemUI = itemUIObj.GetComponent<ICollectionItemUI>();
                    itemUI.Index = dataIndex;
                    var itemRect = itemUI.RootRectTransform;
                    itemRect.SetParent (content, false);
                    var stretchableX = itemRect.anchorMin.x == 0 && Mathf.Approximately(itemRect.anchorMax.x, 1);
                    var stretchableY = itemRect.anchorMin.y == 0 && Mathf.Approximately(itemRect.anchorMax.y, 1);
                    itemRect.anchorMin = itemRect.anchorMax = vertical ? new Vector2(0.5f, 1) : new Vector2(0, 0.5f);
                    itemRect.pivot = new Vector2(0, 1);
                    Vector2 stretchSize = vertical ? 
                        new Vector2((content.rect.width - (_columnCount-1) * ItemSpace.x) / _columnCount, itemRect.rect.height) : 
                        new Vector2(itemRect.rect.width, (content.rect.height - (_columnCount-1) * ItemSpace.y) / _columnCount);
                    itemRect.sizeDelta = new Vector2(stretchableX ? stretchSize.x : itemRect.sizeDelta.x,
                        stretchableY ? stretchSize.y : itemRect.sizeDelta.y);
                    if (_cachedSize.Equals(default))
                        _cachedSize = itemRect.rect.size;
                    itemRect.anchoredPosition = GetPositionByIndex(dataIndex);
                    var baseUi = itemRect.GetComponent<BaseUI>();
                    if (!baseUi.IsReferenceNull())
                    {
                        baseUi.Open();
                    }
                    _itemList.Add (itemUI);
                }
            }
            _isInitView = true;
        }

        public void Refresh() => _itemList.ForEach(item => UpdateItem(item, item.Index));

        /// <summary>
        /// 뷰포트의 크기가 달라지거나 아이템의 크기가 달라질때(ex. 해상도 변경) 현재 생성된 모든 아이템들의 사이즈를 변경하고 위치를 재정렬해주는 함수.
        /// 호출비용이 높아 아이템을 추가한 뒤 한번만 호출하는 편이 좋다.
        /// </summary>
        public async UniTask RefreshItemSize()
        {
            await UniTask.Yield(); // 레이아웃의 변경을 기다리는 단계
            if (!_isInitView || _itemList.Count == 0) return;
            _cachedSize = default;
            _itemList.ForEach(itemUI => DestroyImmediate(itemUI.RootRectTransform.gameObject));
            _itemList.Clear();
            await UniTask.Yield(); // 아이템이 지워지길 기다리는 단계
            ScrollToPosition(0);
            _diffPreFramePosition = 0;
            var count = vertical
                ? viewport.rect.height / (ItemSize.y + ItemSpace.y / 2)
                : viewport.rect.width / (ItemSize.x + ItemSpace.x / 2);
            count = Mathf.Max(0f, count - SizeThreshold);
            _viewableItemCount = Mathf.CeilToInt(count) + 2;
            _isInitView = false;
            InitializeView();
            ResetView();
        }
        
        private void UpdateViewSize()
        {
            float contentSize;
            var countForViewSize = Mathf.CeilToInt(_listData.Count / (float)_columnCount);
            if (countForViewSize < _viewableItemCount - 2)
                contentSize = vertical ? viewport.rect.height : viewport.rect.width;
            else
                contentSize = vertical
                    ? (ItemSize.y + ItemSpace.y) * countForViewSize - ItemSpace.y
                    : (ItemSize.x + ItemSpace.x) * countForViewSize - ItemSpace.x;
            content.SetSizeWithCurrentAnchors(vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal,
                contentSize);
        }

        private Vector2 GetPositionByIndex(int index)
        {
            if (_itemPrefab.IsUnityNull()) return Vector2.zero;
            int column = index % _columnCount;
            int row = index / _columnCount;
            float halfSize = (_columnCount * (ItemSize.x + ItemSpace.x) - ItemSpace.x) / 2;
            return vertical
                ? new Vector2(-halfSize + (ItemSize.x + ItemSpace.x) * column, -(ItemSize.y + ItemSpace.y) * row)
                : new Vector2((ItemSize.x + ItemSpace.x) * row, halfSize - (ItemSize.y + ItemSpace.y) * column);
        }
        
        private void OnValueChanged(Vector2 value)
        {
            if (_itemList.Count == 0) return;
            var itemScale = vertical ? ItemSize.y : ItemSize.x;
            var itemSpace = vertical ? ItemSpace.y : ItemSpace.x;

            RectTransform itemRect;

            // scroll up, item attach bottom  or  right
            var anchoredPosition = vertical ? -content.anchoredPosition.y : content.anchoredPosition.x;
            while (anchoredPosition - _diffPreFramePosition < -(itemScale + itemSpace)) 
            {
                var lastDataIndex = _itemList[^1].Index;
                if (lastDataIndex >= (ItemRow+1) * _columnCount)
                    break;
                
                ++lastDataIndex;
                _diffPreFramePosition -= (itemScale + itemSpace);
                for (int i = 0; i < _columnCount; ++i)
                {
                    var itemUI = _itemList[0];
                    _itemList.RemoveAt (0);
                    _itemList.Add (itemUI);
                    
                    var dataIndex = lastDataIndex + i;
                    itemRect = itemUI.RootRectTransform;
                    itemRect.anchoredPosition = GetPositionByIndex(dataIndex);
                    
                    UpdateItem(itemUI, dataIndex);
                }
            }

            // scroll down, item attach top  or  left
            while (anchoredPosition - _diffPreFramePosition > 0) 
            {
                var firstDataIndex = _itemList[0].Index;
                if (firstDataIndex <= -_columnCount)
                    break;

                --firstDataIndex;
                _diffPreFramePosition += (itemScale + itemSpace);
                for (int i = 0; i < _columnCount; ++i)
                {
                    var itemUI = _itemList[^1];
                    _itemList.RemoveAt(_itemList.Count-1);
                    _itemList.Insert(0, itemUI);
                    
                    var dataIndex = firstDataIndex - i;
                    itemRect = itemUI.RootRectTransform;
                    itemRect.anchoredPosition = GetPositionByIndex(dataIndex);

                    UpdateItem(itemUI, dataIndex);
                }
            }
        }
    }
}

