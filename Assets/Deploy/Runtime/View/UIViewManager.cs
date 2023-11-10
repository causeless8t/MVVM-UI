using System.Collections.Generic;
using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public class UIViewManager : MonoSingleton<UIViewManager>
    {
        private static readonly string PoolTransformName = "Pool";
        
        private Canvas _rootCanvas;
        private Transform _poolTrans;
        private static readonly List<UIView> ViewList = new();
        private static readonly Dictionary<string, UIView> ViewPool = new();

        #region MonoBehaviour
        // Start is called before the first frame update
        private void Start()
        {
            _rootCanvas = GetComponent<Canvas>();
            _poolTrans = transform.Find(PoolTransformName);
            if (_poolTrans == null)
            {
                var poolRootObj = new GameObject(PoolTransformName);
                _poolTrans = poolRootObj.transform;
                _poolTrans.SetParent(transform);
            }
            
            PushView("Sample");
        }
        
        #endregion MonoBehaviour

        public void PushView(string viewName)
        {
            if (ViewList.Count > 0)
                PushDownView();
            var index = ViewList.FindIndex((t) => t.ViewName.Equals(viewName));
            if (index > 0)
            {
                var v = ViewList[index];
                ViewList.RemoveAt(index);
                ViewList.Insert(0, v);
                PullUpView();
                return;
            }
            if (!ViewPool.TryGetValue(viewName, out var view))
                view = CreateView(viewName);
            ViewList.Insert(0, view);
            view.transform.SetParent(_rootCanvas.transform);
            view!.transform.localScale = Vector3.one;
            view.gameObject.SetActive(true);
            view.OnOpen();
        }

        private UIView CreateView(string viewName)
        {
            // temp
            var viewObject = Instantiate(Resources.Load($"Prefab/{viewName}", typeof(GameObject))) as GameObject;
            if (viewObject.IsUnityNull()) return null;
            var view = viewObject!.GetComponent<UIView>();
            view.ViewName = viewName;
            return view;
        }

        public void PopView()
        {
            if (ViewList.Count == 0) return;
            PopView(0);
        }

        public void PopView(string viewName)
        {
            var index = ViewList.FindIndex((t) => t.ViewName.Equals(viewName));
            if (index == -1) return;
            PopView(index);
        }

        private void PopView(int index)
        {
            var view = ViewList[index];
            view.OnClose();
            view.gameObject.SetActive(false);
            view.transform.SetParent(_poolTrans);
            ViewPool.Add(view.ViewName, view);
            ViewList.RemoveAt(index);
            if (ViewList.Count > 0)
                PullUpView();
        }

        public void ClearView() => ViewList.ForEach(_ => PopView());
        private void PullUpView() => ViewList[0].OnPullUp();
        private void PushDownView() => ViewList[0].OnPushDown();
    }
}
