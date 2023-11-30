
namespace Causeless3t.UI.MVVM
{
    public interface IBindableProperty
    {
        void SetPropertyLockFlag(string key);
        string GetBindKey(string propertyName);
        void SyncValue(string key, object value);
    }

    public interface IBindableMethod
    {
        string GetBindKey(string methodName);
        void InvokeMethod(string key, params object[] param);
    }

    public interface IBindableCollection
    {
        string BindKey { get; }
    }
}
