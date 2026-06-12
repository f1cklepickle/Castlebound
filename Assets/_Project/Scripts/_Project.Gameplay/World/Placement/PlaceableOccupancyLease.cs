using Castlebound.Gameplay.Castle;
using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    public class PlaceableOccupancyLease : MonoBehaviour
    {
        private CastleOccupancyMap occupancy;
        private Vector2 snappedWorldPosition;
        private GridFootprint footprint;
        private bool configured;

        public void Configure(CastleOccupancyMap occupancyMap, Vector2 position, GridFootprint gridFootprint)
        {
            occupancy = occupancyMap;
            snappedWorldPosition = position;
            footprint = gridFootprint;
            configured = occupancy != null && footprint.IsValid;
        }

        private void OnDestroy()
        {
            ReleaseNow();
        }

        public void ReleaseNow()
        {
            if (!configured)
            {
                return;
            }

            occupancy.Release(snappedWorldPosition, footprint);
            configured = false;
        }
    }
}
