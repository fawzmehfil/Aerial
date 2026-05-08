using System;
using UnityEngine;

namespace Drift
{
    public enum EnvironmentTheme
    {
        MinimalLightTunnel,
        NeonCyanCorridor,
        RedGateIndustrial,
        CosmicRingVoid,
        MixedAdventure,
        AcheronAbyss
    }

    public enum PortalKind
    {
        SpeedFast,
        SpeedSlow,
        SpeedNormal,
        GravityNormal,
        GravitySideways,
        SizeSmall,
        SizeNormal,
        SizeLarge
    }

    public enum ObstacleKind
    {
        WallPanel,
        Pillar,
        CosmicDebris
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        public int LevelNumber;
        public string DisplayName;
        public string Description;
        public float ForwardSpeed;
        public Vector2 Boundary;
        public EnvironmentTheme Theme;
        public bool IsTutorial;
        public string SoundtrackPath;
        public float SoundtrackDuration;
        public bool SuppressGameplaySoundEffects;
        public RingSpec[] Rings;
        public ObstacleSpec[] Obstacles;
        public PortalSpec[] Portals;

        public LevelDefinition(
            int levelNumber,
            string displayName,
            string description,
            float forwardSpeed,
            Vector2 boundary,
            EnvironmentTheme theme,
            RingSpec[] rings,
            ObstacleSpec[] obstacles,
            PortalSpec[] portals = null,
            bool isTutorial = false,
            string soundtrackPath = null,
            float soundtrackDuration = 0f,
            bool suppressGameplaySoundEffects = false)
        {
            LevelNumber = levelNumber;
            DisplayName = displayName;
            Description = description;
            ForwardSpeed = forwardSpeed;
            Boundary = boundary;
            Theme = theme;
            IsTutorial = isTutorial;
            SoundtrackPath = soundtrackPath;
            SoundtrackDuration = soundtrackDuration;
            SuppressGameplaySoundEffects = suppressGameplaySoundEffects;
            Rings = rings;
            Obstacles = obstacles;
            Portals = portals ?? Array.Empty<PortalSpec>();
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
        public ObstacleKind Kind;
        public float RotationZ;

        public ObstacleSpec(Vector3 position, Vector3 size, ObstacleKind kind = ObstacleKind.WallPanel, float rotationZ = 0f)
        {
            Position = position;
            Size = size;
            Kind = kind;
            RotationZ = rotationZ;
        }
    }

    [Serializable]
    public struct PortalSpec
    {
        public Vector3 Position;
        public PortalKind Kind;
        public float Radius;
        public float Duration;

        public PortalSpec(float x, float y, float z, PortalKind kind, float radius = 2.25f, float duration = 4f)
        {
            Position = new Vector3(x, y, z);
            Kind = kind;
            Radius = radius;
            Duration = duration;
        }
    }
}
