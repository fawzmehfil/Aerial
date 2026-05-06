using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public static class LevelCatalog
    {
        public static IReadOnlyList<LevelDefinition> GetLevels()
        {
            return Levels;
        }

        private static readonly LevelDefinition[] Levels =
        {
            new LevelDefinition(
                1,
                "Tutorial - Launch Sequence",
                "Safe onboarding for movement, highlighted rings, burst controls, portals, restart, and pause.",
                16f,
                new Vector2(8.5f, 5.5f),
                EnvironmentTheme.MinimalLightTunnel,
                new[]
                {
                    new RingSpec(0f, 0f, 18f, 3.25f),
                    new RingSpec(0f, 1.2f, 36f, 3.15f),
                    new RingSpec(-2.4f, 1f, 56f, 3f),
                    new RingSpec(2.5f, 1f, 76f, 3f),
                    new RingSpec(0f, 2.8f, 98f, 2.8f),
                    new RingSpec(0f, -2.4f, 120f, 2.8f),
                    new RingSpec(0f, 0f, 144f, 3f),
                    new RingSpec(-2f, 0.8f, 168f, 2.55f),
                    new RingSpec(2f, -0.8f, 192f, 2.55f),
                    new RingSpec(0f, 0f, 218f, 3.2f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(-5.8f, 0f, 74f), new Vector3(0.8f, 5.8f, 4f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(5.8f, 0f, 94f), new Vector3(0.8f, 5.8f, 4f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(0f, 4.4f, 120f), new Vector3(10f, 0.8f, 4f), ObstacleKind.WallPanel)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 132f, PortalKind.SpeedSlow, 2.5f, 3f),
                    new PortalSpec(0f, 0f, 156f, PortalKind.SizeSmall, 2.3f, 5f),
                    new PortalSpec(0f, 0f, 210f, PortalKind.SpeedNormal, 2.35f, 0f),
                    new PortalSpec(0f, 0f, 214f, PortalKind.SizeNormal, 2.35f, 0f)
                },
                true),

            new LevelDefinition(
                2,
                "Lightline Atrium",
                "Minimal concrete tunnel with white rectangular light frames and large readable gates.",
                18f,
                new Vector2(8f, 5f),
                EnvironmentTheme.MinimalLightTunnel,
                new[]
                {
                    new RingSpec(0f, 0f, 22f, 2.9f),
                    new RingSpec(0f, 0.4f, 44f, 2.9f),
                    new RingSpec(1.2f, 0.7f, 66f, 2.8f),
                    new RingSpec(-1.1f, 0.9f, 88f, 2.8f),
                    new RingSpec(0f, 1.2f, 110f, 2.8f),
                    new RingSpec(1.4f, 0.5f, 132f, 2.7f),
                    new RingSpec(-1.4f, -0.2f, 154f, 2.7f),
                    new RingSpec(0f, -0.5f, 176f, 2.7f),
                    new RingSpec(0.8f, 0.2f, 198f, 2.7f),
                    new RingSpec(0f, 0f, 220f, 3f)
                },
                new ObstacleSpec[0]),

            new LevelDefinition(
                3,
                "Cyan Velocity Hall",
                "A smooth neon corridor with cinematic speed and slow-mode portal beats.",
                21f,
                new Vector2(8.5f, 5.5f),
                EnvironmentTheme.NeonCyanCorridor,
                new[]
                {
                    new RingSpec(0f, 0f, 20f, 2.65f),
                    new RingSpec(-2.2f, 0.8f, 40f, 2.55f),
                    new RingSpec(2.2f, 1.5f, 60f, 2.55f),
                    new RingSpec(-2.7f, 0.4f, 80f, 2.5f),
                    new RingSpec(2.7f, -0.9f, 100f, 2.5f),
                    new RingSpec(-1.6f, -1.7f, 120f, 2.45f),
                    new RingSpec(1.6f, -0.4f, 140f, 2.45f),
                    new RingSpec(0f, 1.6f, 160f, 2.45f),
                    new RingSpec(-2.4f, 0f, 180f, 2.4f),
                    new RingSpec(2.4f, -1.2f, 200f, 2.4f),
                    new RingSpec(0f, 0.4f, 222f, 2.6f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(-5.7f, -0.3f, 86f), new Vector3(1.1f, 6.2f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(5.7f, 0.3f, 126f), new Vector3(1.1f, 6.2f, 5f), ObstacleKind.Pillar)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 48f, PortalKind.SpeedFast, 2.2f, 4f),
                    new PortalSpec(0f, 0f, 116f, PortalKind.SpeedSlow, 2.2f, 4f),
                    new PortalSpec(0f, 0f, 188f, PortalKind.SpeedNormal, 2.2f, 0f)
                }),

            new LevelDefinition(
                4,
                "Red Gate Foundry",
                "Industrial red-light frames with strict but visually aligned wall-panel gaps.",
                23f,
                new Vector2(8.5f, 5.8f),
                EnvironmentTheme.RedGateIndustrial,
                new[]
                {
                    new RingSpec(0f, 0f, 20f, 2.35f),
                    new RingSpec(-2.8f, 1.2f, 39f, 2.2f),
                    new RingSpec(2.6f, 0.8f, 58f, 2.2f),
                    new RingSpec(0.2f, -1.9f, 77f, 2.15f),
                    new RingSpec(-3.2f, -0.7f, 96f, 2.1f),
                    new RingSpec(3.1f, 1.7f, 115f, 2.1f),
                    new RingSpec(1f, 2.7f, 134f, 2.05f),
                    new RingSpec(-2.5f, 1.2f, 153f, 2.05f),
                    new RingSpec(2.8f, -1.4f, 172f, 2f),
                    new RingSpec(0f, -2.4f, 191f, 2f),
                    new RingSpec(-1.8f, 0.6f, 212f, 2.05f),
                    new RingSpec(0f, 0f, 235f, 2.45f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(0f, -4.4f, 58f), new Vector3(12f, 1.1f, 4f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(-5.7f, 0.4f, 78f), new Vector3(1.3f, 7f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(5.7f, 0.6f, 97f), new Vector3(1.3f, 7f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(0f, 4.6f, 119f), new Vector3(12f, 1.2f, 5f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(-5.8f, -0.8f, 153f), new Vector3(1.4f, 7f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(5.8f, -0.4f, 173f), new Vector3(1.4f, 7f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(0f, 4.4f, 193f), new Vector3(12f, 1.2f, 4f), ObstacleKind.WallPanel)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 126f, PortalKind.SizeSmall, 2.1f, 6f),
                    new PortalSpec(0f, 0f, 206f, PortalKind.SizeNormal, 2.2f, 0f)
                }),

            new LevelDefinition(
                5,
                "Cosmic Ring Void",
                "Open starfield ring tunnel over alien silhouettes with debris hazards.",
                22f,
                new Vector2(9.5f, 6.3f),
                EnvironmentTheme.CosmicRingVoid,
                new[]
                {
                    new RingSpec(0f, -1.2f, 22f, 2.6f),
                    new RingSpec(1.6f, -0.2f, 42f, 2.5f),
                    new RingSpec(3.2f, 1.1f, 62f, 2.45f),
                    new RingSpec(1.4f, 2.4f, 84f, 2.35f),
                    new RingSpec(-0.6f, 1.5f, 106f, 2.35f),
                    new RingSpec(-2.8f, 0f, 128f, 2.25f),
                    new RingSpec(-3.4f, -1.5f, 150f, 2.2f),
                    new RingSpec(-1.3f, -2.3f, 172f, 2.15f),
                    new RingSpec(1.4f, -1.2f, 194f, 2.15f),
                    new RingSpec(3f, 0.7f, 216f, 2.1f),
                    new RingSpec(0.8f, 2.4f, 238f, 2.15f),
                    new RingSpec(0f, 0f, 262f, 2.55f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(-4.8f, 2f, 70f), new Vector3(1.4f, 1.8f, 1.2f), ObstacleKind.CosmicDebris, 24f),
                    new ObstacleSpec(new Vector3(4.6f, -1.8f, 116f), new Vector3(1.8f, 1.2f, 1.4f), ObstacleKind.CosmicDebris, 73f),
                    new ObstacleSpec(new Vector3(-4.2f, -2.7f, 164f), new Vector3(1.6f, 1.3f, 1.7f), ObstacleKind.CosmicDebris, 132f),
                    new ObstacleSpec(new Vector3(4.8f, 2.5f, 212f), new Vector3(1.5f, 1.7f, 1.2f), ObstacleKind.CosmicDebris, 211f)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 98f, PortalKind.SpeedSlow, 2.35f, 4f),
                    new PortalSpec(0f, 0f, 186f, PortalKind.SpeedNormal, 2.35f, 0f)
                }),

            new LevelDefinition(
                6,
                "Sideways Spiral",
                "A roll-orientation corridor that uses sideways gravity sparingly for a space-tunnel set piece.",
                24f,
                new Vector2(9f, 6f),
                EnvironmentTheme.NeonCyanCorridor,
                new[]
                {
                    new RingSpec(0f, 0f, 20f, 2.35f),
                    new RingSpec(2f, 1.3f, 40f, 2.2f),
                    new RingSpec(3.4f, -0.4f, 60f, 2.15f),
                    new RingSpec(1.8f, -2.2f, 82f, 2.1f),
                    new RingSpec(-0.4f, -2.8f, 104f, 2.05f),
                    new RingSpec(-2.8f, -1f, 126f, 2.05f),
                    new RingSpec(-3.2f, 1.4f, 148f, 2f),
                    new RingSpec(-0.8f, 2.8f, 170f, 2f),
                    new RingSpec(2.5f, 2.1f, 192f, 1.95f),
                    new RingSpec(3.3f, -0.8f, 214f, 1.95f),
                    new RingSpec(0f, -2.8f, 236f, 2.05f),
                    new RingSpec(0f, 0f, 260f, 2.5f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(0f, 4.6f, 84f), new Vector3(12f, 0.9f, 5f), ObstacleKind.WallPanel, 18f),
                    new ObstacleSpec(new Vector3(-5.8f, 0f, 130f), new Vector3(1.1f, 7.5f, 5f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(5.8f, 0f, 196f), new Vector3(1.1f, 7.5f, 5f), ObstacleKind.Pillar)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 72f, PortalKind.GravitySideways, 2.35f, 0f),
                    new PortalSpec(0f, 0f, 178f, PortalKind.GravityNormal, 2.35f, 0f)
                }),

            new LevelDefinition(
                7,
                "Aerial Expedition",
                "Long mixed adventure course: corridor, void, portal chain, speed shifts, size changes, and final red tunnel.",
                25f,
                new Vector2(9f, 6f),
                EnvironmentTheme.MixedAdventure,
                new[]
                {
                    new RingSpec(0f, 0f, 18f, 2.35f),
                    new RingSpec(2.7f, 1.5f, 38f, 2.15f),
                    new RingSpec(-2.8f, 2f, 58f, 2.15f),
                    new RingSpec(-3.5f, -0.8f, 80f, 2.05f),
                    new RingSpec(1.8f, -2.4f, 102f, 2f),
                    new RingSpec(3.4f, -0.2f, 124f, 1.95f),
                    new RingSpec(0f, 2.9f, 148f, 1.95f),
                    new RingSpec(-3f, 0.8f, 172f, 1.9f),
                    new RingSpec(2.8f, -1.6f, 196f, 1.9f),
                    new RingSpec(0.4f, -2.8f, 220f, 1.85f),
                    new RingSpec(-2.6f, 1.8f, 246f, 1.85f),
                    new RingSpec(2.9f, 2.2f, 272f, 1.85f),
                    new RingSpec(-3.2f, -1.4f, 298f, 1.85f),
                    new RingSpec(0f, 0f, 326f, 2.4f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(0f, -4.4f, 58f), new Vector3(12f, 1f, 4f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(-5.6f, 0f, 82f), new Vector3(1.2f, 7f, 4f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(5.6f, -0.6f, 104f), new Vector3(1.2f, 7f, 4f), ObstacleKind.Pillar),
                    new ObstacleSpec(new Vector3(-4.9f, 2.7f, 160f), new Vector3(1.6f, 1.4f, 1.4f), ObstacleKind.CosmicDebris, 41f),
                    new ObstacleSpec(new Vector3(4.8f, -2.4f, 210f), new Vector3(1.8f, 1.2f, 1.5f), ObstacleKind.CosmicDebris, 86f),
                    new ObstacleSpec(new Vector3(0f, 4.4f, 248f), new Vector3(12f, 1f, 4f), ObstacleKind.WallPanel),
                    new ObstacleSpec(new Vector3(-5.6f, -0.2f, 300f), new Vector3(1.1f, 7.2f, 4f), ObstacleKind.Pillar)
                },
                new[]
                {
                    new PortalSpec(0f, 0f, 44f, PortalKind.SpeedFast, 2.2f, 5f),
                    new PortalSpec(0f, 0f, 132f, PortalKind.SizeSmall, 2.2f, 7f),
                    new PortalSpec(0f, 0f, 188f, PortalKind.SpeedSlow, 2.35f, 4f),
                    new PortalSpec(0f, 0f, 250f, PortalKind.SpeedSlow, 2.2f, 4f),
                    new PortalSpec(0f, 0f, 280f, PortalKind.SpeedNormal, 2.35f, 0f),
                    new PortalSpec(0f, 0f, 306f, PortalKind.SizeNormal, 2.2f, 0f),
                    new PortalSpec(0f, 0f, 316f, PortalKind.SpeedNormal, 2.2f, 0f)
                })
        };
    }
}
