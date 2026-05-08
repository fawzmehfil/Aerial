using UnityEngine;

namespace Drift
{
    [RequireComponent(typeof(Collider))]
    public abstract class PortalBase : MonoBehaviour
    {
        [SerializeField] private PortalKind portalKind;
        [SerializeField] private float duration = 4f;
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private float missThreshold = 3f;

        private bool consumed;
        private bool missed;

        public PortalKind Kind => portalKind;
        public bool IsConsumed => consumed;
        public bool IsMissed => missed;
        public float ZPosition => transform.position.z;

        public void Configure(PortalKind kind, float effectDuration, ParticleSystem particles)
        {
            portalKind = kind;
            duration = effectDuration;
            activationParticles = particles;
            consumed = false;
            missed = false;
        }

        protected abstract void Apply(DroneController drone, float effectDuration);

        public bool IsPastMissPlane(Vector3 dronePosition, float threshold)
        {
            return !consumed && !missed && dronePosition.z > ZPosition + threshold;
        }

        public void ResetForPracticeRespawn(float checkpointZ)
        {
            consumed = ZPosition < checkpointZ - 0.5f;
            missed = false;
            if (activationParticles != null)
            {
                activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void Update()
        {
            if (consumed || missed || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            DroneController drone = Object.FindFirstObjectByType<DroneController>();
            if (drone == null || !IsPastMissPlane(drone.transform.position, missThreshold))
            {
                return;
            }

            missed = true;
            GameManager.Instance.FailCurrentLevel("Missed Portal");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed || !other.CompareTag("Player"))
            {
                return;
            }

            DroneController drone = other.GetComponent<DroneController>();
            if (drone == null)
            {
                return;
            }

            consumed = true;
            if (activationParticles != null)
            {
                activationParticles.Play();
            }

            Apply(drone, duration);
        }
    }
}
