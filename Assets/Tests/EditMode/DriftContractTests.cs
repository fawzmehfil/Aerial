#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Drift;
using NUnit.Framework;
using UnityEngine;

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
            "Drift.PortalManager"
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
}
#endif
