using System.Globalization;
using Causeless3t.UI.MVVM;
using UnityEngine;

public class Sample : MonoBehaviour
{
    private UIView _view;
    private SampleViewModel _viewModel;
    
    // Start is called before the first frame update
    void Start()
    {
        _view = UIViewManager.Instance.PushView("Sample", view => view.ViewModel = ViewModelManager.Instance.GetViewModel(typeof(SampleViewModel)));
    }

    // Update is called once per frame
    void Update()
    {
        _viewModel ??= _view.ViewModel as SampleViewModel;
        var time = Time.realtimeSinceStartup;
        _viewModel!.SampleText = time.ToString(CultureInfo.InvariantCulture);
        _viewModel.SampleToggle = (int)time % 2 == 0;
    }
}
