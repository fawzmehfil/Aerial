using UnityEngine;

namespace Drift
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, -9f);
        [SerializeField] private float positionSmoothTime = 0.18f;
        [SerializeField] private float rotationSmoothing = 9f;
        [SerializeField] private float baseFov = 70f;
        [SerializeField] private float maxFovKick = 8f;
        [SerializeField] private float speedFovKick = 14f;

        private DroneController target;
        private Camera cameraComponent;
        private Vector3 velocity;

        public void SetTarget(DroneController drone)
        {
            target = drone;
            if (target != null)
            {
                transform.position = target.transform.TransformPoint(offset);
                transform.LookAt(target.transform.position + Vector3.forward * 8f);
            }
        }

        public void SetPositionSmoothing(float value)
        {
            positionSmoothTime = Mathf.Clamp(value, 0.08f, 0.32f);
        }

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            cameraComponent.fieldOfView = baseFov;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.transform.TransformPoint(offset);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

            Vector3 up = Quaternion.AngleAxis(target.OrientationRoll, Vector3.forward) * Vector3.up;
            Vector3 lookPoint = target.transform.position + Vector3.forward * 10f + up * 0.4f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothing * Time.deltaTime);

            float speedKick = Mathf.Clamp(target.SpeedFraction - 1f, -0.35f, 0.65f) * speedFovKick;
            float targetFov = baseFov + target.LateralSpeedFraction * maxFovKick + speedKick;
            cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFov, 5f * Time.deltaTime);
        }
    }
}
