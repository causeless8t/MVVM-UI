using System;
using System.Collections.Generic;
using Causeless3t.Core;

namespace Causeless3t.UI.MVVM
{
    public class ViewModelManager : Singleton<ViewModelManager>
    {
        private static readonly Dictionary<Type, BaseViewModel> ViewModelDictionary = new();

        public BaseViewModel GetViewModel(Type key)
        {
            if (!ViewModelDictionary.TryGetValue(key, out var viewModel))
            {
                viewModel = (BaseViewModel)Activator.CreateInstance(key);
                ViewModelDictionary.Add(key, viewModel);
            }
            return viewModel;
        }
    }
}

