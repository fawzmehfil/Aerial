using UnityEngine;

namespace Drift
{
    public sealed class PortalManager : MonoBehaviour
    {
        public PortalBase CreatePortal(Transform parent, PortalSpec spec)
        {
            return RuntimeVisualFactory.CreatePortal(parent, spec);
        }
    }
}
