using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private AudioClip ringPassClip;
        private AudioClip failClip;
        private AudioClip completeClip;
        private int currentLevelNumber = 1;
        private float effectsVolume = 1f;
        private Coroutine failureRoutine;

        public ProgressionService Progression => progression;
        public IReadOnlyList<LevelDefinition> Levels => levels;
        public bool IsPlaying => state == GameState.Playing;

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
            levelManager.ClearLevel();
            uiManager.ShowMainMenu();
        }

        public void ShowLevelSelect()
        {
            state = GameState.LevelSelect;
            Time.timeScale = 1f;
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
            if (!progression.IsLevelUnlocked(levelNumber))
            {
                return;
            }

            currentLevelNumber = Mathf.Clamp(levelNumber, 1, levels.Count);
            state = GameState.Playing;
            Time.timeScale = 1f;
            levelManager.LoadLevel(levels[currentLevelNumber - 1], ringManager, cameraFollow);
            uiManager.ShowHud(levels[currentLevelNumber - 1]);
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
            levelManager.ResetCurrentLevel(ringManager, cameraFollow);
            uiManager.ShowHud(levels[currentLevelNumber - 1]);
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
            uiManager.ShowHud(levels[currentLevelNumber - 1]);
            UpdateHudProgress();
        }

        public void FailCurrentLevel(string reason)
        {
            if (state != GameState.Playing)
            {
                return;
            }

            state = GameState.Failed;
            audioSource.PlayOneShot(failClip, 0.8f * effectsVolume);
            uiManager.ShowFailure(reason);
            failureRoutine = StartCoroutine(ResetAfterFailure());
        }

        public void CompleteCurrentLevel()
        {
            if (state != GameState.Playing)
            {
                return;
            }

            state = GameState.LevelComplete;
            progression.MarkLevelComplete(currentLevelNumber);
            audioSource.PlayOneShot(completeClip, 0.75f * effectsVolume);
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

        public void ShowPortalEffect(string label, float duration)
        {
            uiManager.ShowPortalEffect(label, duration);
            if (audioSource != null)
            {
                audioSource.PlayOneShot(RuntimeVisualFactory.CreateToneClip("Portal Activate", 420f, 0.24f, 0.28f), 0.55f * effectsVolume);
            }
        }

        public void PlayRingPassFeedback()
        {
            audioSource.PlayOneShot(ringPassClip, 0.55f * effectsVolume);
        }

        public void ShowSettings()
        {
            state = GameState.Settings;
            Time.timeScale = 1f;
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
            ringPassClip = RuntimeVisualFactory.CreateToneClip("Ring Pass", 880f, 0.18f, 0.45f);
            failClip = RuntimeVisualFactory.CreateToneClip("Fail", 120f, 0.35f, 0.6f);
            completeClip = RuntimeVisualFactory.CreateToneClip("Level Complete", 660f, 0.55f, 0.55f);
        }

        private IEnumerator ResetAfterFailure()
        {
            yield return new WaitForSeconds(0.65f);
            RestartCurrentLevel();
            failureRoutine = null;
        }
    }
}
