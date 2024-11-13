
using Causeless3t.UI;
using UnityEngine;

namespace Causeless3t.Sample
{
    public struct SampleItemModel : ICollectionItemData
    {
        public int Index;
    }

    public sealed class SampleItemViewModel : BaseUI, ICollectionItemUI
    {
        private SampleItemModel _model;

        public string SampleItemText
        {
            set => BroadcastSetProperty(nameof(SampleItemText), value);
        }

        public int Index { get; set; }

        public RectTransform RootRectTransform { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            RootRectTransform = GetComponent<RectTransform>();
        }

        public void UpdateItem(int index, ICollectionItemData data)
        {
            Index = index;
            if (data == null) return;
            _model = (SampleItemModel)data;
            SampleItemText = _model.Index.ToString();
        }
    }
}
