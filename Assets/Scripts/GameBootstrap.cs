using UnityEngine;

namespace Drift
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeBeforeSceneLoads()
        {
            GameManager.EnsureRuntime();
        }

        private void Awake()
        {
            GameManager.EnsureRuntime();
        }
    }
}
