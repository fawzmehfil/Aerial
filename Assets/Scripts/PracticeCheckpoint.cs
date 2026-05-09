using UnityEngine;

namespace Drift
{
    public struct DronePracticeSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float ForwardSpeed;
        public float TargetForwardSpeed;
        public float OrientationRoll;
        public float TargetOrientationRoll;
        public float SizeMultiplier;
        public float TargetSizeMultiplier;
        public bool HasTimedSpeedReturn;
        public float SpeedReturnSeconds;
        public bool HasTimedSizeReturn;
        public float SizeReturnSeconds;
    }

    public readonly struct PracticeCheckpoint
    {
        public PracticeCheckpoint(int checkpointNumber, int nextRingIndex, float soundtrackTime, DronePracticeSnapshot droneSnapshot)
        {
            CheckpointNumber = checkpointNumber;
            NextRingIndex = nextRingIndex;
            SoundtrackTime = soundtrackTime;
            DroneSnapshot = droneSnapshot;
        }

        public int CheckpointNumber { get; }
        public int NextRingIndex { get; }
        public float SoundtrackTime { get; }
        public DronePracticeSnapshot DroneSnapshot { get; }
        public Vector3 Position => DroneSnapshot.Position;
    }

    public static class PracticeCheckpointPlanner
    {
        public static bool ShouldCreateAutoCheckpoint(LevelDefinition level, int completedRingIndex)
        {
            return level != null && completedRingIndex >= 0 && completedRingIndex < level.Rings.Length;
        }

        public static int CountAutoCheckpoints(LevelDefinition level)
        {
            return level == null || level.Rings == null ? 0 : level.Rings.Length;
        }
    }
}
