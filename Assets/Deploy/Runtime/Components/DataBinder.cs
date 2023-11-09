using UnityEngine;

namespace Causeless3t.UI.MVVM
{
    public sealed class DataBinder : MonoBehaviour
    {
        [SerializeReference]
        private BinderInfo _targetInfo;
        [SerializeReference]
        private BinderInfo _sourceInfo;
    }
}
