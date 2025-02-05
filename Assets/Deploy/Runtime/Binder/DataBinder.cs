using UnityEngine;

namespace Causeless3t.UI
{ 
    public abstract class DataBinder<T> : MonoBehaviour, IBinder, IDataBinder<T> where T : class
    {
        [SerializeField][Tooltip("기본 컴포넌트 Getter의 키")]
        protected string getterKey;
        
        /// <summary>
        /// 연결할 컴포넌트
        /// </summary>
        protected T Target;
        
        public void Bind()
        {
            var parents = transform.GetComponentsInParent<IBinderManager>(true);
            foreach (IBinderManager parent in parents)
            {
                if (parent == null) continue;
                parent.RegisterBinder(this); 
            }
        }

        protected virtual void Awake()
        {
            Target = GetComponent<T>();
        }

        protected virtual void OnEnable()
        {
            LoadData();
        }

        protected virtual void OnDestroy()
        {
            Target = null;
        }

        /// <summary>
        /// 컴포넌트에 Serialize된 정보를 내부 Dictionary로 옮겨담습니다.
        /// </summary>
        protected virtual void LoadData() { }
        
        public virtual string[] GetKeyList() { return null; }

        public void SetProperty(string key, T value)
        {
            // Only Getter
        }

        /// <summary>
        /// 기본 컴포넌트의 Getter
        /// </summary>
        /// <param name="key">기본 Getter의 키</param>
        /// <returns>컴포넌트</returns>
        public T GetProperty(string key)
        {
            if (string.IsNullOrEmpty(getterKey)) return default;
            if (!getterKey.Equals(key)) return default;
            if (Target == null)
            {
                if (typeof(T) == typeof(GameObject))
                    Target = gameObject as T;
                else if (typeof(T) == typeof(Transform))
                    Target = transform as T;
                else
                    Target ??= GetComponent<T>();
            }
            return Target;
        }

        public virtual bool HasKey(string key) => getterKey.Equals(key);
    }
}
