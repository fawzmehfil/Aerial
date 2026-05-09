#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Drift;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class DriftContractTests
{
    [Test]
    public void RequiredGameplayTypesExist()
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
            "Drift.PortalManager",
            "Drift.PracticeCheckpoint",
            "Drift.DronePracticeSnapshot"
        };

        foreach (string typeName in requiredTypes)
        {
            Assert.That(FindType(typeName), Is.Not.Null, $"{typeName} should exist.");
        }
    }

    [Test]
    public void LevelCatalogDefinesExpandedAerialLevels()
    {
        Type catalogType = RequireType("Drift.LevelCatalog");
        MethodInfo getLevels = catalogType.GetMethod("GetLevels", BindingFlags.Public | BindingFlags.Static);
        Assert.That(getLevels, Is.Not.Null);

        IEnumerable levelEnumerable = (IEnumerable)getLevels.Invoke(null, null);
        Assert.That(levelEnumerable, Is.Not.Null);
        object[] levels = levelEnumerable.Cast<object>().ToArray();
        Assert.That(levels.Length, Is.GreaterThanOrEqualTo(8));
        Assert.That(levels.Any(level => (bool)GetField(level, "IsTutorial")), Is.True);

        int portalCount = 0;
        for (int i = 0; i < levels.Length; i++)
        {
            object level = levels[i];
            int levelNumber = (int)GetField(level, "LevelNumber");
            string displayName = (string)GetField(level, "DisplayName");
            float forwardSpeed = (float)GetField(level, "ForwardSpeed");
            IList rings = (IList)GetField(level, "Rings");
            IList portals = (IList)GetField(level, "Portals");
            portalCount += portals.Count;

            Assert.That(levelNumber, Is.EqualTo(i + 1));
            Assert.That(displayName, Is.Not.Empty);
            Assert.That(forwardSpeed, Is.GreaterThan(0f));
            Assert.That(rings.Count, Is.GreaterThanOrEqualTo(10), $"{displayName} should have a substantial ring route.");
        }

        Assert.That(portalCount, Is.GreaterThanOrEqualTo(20));
    }

    [Test]
    public void InvertedGravityModeIsNotAvailableOrAuthored()
    {
        Type portalKindType = RequireType("Drift.PortalKind");
        string[] portalKinds = Enum.GetNames(portalKindType);
        Assert.That(portalKinds, Does.Not.Contain("GravityInverted"));

        Type catalogType = RequireType("Drift.LevelCatalog");
        MethodInfo getLevels = catalogType.GetMethod("GetLevels", BindingFlags.Public | BindingFlags.Static);
        IEnumerable levelEnumerable = (IEnumerable)getLevels.Invoke(null, null);

        foreach (object level in levelEnumerable)
        {
            IList portals = (IList)GetField(level, "Portals");
            foreach (object portal in portals)
            {
                object kind = GetField(portal, "Kind");
                Assert.That(kind.ToString(), Does.Not.Contain("Inverted"));
            }
        }
    }

    [Test]
    public void LevelSevenFinalPortalsAreClearlySeparatedFromRings()
    {
        LevelDefinition level = LevelCatalog.GetLevels().Single(candidate => candidate.LevelNumber == 7);

        foreach (PortalSpec portal in level.Portals.Where(portal => portal.Position.z >= 230f))
        {
            foreach (RingSpec ring in level.Rings.Where(ring => ring.Position.z >= 220f))
            {
                float separation = Mathf.Abs(portal.Position.z - ring.Position.z);
                Assert.That(separation, Is.GreaterThanOrEqualTo(10f), $"Portal {portal.Kind} at z{portal.Position.z} is too close to ring at z{ring.Position.z}.");
            }
        }
    }

    [Test]
    public void RingPassWindowRequiresCenterlineAndPlaneAlignment()
    {
        GameObject ringObject = new GameObject("Ring Check");
        try
        {
            RingCheckpoint ring = ringObject.AddComponent<RingCheckpoint>();
            ring.Configure(0, 1.4f, 1.65f, null, Array.Empty<Renderer>(), null, null, null);

            Assert.That(ring.IsInsidePassWindow(new Vector3(1.39f, 0f, 0f)), Is.True);
            Assert.That(ring.IsInsidePassWindow(new Vector3(1.41f, 0f, 0f)), Is.False);
            Assert.That(ring.IsInsidePassWindow(new Vector3(0f, 0f, 1.66f)), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(ringObject);
        }
    }

    [Test]
    public void PortalMissPlaneRequiresPassingThroughPortal()
    {
        GameObject portalObject = new GameObject("Portal Check");
        try
        {
            SpeedPortal portal = portalObject.AddComponent<SpeedPortal>();
            portal.Configure(PortalKind.SpeedFast, 4f, null);

            Assert.That(portal.IsConsumed, Is.False);
            Assert.That(portal.IsPastMissPlane(new Vector3(0f, 0f, 2.9f), 3f), Is.False);
            Assert.That(portal.IsPastMissPlane(new Vector3(0f, 0f, 3.1f), 3f), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(portalObject);
        }
    }

    [Test]
    public void PortalVisualsUseCenterMeshWithoutOuterRing()
    {
        GameObject parent = new GameObject("Portal Parent");
        try
        {
            PortalBase portal = RuntimeVisualFactory.CreatePortal(parent.transform, new PortalSpec(0f, 0f, 12f, PortalKind.SpeedSlow, 2.2f, 4f));
            Transform portalTransform = portal.transform;

            Assert.That(portalTransform.Cast<Transform>().Count(child => child.name == "Portal Segment"), Is.Zero);
            Assert.That(portalTransform.GetComponentsInChildren<TextMesh>().Length, Is.Zero);
            Assert.That(portalTransform.Find("Portal Inner Mesh Ring"), Is.Not.Null);
            Assert.That(portalTransform.GetComponentsInChildren<Renderer>().Any(renderer => renderer.name.Contains("Portal Slow Pause Bar")), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(parent);
        }
    }

    [Test]
    public void AcheronIsEighthLongestAndHardestSoundtrackLevel()
    {
        LevelDefinition[] levels = LevelCatalog.GetLevels().ToArray();
        LevelDefinition acheron = levels.Single(level => level.LevelNumber == 8);

        Assert.That(acheron.DisplayName, Is.EqualTo("Acheron"));
        Assert.That((string)GetField(acheron, "SoundtrackPath"), Is.EqualTo("Soundtracks/Acheron.mp3"));
        Assert.That((float)GetField(acheron, "SoundtrackDuration"), Is.InRange(76.0f, 76.3f));
        Assert.That((bool)GetField(acheron, "SuppressGameplaySoundEffects"), Is.True);

        float acheronEndZ = acheron.Rings.Max(ring => ring.Position.z);
        float previousEndZ = levels.Where(level => level.LevelNumber < 8).Max(level => level.Rings.Max(ring => ring.Position.z));
        int previousObstacleMax = levels.Where(level => level.LevelNumber < 8).Max(level => level.Obstacles.Length);
        float previousSmallestRing = levels.Where(level => level.LevelNumber < 8).Min(level => level.Rings.Min(ring => ring.Radius));

        Assert.That(acheron.Rings.Length, Is.GreaterThanOrEqualTo(36));
        Assert.That(acheron.Portals.Length, Is.GreaterThanOrEqualTo(14));
        Assert.That(acheron.Obstacles.Length, Is.GreaterThan(previousObstacleMax));
        Assert.That(acheronEndZ, Is.GreaterThan(previousEndZ));
        Assert.That(acheron.Rings.Min(ring => ring.Radius), Is.LessThan(previousSmallestRing));

        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.SpeedFast), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.SpeedSlow), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.SpeedNormal), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.GravitySideways), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.GravityNormal), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.SizeSmall), Is.True);
        Assert.That(acheron.Portals.Any(portal => portal.Kind == PortalKind.SizeNormal), Is.True);
    }

    [Test]
    public void AcheronHasDenseMusicSyncedUpgrade()
    {
        LevelDefinition acheron = LevelCatalog.GetLevels().Single(level => level.LevelNumber == 8);
        float soundtrackDuration = (float)GetField(acheron, "SoundtrackDuration");
        float zPerSecond = acheron.Rings.Max(ring => ring.Position.z) / soundtrackDuration;

        Assert.That(acheron.Rings.Length, Is.GreaterThanOrEqualTo(64));
        Assert.That(acheron.Obstacles.Length, Is.GreaterThanOrEqualTo(80));
        Assert.That(acheron.Portals.Length, Is.GreaterThanOrEqualTo(28));
        Assert.That(acheron.Rings.Min(ring => ring.Radius), Is.LessThanOrEqualTo(1.32f));

        AssertPortalNear(acheron, PortalKind.SpeedFast, 8.0f, zPerSecond, 16f);
        AssertPortalNear(acheron, PortalKind.SpeedFast, 15.0f, zPerSecond, 16f);
        AssertPortalNear(acheron, PortalKind.GravitySideways, 24.0f, zPerSecond, 18f);
        AssertPortalNear(acheron, PortalKind.SpeedSlow, 43.0f, zPerSecond, 18f);
        AssertPortalNear(acheron, PortalKind.SpeedFast, 50.0f, zPerSecond, 18f);
        AssertPortalNear(acheron, PortalKind.GravitySideways, 57.0f, zPerSecond, 18f);
        AssertPortalNear(acheron, PortalKind.SpeedSlow, 65.0f, zPerSecond, 20f);

        for (float sectionStart = 0f; sectionStart < 64f; sectionStart += 8f)
        {
            float zStart = sectionStart * zPerSecond;
            float zEnd = (sectionStart + 8f) * zPerSecond;
            int ringCount = acheron.Rings.Count(ring => ring.Position.z >= zStart && ring.Position.z < zEnd);
            int obstacleCount = acheron.Obstacles.Count(obstacle => obstacle.Position.z >= zStart && obstacle.Position.z < zEnd);

            Assert.That(ringCount, Is.GreaterThanOrEqualTo(5), $"Acheron should keep ring pressure during {sectionStart:0}-{sectionStart + 8f:0}s.");
            Assert.That(obstacleCount, Is.GreaterThanOrEqualTo(7), $"Acheron should keep obstacle pressure during {sectionStart:0}-{sectionStart + 8f:0}s.");
        }
    }

    [Test]
    public void AcheronSoundtrackFileExistsAtAuthoredPath()
    {
        LevelDefinition acheron = LevelCatalog.GetLevels().Single(level => level.LevelNumber == 8);
        string soundtrackPath = (string)GetField(acheron, "SoundtrackPath");
        string absolutePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), soundtrackPath));

        Assert.That(File.Exists(absolutePath), Is.True, $"{soundtrackPath} should exist at the authored project-relative path.");
    }

    [Test]
    public void ArrowKeysAreNotBoundToGameplayInput()
    {
        string controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/DroneController.cs");
        string source = File.ReadAllText(controllerPath);

        Assert.That(source, Does.Not.Contain("KeyCode.LeftArrow"));
        Assert.That(source, Does.Not.Contain("KeyCode.RightArrow"));
        Assert.That(source, Does.Not.Contain("KeyCode.UpArrow"));
        Assert.That(source, Does.Not.Contain("KeyCode.DownArrow"));
    }

    [Test]
    public void PracticeModeContractsExistForCheckpointRespawns()
    {
        Type gameManager = RequireType("Drift.GameManager");
        Type droneController = RequireType("Drift.DroneController");
        Type ringManager = RequireType("Drift.RingManager");
        Type portalBase = RequireType("Drift.PortalBase");
        Type runtimeVisualFactory = RequireType("Drift.RuntimeVisualFactory");

        Assert.That(gameManager.GetMethod("StartPracticeLevel", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(gameManager.GetProperty("IsPracticeMode", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(gameManager.GetMethod("RecordPracticeCheckpoint", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(gameManager.GetMethod("SyncPracticeSoundtrack", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);

        Assert.That(droneController.GetMethod("CapturePracticeSnapshot", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(droneController.GetMethod("RestorePracticeSnapshot", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(ringManager.GetMethod("SetCurrentRingIndex", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(portalBase.GetMethod("ResetForPracticeRespawn", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(runtimeVisualFactory.GetMethod("CreatePracticeCheckpointMarker", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
    }

    [Test]
    public void NoClipContractsExistForAlternateLevelPractice()
    {
        Type gameManager = RequireType("Drift.GameManager");

        Assert.That(gameManager.GetProperty("IsNoClipMode", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(gameManager.GetMethod("ToggleNoClipMode", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(gameManager.GetMethod("DisableNoClipAndRestart", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
    }

    [Test]
    public void NoClipIgnoresFailuresAndCompletionDoesNotMarkProgressionComplete()
    {
        PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();
        manager.StartLevel(1);
        manager.ToggleNoClipMode();

        manager.FailCurrentLevel("Crashed");
        Assert.That(manager.IsPlaying, Is.True);
        Assert.That(manager.IsNoClipMode, Is.True);

        manager.CompleteCurrentLevel();
        Assert.That(manager.Progression.IsLevelCompleted(1), Is.False);
        ClearScene();
    }

    [Test]
    public void TurningNoClipOffRestartsCurrentLevel()
    {
        PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();
        manager.StartLevel(1);
        LevelManager levelManager = (LevelManager)GetField(manager, "levelManager");
        DroneController originalDrone = levelManager.CurrentDrone;

        manager.ToggleNoClipMode();
        originalDrone.transform.position = new Vector3(0f, 0f, 120f);
        manager.ToggleNoClipMode();

        Assert.That(manager.IsNoClipMode, Is.False);
        Assert.That(manager.IsPlaying, Is.True);
        Assert.That(levelManager.CurrentDrone, Is.Not.SameAs(originalDrone));
        Assert.That(levelManager.CurrentDrone.transform.position.z, Is.LessThan(1f));
        ClearScene();
    }

    [Test]
    public void PracticeSnapshotRestoresActivePortalModesAtEffectTargets()
    {
        ClearScene();
        GameObject droneObject = RuntimeVisualFactory.CreateDrone(Vector3.zero, 20f, new Vector2(8f, 5f), true);
        try
        {
            DroneController drone = droneObject.GetComponent<DroneController>();
            drone.ApplySpeedMultiplier(1.38f, 7f, "SPEED UP");
            drone.ApplyOrientation(90f, 0f, "SIDEWAYS GRAVITY");
            drone.ApplySize(0.62f, 6f, "MINI DRONE");

            DronePracticeSnapshot snapshot = drone.CapturePracticeSnapshot();
            drone.ResetDrone();
            drone.RestorePracticeSnapshot(snapshot);

            Assert.That(drone.ForwardSpeed, Is.EqualTo(drone.DefaultForwardSpeed * 1.38f).Within(0.01f));
            Assert.That(drone.OrientationRoll, Is.EqualTo(90f).Within(0.01f));
            Assert.That(drone.SizeMultiplier, Is.EqualTo(0.62f).Within(0.01f));
        }
        finally
        {
            ClearScene();
        }
    }

    [UnityTest]
    public IEnumerator PracticeCheckpointRefreshesWhenPortalTriggersAfterSamePlaneRing()
    {
        PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();
        manager.StartPracticeLevel(8);
        yield return null;

        LevelManager levelManager = (LevelManager)GetField(manager, "levelManager");
        DroneController drone = levelManager.CurrentDrone;
        RingCheckpoint[] rings = UnityEngine.Object.FindObjectsByType<RingCheckpoint>(FindObjectsSortMode.None);
        SpeedPortal speedPortal = UnityEngine.Object.FindObjectsByType<SpeedPortal>(FindObjectsSortMode.None)
            .First(portal => rings.Any(ring => Mathf.Abs(ring.ZPosition - portal.ZPosition) < 0.01f));
        RingCheckpoint speedRing = rings.Single(ring => Mathf.Abs(ring.ZPosition - speedPortal.ZPosition) < 0.01f);

        drone.transform.position = speedPortal.transform.position;
        manager.RecordPracticeCheckpoint(speedRing, speedRing.RingIndex + 1);
        Assert.That(manager.CurrentPracticeCheckpoint.Position.z, Is.GreaterThan(speedPortal.ZPosition));

        speedPortal.SendMessage("OnTriggerEnter", drone.GetComponent<Collider>(), SendMessageOptions.RequireReceiver);
        yield return null;

        Assert.That(manager.CurrentPracticeCheckpoint.DroneSnapshot.ForwardSpeed, Is.GreaterThan(drone.DefaultForwardSpeed * 1.2f));
        ClearScene();
    }

    [Test]
    public void PracticeModeUiHasPracticeButtonsAndSeparateCompletionCopy()
    {
        string uiPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Scripts/UIManager.cs");
        string uiSource = File.ReadAllText(uiPath);

        Assert.That(uiSource, Does.Contain("Practice"));
        Assert.That(uiSource, Does.Contain("StartPracticeLevel"));
        Assert.That(uiSource, Does.Contain("Completed in Practice Mode"));
    }

    [Test]
    public void EveryLevelHasPracticalPracticeCheckpointCadence()
    {
        Type planner = RequireType("Drift.PracticeCheckpointPlanner");
        MethodInfo countMethod = planner.GetMethod("CountAutoCheckpoints", BindingFlags.Public | BindingFlags.Static);
        Assert.That(countMethod, Is.Not.Null);

        foreach (LevelDefinition level in LevelCatalog.GetLevels())
        {
            int count = (int)countMethod.Invoke(null, new object[] { level });
            int expectedMinimum = Mathf.Max(4, Mathf.CeilToInt(level.Rings.Length * 0.65f));
            Assert.That(count, Is.GreaterThanOrEqualTo(expectedMinimum), $"{level.DisplayName} should have frequent practice checkpoints.");
        }
    }

    [Test]
    public void PracticeModeCompletionDoesNotMarkProgressionComplete()
    {
        PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();
        manager.StartPracticeLevel(1);
        manager.CompleteCurrentLevel();

        Assert.That(manager.IsPracticeMode, Is.True);
        Assert.That(manager.Progression.IsLevelCompleted(1), Is.False);
    }

    [Test]
    public void ProgressionServiceUnlocksAndPersistsLevels()
    {
        PlayerPrefs.DeleteKey("Drift.HighestUnlockedLevel");
        PlayerPrefs.DeleteKey("Drift.CompletedLevels");

        Type progressionType = RequireType("Drift.ProgressionService");
        object progression = Activator.CreateInstance(progressionType);

        Assert.That(Invoke<int>(progression, "GetHighestUnlockedLevel"), Is.EqualTo(Drift.LevelCatalog.GetLevels().Count));
        Assert.That(Invoke<bool>(progression, "IsLevelUnlocked", 1), Is.True);
        Assert.That(Invoke<bool>(progression, "IsLevelUnlocked", Drift.LevelCatalog.GetLevels().Count), Is.True);

        Invoke(progression, "MarkLevelComplete", 1);

        Assert.That(Invoke<bool>(progression, "IsLevelCompleted", 1), Is.True);
        Assert.That(Invoke<bool>(progression, "IsLevelUnlocked", 2), Is.True);
        Assert.That(Invoke<int>(progression, "GetHighestUnlockedLevel"), Is.EqualTo(2));
    }

    private static Type RequireType(string typeName)
    {
        Type type = FindType(typeName);
        Assert.That(type, Is.Not.Null, $"{typeName} should exist.");
        return type;
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

    private static object GetField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} should be public.");
        return field.GetValue(target);
    }

    private static void AssertPortalNear(LevelDefinition level, PortalKind kind, float seconds, float zPerSecond, float tolerance)
    {
        float targetZ = seconds * zPerSecond;
        bool hasPortal = level.Portals.Any(portal => portal.Kind == kind && Mathf.Abs(portal.Position.z - targetZ) <= tolerance);

        Assert.That(hasPortal, Is.True, $"{level.DisplayName} should place {kind} near {seconds:0.0}s / z{targetZ:0}.");
    }

    private static T Invoke<T>(object target, string methodName, params object[] args)
    {
        return (T)Invoke(target, methodName, args);
    }

    private static object Invoke(object target, string methodName, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} should be public.");
        return method.Invoke(target, args);
    }

    private static void ClearScene()
    {
        foreach (GameObject gameObject in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }
}
#endif
