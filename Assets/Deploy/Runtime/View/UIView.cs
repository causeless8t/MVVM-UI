using System;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public class UIView : MonoBehaviour
    {
        public BaseViewModel ViewModel { get; set; }
        public string ViewName { get; set; }

        public event Action<UIView> OnOpenEvent;
        public event Action<UIView> OnCloseEvent;
        public event Action<UIView> OnPushDownEvent;
        public event Action<UIView> OnPullUpEvent;
        
        #region MonoBehaviour

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            UnBind();
        }

        #endregion MonoBehaviour

        private void Bind()
        {
            foreach (var binder in GetComponentsInChildren<DataBinder>(true))
                binder.Bind();
        }

        private void UnBind()
        {
            foreach (var binder in GetComponentsInChildren<DataBinder>(true))
                binder.UnBind();
        }

        public void OnOpen()
        {
            OnOpenEvent?.Invoke(this);
        }

        public void OnClose()
        {
            OnCloseEvent?.Invoke(this);
        }

        public void OnPushDown()
        {
            OnPushDownEvent?.Invoke(this);
        }

        public void OnPullUp()
        {
            OnPullUpEvent?.Invoke(this);
        }
    }
}
