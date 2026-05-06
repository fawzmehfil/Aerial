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
                "Level 1 - Basic Flow",
                "Large rings and gentle movement on a straight teaching route.",
                18f,
                new Vector2(8f, 5f),
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
                2,
                "Level 2 - Signal Wave",
                "Alternating horizontal and vertical ring patterns with tighter spacing.",
                20f,
                new Vector2(8.5f, 5.5f),
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
                new ObstacleSpec[0]),

            new LevelDefinition(
                3,
                "Level 3 - Rhythm Arc",
                "Readable arcs and center-to-edge rhythm-game flow.",
                22f,
                new Vector2(9f, 6f),
                new[]
                {
                    new RingSpec(0f, -1.2f, 20f, 2.45f),
                    new RingSpec(1.5f, -0.2f, 38f, 2.35f),
                    new RingSpec(3f, 1.1f, 56f, 2.35f),
                    new RingSpec(1.4f, 2.3f, 74f, 2.3f),
                    new RingSpec(-0.6f, 1.5f, 92f, 2.3f),
                    new RingSpec(-2.8f, 0f, 110f, 2.25f),
                    new RingSpec(-3.4f, -1.5f, 128f, 2.25f),
                    new RingSpec(-1.3f, -2.3f, 146f, 2.2f),
                    new RingSpec(1.4f, -1.2f, 164f, 2.2f),
                    new RingSpec(3f, 0.7f, 182f, 2.15f),
                    new RingSpec(0.8f, 2.4f, 202f, 2.2f),
                    new RingSpec(0f, 0f, 224f, 2.55f)
                },
                new ObstacleSpec[0]),

            new LevelDefinition(
                4,
                "Level 4 - Gate Run",
                "Simple walls and pillars frame the ring route without blind hazards.",
                23f,
                new Vector2(8.5f, 5.8f),
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
                    new ObstacleSpec(new Vector3(0f, -4.4f, 58f), new Vector3(12f, 1.1f, 4f)),
                    new ObstacleSpec(new Vector3(-5.7f, 0.4f, 78f), new Vector3(1.3f, 7f, 5f)),
                    new ObstacleSpec(new Vector3(5.7f, 0.6f, 97f), new Vector3(1.3f, 7f, 5f)),
                    new ObstacleSpec(new Vector3(0f, 4.6f, 119f), new Vector3(12f, 1.2f, 5f)),
                    new ObstacleSpec(new Vector3(-5.8f, -0.8f, 153f), new Vector3(1.4f, 7f, 5f)),
                    new ObstacleSpec(new Vector3(5.8f, -0.4f, 173f), new Vector3(1.4f, 7f, 5f)),
                    new ObstacleSpec(new Vector3(0f, 4.4f, 193f), new Vector3(12f, 1.2f, 4f))
                }),

            new LevelDefinition(
                5,
                "Level 5 - Precision Neon",
                "A tighter final route that rewards smooth anticipation and small corrections.",
                25f,
                new Vector2(8f, 5.5f),
                new[]
                {
                    new RingSpec(0f, 0f, 18f, 2.2f),
                    new RingSpec(2.7f, 1.5f, 34f, 2f),
                    new RingSpec(-2.8f, 2f, 50f, 2f),
                    new RingSpec(-3.5f, -0.8f, 66f, 1.95f),
                    new RingSpec(1.8f, -2.4f, 82f, 1.95f),
                    new RingSpec(3.4f, -0.2f, 98f, 1.9f),
                    new RingSpec(0f, 2.9f, 114f, 1.9f),
                    new RingSpec(-3f, 0.8f, 130f, 1.85f),
                    new RingSpec(2.8f, -1.6f, 146f, 1.85f),
                    new RingSpec(0.4f, -2.8f, 162f, 1.85f),
                    new RingSpec(-2.6f, 1.8f, 180f, 1.8f),
                    new RingSpec(2.9f, 2.2f, 198f, 1.8f),
                    new RingSpec(0f, 0f, 220f, 2.25f)
                },
                new[]
                {
                    new ObstacleSpec(new Vector3(0f, -4.1f, 50f), new Vector3(12f, 1.1f, 4f)),
                    new ObstacleSpec(new Vector3(-5.3f, 0f, 66f), new Vector3(1.2f, 7f, 4f)),
                    new ObstacleSpec(new Vector3(5.2f, -0.6f, 83f), new Vector3(1.2f, 7f, 4f)),
                    new ObstacleSpec(new Vector3(0f, 4.1f, 115f), new Vector3(12f, 1.1f, 4f)),
                    new ObstacleSpec(new Vector3(-5.2f, 0.4f, 131f), new Vector3(1.2f, 7f, 4f)),
                    new ObstacleSpec(new Vector3(5.2f, 0.2f, 147f), new Vector3(1.2f, 7f, 4f)),
                    new ObstacleSpec(new Vector3(0f, -4.1f, 164f), new Vector3(12f, 1.1f, 4f)),
                    new ObstacleSpec(new Vector3(0f, 4.1f, 199f), new Vector3(12f, 1.1f, 4f))
                })
        };
    }
}
