using System.Collections.Generic;

namespace Causeless3t.UI.MVVM
{
    public class BaseViewModel : IBindableProperty
    {
        private readonly List<string> _syncLockFlagList = new();

        #region IBindableProperty

        public void SetPropertyLockFlag(string key)
        {
            if (_syncLockFlagList.Contains(key)) return;
            _syncLockFlagList.Add(key);
        }

        public virtual string GetBindKey(string propertyName) => $"{GetType().FullName}/{propertyName}";

        public void SyncValue(string key, object value)
        {
            if (_syncLockFlagList.Contains(key))
                _syncLockFlagList.Remove(key);
            else
                BinderManager.Instance.BroadcastValue(key, value);
        }

        #endregion IBindableProperty
    }
}
