
namespace Causeless3t.UI.MVVM
{
    public sealed class SampleViewModel : BaseViewModel
    {
        public bool IsActive { get; }

        private bool _sampleToggle;
        public bool SampleToggle
        {
            get => _sampleToggle;
            set {
                _sampleToggle = value;
                SyncValue(GetBindKey(nameof(SampleToggle)), value);
            }
        }

        private string _sampleText;
        public string SampleText
        {
            get => _sampleText;
            set {
                _sampleText = value;
                SyncValue(GetBindKey(nameof(SampleText)), value);
            }
        }

        private float _sampleFloat;
        public float SampleFloat
        {
            get => _sampleFloat;
            set {
                _sampleFloat = value;
                SyncValue(GetBindKey(nameof(SampleFloat)), value);
            }
        }
    }
}
