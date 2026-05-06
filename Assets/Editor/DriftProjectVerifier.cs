using System;
using System.Collections.Generic;
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
            if (levels.Count < 7)
            {
                failures.Add($"Expected at least 7 levels including tutorial and expanded courses, found {levels.Count}.");
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

            if (themes.Count < 5)
            {
                failures.Add($"Expected all five Aerial environment themes, found {themes.Count}.");
            }

            if (portalCount < 10)
            {
                failures.Add($"Expected a substantial portal set, found {portalCount}.");
            }

            if (totalRings < 75)
            {
                failures.Add($"Expected at least 75 rings across expanded levels, found {totalRings}.");
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
    }
}
