using UnityEngine;

namespace Drift
{
    [RequireComponent(typeof(Collider))]
    public abstract class PortalBase : MonoBehaviour
    {
        [SerializeField] private PortalKind portalKind;
        [SerializeField] private float duration = 4f;
        [SerializeField] private ParticleSystem activationParticles;

        private bool consumed;

        public PortalKind Kind => portalKind;

        public void Configure(PortalKind kind, float effectDuration, ParticleSystem particles)
        {
            portalKind = kind;
            duration = effectDuration;
            activationParticles = particles;
        }

        protected abstract void Apply(DroneController drone, float effectDuration);

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
