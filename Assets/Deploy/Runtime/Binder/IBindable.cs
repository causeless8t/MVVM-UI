
namespace Causeless3t.UI.MVVM
{
    public interface IBindable
    {
        string GetBindKey(string name);
        void SyncValue<T>(string key, T value);
    }
}
