// Assets/Scripts/PlatformServices/AndroidPlatformServices.cs
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PSA.PlatformServices
{
    /// <summary>
    /// Android implementation via AndroidJavaObject (no .aar needed).
    /// - Clipboard: android.content.ClipboardManager + ClipData
    /// - Haptics: android.os.Vibrator (+ VibrationEffect on API >= 26)
    /// - Battery: BatteryManager.getIntProperty(BATTERY_PROPERTY_CAPACITY) with ACTION_BATTERY_CHANGED fallback
    /// </summary>
    internal sealed class AndroidPlatformServices : IPlatformServices
    {
        private readonly AndroidJavaObject _activity;
        private readonly AndroidJavaObject _clipboard;
        private readonly AndroidJavaObject _vibrator;
        private readonly AndroidJavaObject _batteryMgr;

        private readonly int _sdkInt;
        private readonly int _BATTERY_PROPERTY_CAPACITY;
        private readonly int _BATTERY_STATUS_CHARGING;
        private readonly int _BATTERY_STATUS_FULL;

        public AndroidPlatformServices()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            _clipboard = _activity.Call<AndroidJavaObject>("getSystemService", "clipboard");
            _vibrator  = _activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            _batteryMgr = _activity.Call<AndroidJavaObject>("getSystemService", "batterymanager");

            var version = new AndroidJavaClass("android.os.Build$VERSION");
            _sdkInt = version.GetStatic<int>("SDK_INT");

            var bm = new AndroidJavaClass("android.os.BatteryManager");
            _BATTERY_PROPERTY_CAPACITY = bm.GetStatic<int>("BATTERY_PROPERTY_CAPACITY");
            _BATTERY_STATUS_CHARGING   = bm.GetStatic<int>("BATTERY_STATUS_CHARGING");
            _BATTERY_STATUS_FULL       = bm.GetStatic<int>("BATTERY_STATUS_FULL");
        }

        public void SetClipboardText(string text)
        {
            try
            {
                string safe = text ?? string.Empty;
                var clipDataClass = new AndroidJavaClass("android.content.ClipData");
                var clip = clipDataClass.CallStatic<AndroidJavaObject>("newPlainText", "PSA", safe);
                _clipboard?.Call("setPrimaryClip", clip);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PSA] SetClipboardText failed: {e.Message}");
            }
        }

        public string GetClipboardText()
        {
            try
            {
                if (_clipboard == null) return string.Empty;
                bool has = _clipboard.Call<bool>("hasPrimaryClip");
                if (!has) return string.Empty;

                var clip = _clipboard.Call<AndroidJavaObject>("getPrimaryClip");
                if (clip == null) return string.Empty;

                var item = clip.Call<AndroidJavaObject>("getItemAt", 0);
                var text = item?.Call<AndroidJavaObject>("getText");
                return text?.Call<string>("toString") ?? string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PSA] GetClipboardText failed: {e.Message}");
                return string.Empty;
            }
        }

        public void Vibrate(VibrationType type)
        {
            try
            {
                if (_vibrator == null || type == VibrationType.None)
                    return;

                long durationMs;
                int amplitude;
                switch (type)
                {
                    case VibrationType.Light:  durationMs = 20; amplitude = 64;  break;
                    case VibrationType.Medium: durationMs = 40; amplitude = 160; break;
                    case VibrationType.Heavy:  durationMs = 80; amplitude = 255; break;
                    default: return;
                }

                if (_sdkInt >= 26)
                {
                    var veClass = new AndroidJavaClass("android.os.VibrationEffect");
                    const int DEFAULT_AMPLITUDE = -1;
                    var effect = veClass.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        durationMs,
                        amplitude
                    );
                    _vibrator.Call("vibrate", effect);
                }
                else
                {
                    _vibrator.Call("vibrate", durationMs);
                }
            }
            catch (Exception e)
            {
                try { Handheld.Vibrate(); } catch { /* ignore */ }
                Debug.LogWarning($"[PSA] Vibrate failed; used fallback Handheld.Vibrate(). Reason: {e.Message}");
            }
        }

        public Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default)
        {
            int pct = TryGetBatteryPercentByProperty();
            bool charging = TryGetIsChargingIntent();

            if (pct <= 0)
            {
                var intentPct = TryGetBatteryPercentByIntent();
                if (intentPct >= 0) pct = intentPct;
            }

            pct = Mathf.Clamp(pct, 0, 100);
            return Task.FromResult(new BatteryInfo(pct, charging));
        }

        private int TryGetBatteryPercentByProperty()
        {
            try
            {
                if (_batteryMgr == null) return -1;
                int value = _batteryMgr.Call<int>("getIntProperty", _BATTERY_PROPERTY_CAPACITY);
                return value;
            }
            catch { return -1; }
        }

        private int TryGetBatteryPercentByIntent()
        {
            try
            {
                var intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");
                var intent = _activity.Call<AndroidJavaObject>("registerReceiver", null, intentFilter);
                if (intent == null) return -1;

                int level = intent.Call<int>("getIntExtra", "level", -1);
                int scale = intent.Call<int>("getIntExtra", "scale", -1);
                if (level < 0 || scale <= 0) return -1;

                return Mathf.RoundToInt((level / (float)scale) * 100f);
            }
            catch { return -1; }
        }

        private bool TryGetIsChargingIntent()
        {
            try
            {
                var intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");
                var intent = _activity.Call<AndroidJavaObject>("registerReceiver", null, intentFilter);
                if (intent == null) return false;

                int status = intent.Call<int>("getIntExtra", "status", -1);
                return status == _BATTERY_STATUS_CHARGING || status == _BATTERY_STATUS_FULL;
            }
            catch { return false; }
        }
    }
}
#endif
