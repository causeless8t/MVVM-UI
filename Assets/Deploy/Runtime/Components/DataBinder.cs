using System.Text;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public sealed class DataBinder : MonoBehaviour, IBindable
    {
        public enum eObserveCycle
        {
            None = 0,
            OnEnable,
            Update,
            FixedUpdate
        }
        
        [SerializeReference]
        private BinderInfo _targetInfo;
        [SerializeField] 
        private eObserveCycle _observeCycle;
        [SerializeReference]
        private BinderInfo _sourceInfo;
        
        private Component _targetComponent;
        private object _targetValue;
        private bool _syncLockFlag = true;

        public bool IsInitialize => !_targetComponent.IsUnityNull();

        #region MonoBehaviour

        private void OnEnable()
        {
            if (!IsInitialize) 
                Initialize();
            if (_observeCycle != eObserveCycle.OnEnable) return;
            CheckChangedValue();
        }

        private void Update()
        {
            if (!IsInitialize) 
                Initialize();
            if (_observeCycle != eObserveCycle.Update) return;
            CheckChangedValue();
        }

        private void FixedUpdate()
        {
            if (!IsInitialize) 
                Initialize();
            if (_observeCycle != eObserveCycle.FixedUpdate) return;
            CheckChangedValue();
        }

        #endregion MonoBehaviour

        private void Initialize()
        {
            if (_targetInfo?.Owner == null) return;
            _targetComponent = transform.GetComponent(_targetInfo.Owner);
            _targetValue = _targetInfo.PInfo.GetValue(_targetComponent);
        }
        
        public void Bind()
        {
            if (!IsInitialize) 
                Initialize();
            var viewModel = ViewModelManager.Instance.GetViewModel(_sourceInfo.Owner);
            if (_targetInfo.Range is BinderInfo.eBindRange.GetNSet)
                BinderManager.Instance.Bind(GetBindKey(_targetInfo.PInfo.Name), viewModel, _sourceInfo.PInfo);
            BinderManager.Instance.Bind(viewModel.GetBindKey(_sourceInfo.PInfo.Name), this, _targetInfo.PInfo, _targetComponent);
        }

        public void UnBind()
        {
            if (_targetInfo?.Owner == null) return;
            BinderManager.Instance.UnBind(GetBindKey(_targetInfo.PInfo.Name));
        }

        public void CheckChangedValue()
        {
            if (_targetValue.Equals(_targetInfo.PInfo.GetValue(_targetComponent))) return;
            _targetValue = _targetInfo.PInfo.GetValue(_targetComponent);
            SyncValue(GetBindKey(_targetInfo.PInfo.Name), _targetValue);
        }

        public void SetPropertyLockFlag(string key)
        {
            if (_targetInfo.Range is BinderInfo.eBindRange.GetNSet && _observeCycle != eObserveCycle.None)
                _syncLockFlag = true;
        }

        public string GetBindKey(string propertyName) => $"{GetHierarchyFullPath()}/{_targetComponent.GetType().FullName}/{propertyName}";

        private string GetHierarchyFullPath()
        {
            StringBuilder sb = new StringBuilder(name);
            var currentTransform = transform;
            while (currentTransform.parent != null)
            {
                currentTransform = currentTransform.parent;
                sb.Insert(0, "/");
                sb.Insert(0, currentTransform.name);
            }
            return sb.ToString();
        }
        
        public void SyncValue(string key, object value)
        {
            if (_syncLockFlag)
                _syncLockFlag = false;
            else
                BinderManager.Instance.BroadcastValue(key, value);
        }
    }
}
