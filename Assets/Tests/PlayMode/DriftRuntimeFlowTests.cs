#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS
using System.Collections;
using Drift;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class DriftRuntimeFlowTests
{
    [UnityTest]
    public IEnumerator BootstrapCreatesMainMenuAndCanStartLevel()
    {
        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();

        yield return null;
        Assert.That(manager, Is.Not.Null);
        Assert.That(Object.FindFirstObjectByType<UIManager>(), Is.Not.Null);

        manager.StartLevel(1);
        yield return null;

        Assert.That(Object.FindFirstObjectByType<DroneController>(), Is.Not.Null);
        Assert.That(Object.FindObjectsByType<RingCheckpoint>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(10));
        Assert.That(Object.FindFirstObjectByType<CameraFollow>(), Is.Not.Null);
    }

    [UnityTest]
    public IEnumerator CompletingLevelUnlocksNextLevel()
    {
        PlayerPrefs.DeleteKey(ProgressionService.HighestUnlockedLevelKey);
        PlayerPrefs.DeleteKey(ProgressionService.CompletedLevelsKey);

        ClearScene();
        GameManager manager = GameManager.EnsureRuntime();
        yield return null;

        manager.StartLevel(1);
        yield return null;
        manager.CompleteCurrentLevel();
        yield return null;

        Assert.That(manager.Progression.IsLevelCompleted(1), Is.True);
        Assert.That(manager.Progression.IsLevelUnlocked(2), Is.True);
    }

    private static void ClearScene()
    {
        foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
#endif
