
namespace Causeless3t.UI.MVVM
{
    public interface IBindable
    {
        void SetPropertyLockFlag(string key);
        string GetBindKey(string propertyName);
        void SyncValue(string key, object value);
    }
}
