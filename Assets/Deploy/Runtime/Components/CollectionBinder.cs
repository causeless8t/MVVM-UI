using Causeless3t.Core;
using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    [RequireComponent(typeof(ReusableScrollView))]
    public sealed class CollectionBinder : MonoBehaviour, IBinder
    {
        [SerializeReference]
        private BinderInfo _sourceInfo;
        private ReusableScrollView _targetComponent;

        private bool IsInitialize => !_targetComponent.IsUnityNull();

        private void Initialize()
        {
            _targetComponent = transform.GetComponent<ReusableScrollView>();
        }

        #region IBinder

        public void Bind(BaseViewModel viewModel = null)
        {
            if (!IsInitialize) 
                Initialize();
            viewModel ??= ViewModelManager.Instance.GetViewModel(_sourceInfo.Owner);
            var collectionViewModel = (CollectionViewModel)_sourceInfo.PInfo.GetValue(viewModel);
            BinderManager.Instance.BindCollection(collectionViewModel, _targetComponent);
        }

        public void UnBind()
        {
        }

        #endregion IBinder
    }
}

