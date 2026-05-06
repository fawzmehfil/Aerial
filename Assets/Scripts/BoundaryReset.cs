using UnityEngine;

namespace Drift
{
    public sealed class BoundaryReset : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && GameManager.Instance != null)
            {
                GameManager.Instance.FailCurrentLevel("Out of Bounds");
            }
        }
    }
}
