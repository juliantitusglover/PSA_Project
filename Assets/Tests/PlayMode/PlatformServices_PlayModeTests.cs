using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using PSA.PlatformServices;

namespace PSA.PlatformServices.Tests
{
    public class PlatformServices_PlayModeTests
    {
        [UnityTest]
        public IEnumerator Clipboard_SetAndGet_Works_Through_Factory()
        {
            var fake = new FakePlatformServices();
            PlatformServicesFactory.ResetForTests(fake);

            const string msg = "hello psa";
            fake.SetClipboardText(""); // ensure clean
            var svc = PlatformServicesFactory.GetOrCreate();
            svc.SetClipboardText(msg);

            Assert.AreEqual(msg, svc.GetClipboardText());
            yield return null;
        }

        [UnityTest]
        public IEnumerator Vibrate_Increments_Counter_In_Fake()
        {
            var fake = new FakePlatformServices();
            PlatformServicesFactory.ResetForTests(fake);

            var svc = PlatformServicesFactory.GetOrCreate();
            svc.Vibrate(VibrationType.Light);
            svc.Vibrate(VibrationType.Medium);

            Assert.AreEqual(2, fake.VibrateCount);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Battery_Returns_Configured_Value_From_Fake()
        {
            var fake = new FakePlatformServices { NextBattery = new BatteryInfo(88, false) };
            PlatformServicesFactory.ResetForTests(fake);

            var svc = PlatformServicesFactory.GetOrCreate();
            var task = svc.GetBatteryInfoAsync();
            while (!task.IsCompleted) yield return null;

            var info = task.Result;
            Assert.AreEqual(88, info.Percentage);
            Assert.IsFalse(info.IsCharging);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean out the cached service after each test
            PlatformServicesFactory.ResetForTests(null);
        }
    }
}
