using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [AddComponentMenu("UI/LongTapButton", 31)]
    public sealed class LongTapButton : Button
    {
        /// <summary>
        /// 롱탭을 인식할 최초 진입시간
        /// </summary>
        private static readonly float RecognizeLongTapTime = 0.2f;
        /// <summary>
        /// 롱탭 인식 후 반복처리될 주기
        /// </summary>
        private static readonly float RecognizeCycle = 0.04f;
        private static readonly float RecognizeVeryLongTabCycle = 0.02f;
        /// <summary>
        /// 긴 롱탭 인식할 진입시간
        /// </summary>
        private static readonly float RecognizeVeryLongTapTime = 5f;
        
        private CancellationTokenSource _longTapLoopToken;
        private float _loopingTimer; 
        [SerializeField] private ButtonClickedEvent _longTapEvent = new();
        [SerializeField] private ButtonClickedEvent _veryLongTapEvent = new();
        
        
        
        AnimationCurve scale = AnimationCurve.Linear(0, 1.0f, 2, 0.2f);
        public ButtonClickedEvent onLongTap
        {
            get => _longTapEvent;
            set => _longTapEvent = value;
        }

        float GetRecognizeVeryLongTabCycle()
        { 
            var longTabCycle = RecognizeVeryLongTabCycle * (scale.Evaluate(_loopingTimer));
            // this.Log(longTabCycle.ToString("0.000") +"," + scale.Evaluate(_loopingTimer));
            return longTabCycle;
        }
        
        public ButtonClickedEvent onVeryLongTap
        {
            get => _veryLongTapEvent;
            set => _veryLongTapEvent = value;
        }

        private async UniTask UpdateLongTap()
        {
            _loopingTimer = 0f; 
            while (_longTapLoopToken is { IsCancellationRequested: false })
            { 
                await UniTask.WaitForSeconds(GetRecognizeVeryLongTabCycle(), cancellationToken: _longTapLoopToken.Token,
                    cancelImmediately: true, ignoreTimeScale: true);
                _loopingTimer += RecognizeVeryLongTabCycle;
                if (_loopingTimer < RecognizeLongTapTime) continue;
                if (_loopingTimer < RecognizeVeryLongTapTime)
                {
                    await UniTask.WaitForSeconds(RecognizeCycle, cancellationToken: _longTapLoopToken.Token,
                        cancelImmediately: true, ignoreTimeScale: true);
                    _loopingTimer += RecognizeCycle;
                    UISystemProfilerApi.AddMarker("LongTapButton.onLongTap", this);
                    _longTapEvent.Invoke();
                    continue;
                }
                UISystemProfilerApi.AddMarker("LongTapButton.onVeryLongTap", this);
                _veryLongTapEvent.Invoke();
                
            }
        }

        /// <summary>
        /// 롱탭버튼과 스크롤뷰가 겹쳤을때 드래그 이벤트로 인해 롱탭이 풀리는 버그로 구현한 임시 메소드.
        /// 버튼에서 실제로 손을 뗐을 때를 체크한다. 
        /// </summary>
        private async UniTask UpdatePointerUp()
        {
#if UNITY_EDITOR
            await UniTask.WaitUntil(() => Input.GetMouseButtonUp(0) || !gameObject.activeInHierarchy, cancellationToken:_longTapLoopToken.Token);
#else
            await UniTask.WaitUntil(() => Input.touchCount == 0 || !gameObject.activeInHierarchy, cancellationToken:_longTapLoopToken.Token);
#endif
            OnPointerUp(new PointerEventData(null){ button = PointerEventData.InputButton.Left });
            _longTapLoopToken?.Cancel();
            _longTapLoopToken = null;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _longTapLoopToken?.Cancel();
            _longTapLoopToken = null;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            if (!IsActive() || !IsInteractable())
                return;
            
            _longTapLoopToken?.Cancel();
            _longTapLoopToken = new CancellationTokenSource();
            UpdateLongTap().Forget();
            UpdatePointerUp().Forget();
        }
    }
}
