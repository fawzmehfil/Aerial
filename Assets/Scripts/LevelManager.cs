using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public sealed class LevelManager : MonoBehaviour
    {
        private GameObject currentLevelRoot;
        private DroneController currentDrone;
        private LevelDefinition currentLevel;
        private PortalManager portalManager;

        public DroneController CurrentDrone => currentDrone;
        public LevelDefinition CurrentLevel => currentLevel;

        public void LoadLevel(LevelDefinition level, RingManager ringManager, CameraFollow cameraFollow)
        {
            ClearLevel();
            currentLevel = level;
            currentLevelRoot = new GameObject(level.DisplayName);

            CreateLighting();
            CreateCourseFrame(level);

            GameObject droneObject = RuntimeVisualFactory.CreateDrone(new Vector3(0f, 0f, 0f), level.ForwardSpeed, level.Boundary);
            droneObject.transform.SetParent(currentLevelRoot.transform, true);
            currentDrone = droneObject.GetComponent<DroneController>();
            cameraFollow.SetTarget(currentDrone);

            List<RingCheckpoint> rings = new List<RingCheckpoint>();
            GameObject ringRoot = new GameObject("Rings");
            ringRoot.transform.SetParent(currentLevelRoot.transform, false);
            for (int i = 0; i < level.Rings.Length; i++)
            {
                rings.Add(RuntimeVisualFactory.CreateRing(ringRoot.transform, level.Rings[i], i, ringManager));
            }

            GameObject obstacleRoot = new GameObject("Obstacles");
            obstacleRoot.transform.SetParent(currentLevelRoot.transform, false);
            foreach (ObstacleSpec obstacle in level.Obstacles)
            {
                RuntimeVisualFactory.CreateObstacle(obstacleRoot.transform, obstacle);
            }

            portalManager = GetComponent<PortalManager>();
            if (portalManager == null)
            {
                portalManager = gameObject.AddComponent<PortalManager>();
            }

            GameObject portalRoot = new GameObject("Portals");
            portalRoot.transform.SetParent(currentLevelRoot.transform, false);
            foreach (PortalSpec portal in level.Portals)
            {
                portalManager.CreatePortal(portalRoot.transform, portal);
            }

            ringManager.Configure(rings, currentDrone);
        }

        public void ResetCurrentLevel(RingManager ringManager, CameraFollow cameraFollow)
        {
            if (currentLevel != null)
            {
                LoadLevel(currentLevel, ringManager, cameraFollow);
            }
        }

        public void ClearLevel()
        {
            if (currentLevelRoot != null)
            {
                Destroy(currentLevelRoot);
            }

            currentDrone = null;
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientLight = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.015f, 0.018f, 0.03f);
            RenderSettings.fogDensity = 0.008f;

            if (Object.FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Key Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                light.color = new Color(0.75f, 0.85f, 1f);
                lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            }
        }

        private void CreateCourseFrame(LevelDefinition level)
        {
            GameObject frameRoot = new GameObject("Course Frame");
            frameRoot.transform.SetParent(currentLevelRoot.transform, false);

            float courseLength = level.Rings[level.Rings.Length - 1].Position.z + 28f;
            float halfX = level.Boundary.x;
            float halfY = level.Boundary.y;

            RuntimeVisualFactory.CreateEnvironment(frameRoot.transform, level);

            for (float z = 0f; z <= courseLength; z += 16f)
            {
                RuntimeVisualFactory.CreateGuideRail(frameRoot.transform, new Vector3(-halfX, -halfY, z), new Vector3(0.08f, 0.08f, 8f));
                RuntimeVisualFactory.CreateGuideRail(frameRoot.transform, new Vector3(halfX, -halfY, z), new Vector3(0.08f, 0.08f, 8f));
                RuntimeVisualFactory.CreateGuideRail(frameRoot.transform, new Vector3(-halfX, halfY, z), new Vector3(0.08f, 0.08f, 8f));
                RuntimeVisualFactory.CreateGuideRail(frameRoot.transform, new Vector3(halfX, halfY, z), new Vector3(0.08f, 0.08f, 8f));
            }

            GameObject boundaryObject = new GameObject("Playable Boundary");
            boundaryObject.transform.SetParent(frameRoot.transform, false);
            boundaryObject.transform.position = new Vector3(0f, 0f, courseLength * 0.5f);
            BoxCollider boundaryCollider = boundaryObject.AddComponent<BoxCollider>();
            boundaryCollider.isTrigger = true;
            boundaryCollider.size = new Vector3(halfX * 2.25f, halfY * 2.25f, courseLength + 40f);
            boundaryObject.AddComponent<BoundaryReset>();
        }
    }
}
