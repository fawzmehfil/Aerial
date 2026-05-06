using UnityEngine;

namespace Drift
{
    public enum RingVisualState
    {
        Future,
        Current,
        Completed
    }

    public sealed class RingCheckpoint : MonoBehaviour
    {
        [SerializeField] private int ringIndex;
        [SerializeField] private float passRadius = 2f;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private ParticleSystem passParticles;
        [SerializeField] private ParticleSystem missParticles;
        [SerializeField] private Collider triggerCollider;

        private RingManager ringManager;
        private RingVisualState state;
        private float missPlaneOffset = 4f;

        public int RingIndex => ringIndex;
        public float ZPosition => transform.position.z;
        public float PassRadius => passRadius;
        public bool IsCompleted => state == RingVisualState.Completed;

        public void Configure(int index, float radius, RingManager manager, Renderer[] ringRenderers, ParticleSystem particles, ParticleSystem missedParticles, Collider trigger)
        {
            ringIndex = index;
            passRadius = radius;
            ringManager = manager;
            renderers = ringRenderers;
            passParticles = particles;
            missParticles = missedParticles;
            triggerCollider = trigger;
            SetState(RingVisualState.Future);
        }

        public void SetState(RingVisualState nextState)
        {
            state = nextState;
            Color color = RuntimeVisualFactory.FutureRingColor;
            float alpha = 0.5f;
            bool triggerEnabled = false;

            if (nextState == RingVisualState.Current)
            {
                color = RuntimeVisualFactory.CurrentRingColor;
                alpha = 1f;
                triggerEnabled = true;
            }
            else if (nextState == RingVisualState.Completed)
            {
                color = RuntimeVisualFactory.CompletedRingColor;
                alpha = 0.18f;
            }

            if (triggerCollider != null)
            {
                triggerCollider.enabled = triggerEnabled;
            }

            if (renderers == null)
            {
                return;
            }

            foreach (Renderer ringRenderer in renderers)
            {
                if (ringRenderer == null)
                {
                    continue;
                }

                Material material = ringRenderer.material;
                material.color = new Color(color.r, color.g, color.b, alpha);
                material.SetColor("_EmissionColor", color * (nextState == RingVisualState.Current ? 3.2f : 0.8f));
            }
        }

        public void PlayPassEffect()
        {
            if (passParticles != null)
            {
                passParticles.Play();
            }
        }

        public void PlayMissEffect()
        {
            if (missParticles != null)
            {
                missParticles.Play();
            }

            foreach (Renderer ringRenderer in renderers)
            {
                if (ringRenderer == null)
                {
                    continue;
                }

                Material material = ringRenderer.material;
                Color missColor = new Color(1f, 0.12f, 0.08f);
                material.color = missColor;
                material.SetColor("_EmissionColor", missColor * 4.5f);
            }
        }

        public bool IsPastMissPlane(Vector3 dronePosition, float threshold)
        {
            missPlaneOffset = threshold;
            return dronePosition.z > ZPosition + threshold;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (state != RingVisualState.Current || !other.CompareTag("Player"))
            {
                return;
            }

            Vector3 local = transform.InverseTransformPoint(other.transform.position);
            float radialDistance = new Vector2(local.x, local.y).magnitude;
            if (radialDistance <= passRadius && ringManager != null)
            {
                ringManager.PassRing(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.95f, 0.45f);
            Gizmos.DrawWireSphere(transform.position, passRadius);
            Gizmos.color = new Color(1f, 0.18f, 0.12f, 0.55f);
            Vector3 center = transform.position + Vector3.forward * missPlaneOffset;
            Gizmos.DrawWireCube(center, new Vector3(passRadius * 2.2f, passRadius * 2.2f, 0.08f));
        }
    }
}
