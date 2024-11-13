
using System;

namespace Causeless3t.UI
{
    public interface IBinder
    {
        void Bind();
    }
 
    public interface IDataBinder<T> 
    {
        void SetProperty(string key, T value);
        T GetProperty(string key);
        bool HasKey(string key);
    }
    
    public interface IUIEventBinder
    {
        void AddListener(string key, Delegate action);
        void RemoveListener(string key, Delegate action);
    }
    
    public interface ICommandBinder<T>
    {
        void InvokeMethod(string key, T target);
    }

    public interface IBinderManager
    {
        void RegisterBinder(IBinder dataBinder);
        void SearchBinders();
        void RegisterUIEvents();
        void UnRegisterUIEvents();
    }
}
