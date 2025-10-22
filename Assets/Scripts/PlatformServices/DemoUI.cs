// Assets/Scripts/PlatformServices/DemoUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace PSA.PlatformServices
{
    public sealed class DemoUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private InputField clipboardInput;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button vibrateLightButton;
        [SerializeField] private Button readBatteryButton;
        [SerializeField] private Text batteryLabel;

        private IPlatformServices _services; // cached; no per-frame GetOrCreate()

        private void Awake()
        {
            _services = PlatformServicesFactory.GetOrCreate();

            // Wire once to avoid allocations every click registration in Update.
            copyButton.onClick.AddListener(OnCopyClicked);
            vibrateLightButton.onClick.AddListener(OnVibrateLightClicked);
            readBatteryButton.onClick.AddListener(OnReadBatteryClicked);
        }

        private void OnDestroy()
        {
            // Clean up listeners (good hygiene).
            copyButton.onClick.RemoveListener(OnCopyClicked);
            vibrateLightButton.onClick.RemoveListener(OnVibrateLightClicked);
            readBatteryButton.onClick.RemoveListener(OnReadBatteryClicked);
        }

        private void OnCopyClicked()
        {
            var text = clipboardInput ? clipboardInput.text : string.Empty;
            _services.SetClipboardText(text);
            // Optional UX: flash label
            if (batteryLabel) batteryLabel.text = "Copied to clipboard.";
        }

        private void OnVibrateLightClicked()
        {
            _services.Vibrate(VibrationType.Light); // no-op on Desktop, valid call on Android later
        }

        private async void OnReadBatteryClicked()
        {
            var info = await _services.GetBatteryInfoAsync();
            if (batteryLabel)
                batteryLabel.text = $"Battery: {info.Percentage}% {(info.IsCharging ? "(charging)" : "")}";
        }
    }
}
