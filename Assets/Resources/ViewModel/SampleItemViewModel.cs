
namespace Causeless3t.UI.MVVM
{
    public sealed class SampleItemViewModel : BaseViewModel, ICollectionItem
    {
        public SampleItemViewModel(int index) => Index = index;
        
        private string _sampleText;
        public string SampleText
        {
            get => _sampleText;
            set {
                _sampleText = value;
                SyncValue(GetBindKey(nameof(SampleText)), value);
            }
        }
        
        public override string GetBindKey(string propertyName) => $"{GetType().FullName}/{Index}/{propertyName}";

        public int Index { get; }

        public void UpdateItem()
        {
            SyncValue(GetBindKey(nameof(SampleText)), _sampleText);
        }
    }
}
