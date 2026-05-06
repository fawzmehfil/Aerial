using System.Collections;
using UnityEngine;

namespace Drift
{
    public sealed class DroneController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float forwardSpeed = 18f;
        [SerializeField] private float lateralSpeed = 8.5f;
        [SerializeField] private float acceleration = 28f;
        [SerializeField] private float damping = 20f;
        [SerializeField] private Vector2 boundary = new Vector2(8f, 5f);
        [SerializeField] private float snapImpulse = 7.5f;
        [SerializeField] private float burstImpulse = 7.2f;
        [SerializeField] private float abilityCooldown = 0.42f;

        [Header("Visual Tilt")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float maxRoll = 28f;
        [SerializeField] private float maxPitch = 15f;
        [SerializeField] private float tiltSmoothing = 10f;

        private Vector2 lateralVelocity;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private BoxCollider hitbox;
        private AudioSource abilityAudio;
        private ParticleSystem abilityParticles;
        private float defaultForwardSpeed;
        private float currentForwardSpeed;
        private float nextAbilityTime;
        private float abilityRollKick;
        private float abilityPitchKick;
        private float orientationRoll;
        private float targetOrientationRoll;
        private float sizeMultiplier = 1f;
        private Vector3 baseHitboxSize;
        private Coroutine speedRoutine;
        private Coroutine sizeRoutine;
        private Coroutine orientationRoutine;

        public float ForwardSpeed => currentForwardSpeed;
        public float DefaultForwardSpeed => defaultForwardSpeed;
        public Vector2 LateralVelocity => lateralVelocity;
        public float LateralSpeedFraction => Mathf.Clamp01(lateralVelocity.magnitude / lateralSpeed);
        public float SpeedFraction => defaultForwardSpeed <= 0.01f ? 1f : currentForwardSpeed / defaultForwardSpeed;
        public float OrientationRoll => orientationRoll;
        public float SizeMultiplier => sizeMultiplier;

        public void Configure(float speed, Vector2 playableBoundary, Transform droneVisualRoot)
        {
            defaultForwardSpeed = speed;
            forwardSpeed = speed;
            currentForwardSpeed = speed;
            boundary = playableBoundary;
            visualRoot = droneVisualRoot;
            hitbox = GetComponent<BoxCollider>();
            if (hitbox != null)
            {
                baseHitboxSize = hitbox.size;
            }

            abilityAudio = GetComponent<AudioSource>();
            abilityParticles = GetComponentInChildren<ParticleSystem>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void ResetDrone()
        {
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            lateralVelocity = Vector2.zero;
            currentForwardSpeed = defaultForwardSpeed > 0.01f ? defaultForwardSpeed : forwardSpeed;
            sizeMultiplier = 1f;
            orientationRoll = 0f;
            targetOrientationRoll = 0f;
            abilityRollKick = 0f;
            abilityPitchKick = 0f;
            UpdateHitboxScale();
            if (visualRoot != null)
            {
                visualRoot.localRotation = Quaternion.identity;
                visualRoot.localScale = Vector3.one;
            }
        }

        public void ApplySpeedMultiplier(float multiplier, float duration, string hudLabel)
        {
            if (speedRoutine != null)
            {
                StopCoroutine(speedRoutine);
            }

            speedRoutine = StartCoroutine(SpeedRoutine(multiplier, duration));
            GameManager.Instance?.ShowPortalEffect(hudLabel, duration);
        }

        public void ApplyOrientation(float rollDegrees, float duration, string hudLabel)
        {
            targetOrientationRoll = rollDegrees;
            if (orientationRoutine != null)
            {
                StopCoroutine(orientationRoutine);
            }

            orientationRoutine = StartCoroutine(OrientationRoutine(duration));
            GameManager.Instance?.ShowPortalEffect(hudLabel, duration);
        }

        public void ApplySize(float multiplier, float duration, string hudLabel)
        {
            if (sizeRoutine != null)
            {
                StopCoroutine(sizeRoutine);
            }

            sizeRoutine = StartCoroutine(SizeRoutine(multiplier, duration));
            GameManager.Instance?.ShowPortalEffect(hudLabel, duration);
        }

        private void Awake()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            hitbox = GetComponent<BoxCollider>();
            if (hitbox != null)
            {
                baseHitboxSize = hitbox.size;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                DampLateralVelocity();
                return;
            }

            HandleAbilityInput();

            Vector2 input = ReadMoveInput(orientationRoll);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            float sizeSpeedBonus = sizeMultiplier < 0.9f ? 1.08f : sizeMultiplier > 1.05f ? 0.92f : 1f;
            Vector2 targetVelocity = input * lateralSpeed * sizeSpeedBonus;
            float rate = input.sqrMagnitude > 0.01f ? acceleration : damping;
            lateralVelocity = Vector2.MoveTowards(lateralVelocity, targetVelocity, rate * Time.deltaTime);

            Vector3 delta = new Vector3(lateralVelocity.x, lateralVelocity.y, currentForwardSpeed) * Time.deltaTime;
            transform.position += delta;
            CheckBoundary();
            UpdateVisualTilt();
        }

        private static Vector2 ReadMoveInput(float rollDegrees)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                vertical += 1f;
            }

            Vector3 rotated = Quaternion.Euler(0f, 0f, rollDegrees) * new Vector3(horizontal, vertical, 0f);
            return new Vector2(rotated.x, rotated.y);
        }

        private void HandleAbilityInput()
        {
            if (Time.time < nextAbilityTime)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TriggerAbility(new Vector2(-snapImpulse, 0f), -95f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TriggerAbility(new Vector2(snapImpulse, 0f), 95f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector3 rotated = Quaternion.Euler(0f, 0f, orientationRoll) * Vector3.up;
                TriggerAbility(new Vector2(rotated.x, rotated.y) * burstImpulse, 0f, 34f);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Vector3 rotated = Quaternion.Euler(0f, 0f, orientationRoll) * Vector3.down;
                TriggerAbility(new Vector2(rotated.x, rotated.y) * burstImpulse, 0f, -34f);
            }
        }

        private void TriggerAbility(Vector2 impulse, float rollKick, float pitchKick)
        {
            lateralVelocity += impulse;
            lateralVelocity = Vector2.ClampMagnitude(lateralVelocity, lateralSpeed * 1.45f);
            abilityRollKick = rollKick;
            abilityPitchKick = pitchKick;
            nextAbilityTime = Time.time + abilityCooldown;

            if (abilityParticles != null)
            {
                abilityParticles.Play();
            }

            if (abilityAudio == null)
            {
                abilityAudio = GetComponent<AudioSource>();
            }

            if (abilityAudio != null)
            {
                abilityAudio.PlayOneShot(RuntimeVisualFactory.CreateToneClip("Ability Whoosh", 520f, 0.16f, 0.25f), 0.45f);
            }
        }

        private void DampLateralVelocity()
        {
            lateralVelocity = Vector2.MoveTowards(lateralVelocity, Vector2.zero, damping * Time.deltaTime);
            UpdateVisualTilt();
        }

        private void CheckBoundary()
        {
            Vector3 position = transform.position;
            bool outsideBoundary = Mathf.Abs(position.x) > boundary.x || Mathf.Abs(position.y) > boundary.y;
            position.x = Mathf.Clamp(position.x, -boundary.x, boundary.x);
            position.y = Mathf.Clamp(position.y, -boundary.y, boundary.y);
            transform.position = position;

            if (outsideBoundary && GameManager.Instance != null)
            {
                GameManager.Instance.FailCurrentLevel("Out of Bounds");
            }
        }

        private void UpdateVisualTilt()
        {
            if (visualRoot == null)
            {
                return;
            }

            float roll = -Mathf.Clamp(lateralVelocity.x / lateralSpeed, -1f, 1f) * maxRoll;
            float pitch = Mathf.Clamp(lateralVelocity.y / lateralSpeed, -1f, 1f) * maxPitch;
            abilityRollKick = Mathf.MoveTowards(abilityRollKick, 0f, 220f * Time.deltaTime);
            abilityPitchKick = Mathf.MoveTowards(abilityPitchKick, 0f, 120f * Time.deltaTime);
            Quaternion targetRotation = Quaternion.Euler(pitch + abilityPitchKick, 0f, roll + abilityRollKick + orientationRoll);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, tiltSmoothing * Time.deltaTime);
        }

        private IEnumerator SpeedRoutine(float multiplier, float duration)
        {
            float targetSpeed = defaultForwardSpeed * multiplier;
            float startSpeed = currentForwardSpeed;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                currentForwardSpeed = Mathf.Lerp(startSpeed, targetSpeed, t / 0.35f);
                yield return null;
            }

            currentForwardSpeed = targetSpeed;
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
                startSpeed = currentForwardSpeed;
                for (float t = 0f; t < 0.4f; t += Time.deltaTime)
                {
                    currentForwardSpeed = Mathf.Lerp(startSpeed, defaultForwardSpeed, t / 0.4f);
                    yield return null;
                }
            }

            currentForwardSpeed = defaultForwardSpeed;
            speedRoutine = null;
        }

        private IEnumerator OrientationRoutine(float duration)
        {
            float startRoll = orientationRoll;
            for (float t = 0f; t < 0.65f; t += Time.deltaTime)
            {
                orientationRoll = Mathf.LerpAngle(startRoll, targetOrientationRoll, t / 0.65f);
                yield return null;
            }

            orientationRoll = targetOrientationRoll;
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            orientationRoutine = null;
        }

        private IEnumerator SizeRoutine(float multiplier, float duration)
        {
            float startSize = sizeMultiplier;
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                sizeMultiplier = Mathf.Lerp(startSize, multiplier, t / 0.25f);
                UpdateHitboxScale();
                yield return null;
            }

            sizeMultiplier = multiplier;
            UpdateHitboxScale();
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
                startSize = sizeMultiplier;
                for (float t = 0f; t < 0.25f; t += Time.deltaTime)
                {
                    sizeMultiplier = Mathf.Lerp(startSize, 1f, t / 0.25f);
                    UpdateHitboxScale();
                    yield return null;
                }
            }

            sizeMultiplier = 1f;
            UpdateHitboxScale();
            sizeRoutine = null;
        }

        private void UpdateHitboxScale()
        {
            if (visualRoot != null)
            {
                visualRoot.localScale = Vector3.one * sizeMultiplier;
            }

            if (hitbox != null && baseHitboxSize != Vector3.zero)
            {
                hitbox.size = baseHitboxSize * sizeMultiplier;
            }
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider box = hitbox != null ? hitbox : GetComponent<BoxCollider>();
            if (box == null)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 1f, 0.95f, 0.45f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
