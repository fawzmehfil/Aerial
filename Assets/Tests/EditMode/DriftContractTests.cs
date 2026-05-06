#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
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
        Assert.That(levels.Length, Is.GreaterThanOrEqualTo(7));
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

        Assert.That(portalCount, Is.GreaterThanOrEqualTo(10));
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
