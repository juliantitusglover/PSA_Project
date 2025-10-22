// Assets/Scripts/PlatformServices/PlatformServicesFactory.cs
namespace PSA.PlatformServices
{
    /// <summary>
    /// Single entry point for consumers. Uses platform-dependent compilation.
    /// Cache the instance to avoid per-frame allocations.
    /// </summary>
    public static class PlatformServicesFactory
    {
        private static IPlatformServices _cached;

        public static IPlatformServices GetOrCreate()
        {
            if (_cached != null) return _cached;

#if UNITY_ANDROID && !UNITY_EDITOR
            _cached = new AndroidPlatformServices();
#elif UNITY_IOS && !UNITY_EDITOR
            _cached = new IosPlatformServices();
#else
            _cached = new DesktopPlatformServices();
#endif
            return _cached;
        }

        /// <summary>For tests: inject a fake or reset to null.</summary>
        public static void ResetForTests(IPlatformServices fake = null) => _cached = fake;
    }
}
