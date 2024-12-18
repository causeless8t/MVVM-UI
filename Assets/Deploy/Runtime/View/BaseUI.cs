
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.UI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class BaseUIEventRegisterAttribute : System.Attribute
    {
        public Type EventType { get; }

        public BaseUIEventRegisterAttribute(System.Type type)
        {
            EventType = type;
        }
    }

    public class BaseUI : MonoBehaviour, IBinderManager
    {
        protected static readonly Dictionary<Type, List<(string, Type, MethodInfo)>> UIEventBindInfo = new();
        private static bool _isInitializeBindInfo;
        protected readonly HashSet<IBinder> _binders = new();

        public bool IsInitializedBinder { get; private set; }


        public RectTransform RectTransform => this.transform as RectTransform;
        /// <summary>
        /// 씬 자체에 이미 있는 Ui인지 여부
        /// </summary>
        public bool IsOriginUi { get; set; }

        /// <summary>
        /// Ui가 오픈된 후 불리는 Event
        /// </summary>
        public event Action<BaseUI> OnOpenEvent;
        /// <summary>
        /// Ui가 닫히기 직전 불리는 Event
        /// </summary>
        public event Action<BaseUI> OnCloseEvent;

        #region member method

        protected virtual void Awake()
        {
            if (!_isInitializeBindInfo)
            {
                _isInitializeBindInfo = true;
                InitUIEventBindInfo();
            }
            SearchBinders();
        }

        protected virtual void OnEnable()
        {
            RegisterUIEvents();
        }

        protected virtual void OnDisable()
        {
            UnRegisterUIEvents();
        }

        protected virtual void Start()
        {
        }
        
        
        public virtual void Open()
        {
            OnOpenEvent?.Invoke(this);
            OnOpenEvent = null;
            OpeningProcess().Forget();
        }

        protected virtual async UniTask OpeningProcess()
        {
            gameObject.SetActive(true);
            await UniTask.CompletedTask;
        }

        public virtual void Close()
        {
            OnCloseEvent?.Invoke(this);
            OnCloseEvent = null;
            ClosingProcess().Forget();
        }
        
        protected virtual async UniTask ClosingProcess()
        {
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        protected virtual void OnDestroy()
        {
            UnRegisterUIEvents();
            _binders.Clear();
        } 

        #endregion

        private static void InitUIEventBindInfo()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute(typeof(UIRegisterAttribute), true) != null);
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<UIRegisterAttribute>();
                    if (attribute.DelegateType == null)
                    {
                        Debug.LogError("AutoRegistUIEvents Failed. actionType is error");
                        return;
                    }

                    var key = attribute.Key;
                    if (string.IsNullOrEmpty(key))
                    {
                        key = method.Name;
                    }

                    if (!UIEventBindInfo.TryGetValue(type, out var list))
                    {
                        list = new();
                        UIEventBindInfo.Add(type, list);
                    }
                    
                    list.Add(new()
                    {
                        Item1 = key,
                        Item2 = attribute.DelegateType,
                        Item3 = method
                    });
                }
            }
        }
        
        /// <summary>
        /// 프로퍼티의 Setter에서 호출하여 변경사항을 키와 연결된 Ui에 적용합니다.  
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="type">프로퍼티의 타입</param>
        /// <param name="value">프로퍼티의 값</param>
        public void BroadcastSetProperty<T>(string key, T value)
        {
            foreach (var binder in _binders)
                (binder as IDataBinder<T>)?.SetProperty(key, value);
        }

        /// <summary>
        /// 키와 연결된 Ui에서 값을 가져옵니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <returns>Ui의 값</returns>
        public T BroadcastGetProperty<T>(string key)
        { 
            foreach (var binder in _binders)
            {
                if (binder is not IDataBinder<T> dataBinder) continue;
                if (!dataBinder.HasKey(key)) continue;
                return dataBinder.GetProperty(key);
            }
            return default;
        }


        private void AutoRegistUIEvents(bool isRegister = true)
        {
            if (!_isInitializeBindInfo) return;
            
            if (!UIEventBindInfo.TryGetValue(GetType(), out var bindInfoList))
                return;

            foreach (var tuple in bindInfoList)
            {
                try
                {
                    // Delegate 생성
                    var createdDelegate = Delegate.CreateDelegate(tuple.Item2, this, tuple.Item3);

                    // 이벤트 등록
                    if (isRegister)
                        RegisterUIEvent(tuple.Item1, createdDelegate);
                    else
                        UnregisterUIEvent(tuple.Item1, createdDelegate);
                }
                catch(Exception e)
                {
                    if (e is TargetParameterCountException)
                    {
                        Debug.LogError("바인딩 키값이 적용된 함수의 함수 파라미터 인자 개수가 잘못되었습니다. =>" + tuple.Item3.Name +"," + tuple.Item2.ToString());
                    }
                    else
                        Debug.LogError(e);
                    continue;
                }
            }
        }
        
        protected void RegisterUIEvent(string key, Delegate action)
        {
            foreach (var binder in _binders)
                (binder as IUIEventBinder)?.AddListener(key, action);
        }
        
        protected void UnregisterUIEvent(string key, Delegate action)
        {
            foreach (var binder in _binders)
                (binder as IUIEventBinder)?.RemoveListener(key, action);
        }

        /// <summary>
        /// 키와 연결된 Ui의 메소드를 호출합니다.
        /// </summary>
        /// <param name="key">Ui에 연결된 키</param>
        /// <param name="param">파라미터의 값</param>
        // BaseUi -> UI Component (ex: GameObject.SetActive(bool))
        public void BroadcastInvokeMethod<T>(string key, T param)
        {
            foreach (var binder in _binders)
                (binder as ICommandBinder<T>)?.InvokeMethod(key, param);
        }

        /// <summary>
        /// 바인더를 이 Ui의 리스너로 등록합니다.
        /// </summary>
        /// <param name="dataBinder">등록할 바인더</param>
        public void RegisterBinder(IBinder dataBinder)
        {
            _binders.Add(dataBinder);
        }

        /// <summary>
        /// 하위에 있는 Binder 요소를 탐색한 후 바인딩한다.
        /// 동적으로 생성되는 객체의 경우 따로 호출해줘야 정상적으로 Binder들이 등록됩니다.
        /// </summary>
        public void SearchBinders()
        { 
            _binders.Clear();
            var binders = transform.GetComponentsInChildren<IBinder>(true);

            
            foreach (var binder in binders)
                binder.Bind(); // 바인딩

            IsInitializedBinder = true;
        }

        /// <summary>
        /// UI 이벤트의 콜백을 등록하기 위한 메소드
        /// </summary>
        public void RegisterUIEvents()
        {
            AutoRegistUIEvents();
        }

        /// <summary>
        /// UI 이벤트의 콜백을 등록해제하기 위한 메소드
        /// </summary>
        public void UnRegisterUIEvents()
        {
            AutoRegistUIEvents(false);
        }
    }
}