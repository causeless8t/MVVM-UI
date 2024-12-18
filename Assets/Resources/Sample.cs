using System.Collections.Generic;
using System.Globalization;
using Causeless3t.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Causeless3t.Sample
{
    public sealed class Sample : BaseUI
    {
        public bool IsActive { get; }

        public bool SampleToggle
        {
            get => BroadcastGetProperty<bool>(nameof(SampleToggle));
            set => BroadcastSetProperty(nameof(SampleToggle), value);
        }

        public string SampleText
        {
            set => BroadcastSetProperty(nameof(SampleText), value);
        }

        public ReusableScrollView SampleCollectionViewModel => BroadcastGetProperty<ReusableScrollView>(nameof(SampleCollectionViewModel));

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            UniTask.Create(async () =>
            {
                await UniTask.WaitUntil(() => SampleCollectionViewModel.IsInitialized);
                for (int i = 0; i < 200; i++)
                    SampleCollectionViewModel.AddItem(new SampleItemModel() { Index = i });
            });
        }

        // Update is called once per frame
        void Update()
        {
            var time = Time.realtimeSinceStartup;
            SampleText = time.ToString(CultureInfo.InvariantCulture);
            SampleToggle = (int)time % 2 == 0;
        }

        [ButtonClickEventRegister("GoToLastIndexButton")]
        private void GoToLastIndexButton(Button target)
        {
            SampleCollectionViewModel.ScrollToPosition(1f, 1f);
        }
    }
}

