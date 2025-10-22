// Assets/Scripts/PlatformServices/DesktopPlatformServices.cs
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PSA.PlatformServices
{
    /// <summary>
    /// Desktop + Editor: clipboard via GUIUtility.systemCopyBuffer;
    /// vibration is a no-op; battery via SystemInfo with "unknown" handled.
    /// </summary>
    internal sealed class DesktopPlatformServices : IPlatformServices
    {
        public void SetClipboardText(string text)
        {
            GUIUtility.systemCopyBuffer = text ?? string.Empty;
        }
        public string GetClipboardText()
        {
            return GUIUtility.systemCopyBuffer ?? string.Empty;
        }
        public void Vibrate(VibrationType type) { }

        public Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default)
        {
            // SystemInfo.batteryLevel: 0..1, or -1 if unavailable (e.g., desktops).
            // SystemInfo.batteryStatus: Unknown/Charging/Discharging/NotCharging/Full.
            float level = SystemInfo.batteryLevel;
            int percentage = level < 0f ? 0 : Mathf.RoundToInt(level * 100f);

            var status = SystemInfo.batteryStatus;
            bool isCharging = status == BatteryStatus.Charging || status == BatteryStatus.Full;

            // If status is Unknown on desktop, this stays false (documented).
            var info = new BatteryInfo(percentage, isCharging);
            return Task.FromResult(info);
        }
    }
}
