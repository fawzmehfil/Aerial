using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public sealed class RingManager : MonoBehaviour
    {
        [SerializeField] private float missThreshold = 4f;

        private readonly List<RingCheckpoint> rings = new List<RingCheckpoint>();
        private DroneController drone;
        private int currentRingIndex;
        private bool complete;

        public int CurrentRingNumber => Mathf.Min(currentRingIndex + 1, rings.Count);
        public int TotalRings => rings.Count;

        public void Configure(IReadOnlyList<RingCheckpoint> levelRings, DroneController levelDrone)
        {
            rings.Clear();
            rings.AddRange(levelRings);
            drone = levelDrone;
            currentRingIndex = 0;
            complete = false;

            for (int i = 0; i < rings.Count; i++)
            {
                rings[i].SetState(i == 0 ? RingVisualState.Current : RingVisualState.Future);
            }
        }

        public void PassRing(RingCheckpoint ring)
        {
            if (complete || currentRingIndex >= rings.Count || rings[currentRingIndex] != ring)
            {
                return;
            }

            ring.SetState(RingVisualState.Completed);
            ring.PlayPassEffect();
            GameManager.Instance.PlayRingPassFeedback();
            currentRingIndex++;

            if (currentRingIndex >= rings.Count)
            {
                complete = true;
                GameManager.Instance.CompleteCurrentLevel();
                return;
            }

            rings[currentRingIndex].SetState(RingVisualState.Current);
            GameManager.Instance.UpdateHudProgress();
        }

        public void ResetRings()
        {
            currentRingIndex = 0;
            complete = false;
            for (int i = 0; i < rings.Count; i++)
            {
                rings[i].SetState(i == 0 ? RingVisualState.Current : RingVisualState.Future);
            }
        }

        private void Update()
        {
            if (complete || drone == null || rings.Count == 0 || currentRingIndex >= rings.Count)
            {
                return;
            }

            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            RingCheckpoint currentRing = rings[currentRingIndex];
            if (drone.transform.position.z > currentRing.ZPosition + missThreshold)
            {
                GameManager.Instance.FailCurrentLevel("Missed Ring");
            }
        }
    }
}
