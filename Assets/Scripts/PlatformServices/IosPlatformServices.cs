// Assets/Scripts/PlatformServices/IosPlatformServices.cs
using System.Threading;
using System.Threading.Tasks;

namespace PSA.PlatformServices
{
    /// <summary>
    /// iOS will use a native plugin (UIPasteboard, UIImpactFeedbackGenerator) later.
    /// Stub prevents crashes when no plugin is present.
    /// </summary>
    internal sealed class IosPlatformServices : IPlatformServices
    {
        public void SetClipboardText(string text) { }
        public string GetClipboardText() => string.Empty;
        public void Vibrate(VibrationType type) { }

        public Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default)
            => Task.FromResult(new BatteryInfo(0, false));
    }
}
