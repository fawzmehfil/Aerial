using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Drift
{
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing,
        Paused,
        Failed,
        LevelComplete,
        PracticeComplete,
        Settings
    }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly ProgressionService progression = new ProgressionService();
        private IReadOnlyList<LevelDefinition> levels;
        private GameState state;
        private LevelManager levelManager;
        private RingManager ringManager;
        private UIManager uiManager;
        private CameraFollow cameraFollow;
        private PortalManager portalManager;
        private AudioSource audioSource;
        private AudioSource soundtrackSource;
        private AudioClip ringPassClip;
        private AudioClip failClip;
        private AudioClip completeClip;
        private int currentLevelNumber = 1;
        private float effectsVolume = 1f;
        private Coroutine failureRoutine;
        private Coroutine soundtrackRoutine;
        private bool practiceMode;
        private PracticeCheckpoint currentPracticeCheckpoint;
        private int practiceCheckpointCount;
        private float pendingSoundtrackStartTime;
        private float soundtrackClockStartTime;

        public ProgressionService Progression => progression;
        public IReadOnlyList<LevelDefinition> Levels => levels;
        public bool IsPlaying => state == GameState.Playing;
        public bool IsPracticeMode => practiceMode;
        public PracticeCheckpoint CurrentPracticeCheckpoint => currentPracticeCheckpoint;

        public static GameManager EnsureRuntime()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject gameObject = new GameObject("GameManager");
            return gameObject.AddComponent<GameManager>();
        }

        public void ShowMainMenu()
        {
            state = GameState.MainMenu;
            Time.timeScale = 1f;
            practiceMode = false;
            StopLevelSoundtrack();
            levelManager.ClearLevel();
            uiManager.ShowMainMenu();
        }

        public void ShowLevelSelect()
        {
            state = GameState.LevelSelect;
            Time.timeScale = 1f;
            practiceMode = false;
            StopLevelSoundtrack();
            levelManager.ClearLevel();
            uiManager.ShowLevelSelect(levels, progression);
        }

        public void StartFirstUnlockedLevel()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (!levels[i].IsTutorial)
                {
                    StartLevel(levels[i].LevelNumber);
                    return;
                }
            }

            StartLevel(1);
        }

        public void StartTutorial()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].IsTutorial)
                {
                    StartLevel(levels[i].LevelNumber);
                    return;
                }
            }

            StartLevel(1);
        }

        public void StartLevel(int levelNumber)
        {
            StartLevel(levelNumber, false);
        }

        public void StartPracticeLevel(int levelNumber)
        {
            StartLevel(levelNumber, true);
        }

        private void StartLevel(int levelNumber, bool usePracticeMode)
        {
            if (!progression.IsLevelUnlocked(levelNumber))
            {
                return;
            }

            currentLevelNumber = Mathf.Clamp(levelNumber, 1, levels.Count);
            state = GameState.Playing;
            Time.timeScale = 1f;
            practiceMode = usePracticeMode;
            LevelDefinition level = levels[currentLevelNumber - 1];
            levelManager.LoadLevel(level, ringManager, cameraFollow);
            InitializePracticeCheckpoint();
            uiManager.ShowHud(level, practiceMode);
            StartLevelSoundtrack(level);
            UpdateHudProgress();
        }

        public void RestartCurrentLevel()
        {
            if (failureRoutine != null)
            {
                StopCoroutine(failureRoutine);
                failureRoutine = null;
            }

            state = GameState.Playing;
            Time.timeScale = 1f;
            StopLevelSoundtrack();
            levelManager.ResetCurrentLevel(ringManager, cameraFollow);
            InitializePracticeCheckpoint();
            uiManager.ShowHud(levels[currentLevelNumber - 1], practiceMode);
            StartLevelSoundtrack(levels[currentLevelNumber - 1]);
            UpdateHudProgress();
        }

        public void Pause()
        {
            if (state != GameState.Playing)
            {
                return;
            }

            state = GameState.Paused;
            Time.timeScale = 0f;
            PauseLevelSoundtrack();
            uiManager.ShowPauseMenu();
        }

        public void Resume()
        {
            if (state != GameState.Paused)
            {
                return;
            }

            state = GameState.Playing;
            Time.timeScale = 1f;
            ResumeLevelSoundtrack();
            uiManager.ShowHud(levels[currentLevelNumber - 1], practiceMode);
            UpdateHudProgress();
        }

        public void FailCurrentLevel(string reason)
        {
            if (state != GameState.Playing)
            {
                return;
            }

            state = GameState.Failed;
            if (!CurrentLevelSuppressesGameplaySoundEffects())
            {
                audioSource.PlayOneShot(failClip, 0.8f * effectsVolume);
            }

            if (practiceMode)
            {
                uiManager.ShowFailure($"{reason} - practice checkpoint");
                failureRoutine = StartCoroutine(ResetAfterPracticeFailure());
                return;
            }

            StopLevelSoundtrack();
            uiManager.ShowFailure(reason);
            failureRoutine = StartCoroutine(ResetAfterFailure());
        }

        public void CompleteCurrentLevel()
        {
            if (state != GameState.Playing)
            {
                return;
            }

            if (practiceMode)
            {
                state = GameState.PracticeComplete;
                StopLevelSoundtrack();
                uiManager.ShowPracticeComplete(currentLevelNumber);
                return;
            }

            state = GameState.LevelComplete;
            progression.MarkLevelComplete(currentLevelNumber);
            if (!CurrentLevelSuppressesGameplaySoundEffects())
            {
                audioSource.PlayOneShot(completeClip, 0.75f * effectsVolume);
            }

            StopLevelSoundtrack();
            uiManager.ShowLevelComplete(currentLevelNumber, currentLevelNumber < levels.Count);
        }

        public void StartNextLevel()
        {
            StartLevel(Mathf.Min(currentLevelNumber + 1, levels.Count));
        }

        public void UpdateHudProgress()
        {
            uiManager.UpdateHudProgress(ringManager.CurrentRingNumber, ringManager.TotalRings);
        }

        public void RecordPracticeCheckpoint(RingCheckpoint passedRing, int nextRingIndex)
        {
            if (!practiceMode || passedRing == null || levelManager.CurrentDrone == null)
            {
                return;
            }

            LevelDefinition level = levels[currentLevelNumber - 1];
            if (!PracticeCheckpointPlanner.ShouldCreateAutoCheckpoint(level, passedRing.RingIndex))
            {
                return;
            }

            DronePracticeSnapshot snapshot = levelManager.CurrentDrone.CapturePracticeSnapshot();
            snapshot.Position = new Vector3(snapshot.Position.x, snapshot.Position.y, passedRing.ZPosition + 1.6f);
            practiceCheckpointCount++;
            currentPracticeCheckpoint = new PracticeCheckpoint(practiceCheckpointCount, nextRingIndex, GetCurrentSoundtrackTime(), snapshot);
            levelManager.ShowPracticeCheckpointMarker(snapshot.Position, practiceCheckpointCount);
            uiManager.UpdatePracticeCheckpoint(practiceCheckpointCount);
        }

        public void SyncPracticeSoundtrack(float checkpointTime)
        {
            pendingSoundtrackStartTime = Mathf.Max(0f, checkpointTime);
            if (soundtrackSource == null || soundtrackSource.clip == null)
            {
                return;
            }

            float seekTime = Mathf.Clamp(pendingSoundtrackStartTime, 0f, Mathf.Max(0f, soundtrackSource.clip.length - 0.05f));
            soundtrackSource.time = seekTime;
            soundtrackClockStartTime = Time.time - seekTime;
            if (!soundtrackSource.isPlaying)
            {
                soundtrackSource.Play();
            }

            pendingSoundtrackStartTime = 0f;
        }

        public void RefreshPracticeCheckpointAfterPortal(PortalBase portal, DroneController drone)
        {
            if (!practiceMode || portal == null || drone == null || currentPracticeCheckpoint.DroneSnapshot.Position == default)
            {
                return;
            }

            if (portal.ZPosition > currentPracticeCheckpoint.Position.z + 0.05f)
            {
                return;
            }

            DronePracticeSnapshot snapshot = drone.CapturePracticeSnapshot();
            snapshot.Position = currentPracticeCheckpoint.Position;
            currentPracticeCheckpoint = new PracticeCheckpoint(
                currentPracticeCheckpoint.CheckpointNumber,
                currentPracticeCheckpoint.NextRingIndex,
                currentPracticeCheckpoint.SoundtrackTime,
                snapshot);
        }

        public void ShowPortalEffect(string label, float duration)
        {
            uiManager.ShowPortalEffect(label, duration);
            if (audioSource != null && !CurrentLevelSuppressesGameplaySoundEffects())
            {
                audioSource.PlayOneShot(RuntimeVisualFactory.CreateToneClip("Portal Activate", 420f, 0.24f, 0.28f), 0.55f * effectsVolume);
            }
        }

        public void PlayRingPassFeedback()
        {
            if (audioSource != null && !CurrentLevelSuppressesGameplaySoundEffects())
            {
                audioSource.PlayOneShot(ringPassClip, 0.55f * effectsVolume);
            }
        }

        public void ShowSettings()
        {
            state = GameState.Settings;
            Time.timeScale = 1f;
            practiceMode = false;
            StopLevelSoundtrack();
            levelManager.ClearLevel();
            uiManager.ShowSettings();
        }

        public void SetMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
        }

        public void SetEffectsVolume(float value)
        {
            effectsVolume = Mathf.Clamp01(value);
        }

        public void SetCameraSmoothing(float value)
        {
            if (cameraFollow != null)
            {
                cameraFollow.SetPositionSmoothing(value);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            levels = LevelCatalog.GetLevels();
            ConfigureCoreObjects();
            ConfigureAudio();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && (state == GameState.Playing || state == GameState.Failed || state == GameState.Paused))
            {
                RestartCurrentLevel();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == GameState.Playing)
                {
                    Pause();
                }
                else if (state == GameState.Paused)
                {
                    Resume();
                }
                else if (state == GameState.LevelSelect)
                {
                    ShowMainMenu();
                }
                else if (state == GameState.Settings)
                {
                    ShowMainMenu();
                }
            }
        }

        private void ConfigureCoreObjects()
        {
            levelManager = GetComponent<LevelManager>();
            if (levelManager == null)
            {
                levelManager = gameObject.AddComponent<LevelManager>();
            }

            portalManager = GetComponent<PortalManager>();
            if (portalManager == null)
            {
                portalManager = gameObject.AddComponent<PortalManager>();
            }

            ringManager = GetComponent<RingManager>();
            if (ringManager == null)
            {
                ringManager = gameObject.AddComponent<RingManager>();
            }

            uiManager = GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<UIManager>();
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.01f, 0.012f, 0.022f);
            mainCamera.transform.position = new Vector3(0f, 2f, -9f);

            cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }
        }

        private void ConfigureAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            soundtrackSource = gameObject.AddComponent<AudioSource>();
            soundtrackSource.spatialBlend = 0f;
            soundtrackSource.loop = false;
            soundtrackSource.playOnAwake = false;
            ringPassClip = RuntimeVisualFactory.CreateToneClip("Ring Pass", 880f, 0.18f, 0.45f);
            failClip = RuntimeVisualFactory.CreateToneClip("Fail", 120f, 0.35f, 0.6f);
            completeClip = RuntimeVisualFactory.CreateToneClip("Level Complete", 660f, 0.55f, 0.55f);
        }

        private void InitializePracticeCheckpoint()
        {
            practiceCheckpointCount = 0;
            if (!practiceMode || levelManager.CurrentDrone == null)
            {
                currentPracticeCheckpoint = default;
                uiManager.UpdatePracticeCheckpoint(0);
                return;
            }

            DronePracticeSnapshot snapshot = levelManager.CurrentDrone.CapturePracticeSnapshot();
            currentPracticeCheckpoint = new PracticeCheckpoint(0, 0, 0f, snapshot);
            uiManager.UpdatePracticeCheckpoint(0);
        }

        private bool CurrentLevelSuppressesGameplaySoundEffects()
        {
            if (levels == null || currentLevelNumber < 1 || currentLevelNumber > levels.Count)
            {
                return false;
            }

            return levels[currentLevelNumber - 1].SuppressGameplaySoundEffects;
        }

        private void StartLevelSoundtrack(LevelDefinition level)
        {
            StopLevelSoundtrack();
            pendingSoundtrackStartTime = 0f;
            soundtrackClockStartTime = Time.time;
            if (level == null || string.IsNullOrWhiteSpace(level.SoundtrackPath))
            {
                return;
            }

            soundtrackRoutine = StartCoroutine(LoadAndPlaySoundtrack(level.SoundtrackPath));
        }

        private void StopLevelSoundtrack()
        {
            if (soundtrackRoutine != null)
            {
                StopCoroutine(soundtrackRoutine);
                soundtrackRoutine = null;
            }

            if (soundtrackSource != null)
            {
                soundtrackSource.Stop();
                soundtrackSource.clip = null;
            }
        }

        private void PauseLevelSoundtrack()
        {
            if (soundtrackSource != null && soundtrackSource.isPlaying)
            {
                soundtrackSource.Pause();
            }
        }

        private void ResumeLevelSoundtrack()
        {
            if (soundtrackSource != null && soundtrackSource.clip != null)
            {
                soundtrackSource.UnPause();
            }
        }

        private IEnumerator LoadAndPlaySoundtrack(string projectRelativePath)
        {
            string fullPath = ResolveProjectRelativePath(projectRelativePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Soundtrack not found: {projectRelativePath}");
                soundtrackRoutine = null;
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(fullPath).AbsoluteUri, AudioType.MPEG))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Could not load soundtrack {projectRelativePath}: {request.error}");
                    soundtrackRoutine = null;
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                float startTime = pendingSoundtrackStartTime > 0f ? pendingSoundtrackStartTime : Mathf.Max(0f, Time.time - soundtrackClockStartTime);
                soundtrackSource.clip = clip;
                soundtrackSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.05f));
                soundtrackClockStartTime = Time.time - soundtrackSource.time;
                soundtrackSource.Play();
                pendingSoundtrackStartTime = 0f;
            }

            soundtrackRoutine = null;
        }

        private static string ResolveProjectRelativePath(string projectRelativePath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, projectRelativePath));
        }

        private IEnumerator ResetAfterFailure()
        {
            yield return new WaitForSeconds(0.65f);
            RestartCurrentLevel();
            failureRoutine = null;
        }

        private IEnumerator ResetAfterPracticeFailure()
        {
            yield return new WaitForSeconds(0.45f);
            RespawnAtPracticeCheckpoint();
            failureRoutine = null;
        }

        private void RespawnAtPracticeCheckpoint()
        {
            if (!practiceMode || levelManager.CurrentDrone == null)
            {
                RestartCurrentLevel();
                return;
            }

            state = GameState.Playing;
            Time.timeScale = 1f;
            levelManager.CurrentDrone.RestorePracticeSnapshot(currentPracticeCheckpoint.DroneSnapshot);
            ringManager.SetCurrentRingIndex(currentPracticeCheckpoint.NextRingIndex);
            ResetPortalsForPracticeRespawn(currentPracticeCheckpoint.Position.z);
            SyncPracticeSoundtrack(currentPracticeCheckpoint.SoundtrackTime);
            uiManager.ShowHud(levels[currentLevelNumber - 1], true);
            uiManager.UpdatePracticeCheckpoint(currentPracticeCheckpoint.CheckpointNumber);
            UpdateHudProgress();
        }

        private void ResetPortalsForPracticeRespawn(float checkpointZ)
        {
            foreach (PortalBase portal in UnityEngine.Object.FindObjectsByType<PortalBase>(FindObjectsSortMode.None))
            {
                portal.ResetForPracticeRespawn(checkpointZ);
            }
        }

        private float GetCurrentSoundtrackTime()
        {
            return soundtrackSource != null && soundtrackSource.clip != null ? soundtrackSource.time : Mathf.Max(0f, Time.time - soundtrackClockStartTime);
        }
    }
}
