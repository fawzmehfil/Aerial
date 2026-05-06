using UnityEngine;

namespace Drift
{
    public sealed class ObstacleReset : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player") && GameManager.Instance != null)
            {
                GameManager.Instance.FailCurrentLevel("Crashed");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && GameManager.Instance != null)
            {
                GameManager.Instance.FailCurrentLevel("Crashed");
            }
        }
    }
}
