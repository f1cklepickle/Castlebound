using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierTowerPlotCollection : MonoBehaviour
    {
        [SerializeField] private TowerPlot[] plots = Array.Empty<TowerPlot>();

        public IReadOnlyList<TowerPlot> Plots => plots;
        public int PlotCount => plots.Length;

        public void SetPlots(IEnumerable<TowerPlot> value)
        {
            plots = Normalize(value);
        }

        public bool Contains(TowerPlot plot)
        {
            return plot != null && plots.Contains(plot);
        }

        private void OnValidate()
        {
            plots = Normalize(plots);
        }

        private static TowerPlot[] Normalize(IEnumerable<TowerPlot> value)
        {
            if (value == null)
            {
                return Array.Empty<TowerPlot>();
            }

            return value
                .Where(plot => plot != null)
                .Distinct()
                .ToArray();
        }
    }
}
