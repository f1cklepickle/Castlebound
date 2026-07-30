using NUnit.Framework;
using UnityEditor;

namespace Castlebound.Tests.Scale
{
    public class MobilePresentationSettingsTests
    {
        [Test]
        public void PlayerSettings_AllowOnlyLandscapeAutorotation()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.IsFalse(PlayerSettings.allowedAutorotateToPortrait);
            Assert.IsFalse(PlayerSettings.allowedAutorotateToPortraitUpsideDown);
            Assert.IsTrue(PlayerSettings.allowedAutorotateToLandscapeLeft);
            Assert.IsTrue(PlayerSettings.allowedAutorotateToLandscapeRight);
        }
    }
}
