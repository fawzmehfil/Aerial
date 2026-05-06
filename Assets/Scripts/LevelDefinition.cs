using System;
using UnityEngine;

namespace Drift
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public int LevelNumber;
        public string DisplayName;
        public string Description;
        public float ForwardSpeed;
        public Vector2 Boundary;
        public RingSpec[] Rings;
        public ObstacleSpec[] Obstacles;

        public LevelDefinition(
            int levelNumber,
            string displayName,
            string description,
            float forwardSpeed,
            Vector2 boundary,
            RingSpec[] rings,
            ObstacleSpec[] obstacles)
        {
            LevelNumber = levelNumber;
            DisplayName = displayName;
            Description = description;
            ForwardSpeed = forwardSpeed;
            Boundary = boundary;
            Rings = rings;
            Obstacles = obstacles;
        }
    }

    [Serializable]
    public struct RingSpec
    {
        public Vector3 Position;
        public float Radius;

        public RingSpec(float x, float y, float z, float radius)
        {
            Position = new Vector3(x, y, z);
            Radius = radius;
        }
    }

    [Serializable]
    public struct ObstacleSpec
    {
        public Vector3 Position;
        public Vector3 Size;

        public ObstacleSpec(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
        }
    }
}
