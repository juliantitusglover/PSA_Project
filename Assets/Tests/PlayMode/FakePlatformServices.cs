
// Assets/Tests/PlayMode/FakePlatformServices.cs
using System.Threading;
using System.Threading.Tasks;
using PSA.PlatformServices;

namespace PSA.PlatformServices.Tests
{
    internal sealed class FakePlatformServices : IPlatformServices
    {
        public string LastClipboard { get; private set; } = string.Empty;
        public int VibrateCount { get; private set; } = 0;
        public BatteryInfo NextBattery { get; set; } = new BatteryInfo(42, true);

        public void SetClipboardText(string text) => LastClipboard = text ?? string.Empty;
        public string GetClipboardText() => LastClipboard;

        public void Vibrate(VibrationType type)
        {
            if (type != VibrationType.None) VibrateCount++;
        }

        public Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default)
            => Task.FromResult(NextBattery);
    }
}
