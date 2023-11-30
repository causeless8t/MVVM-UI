using System.Collections.Generic;
using System.Globalization;
using Causeless3t.UI.MVVM;
using UnityEngine;

public class Sample : MonoBehaviour
{
    private UIView _view;
    private SampleViewModel _viewModel;
    private List<SampleItemViewModel> _itemList = new();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 200; i++)
        {
            var item = new SampleItemViewModel(i)
            {
                SampleText = i.ToString()
            };
            _itemList.Add(item);
        }
        _view = UIViewManager.Instance.PushView("Sample", view =>
        {
            view.ViewModel = ViewModelManager.Instance.GetViewModel(typeof(SampleViewModel));
            _viewModel = view.ViewModel as SampleViewModel;
            _viewModel!.SampleCollectionViewModel.AddItems(_itemList);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (_viewModel == null) return;
        var time = Time.realtimeSinceStartup;
        _viewModel!.SampleText = time.ToString(CultureInfo.InvariantCulture);
        _viewModel.SampleToggle = (int)time % 2 == 0;
    }
}
