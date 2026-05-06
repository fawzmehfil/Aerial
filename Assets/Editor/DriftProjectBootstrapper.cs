#if UNITY_EDITOR
using Drift;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DriftProjectBootstrapper
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string PrefabFolder = "Assets/Prefabs";

    [InitializeOnLoadMethod]
    private static void CreateProjectAssetsOnLoad()
    {
        EditorApplication.delayCall += EnsureProjectAssets;
    }

    [MenuItem("Drift/Rebuild Generated Scene And Prefabs")]
    public static void EnsureProjectAssets()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        CreateMainSceneIfMissing();
        CreatePrefabAssetsIfMissing();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateMainSceneIfMissing()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) != null)
        {
            EnsureBuildSettings();
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = new GameObject("Game Bootstrap");
        bootstrap.AddComponent<GameBootstrap>();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.01f, 0.012f, 0.022f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow>();
        cameraObject.transform.position = new Vector3(0f, 2f, -9f);

        EditorSceneManager.SaveScene(scene, MainScenePath);
        EnsureBuildSettings();
    }

    private static void CreatePrefabAssetsIfMissing()
    {
        SavePrefabIfMissing("Drone.prefab", () => RuntimeVisualFactory.CreateDrone(Vector3.zero, 18f, new Vector2(8f, 5f)));

        SavePrefabIfMissing("Ring.prefab", () =>
        {
            RingCheckpoint ring = RuntimeVisualFactory.CreateRing(null, new RingSpec(0f, 0f, 0f, 2.5f), 0, null);
            return ring.gameObject;
        });

        SavePrefabIfMissing("Obstacle.prefab", () =>
            RuntimeVisualFactory.CreateObstacle(null, new ObstacleSpec(Vector3.zero, new Vector3(2f, 2f, 2f))));

        SavePrefabIfMissing("LevelContainer.prefab", () => new GameObject("Level Container"));

        SavePrefabIfMissing("ParticleBurst.prefab", () =>
        {
            GameObject ring = new GameObject("Particle Burst");
            ParticleSystem particles = ring.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startColor = RuntimeVisualFactory.CurrentRingColor;
            main.startLifetime = 0.35f;
            main.startSpeed = 8f;
            return ring;
        });
    }

    private static void SavePrefabIfMissing(string fileName, System.Func<GameObject> create)
    {
        string path = $"{PrefabFolder}/{fileName}";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            return;
        }

        GameObject instance = create();
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }

    private static void EnsureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainScenePath, true)
        };
    }
}
#endif
