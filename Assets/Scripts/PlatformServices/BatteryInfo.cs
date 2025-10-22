// Assets/Scripts/PlatformServices/BatteryInfo.cs
using UnityEngine;

namespace PSA.PlatformServices
{
    /// <summary>Immutable value type to avoid GC pressure.</summary>
    public readonly struct BatteryInfo
    {
        public int Percentage { get; }
        public bool IsCharging { get; }

        public BatteryInfo(int percentage, bool isCharging)
        {
            Percentage = Mathf.Clamp(percentage, 0, 100);
            IsCharging = isCharging;
        }

        public override string ToString() => $"{Percentage}% {(IsCharging ? "charging" : "not charging")}";
    }
}
