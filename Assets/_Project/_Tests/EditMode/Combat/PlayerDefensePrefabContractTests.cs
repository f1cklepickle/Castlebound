using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerDefensePrefabContractTests
    {
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        [Test]
        public void PlayerPrefab_OwnsDefenseStateAndGuardPresentation()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var playerController = root.GetComponent<PlayerController>();
                var defenseController = root.GetComponent<PlayerDefenseController>();
                var presenter = root.GetComponent<PlayerGuardArcPresenter>();

                Assert.NotNull(playerController);
                Assert.NotNull(defenseController);
                Assert.NotNull(presenter);
                Assert.NotNull(root.GetComponent<Health>());
                Assert.That(root.tag, Is.EqualTo("Player"));
                Assert.That(root.layer, Is.EqualTo(LayerMask.NameToLayer("Player")));
                Assert.That(defenseController.BlockArcDegrees, Is.EqualTo(120f));

                var field = typeof(PlayerController).GetField(
                    "defenseController",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(field);
                Assert.That(field.GetValue(playerController), Is.SameAs(defenseController));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
