using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public static class RuntimeVisualFactory
    {
        public static readonly Color CurrentRingColor = new Color(0.2f, 1f, 0.95f);
        public static readonly Color FutureRingColor = new Color(0.35f, 0.25f, 0.95f);
        public static readonly Color CompletedRingColor = new Color(0.15f, 0.95f, 0.35f);
        private static readonly Vector3 DroneHitboxSize = new Vector3(0.95f, 0.38f, 1.05f);

        private static Material droneMaterial;
        private static Material droneDarkMaterial;
        private static Material droneHitboxMaterial;
        private static Material droneAccentMaterial;
        private static Material obstacleMaterial;
        private static Material debrisMaterial;
        private static Material guideMaterial;
        private static Material panelMaterial;

        public static Material CreateNeonMaterial(string name, Color color, float emissionStrength, bool transparent = false)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emissionStrength);
            }

            if (transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }

            return material;
        }

        public static GameObject CreateDrone(Vector3 position, float forwardSpeed, Vector2 boundary)
        {
            GameObject root = new GameObject("Drone");
            root.tag = "Player";
            root.transform.position = position;

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = DroneHitboxSize;

            GameObject visualRoot = new GameObject("Drone Visual");
            visualRoot.transform.SetParent(root.transform, false);

            if (droneMaterial == null)
            {
                droneMaterial = CreateNeonMaterial("Drone White", new Color(0.82f, 0.9f, 1f), 0.4f);
                droneDarkMaterial = CreateNeonMaterial("Drone Graphite", new Color(0.08f, 0.1f, 0.14f), 0.1f);
            }

            if (droneHitboxMaterial == null)
            {
                droneHitboxMaterial = CreateNeonMaterial("Drone Hitbox Cyan", new Color(0.16f, 1f, 0.95f, 0.2f), 1.8f, true);
                droneAccentMaterial = CreateNeonMaterial("Drone Accent Cyan", new Color(0.16f, 1f, 0.95f), 2.8f);
            }

            AddPrimitive(visualRoot.transform, PrimitiveType.Cube, "Hitbox Core", Vector3.zero, DroneHitboxSize, droneHitboxMaterial);
            AddPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Hitbox Center", Vector3.zero, Vector3.one * 0.16f, droneAccentMaterial);
            AddPrimitive(visualRoot.transform, PrimitiveType.Cube, "Hitbox Nose Marker", new Vector3(0f, 0.02f, DroneHitboxSize.z * 0.48f), Vector3.one, droneAccentMaterial)
                .transform.localScale = new Vector3(0.28f, 0.08f, 0.08f);

            GameObject chassis = AddPrimitive(visualRoot.transform, PrimitiveType.Cube, "Chassis", Vector3.zero, Vector3.one, droneDarkMaterial);
            chassis.transform.localScale = new Vector3(0.78f, 0.26f, 0.78f);

            AddPrimitive(visualRoot.transform, PrimitiveType.Cube, "Nose", new Vector3(0f, 0.03f, 0.5f), Vector3.one, droneMaterial)
                .transform.localScale = new Vector3(0.42f, 0.2f, 0.26f);

            AddArm(visualRoot.transform, new Vector3(-0.56f, 0f, 0.42f), 34f);
            AddArm(visualRoot.transform, new Vector3(0.56f, 0f, 0.42f), -34f);
            AddArm(visualRoot.transform, new Vector3(-0.56f, 0f, -0.42f), -34f);
            AddArm(visualRoot.transform, new Vector3(0.56f, 0f, -0.42f), 34f);

            Vector3[] rotorPositions =
            {
                new Vector3(-0.82f, 0f, 0.68f),
                new Vector3(0.82f, 0f, 0.68f),
                new Vector3(-0.82f, 0f, -0.68f),
                new Vector3(0.82f, 0f, -0.68f)
            };

            Material engineMaterial = CreateNeonMaterial("Engine Cyan", new Color(0.15f, 0.95f, 1f), 3f);
            foreach (Vector3 rotorPosition in rotorPositions)
            {
                GameObject rotor = AddPrimitive(visualRoot.transform, PrimitiveType.Cylinder, "Rotor", rotorPosition, Vector3.one, droneDarkMaterial);
                rotor.transform.localScale = new Vector3(0.26f, 0.045f, 0.26f);

                GameObject glow = AddPrimitive(visualRoot.transform, PrimitiveType.Sphere, "Engine Glow", rotorPosition + Vector3.down * 0.18f, Vector3.one, engineMaterial);
                glow.transform.localScale = Vector3.one * 0.13f;

                TrailRenderer trail = glow.AddComponent<TrailRenderer>();
                trail.time = 0.28f;
                trail.startWidth = 0.08f;
                trail.endWidth = 0f;
                trail.material = engineMaterial;
                trail.startColor = new Color(0.15f, 0.95f, 1f, 0.55f);
                trail.endColor = new Color(0.15f, 0.95f, 1f, 0f);
            }

            CreateAbilityParticles(visualRoot.transform);

            DroneController controller = root.AddComponent<DroneController>();
            controller.Configure(forwardSpeed, boundary, visualRoot.transform);

            AudioSource engine = root.AddComponent<AudioSource>();
            engine.clip = CreateToneClip("Engine Hum", 82f, 1.5f, 0.12f);
            engine.loop = true;
            engine.volume = 0.2f;
            engine.spatialBlend = 0.35f;
            engine.Play();

            return root;
        }

        public static RingCheckpoint CreateRing(Transform parent, RingSpec spec, int index, RingManager manager)
        {
            GameObject root = new GameObject($"Ring {index + 1:00}");
            root.transform.SetParent(parent, false);
            root.transform.position = spec.Position;

            Material segmentMaterial = CreateNeonMaterial($"Ring {index + 1:00} Material", FutureRingColor, 0.8f, true);
            List<Renderer> renderers = new List<Renderer>();
            int segmentCount = 32;
            float thickness = 0.18f;
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i / (float)segmentCount * Mathf.PI * 2f;
                Vector3 localPosition = new Vector3(Mathf.Cos(angle) * spec.Radius, Mathf.Sin(angle) * spec.Radius, 0f);
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "Ring Segment";
                segment.transform.SetParent(root.transform, false);
                segment.transform.localPosition = localPosition;
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
                segment.transform.localScale = new Vector3(spec.Radius * 0.19f, thickness, 0.34f);
                DestroyObject(segment.GetComponent<Collider>());
                Renderer renderer = segment.GetComponent<Renderer>();
                renderer.sharedMaterial = segmentMaterial;
                renderers.Add(renderer);
            }

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(spec.Radius * 2f, spec.Radius * 2f, 2.8f);

            ParticleSystem particles = CreateRingParticles(root.transform, CurrentRingColor);
            ParticleSystem missParticles = CreateRingParticles(root.transform, new Color(1f, 0.12f, 0.08f));

            RingCheckpoint checkpoint = root.AddComponent<RingCheckpoint>();
            checkpoint.Configure(index, spec.Radius * 0.84f, manager, renderers.ToArray(), particles, missParticles, trigger);
            return checkpoint;
        }

        public static GameObject CreateObstacle(Transform parent, ObstacleSpec spec)
        {
            Material material = GetObstacleMaterial(spec.Kind);
            PrimitiveType primitiveType = spec.Kind == ObstacleKind.CosmicDebris ? PrimitiveType.Sphere : PrimitiveType.Cube;
            GameObject obstacle = GameObject.CreatePrimitive(primitiveType);
            obstacle.name = "Obstacle";
            obstacle.transform.SetParent(parent, false);
            obstacle.transform.position = spec.Position;
            obstacle.transform.rotation = Quaternion.Euler(0f, 0f, spec.RotationZ);
            obstacle.transform.localScale = spec.Size;
            obstacle.GetComponent<Renderer>().sharedMaterial = material;

            Collider collider = obstacle.GetComponent<Collider>();
            collider.isTrigger = true;
            obstacle.AddComponent<ObstacleReset>();

            if (spec.Kind == ObstacleKind.Pillar)
            {
                AddPrimitive(obstacle.transform, PrimitiveType.Cube, "Danger Light", Vector3.zero, new Vector3(1.04f, 0.08f, 1.04f), CreateNeonMaterial("Pillar Danger Light", new Color(1f, 0.25f, 0.08f), 2.8f));
            }
            else if (spec.Kind == ObstacleKind.CosmicDebris)
            {
                obstacle.transform.rotation = Quaternion.Euler(spec.RotationZ * 0.7f, spec.RotationZ * 1.3f, spec.RotationZ);
            }

            return obstacle;
        }

        public static PortalBase CreatePortal(Transform parent, PortalSpec spec)
        {
            GameObject root = new GameObject($"{spec.Kind} Portal");
            root.transform.SetParent(parent, false);
            root.transform.position = spec.Position;

            Color color = GetPortalColor(spec.Kind);
            Material material = CreateNeonMaterial($"{spec.Kind} Material", color, 3.5f, true);
            int segmentCount = 24;
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = i / (float)segmentCount * Mathf.PI * 2f;
                Vector3 localPosition = new Vector3(Mathf.Cos(angle) * spec.Radius, Mathf.Sin(angle) * spec.Radius, 0f);
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "Portal Segment";
                segment.transform.SetParent(root.transform, false);
                segment.transform.localPosition = localPosition;
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
                segment.transform.localScale = new Vector3(spec.Radius * 0.24f, 0.11f, 0.48f);
                segment.GetComponent<Renderer>().sharedMaterial = material;
                DestroyObject(segment.GetComponent<Collider>());
            }

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            core.name = "Portal Core";
            core.transform.SetParent(root.transform, false);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            core.transform.localScale = new Vector3(spec.Radius * 1.45f, 0.04f, spec.Radius * 1.45f);
            core.GetComponent<Renderer>().sharedMaterial = CreateNeonMaterial($"{spec.Kind} Core", new Color(color.r, color.g, color.b, 0.16f), 1.2f, true);
            DestroyObject(core.GetComponent<Collider>());

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(spec.Radius * 1.7f, spec.Radius * 1.7f, 2.2f);

            ParticleSystem particles = CreateRingParticles(root.transform, color);
            PortalBase portal;
            if (spec.Kind == PortalKind.SpeedFast || spec.Kind == PortalKind.SpeedSlow || spec.Kind == PortalKind.SpeedNormal)
            {
                portal = root.AddComponent<SpeedPortal>();
            }
            else if (spec.Kind == PortalKind.GravityNormal || spec.Kind == PortalKind.GravitySideways)
            {
                portal = root.AddComponent<GravityPortal>();
            }
            else
            {
                portal = root.AddComponent<SizePortal>();
            }

            portal.Configure(spec.Kind, spec.Duration, particles);
            return portal;
        }

        public static void CreateEnvironment(Transform parent, LevelDefinition level)
        {
            float courseLength = level.Rings[level.Rings.Length - 1].Position.z + 34f;
            switch (level.Theme)
            {
                case EnvironmentTheme.MinimalLightTunnel:
                    CreateRectangularTunnel(parent, level.Boundary, courseLength, new Color(0.72f, 0.74f, 0.7f), Color.white, 18f, false);
                    break;
                case EnvironmentTheme.NeonCyanCorridor:
                    CreateRectangularTunnel(parent, level.Boundary, courseLength, new Color(0.05f, 0.08f, 0.16f), new Color(0.08f, 0.95f, 1f), 12f, true);
                    break;
                case EnvironmentTheme.RedGateIndustrial:
                    CreateRectangularTunnel(parent, level.Boundary, courseLength, new Color(0.12f, 0.065f, 0.075f), new Color(1f, 0.18f, 0.12f), 14f, true);
                    break;
                case EnvironmentTheme.CosmicRingVoid:
                    CreateCosmicVoid(parent, level.Boundary, courseLength);
                    break;
                default:
                    CreateRectangularTunnel(parent, level.Boundary, courseLength * 0.45f, new Color(0.04f, 0.075f, 0.13f), new Color(0.1f, 0.9f, 1f), 14f, true);
                    CreateCosmicVoid(parent, level.Boundary, courseLength);
                    CreateRectangularTunnel(parent, level.Boundary, courseLength, new Color(0.1f, 0.06f, 0.08f), new Color(1f, 0.16f, 0.2f), 20f, false, courseLength * 0.55f);
                    break;
            }
        }

        public static GameObject CreateGuideRail(Transform parent, Vector3 position, Vector3 scale)
        {
            if (guideMaterial == null)
            {
                guideMaterial = CreateNeonMaterial("Guide Rail Blue", new Color(0.08f, 0.28f, 1f), 1.6f, true);
            }

            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Neon Guide Rail";
            rail.transform.SetParent(parent, false);
            rail.transform.position = position;
            rail.transform.localScale = scale;
            rail.GetComponent<Renderer>().sharedMaterial = guideMaterial;
            DestroyObject(rail.GetComponent<Collider>());
            return rail;
        }

        public static AudioClip CreateToneClip(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static void AddArm(Transform parent, Vector3 position, float angle)
        {
            GameObject arm = AddPrimitive(parent, PrimitiveType.Cube, "Arm", position * 0.5f, Vector3.one, droneMaterial);
            arm.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            arm.transform.localScale = new Vector3(0.12f, 0.12f, 0.96f);
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            DestroyObject(primitive.GetComponent<Collider>());
            return primitive;
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private static ParticleSystem CreateRingParticles(Transform parent, Color color)
        {
            GameObject particleObject = new GameObject("Pass Burst");
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.startLifetime = 0.35f;
            main.startSpeed = 8f;
            main.startSize = 0.08f;
            main.startColor = color;
            main.maxParticles = 96;
            main.loop = false;
            main.playOnAwake = false;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.4f;

            return particles;
        }

        private static ParticleSystem CreateAbilityParticles(Transform parent)
        {
            GameObject particleObject = new GameObject("Ability Burst");
            particleObject.transform.SetParent(parent, false);
            particleObject.transform.localPosition = Vector3.zero;
            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.startLifetime = 0.18f;
            main.startSpeed = 5.5f;
            main.startSize = 0.05f;
            main.startColor = new Color(0.35f, 1f, 0.95f, 0.85f);
            main.maxParticles = 48;
            main.loop = false;
            main.playOnAwake = false;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            return particles;
        }

        private static Material GetObstacleMaterial(ObstacleKind kind)
        {
            if (kind == ObstacleKind.CosmicDebris)
            {
                if (debrisMaterial == null)
                {
                    debrisMaterial = CreateNeonMaterial("Cosmic Debris Violet", new Color(0.36f, 0.2f, 0.58f), 0.55f);
                }

                return debrisMaterial;
            }

            if (obstacleMaterial == null)
            {
                obstacleMaterial = CreateNeonMaterial("Obstacle Red Magenta", new Color(0.95f, 0.16f, 0.35f), 2.1f);
            }

            return obstacleMaterial;
        }

        private static Color GetPortalColor(PortalKind kind)
        {
            switch (kind)
            {
                case PortalKind.SpeedFast:
                    return new Color(1f, 0.32f, 0.08f);
                case PortalKind.SpeedSlow:
                    return new Color(0.08f, 0.72f, 1f);
                case PortalKind.GravitySideways:
                    return new Color(0.95f, 0.95f, 1f);
                case PortalKind.SizeSmall:
                case PortalKind.SizeLarge:
                    return new Color(0.74f, 0.18f, 1f);
                default:
                    return new Color(0.38f, 1f, 0.52f);
            }
        }

        private static void CreateRectangularTunnel(Transform parent, Vector2 boundary, float courseLength, Color surfaceColor, Color lightColor, float spacing, bool addPanels, float startZ = 0f)
        {
            Material surface = GetPanelMaterial(surfaceColor);
            Material light = CreateNeonMaterial("Tunnel Light", lightColor, 2.5f, true);
            float width = boundary.x * 2.3f;
            float height = boundary.y * 2.35f;

            for (float z = startZ; z <= courseLength; z += spacing)
            {
                GameObject frame = new GameObject("Light Frame");
                frame.transform.SetParent(parent, false);
                frame.transform.position = new Vector3(0f, 0f, z);
                AddPrimitive(frame.transform, PrimitiveType.Cube, "Top Light", new Vector3(0f, height * 0.5f, 0f), new Vector3(width, 0.08f, 0.16f), light);
                AddPrimitive(frame.transform, PrimitiveType.Cube, "Bottom Light", new Vector3(0f, -height * 0.5f, 0f), new Vector3(width, 0.08f, 0.16f), light);
                AddPrimitive(frame.transform, PrimitiveType.Cube, "Left Light", new Vector3(-width * 0.5f, 0f, 0f), new Vector3(0.08f, height, 0.16f), light);
                AddPrimitive(frame.transform, PrimitiveType.Cube, "Right Light", new Vector3(width * 0.5f, 0f, 0f), new Vector3(0.08f, height, 0.16f), light);

                if (addPanels && Mathf.FloorToInt(z / spacing) % 2 == 0)
                {
                    AddPrimitive(frame.transform, PrimitiveType.Cube, "Wall Panel Left", new Vector3(-width * 0.53f, 0f, spacing * 0.25f), new Vector3(0.08f, height * 0.72f, spacing * 0.55f), surface);
                    AddPrimitive(frame.transform, PrimitiveType.Cube, "Wall Panel Right", new Vector3(width * 0.53f, 0f, spacing * 0.25f), new Vector3(0.08f, height * 0.72f, spacing * 0.55f), surface);
                }
            }
        }

        private static void CreateCosmicVoid(Transform parent, Vector2 boundary, float courseLength)
        {
            Material starMaterial = CreateNeonMaterial("Star Magenta", new Color(1f, 0.28f, 0.86f), 2.5f, true);
            Material terrainMaterial = CreateNeonMaterial("Alien Terrain", new Color(0.18f, 0.08f, 0.28f), 0.35f);
            for (int i = 0; i < 84; i++)
            {
                float z = 20f + (i * 17f) % Mathf.Max(courseLength, 40f);
                float x = Mathf.Sin(i * 6.17f) * boundary.x * 3.4f;
                float y = boundary.y + 4f + Mathf.Abs(Mathf.Cos(i * 2.31f)) * 7f;
                GameObject star = AddPrimitive(parent, PrimitiveType.Sphere, "Star", new Vector3(x, y, z), Vector3.one * (0.05f + (i % 3) * 0.025f), starMaterial);
                star.transform.localScale = Vector3.one * (0.06f + (i % 4) * 0.025f);
            }

            for (float z = 18f; z <= courseLength; z += 22f)
            {
                float x = Mathf.Sin(z * 0.19f) * boundary.x * 1.2f;
                AddPrimitive(parent, PrimitiveType.Cube, "Distant Ridge", new Vector3(x, -boundary.y - 4.2f, z), new Vector3(boundary.x * 2.8f, 1.2f + Mathf.Sin(z) * 0.4f, 8f), terrainMaterial);
            }
        }

        private static Material GetPanelMaterial(Color color)
        {
            panelMaterial = CreateNeonMaterial("Tunnel Panel", color, 0.05f);
            return panelMaterial;
        }
    }
}
