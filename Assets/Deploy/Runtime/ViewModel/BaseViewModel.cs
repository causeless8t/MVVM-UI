
namespace Causeless3t.UI.MVVM
{
    public class BaseViewModel : IBindable
    {
        public string GetBindKey(string name) => $"{GetType().AssemblyQualifiedName}/{name}";

        public void SyncValue<T>(string key, T value)
        {
            throw new System.NotImplementedException();
        }
    }
}
