using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public readonly struct GridFootprint : IEquatable<GridFootprint>
    {
        public GridFootprint(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Footprint width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Footprint height must be positive.");
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsValid => Width > 0 && Height > 0;

        public static GridFootprint OneByOne => new GridFootprint(1, 1);

        public static GridFootprint ThreeByThree => new GridFootprint(3, 3);

        public IEnumerable<Vector2Int> EnumerateCells(Vector2Int origin)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot enumerate an invalid grid footprint.");
            }

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    yield return new Vector2Int(origin.x + x, origin.y + y);
                }
            }
        }

        public bool Equals(GridFootprint other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is GridFootprint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }
    }
}
