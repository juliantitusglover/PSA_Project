// Assets/Scripts/PlatformServices/AndroidPlatformServices.cs
using System.Threading;
using System.Threading.Tasks;

namespace PSA.PlatformServices
{
    internal sealed class AndroidPlatformServices : IPlatformServices
    {
        public void SetClipboardText(string text) { }
        public string GetClipboardText() => string.Empty;
        public void Vibrate(VibrationType type) { }

        public Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default)
            => Task.FromResult(new BatteryInfo(0, false));
    }
}
