using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Drift;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DriftEditor
{
    public static class DriftProjectVerifier
    {
        [MenuItem("Aerial/Run Project Verifier")]
        public static void RunFromMenu()
        {
            RunChecks(exitEditor: false);
        }

        public static void RunBatch()
        {
            RunChecks(exitEditor: true);
        }

        private static void RunChecks(bool exitEditor)
        {
            List<string> failures = new List<string>();

            CheckRequiredTypes(failures);
            CheckLevelCatalog(failures);
            CheckAcheronLevel(failures);
            CheckRingAndPortalContracts(failures);
            CheckPortalVisualContract(failures);
            CheckNoArrowKeyBindings(failures);
            CheckNoInvertedMode(failures);
            CheckProgression(failures);
            CheckProjectAssets(failures);
            CheckRuntimeBootstrap(failures);

            if (failures.Count == 0)
            {
                Debug.Log("Aerial project verifier passed.");
                if (exitEditor)
                {
                    EditorApplication.Exit(0);
                }

                return;
            }

            foreach (string failure in failures)
            {
                Debug.LogError(failure);
            }

            if (exitEditor)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void CheckRequiredTypes(List<string> failures)
        {
            string[] requiredTypes =
            {
                "Drift.DroneController",
                "Drift.CameraFollow",
                "Drift.RingCheckpoint",
                "Drift.RingManager",
                "Drift.LevelManager",
                "Drift.GameManager",
                "Drift.UIManager",
                "Drift.LevelSelectManager",
                "Drift.BoundaryReset",
                "Drift.ObstacleReset",
                "Drift.PortalBase",
                "Drift.SpeedPortal",
                "Drift.GravityPortal",
                "Drift.SizePortal",
                "Drift.PortalManager"
            };

            foreach (string typeName in requiredTypes)
            {
                if (FindType(typeName) == null)
                {
                    failures.Add($"{typeName} is missing.");
                }
            }
        }

        private static void CheckLevelCatalog(List<string> failures)
        {
            IReadOnlyList<LevelDefinition> levels = LevelCatalog.GetLevels();
            if (levels.Count < 8)
            {
                failures.Add($"Expected at least 8 levels including Acheron, found {levels.Count}.");
            }

            int totalRings = 0;
            int portalCount = 0;
            bool hasTutorial = false;
            HashSet<EnvironmentTheme> themes = new HashSet<EnvironmentTheme>();
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                totalRings += level.Rings.Length;
                portalCount += level.Portals.Length;
                hasTutorial |= level.IsTutorial;
                themes.Add(level.Theme);
                if (level.LevelNumber != i + 1)
                {
                    failures.Add($"Level index {i} has level number {level.LevelNumber}.");
                }

                if (string.IsNullOrWhiteSpace(level.DisplayName))
                {
                    failures.Add($"Level {level.LevelNumber} has no display name.");
                }

                if (level.ForwardSpeed <= 0f)
                {
                    failures.Add($"{level.DisplayName} has non-positive forward speed.");
                }

                if (level.Rings.Length < 10)
                {
                    failures.Add($"{level.DisplayName} should have at least 10 rings.");
                }
            }

            if (!hasTutorial)
            {
                failures.Add("Expected a playable tutorial level.");
            }

            if (themes.Count < 6)
            {
                failures.Add($"Expected all six Aerial environment themes, found {themes.Count}.");
            }

            if (portalCount < 20)
            {
                failures.Add($"Expected a substantial portal set, found {portalCount}.");
            }

            if (totalRings < 110)
            {
                failures.Add($"Expected at least 110 rings across expanded levels, found {totalRings}.");
            }
        }

        private static void CheckProgression(List<string> failures)
        {
            PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
            PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

            ProgressionService progression = new ProgressionService();
            if (progression.GetHighestUnlockedLevel() != LevelCatalog.GetLevels().Count)
            {
                failures.Add("All Aerial levels should be accessible by default.");
            }

            if (!progression.IsLevelUnlocked(1) || !progression.IsLevelUnlocked(LevelCatalog.GetLevels().Count))
            {
                failures.Add("Progression initial all-level access state is incorrect.");
            }

            progression.MarkLevelComplete(1);
            if (!progression.IsLevelCompleted(1) || !progression.IsLevelUnlocked(LevelCatalog.GetLevels().Count))
            {
                failures.Add("Completing level 1 should mark it complete without locking later levels.");
            }
        }

        private static void CheckRingAndPortalContracts(List<string> failures)
        {
            LevelDefinition finalLevel = LevelCatalog.GetLevels().FirstOrDefault(level => level.LevelNumber == 7);
            if (finalLevel == null)
            {
                failures.Add("Level 7 is missing.");
                return;
            }

            foreach (PortalSpec portal in finalLevel.Portals.Where(portal => portal.Position.z >= 230f))
            {
                foreach (RingSpec ring in finalLevel.Rings.Where(ring => ring.Position.z >= 220f))
                {
                    float separation = Mathf.Abs(portal.Position.z - ring.Position.z);
                    if (separation < 10f)
                    {
                        failures.Add($"Level 7 portal {portal.Kind} at z{portal.Position.z} is too close to ring at z{ring.Position.z}.");
                    }
                }
            }
        }

        private static void CheckAcheronLevel(List<string> failures)
        {
            IReadOnlyList<LevelDefinition> levels = LevelCatalog.GetLevels();
            LevelDefinition acheron = levels.FirstOrDefault(level => level.LevelNumber == 8);
            if (acheron == null)
            {
                failures.Add("Level 8 Acheron is missing.");
                return;
            }

            if (acheron.DisplayName != "Acheron")
            {
                failures.Add("Level 8 should be titled Acheron.");
            }

            string soundtrackPath = GetOptionalField<string>(acheron, "SoundtrackPath");
            if (soundtrackPath != "Soundtracks/Acheron.mp3")
            {
                failures.Add("Acheron should use Soundtracks/Acheron.mp3.");
            }
            else if (!File.Exists(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), soundtrackPath))))
            {
                failures.Add("Soundtracks/Acheron.mp3 is missing from the project root.");
            }

            float soundtrackDuration = GetOptionalField<float>(acheron, "SoundtrackDuration");
            if (soundtrackDuration < 76f || soundtrackDuration > 76.3f)
            {
                failures.Add($"Acheron soundtrack duration should match the full track, found {soundtrackDuration:0.00}s.");
            }

            if (!GetOptionalField<bool>(acheron, "SuppressGameplaySoundEffects"))
            {
                failures.Add("Acheron should suppress gameplay sound effects.");
            }

            float acheronEndZ = acheron.Rings.Length == 0 ? 0f : acheron.Rings.Max(ring => ring.Position.z);
            float previousEndZ = levels.Where(level => level.LevelNumber < 8).Max(level => level.Rings.Max(ring => ring.Position.z));
            int previousObstacleMax = levels.Where(level => level.LevelNumber < 8).Max(level => level.Obstacles.Length);
            float previousSmallestRing = levels.Where(level => level.LevelNumber < 8).Min(level => level.Rings.Min(ring => ring.Radius));
            if (acheronEndZ <= previousEndZ)
            {
                failures.Add("Acheron should be the longest authored course.");
            }

            float estimatedCompletion = EstimateCompletionSeconds(acheron);
            if (soundtrackDuration > 0f && Mathf.Abs(estimatedCompletion - soundtrackDuration) > 0.75f)
            {
                failures.Add($"Acheron's estimated route duration should match the soundtrack; estimated {estimatedCompletion:0.00}s for {soundtrackDuration:0.00}s audio.");
            }

            if (acheron.Rings.Length < 36)
            {
                failures.Add($"Acheron should have at least 36 rings, found {acheron.Rings.Length}.");
            }

            if (acheron.Portals.Length < 14)
            {
                failures.Add($"Acheron should have a dense portal route, found {acheron.Portals.Length}.");
            }

            if (acheron.Obstacles.Length <= previousObstacleMax)
            {
                failures.Add("Acheron should have more obstacles than any previous level.");
            }

            if (acheron.Rings.Length > 0 && acheron.Rings.Min(ring => ring.Radius) >= previousSmallestRing)
            {
                failures.Add("Acheron should include the smallest rings in the game.");
            }

            PortalKind[] requiredPortalKinds =
            {
                PortalKind.SpeedFast,
                PortalKind.SpeedSlow,
                PortalKind.SpeedNormal,
                PortalKind.GravitySideways,
                PortalKind.GravityNormal,
                PortalKind.SizeSmall,
                PortalKind.SizeNormal
            };

            foreach (PortalKind kind in requiredPortalKinds)
            {
                if (!acheron.Portals.Any(portal => portal.Kind == kind))
                {
                    failures.Add($"Acheron is missing a {kind} portal.");
                }
            }
        }

        private static void CheckPortalVisualContract(List<string> failures)
        {
            GameObject parent = new GameObject("Verifier Portal Parent");
            try
            {
                PortalBase portal = RuntimeVisualFactory.CreatePortal(parent.transform, new PortalSpec(0f, 0f, 12f, PortalKind.SpeedSlow, 2.2f, 4f));
                Transform portalTransform = portal.transform;
                if (portalTransform.Cast<Transform>().Any(child => child.name == "Portal Segment"))
                {
                    failures.Add("Portal visuals should not include the old outside Portal Segment ring.");
                }

                if (portalTransform.GetComponentsInChildren<TextMesh>().Length > 0)
                {
                    failures.Add("Portal visuals should use central mesh glyphs instead of text labels.");
                }

                if (portalTransform.Find("Portal Inner Mesh Ring") == null)
                {
                    failures.Add("Portal visuals should include a central mesh ring.");
                }
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        private static void CheckNoArrowKeyBindings(List<string> failures)
        {
            string controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/DroneController.cs");
            if (!File.Exists(controllerPath))
            {
                failures.Add("DroneController.cs could not be inspected for arrow-key bindings.");
                return;
            }

            string source = File.ReadAllText(controllerPath);
            string[] forbiddenBindings =
            {
                "KeyCode.LeftArrow",
                "KeyCode.RightArrow",
                "KeyCode.UpArrow",
                "KeyCode.DownArrow"
            };

            foreach (string binding in forbiddenBindings)
            {
                if (source.Contains(binding))
                {
                    failures.Add($"{binding} should not be bound to gameplay input.");
                }
            }
        }

        private static void CheckNoInvertedMode(List<string> failures)
        {
            if (Enum.GetNames(typeof(PortalKind)).Any(name => name.Contains("Inverted")))
            {
                failures.Add("Inverted gravity mode should not exist in PortalKind.");
            }

            foreach (LevelDefinition level in LevelCatalog.GetLevels())
            {
                foreach (PortalSpec portal in level.Portals)
                {
                    if (portal.Kind.ToString().Contains("Inverted"))
                    {
                        failures.Add($"{level.DisplayName} still authors an inverted gravity portal.");
                    }
                }
            }
        }

        private static void CheckProjectAssets(List<string> failures)
        {
            string[] assetPaths =
            {
                "Assets/Scenes/Main.unity",
                "Assets/Prefabs/Drone.prefab",
                "Assets/Prefabs/Ring.prefab",
                "Assets/Prefabs/Obstacle.prefab",
                "Assets/Prefabs/LevelContainer.prefab",
                "Assets/Prefabs/ParticleBurst.prefab"
            };

            foreach (string path in assetPaths)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    failures.Add($"{path} is missing or not importable.");
                }
            }

            bool sceneInBuild = EditorBuildSettings.scenes.Any(scene => scene.path == "Assets/Scenes/Main.unity" && scene.enabled);
            if (!sceneInBuild)
            {
                failures.Add("Assets/Scenes/Main.unity is not enabled in build settings.");
            }
        }

        private static void CheckRuntimeBootstrap(List<string> failures)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
            PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

            GameManager manager = GameManager.EnsureRuntime();
            if (manager == null)
            {
                failures.Add("GameManager.EnsureRuntime did not create a manager.");
                return;
            }

            manager.StartLevel(1);

            if (Object.FindFirstObjectByType<DroneController>() == null)
            {
                failures.Add("Starting level 1 did not create a drone.");
            }

            if (Object.FindFirstObjectByType<CameraFollow>() == null)
            {
                failures.Add("Runtime bootstrap did not create a camera follow component.");
            }

            int ringCount = Object.FindObjectsByType<RingCheckpoint>(FindObjectsSortMode.None).Length;
            if (ringCount < 10)
            {
                failures.Add($"Starting level 1 created too few rings: {ringCount}.");
            }

            int portalCount = Object.FindObjectsByType<PortalBase>(FindObjectsSortMode.None).Length;
            if (portalCount < 3)
            {
                failures.Add($"Starting tutorial created too few portals: {portalCount}.");
            }

            manager.CompleteCurrentLevel();
            if (!manager.Progression.IsLevelCompleted(1) || !manager.Progression.IsLevelUnlocked(2))
            {
                failures.Add("Completing level 1 through GameManager did not save progression.");
            }
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static T GetOptionalField<T>(LevelDefinition level, string fieldName)
        {
            FieldInfo field = typeof(LevelDefinition).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null || field.FieldType != typeof(T))
            {
                return default;
            }

            return (T)field.GetValue(level);
        }

        private static float EstimateCompletionSeconds(LevelDefinition level)
        {
            float finalZ = level.Rings.Max(ring => ring.Position.z);
            PortalSpec[] portals = level.Portals.OrderBy(portal => portal.Position.z).ToArray();
            int portalIndex = 0;
            float z = 0f;
            float elapsed = 0f;
            float speed = level.ForwardSpeed;
            SpeedEstimate speedEstimate = SpeedEstimate.Inactive;
            const float step = 0.02f;

            while (z < finalZ && elapsed < 180f)
            {
                while (portalIndex < portals.Length && z >= portals[portalIndex].Position.z)
                {
                    PortalSpec portal = portals[portalIndex];
                    if (TryGetSpeedMultiplier(portal.Kind, out float multiplier))
                    {
                        speedEstimate = SpeedEstimate.Start(speed, level.ForwardSpeed * multiplier, portal.Duration);
                    }

                    portalIndex++;
                }

                speedEstimate.Advance(step, level.ForwardSpeed, ref speed);
                z += speed * step;
                elapsed += step;
            }

            return elapsed;
        }

        private static bool TryGetSpeedMultiplier(PortalKind kind, out float multiplier)
        {
            switch (kind)
            {
                case PortalKind.SpeedFast:
                    multiplier = 1.38f;
                    return true;
                case PortalKind.SpeedSlow:
                    multiplier = 0.68f;
                    return true;
                case PortalKind.SpeedNormal:
                    multiplier = 1f;
                    return true;
                default:
                    multiplier = 1f;
                    return false;
            }
        }

        private struct SpeedEstimate
        {
            private const int InactivePhase = 0;
            private const int RampPhase = 1;
            private const int HoldPhase = 2;
            private const int ReturnPhase = 3;

            private int phase;
            private float startSpeed;
            private float targetSpeed;
            private float elapsed;
            private float duration;
            private float holdElapsed;

            public static SpeedEstimate Inactive => new SpeedEstimate { phase = InactivePhase };

            public static SpeedEstimate Start(float currentSpeed, float targetSpeed, float duration)
            {
                return new SpeedEstimate
                {
                    phase = RampPhase,
                    startSpeed = currentSpeed,
                    targetSpeed = targetSpeed,
                    duration = duration
                };
            }

            public void Advance(float step, float defaultSpeed, ref float currentSpeed)
            {
                if (phase == InactivePhase)
                {
                    return;
                }

                if (phase == RampPhase)
                {
                    elapsed += step;
                    currentSpeed = Mathf.Lerp(startSpeed, targetSpeed, Mathf.Clamp01(elapsed / 0.35f));
                    if (elapsed >= 0.35f)
                    {
                        currentSpeed = targetSpeed;
                        phase = HoldPhase;
                        elapsed = 0f;
                    }

                    return;
                }

                if (phase == HoldPhase)
                {
                    if (duration <= 0f)
                    {
                        currentSpeed = targetSpeed;
                        phase = InactivePhase;
                        return;
                    }

                    holdElapsed += step;
                    if (holdElapsed >= duration)
                    {
                        phase = ReturnPhase;
                        startSpeed = currentSpeed;
                        targetSpeed = defaultSpeed;
                        elapsed = 0f;
                    }

                    return;
                }

                elapsed += step;
                currentSpeed = Mathf.Lerp(startSpeed, defaultSpeed, Mathf.Clamp01(elapsed / 0.4f));
                if (elapsed >= 0.4f)
                {
                    currentSpeed = defaultSpeed;
                    phase = InactivePhase;
                }
            }
        }
    }
}
