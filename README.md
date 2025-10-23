# Platform Services Abstraction (PSA)

A tiny cross‑platform service layer I built to show I can design clean, testable Unity APIs across Desktop/Editor and Android, iOS is stubbed. It exposes one interface for three features: Clipboard, Haptics, and Battery.

## Overview
- One interface, platform‑specific implementations picked at compile time.
- Desktop uses Unity APIs, Android uses minimal `AndroidJavaObject` bridges, iOS is a safe stub with notes.
- Small demo scene + PlayMode tests with a fake to prove the contract.

## Interface
```csharp
public interface IPlatformServices {
  void SetClipboardText(string text);
  string GetClipboardText();
  void Vibrate(VibrationType type);
  Task<BatteryInfo> GetBatteryInfoAsync(CancellationToken ct = default);
}
```

## Project Structure
```
Assets/
├─ Scenes/Demo.unity
├─ Scripts/PlatformServices/
│  ├─ IPlatformServices.cs
│  ├─ BatteryInfo.cs              
│  ├─ VibrationType.cs
│  ├─ PlatformServicesFactory.cs  
│  ├─ DesktopPlatformServices.cs  
│  ├─ AndroidPlatformServices.cs  
│  └─ IosPlatformServices.cs      
└─ Tests/PlayMode/
   ├─ FakePlatformServices.cs
   └─ PlatformServices_PlayModeTests.cs
```

## Platforms
- **Editor / Desktop**  
  - Clipboard - `GUIUtility.systemCopyBuffer`  
  - Battery - `SystemInfo.batteryLevel` (`0..1`, or `-1` when unknown) + `batteryStatus`  
  - Haptics - not implemented on Desktop (intentional)
- **Android**  
  - Clipboard - `ClipboardManager` / `ClipData.newPlainText(..)` via `AndroidJavaObject`  
  - Haptics - `Vibrator.vibrate(ms)` (API < 26) or `VibrationEffect.createOneShot(ms, amp)` (API ≥ 26), fallback `Handheld.Vibrate()`  
  - Battery - `BatteryManager.getIntProperty(BATTERY_PROPERTY_CAPACITY)`, fallback `ACTION_BATTERY_CHANGED` intent (`level/scale`) + `status`
- **iOS (stub)**  
  - Notes in code on wiring `UIPasteboard` + `UIImpactFeedbackGenerator` via a tiny native plugin

## Demo
Open `Scenes/Demo.unity` and press Play:
- Typing text and then hitting Copy puts it on the system clipboard (Desktop/Editor).
- Vibrate (Light) has no effect on Desktop, on Android it triggers short haptics.
- Read Battery shows a percentage if available, handles “unknown” gracefully.

## Tests
PlayMode tests run with a fake injected via `PlatformServicesFactory.ResetForTests(fake)`.  
They assert: clipboard round‑trip, vibrate call count, and a configured battery value.

## Design notes
- **Compile‑time selection** via `#if` keeps dead code out of builds and avoids runtime branching.
- **No per‑frame allocations:** the factory caches a single instance, Android bridges cache `AndroidJavaObject` references.
- **Small value types:** `BatteryInfo` is a `readonly struct` to keep Garbage Collection tidy.
- **Clear extension path:** add a method to the interface, fill in Desktop/Android, keep iOS documented until you add the plugin.

## Build & target
- Editor/Standalone for quick iteration.
- Android build target to exercise haptics/clipboard/battery on device.
