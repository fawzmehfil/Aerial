#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

required_files=(
  "GOALS.md"
  "README.md"
  "Packages/manifest.json"
  "ProjectSettings/ProjectVersion.txt"
  "ProjectSettings/EditorBuildSettings.asset"
  "Assets/Scenes/Main.unity"
  "Assets/Scripts/DroneController.cs"
  "Assets/Scripts/CameraFollow.cs"
  "Assets/Scripts/RingCheckpoint.cs"
  "Assets/Scripts/RingManager.cs"
  "Assets/Scripts/LevelManager.cs"
  "Assets/Scripts/GameManager.cs"
  "Assets/Scripts/UIManager.cs"
  "Assets/Scripts/LevelSelectManager.cs"
  "Assets/Scripts/BoundaryReset.cs"
  "Assets/Scripts/ObstacleReset.cs"
  "Assets/Scripts/LevelCatalog.cs"
  "Assets/Scripts/PracticeCheckpoint.cs"
  "Assets/Scripts/ProgressionService.cs"
  "Assets/Prefabs/Drone.prefab"
  "Assets/Prefabs/Ring.prefab"
  "Assets/Prefabs/Obstacle.prefab"
  "Assets/Prefabs/LevelContainer.prefab"
  "Assets/Prefabs/ParticleBurst.prefab"
  "Assets/Editor/DriftProjectVerifier.cs"
  "Assets/Tests/EditMode/DriftContractTests.cs"
  "Assets/Tests/PlayMode/DriftRuntimeFlowTests.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "missing: $file" >&2
    exit 1
  fi
done

grep -q "Assets/Scenes/Main.unity" ProjectSettings/EditorBuildSettings.asset
grep -q "DriftProjectVerifier" Assets/Editor/DriftProjectVerifier.cs
for dependency in \
  '"com.unity.modules.audio": "1.0.0"' \
  '"com.unity.modules.imgui": "1.0.0"' \
  '"com.unity.modules.particlesystem": "1.0.0"' \
  '"com.unity.modules.physics": "1.0.0"' \
  '"com.unity.modules.ui": "1.0.0"'; do
  grep -q "$dependency" Packages/manifest.json
done
if find Assets -name '*.asmdef' | grep -q .; then
  echo "unexpected asmdef files; project should use predefined assemblies for package-free import" >&2
  exit 1
fi
if grep -q "com.unity.ugui\\|com.unity.test-framework\\|\\\"UnityEngine.UI\\\"" Packages/manifest.json Assets/Scripts/UIManager.cs Assets/Scripts/LevelSelectManager.cs; then
  echo "unexpected UGUI or Unity Test Framework dependency" >&2
  exit 1
fi
grep -q "#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS" Assets/Tests/EditMode/DriftContractTests.cs
grep -q "#if UNITY_INCLUDE_TESTS && DRIFT_ENABLE_UNITY_TESTS" Assets/Tests/PlayMode/DriftRuntimeFlowTests.cs
grep -q "DroneHitboxSize" Assets/Scripts/RuntimeVisualFactory.cs
grep -q "\"Hitbox Core\"" Assets/Scripts/RuntimeVisualFactory.cs
grep -q "StartPracticeLevel" Assets/Scripts/GameManager.cs
grep -q "Completed in Practice Mode" Assets/Scripts/UIManager.cs
grep -q "6000.0.74f1" ProjectSettings/ProjectVersion.txt

level_count="$(grep -c "new LevelDefinition(" Assets/Scripts/LevelCatalog.cs)"
if [[ "$level_count" -lt 5 ]]; then
  echo "expected at least 5 level definitions, found $level_count" >&2
  exit 1
fi

ring_count="$(grep -c "new RingSpec" Assets/Scripts/LevelCatalog.cs)"
if [[ "$ring_count" -lt 50 ]]; then
  echo "expected at least 50 ring specs across levels, found $ring_count" >&2
  exit 1
fi

echo "Project structure verification passed."
