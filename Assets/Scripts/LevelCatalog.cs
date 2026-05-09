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
                "Safe onboarding for movement, highlighted rings, portals, restart, and pause.",
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
                    new RingSpec(-3.2f, -0.8f, 80f, 2.05f),
                    new RingSpec(1.8f, -2.4f, 102f, 2f),
                    new RingSpec(3.4f, -0.2f, 124f, 1.95f),
                    new RingSpec(0f, 2.9f, 148f, 1.95f),
                    new RingSpec(-3f, 0.8f, 172f, 1.9f),
                    new RingSpec(2.8f, -1.6f, 196f, 1.9f),
                    new RingSpec(0.4f, -2.8f, 220f, 1.85f),
                    new RingSpec(-2.3f, 1.7f, 248f, 2.05f),
                    new RingSpec(2.6f, 1.9f, 278f, 2f),
                    new RingSpec(-2.7f, -1.2f, 308f, 2f),
                    new RingSpec(0f, 0f, 342f, 2.55f)
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
                    new PortalSpec(0f, 0f, 236f, PortalKind.SpeedSlow, 2.2f, 4f),
                    new PortalSpec(0f, 0f, 292f, PortalKind.SpeedNormal, 2.35f, 0f),
                    new PortalSpec(0f, 0f, 322f, PortalKind.SizeNormal, 2.2f, 0f)
                }),

            CreateAcheron()
        };

        private static LevelDefinition CreateAcheron()
        {
            return new LevelDefinition(
                8,
                "Acheron",
                "A full-track extreme gauntlet cut tightly to Acheron's ramp, twin drops, sideways breaks, and fading outro.",
                26.5f,
                new Vector2(10.2f, 6.9f),
                EnvironmentTheme.AcheronAbyss,
                new[]
                {
                    new RingSpec(0.0f, 0.0f, 28f, 2.15f),
                    new RingSpec(1.8f, 2.0f, 60f, 2.09f),
                    new RingSpec(3.6f, 2.6f, 94f, 2.03f),
                    new RingSpec(3.4f, 2.2f, 132f, 1.97f),
                    new RingSpec(0.9f, 0.8f, 172f, 1.91f),
                    new RingSpec(-3.7f, -2.7f, 210f, 1.75f),
                    new RingSpec(-1.1f, -1.9f, 244f, 1.72f),
                    new RingSpec(2.2f, -0.3f, 278f, 1.69f),
                    new RingSpec(4.1f, 1.4f, 312f, 1.65f),
                    new RingSpec(3.4f, 2.6f, 346f, 1.61f),
                    new RingSpec(1.1f, 2.8f, 374f, 1.58f),
                    new RingSpec(1.2f, 2.2f, 398f, 1.54f),
                    new RingSpec(3.4f, 2.9f, 426f, 1.48f),
                    new RingSpec(4.6f, 3.0f, 454f, 1.46f),
                    new RingSpec(4.4f, 2.5f, 482f, 1.44f),
                    new RingSpec(2.7f, 1.5f, 510f, 1.42f),
                    new RingSpec(0.2f, 0.2f, 538f, 1.40f),
                    new RingSpec(-2.3f, -1.2f, 566f, 1.38f),
                    new RingSpec(-4.2f, -2.3f, 596f, 1.36f),
                    new RingSpec(-4.9f, 0.5f, 638f, 1.34f),
                    new RingSpec(-4.1f, 1.3f, 660f, 1.48f),
                    new RingSpec(-2.1f, 2.3f, 690f, 1.46f),
                    new RingSpec(0.5f, 3.0f, 720f, 1.44f),
                    new RingSpec(2.9f, 3.1f, 750f, 1.42f),
                    new RingSpec(4.6f, 2.7f, 780f, 1.40f),
                    new RingSpec(5.0f, 1.9f, 810f, 1.38f),
                    new RingSpec(4.0f, 0.8f, 840f, 1.31f),
                    new RingSpec(3.1f, 2.8f, 870f, 1.42f),
                    new RingSpec(4.2f, 3.0f, 895f, 1.40f),
                    new RingSpec(4.9f, 3.1f, 922f, 1.38f),
                    new RingSpec(5.1f, 3.0f, 952f, 1.37f),
                    new RingSpec(4.6f, 2.6f, 982f, 1.35f),
                    new RingSpec(3.5f, 2.0f, 1012f, 1.33f),
                    new RingSpec(1.9f, 1.2f, 1044f, 1.31f),
                    new RingSpec(-0.1f, 0.3f, 1076f, 1.42f),
                    new RingSpec(-2.0f, -0.6f, 1108f, 1.40f),
                    new RingSpec(-3.7f, -1.5f, 1142f, 1.38f),
                    new RingSpec(-4.8f, -2.3f, 1174f, 1.37f),
                    new RingSpec(-5.1f, -2.8f, 1206f, 1.35f),
                    new RingSpec(-4.7f, -3.1f, 1238f, 1.33f),
                    new RingSpec(-4.7f, -3.1f, 1270f, 1.31f),
                    new RingSpec(-2.6f, -2.2f, 1302f, 1.34f),
                    new RingSpec(-0.1f, -1.0f, 1328f, 1.33f),
                    new RingSpec(2.7f, 0.6f, 1358f, 1.28f),
                    new RingSpec(4.7f, 2.0f, 1388f, 1.30f),
                    new RingSpec(5.2f, 3.0f, 1418f, 1.29f),
                    new RingSpec(4.1f, 3.3f, 1448f, 1.28f),
                    new RingSpec(0.0f, 2.2f, 1478f, 1.27f),
                    new RingSpec(1.6f, 2.9f, 1514f, 1.28f),
                    new RingSpec(3.6f, 3.3f, 1538f, 1.34f),
                    new RingSpec(5.0f, 3.2f, 1568f, 1.33f),
                    new RingSpec(5.0f, 2.5f, 1598f, 1.32f),
                    new RingSpec(3.5f, 1.3f, 1628f, 1.28f),
                    new RingSpec(1.0f, -0.1f, 1658f, 1.29f),
                    new RingSpec(-1.9f, -1.5f, 1688f, 1.28f),
                    new RingSpec(1.6f, -1.0f, 1726f, 1.27f),
                    new RingSpec(4.0f, 0.1f, 1748f, 1.55f),
                    new RingSpec(2.9f, 1.2f, 1784f, 1.60f),
                    new RingSpec(0.9f, 2.0f, 1820f, 1.66f),
                    new RingSpec(-1.2f, 2.4f, 1856f, 1.72f),
                    new RingSpec(-2.8f, 2.1f, 1892f, 1.77f),
                    new RingSpec(-3.4f, 1.4f, 1930f, 1.83f),
                    new RingSpec(-2.8f, 0.5f, 1964f, 1.55f),
                    new RingSpec(-1.7f, -0.3f, 1992f, 1.60f),
                    new RingSpec(-0.2f, -1.1f, 2023f, 1.66f)
                },
                CreateAcheronObstacles(),
                new[]
                {
                    new PortalSpec(-3.7f, -2.7f, 210f, PortalKind.SpeedFast, 2.1f, 6f),
                    new PortalSpec(2.2f, -0.3f, 278f, PortalKind.SizeSmall, 1.95f, 6f),
                    new PortalSpec(1.2f, 2.2f, 398f, PortalKind.SpeedFast, 2.05f, 8f),
                    new PortalSpec(3.4f, 2.9f, 426f, PortalKind.SizeNormal, 2f, 0f),
                    new PortalSpec(0.2f, 0.2f, 538f, PortalKind.SpeedSlow, 1.95f, 4f),
                    new PortalSpec(-4.2f, -2.3f, 596f, PortalKind.SpeedNormal, 2f, 0f),
                    new PortalSpec(-4.9f, 0.5f, 638f, PortalKind.GravitySideways, 1.9f, 0f),
                    new PortalSpec(0.5f, 3.0f, 720f, PortalKind.SizeSmall, 1.88f, 7f),
                    new PortalSpec(4.0f, 0.8f, 840f, PortalKind.GravityNormal, 2f, 0f),
                    new PortalSpec(3.1f, 2.8f, 870f, PortalKind.SpeedFast, 1.95f, 7f),
                    new PortalSpec(4.6f, 2.6f, 982f, PortalKind.SpeedSlow, 1.9f, 4f),
                    new PortalSpec(3.5f, 2.0f, 1012f, PortalKind.SizeNormal, 2f, 0f),
                    new PortalSpec(-0.1f, 0.3f, 1076f, PortalKind.SpeedNormal, 2.05f, 0f),
                    new PortalSpec(-3.7f, -1.5f, 1142f, PortalKind.SpeedSlow, 2.05f, 5f),
                    new PortalSpec(-4.8f, -2.3f, 1174f, PortalKind.SizeSmall, 1.9f, 6f),
                    new PortalSpec(-4.7f, -3.1f, 1238f, PortalKind.SpeedNormal, 2f, 0f),
                    new PortalSpec(-0.1f, -1.0f, 1328f, PortalKind.SpeedFast, 2.05f, 9f),
                    new PortalSpec(2.7f, 0.6f, 1358f, PortalKind.GravitySideways, 1.9f, 0f),
                    new PortalSpec(5.2f, 3.0f, 1418f, PortalKind.SizeNormal, 1.95f, 0f),
                    new PortalSpec(0.0f, 2.2f, 1478f, PortalKind.GravityNormal, 2.05f, 0f),
                    new PortalSpec(1.6f, 2.9f, 1514f, PortalKind.GravitySideways, 1.9f, 0f),
                    new PortalSpec(5.0f, 3.2f, 1568f, PortalKind.SpeedFast, 2.05f, 8f),
                    new PortalSpec(3.5f, 1.3f, 1628f, PortalKind.SizeSmall, 1.85f, 7f),
                    new PortalSpec(-1.9f, -1.5f, 1688f, PortalKind.GravityNormal, 2.05f, 0f),
                    new PortalSpec(1.6f, -1.0f, 1726f, PortalKind.SpeedSlow, 2.1f, 7f),
                    new PortalSpec(2.9f, 1.2f, 1784f, PortalKind.SizeNormal, 2.05f, 0f),
                    new PortalSpec(0.9f, 2.0f, 1820f, PortalKind.SpeedNormal, 2.1f, 0f),
                    new PortalSpec(-2.8f, 2.1f, 1892f, PortalKind.SpeedSlow, 2.2f, 4f),
                    new PortalSpec(-2.8f, 0.5f, 1964f, PortalKind.SpeedNormal, 2.2f, 0f),
                    new PortalSpec(-1.7f, -0.3f, 1992f, PortalKind.GravityNormal, 2.2f, 0f)
                },
                soundtrackPath: "Soundtracks/Acheron.mp3",
                soundtrackDuration: 76.138f,
                suppressGameplaySoundEffects: true);
        }

        private static ObstacleSpec[] CreateAcheronObstacles()
        {
            List<ObstacleSpec> obstacles = new List<ObstacleSpec>();
            AddAcheronHazardRun(obstacles, 42f, 210f, 30f, 0, 0.35f);
            AddAcheronHazardRun(obstacles, 225f, 426f, 24f, 1, 0.6f);
            AddAcheronHazardRun(obstacles, 440f, 840f, 22f, 2, 0.82f);
            AddAcheronHazardRun(obstacles, 860f, 1270f, 23f, 3, 0.72f);
            AddAcheronHazardRun(obstacles, 1290f, 1726f, 20f, 4, 0.95f);
            AddAcheronHazardRun(obstacles, 1742f, 1998f, 30f, 5, 0.45f);

            return obstacles.ToArray();
        }

        private static void AddAcheronHazardRun(List<ObstacleSpec> obstacles, float startZ, float endZ, float spacing, int phase, float aggression)
        {
            int beat = 0;
            for (float z = startZ; z <= endZ; z += spacing)
            {
                float side = (beat + phase) % 2 == 0 ? -1f : 1f;
                float verticalSide = (beat + phase) % 3 == 0 ? 1f : -1f;
                float pillarX = side * Mathf.Lerp(6.1f, 7.25f, aggression);
                float pillarY = Mathf.Sin((z + phase * 19f) * 0.043f) * Mathf.Lerp(0.9f, 1.55f, aggression);
                float panelY = verticalSide * Mathf.Lerp(5.1f, 5.7f, aggression);
                float panelX = Mathf.Sin((z + phase * 11f) * 0.031f) * Mathf.Lerp(1.2f, 2.4f, aggression);

                obstacles.Add(new ObstacleSpec(new Vector3(pillarX, pillarY, z), new Vector3(Mathf.Lerp(1.15f, 0.82f, aggression), Mathf.Lerp(7.2f, 8.9f, aggression), 4.5f), ObstacleKind.Pillar, side * Mathf.Lerp(3f, 10f, aggression)));
                obstacles.Add(new ObstacleSpec(new Vector3(panelX, panelY, z + spacing * 0.45f), new Vector3(Mathf.Lerp(14.2f, 16.9f, aggression), Mathf.Lerp(0.88f, 0.62f, aggression), 4.2f), ObstacleKind.WallPanel, verticalSide * Mathf.Lerp(0f, 7f, aggression)));

                if (aggression > 0.5f && beat % 2 == 0)
                {
                    float debrisX = -side * Mathf.Lerp(3.0f, 4.75f, aggression);
                    float debrisY = -verticalSide * Mathf.Lerp(4.35f, 4.85f, aggression);
                    obstacles.Add(new ObstacleSpec(new Vector3(debrisX, debrisY, z + spacing * 0.22f), new Vector3(Mathf.Lerp(1.55f, 1.05f, aggression), Mathf.Lerp(1.25f, 1.75f, aggression), Mathf.Lerp(1.65f, 1.1f, aggression)), ObstacleKind.CosmicDebris, (z * 0.73f + phase * 29f) % 360f));
                }

                if (aggression > 0.8f && beat % 3 == 1)
                {
                    obstacles.Add(new ObstacleSpec(new Vector3(side * 1.8f, -verticalSide * 4.35f, z + spacing * 0.72f), new Vector3(6.2f, 0.58f, 3.8f), ObstacleKind.WallPanel, -verticalSide * 14f));
                }

                beat++;
            }
        }
    }
}
