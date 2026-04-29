using System.Linq;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class TowerPlotContractTests
    {
        [Test]
        public void TowerPlot_DefaultsToOwnTransformAnchor_AndStartsEmpty()
        {
            var plotRoot = new GameObject("TowerPlot");

            try
            {
                var plot = plotRoot.AddComponent<TowerPlot>();

                Assert.AreSame(plotRoot.transform, plot.Anchor, "TowerPlot should default its anchor to the plot transform.");
                Assert.IsFalse(plot.IsOccupied, "TowerPlot should start empty.");
                Assert.IsNull(plot.OccupantInstance, "TowerPlot should not start with an occupant.");
            }
            finally
            {
                Object.DestroyImmediate(plotRoot);
            }
        }

        [Test]
        public void TowerPlot_TracksOccupantLifecycle_WithoutAllowingDuplicateAssignment()
        {
            var plotRoot = new GameObject("TowerPlot");
            var firstTower = new GameObject("Tower_A");
            var secondTower = new GameObject("Tower_B");

            try
            {
                var plot = plotRoot.AddComponent<TowerPlot>();

                Assert.IsTrue(plot.TryAssignOccupant(firstTower), "First occupant assignment should succeed.");
                Assert.IsTrue(plot.IsOccupied, "TowerPlot should report occupied after assignment.");
                Assert.AreSame(firstTower, plot.OccupantInstance, "TowerPlot should track the assigned occupant.");

                Assert.IsFalse(plot.TryAssignOccupant(secondTower), "TowerPlot should reject assigning a second occupant while occupied.");
                Assert.AreSame(firstTower, plot.OccupantInstance, "Rejected assignment should not replace the current occupant.");

                plot.ClearOccupant(secondTower);
                Assert.AreSame(firstTower, plot.OccupantInstance, "Clearing with the wrong occupant should not empty the plot.");

                plot.ClearOccupant(firstTower);
                Assert.IsFalse(plot.IsOccupied, "TowerPlot should become empty after clearing the assigned occupant.");
                Assert.IsNull(plot.OccupantInstance, "TowerPlot should no longer track an occupant after clearing.");
            }
            finally
            {
                Object.DestroyImmediate(secondTower);
                Object.DestroyImmediate(firstTower);
                Object.DestroyImmediate(plotRoot);
            }
        }

        [Test]
        public void BarrierTowerPlotCollection_ExposesReusablePlotCollection_WithoutFixedSlotFields()
        {
            var barrier = new GameObject("Barrier");
            var plotA = new GameObject("Plot_A").AddComponent<TowerPlot>();
            var plotB = new GameObject("Plot_B").AddComponent<TowerPlot>();
            var plotC = new GameObject("Plot_C").AddComponent<TowerPlot>();

            try
            {
                var collection = barrier.AddComponent<BarrierTowerPlotCollection>();
                collection.SetPlots(new[] { plotA, plotB, plotC });

                Assert.That(collection.PlotCount, Is.EqualTo(3), "Barrier plot collection should support more than two plots at the contract level.");
                CollectionAssert.AreEquivalent(new[] { plotA, plotB, plotC }, collection.Plots.ToArray(), "Barrier plot collection should expose all configured plots.");
                Assert.IsTrue(collection.Contains(plotA), "Barrier plot collection should expose plots by membership rather than fixed slot names.");
                Assert.IsTrue(collection.Contains(plotB), "Barrier plot collection should expose plots by membership rather than fixed slot names.");
                Assert.IsTrue(collection.Contains(plotC), "Barrier plot collection should expose plots by membership rather than fixed slot names.");
            }
            finally
            {
                Object.DestroyImmediate(plotC.gameObject);
                Object.DestroyImmediate(plotB.gameObject);
                Object.DestroyImmediate(plotA.gameObject);
                Object.DestroyImmediate(barrier);
            }
        }

        [Test]
        public void BarrierTowerPlotCollection_NormalizesNullAndDuplicatePlots_ForPlugAndPlayAuthoring()
        {
            var barrier = new GameObject("Barrier");
            var plotA = new GameObject("Plot_A").AddComponent<TowerPlot>();
            var plotB = new GameObject("Plot_B").AddComponent<TowerPlot>();

            try
            {
                var collection = barrier.AddComponent<BarrierTowerPlotCollection>();
                collection.SetPlots(new TowerPlot[] { plotA, null, plotA, plotB });

                Assert.That(collection.PlotCount, Is.EqualTo(2), "Barrier plot collection should ignore null and duplicate plot assignments.");
                CollectionAssert.AreEquivalent(new[] { plotA, plotB }, collection.Plots.ToArray(), "Barrier plot collection should keep only unique valid plots.");
            }
            finally
            {
                Object.DestroyImmediate(plotB.gameObject);
                Object.DestroyImmediate(plotA.gameObject);
                Object.DestroyImmediate(barrier);
            }
        }
    }
}
