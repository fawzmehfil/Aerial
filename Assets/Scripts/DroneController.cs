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

        [Header("Visual Tilt")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float maxRoll = 28f;
        [SerializeField] private float maxPitch = 15f;
        [SerializeField] private float tiltSmoothing = 10f;

        private Vector2 lateralVelocity;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public float ForwardSpeed => forwardSpeed;
        public Vector2 LateralVelocity => lateralVelocity;
        public float LateralSpeedFraction => Mathf.Clamp01(lateralVelocity.magnitude / lateralSpeed);

        public void Configure(float speed, Vector2 playableBoundary, Transform droneVisualRoot)
        {
            forwardSpeed = speed;
            boundary = playableBoundary;
            visualRoot = droneVisualRoot;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        public void ResetDrone()
        {
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            lateralVelocity = Vector2.zero;
            if (visualRoot != null)
            {
                visualRoot.localRotation = Quaternion.identity;
            }
        }

        private void Awake()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                DampLateralVelocity();
                return;
            }

            Vector2 input = ReadInput();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Vector2 targetVelocity = input * lateralSpeed;
            float rate = input.sqrMagnitude > 0.01f ? acceleration : damping;
            lateralVelocity = Vector2.MoveTowards(lateralVelocity, targetVelocity, rate * Time.deltaTime);

            Vector3 delta = new Vector3(lateralVelocity.x, lateralVelocity.y, forwardSpeed) * Time.deltaTime;
            transform.position += transform.TransformDirection(delta);
            CheckBoundary();
            UpdateVisualTilt();
        }

        private static Vector2 ReadInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                vertical += 1f;
            }

            return new Vector2(horizontal, vertical);
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
            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, roll);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, tiltSmoothing * Time.deltaTime);
        }
    }
}
