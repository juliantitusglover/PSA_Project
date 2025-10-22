// Assets/Scripts/PlatformServices/IPlatformServices.cs
using System.Threading;
using System.Threading.Tasks;

namespace PSA.PlatformServices
{
    public interface IPlatformServices
    {
        void SetClipboardText(string text);
        string GetClipboardText();

        void Vibrate(VibrationType type);

        Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default);
    }
}
