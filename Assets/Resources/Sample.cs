using System.Collections.Generic;
using System.Globalization;
using Causeless3t.UI;
using Causeless3t.UI.MVVM;
using UnityEngine;

public class Sample : MonoBehaviour
{
    private UIView _view;
    private SampleViewModel _viewModel;

    private class SampleItem : ICollectionItem
    {
        public int Index { get; set; }
        public void UpdateItem(int index)
        {
            
        }
    }

    private ReusableScrollView _scrollView;
    private List<SampleItem> _itemList = new();

    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<200; i++)
            _itemList.Add(new SampleItem());
        _view = UIViewManager.Instance.PushView("Sample", view =>
        {
            view.ViewModel = ViewModelManager.Instance.GetViewModel(typeof(SampleViewModel));
            _scrollView = view.GetComponentInChildren<ReusableScrollView>();
            _scrollView.SetListData(_itemList);
        });
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
