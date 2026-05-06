using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public sealed class UIManager : MonoBehaviour
    {
        private enum View
        {
            MainMenu,
            LevelSelect,
            Hud,
            Pause,
            Complete,
            Failure,
            Settings
        }

        private View currentView = View.MainMenu;
        private IReadOnlyList<LevelDefinition> levelSelectLevels;
        private ProgressionService levelSelectProgression;
        private LevelDefinition hudLevel;
        private int hudCurrentRing = 1;
        private int hudTotalRings;
        private string failureReason = "Missed Ring";
        private int completeLevelNumber;
        private bool completeHasNextLevel;
        private string portalEffectLabel = string.Empty;
        private float portalEffectUntil;
        private float masterVolume = 1f;
        private float effectsVolume = 1f;
        private float cameraSmoothing = 0.18f;
        private int graphicsQuality = 1;
        private LevelSelectManager levelSelectManager;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle hudStyle;
        private GUIStyle panelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;
        private GUIStyle completedButtonStyle;
        private readonly List<Texture2D> styleTextures = new List<Texture2D>();

        public void ShowMainMenu()
        {
            currentView = View.MainMenu;
        }

        public void ShowLevelSelect(IReadOnlyList<LevelDefinition> levels, ProgressionService progression)
        {
            levelSelectLevels = levels;
            levelSelectProgression = progression;
            EnsureLevelSelectManager();
            currentView = View.LevelSelect;
        }

        public void ShowHud(LevelDefinition level)
        {
            hudLevel = level;
            currentView = View.Hud;
        }

        public void UpdateHudProgress(int currentRing, int totalRings)
        {
            hudCurrentRing = Mathf.Clamp(currentRing, 1, Mathf.Max(totalRings, 1));
            hudTotalRings = totalRings;
        }

        public void ShowPauseMenu()
        {
            currentView = View.Pause;
        }

        public void ShowLevelComplete(int levelNumber, bool hasNextLevel)
        {
            completeLevelNumber = levelNumber;
            completeHasNextLevel = hasNextLevel;
            currentView = View.Complete;
        }

        public void ShowFailure(string reason)
        {
            failureReason = string.IsNullOrWhiteSpace(reason) ? "Missed Ring" : reason;
            currentView = View.Failure;
        }

        public void ShowSettings()
        {
            currentView = View.Settings;
        }

        public void ShowPortalEffect(string label, float duration)
        {
            portalEffectLabel = label;
            portalEffectUntil = Time.time + Mathf.Max(duration, 1.2f);
        }

        private void OnGUI()
        {
            EnsureStyles();

            switch (currentView)
            {
                case View.MainMenu:
                    DrawMainMenu();
                    break;
                case View.LevelSelect:
                    DrawLevelSelect();
                    break;
                case View.Hud:
                    DrawHud();
                    break;
                case View.Pause:
                    DrawPauseMenu();
                    break;
                case View.Complete:
                    DrawCompleteMenu();
                    break;
                case View.Failure:
                    DrawFailure();
                    break;
                case View.Settings:
                    DrawSettings();
                    break;
            }
        }

        private void DrawMainMenu()
        {
            DrawMenuBackdrop();
            Rect panel = CenteredPanel(500f, 520f);
            DrawPanel(panel, new Color(0.006f, 0.01f, 0.02f, 0.82f));
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("AERIAL", titleStyle);
            GUILayout.Label("Space Arcade Ring Racing", subtitleStyle);
            GUILayout.Space(26f);
            if (DrawButton("Play"))
            {
                GameManager.Instance.StartFirstUnlockedLevel();
            }

            if (DrawButton("Tutorial"))
            {
                GameManager.Instance.StartTutorial();
            }

            if (DrawButton("Level Select"))
            {
                GameManager.Instance.ShowLevelSelect();
            }

            if (DrawButton("Settings"))
            {
                GameManager.Instance.ShowSettings();
            }

            if (DrawButton("Quit"))
            {
                GameManager.Instance.QuitGame();
            }

            GUILayout.EndArea();
        }

        private void DrawLevelSelect()
        {
            Rect panel = CenteredPanel(620f, 600f);
            DrawPanel(panel);
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("Level Select", titleStyle);
            GUILayout.Space(14f);

            foreach (LevelSelectEntry entry in levelSelectManager.GetEntries(levelSelectLevels, levelSelectProgression))
            {
                GUIStyle style = entry.Completed ? completedButtonStyle : entry.Unlocked ? buttonStyle : disabledButtonStyle;
                GUI.enabled = entry.Unlocked;
                if (GUILayout.Button(entry.Label, style, GUILayout.Height(48f)))
                {
                    GameManager.Instance.StartLevel(entry.LevelNumber);
                }

                GUI.enabled = true;
            }

            GUILayout.Space(12f);
            if (DrawButton("Back"))
            {
                GameManager.Instance.ShowMainMenu();
            }

            GUILayout.EndArea();
        }

        private void DrawHud()
        {
            GUI.Label(new Rect(24f, 18f, 420f, 34f), hudLevel != null ? hudLevel.DisplayName : "Level", hudStyle);
            GUI.Label(new Rect(Screen.width - 220f, 18f, 196f, 34f), $"{hudCurrentRing} / {hudTotalRings}", RightAligned(hudStyle));
            if (hudLevel != null && hudLevel.IsTutorial)
            {
                GUI.Label(new Rect(24f, 54f, 680f, 30f), "WASD move  |  Arrows burst/roll  |  Portals change speed, gravity, and size", hudStyle);
            }

            if (!string.IsNullOrEmpty(portalEffectLabel) && Time.time < portalEffectUntil)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 190f, 58f, 380f, 34f), portalEffectLabel, Centered(hudStyle));
            }

            GUI.Label(new Rect(Screen.width * 0.5f - 230f, Screen.height - 44f, 460f, 28f), "R Restart   Esc Pause   Arrow Keys Burst / Snap Roll", Centered(hudStyle));
        }

        private void DrawPauseMenu()
        {
            Rect panel = CenteredPanel(440f, 390f);
            DrawPanel(panel);
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("Paused", titleStyle);
            GUILayout.Space(22f);
            if (DrawButton("Resume"))
            {
                GameManager.Instance.Resume();
            }

            if (DrawButton("Restart Level"))
            {
                GameManager.Instance.RestartCurrentLevel();
            }

            if (DrawButton("Level Select"))
            {
                GameManager.Instance.ShowLevelSelect();
            }

            if (DrawButton("Main Menu"))
            {
                GameManager.Instance.ShowMainMenu();
            }

            GUILayout.EndArea();
        }

        private void DrawCompleteMenu()
        {
            Rect panel = CenteredPanel(480f, completeHasNextLevel ? 440f : 380f);
            DrawPanel(panel);
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("Level Complete", titleStyle);
            GUILayout.Label($"Level {completeLevelNumber} cleared", subtitleStyle);
            GUILayout.Space(22f);
            if (completeHasNextLevel && DrawButton("Next Level"))
            {
                GameManager.Instance.StartNextLevel();
            }

            if (DrawButton("Replay"))
            {
                GameManager.Instance.RestartCurrentLevel();
            }

            if (DrawButton("Level Select"))
            {
                GameManager.Instance.ShowLevelSelect();
            }

            if (DrawButton("Main Menu"))
            {
                GameManager.Instance.ShowMainMenu();
            }

            GUILayout.EndArea();
        }

        private void DrawFailure()
        {
            DrawFullscreenTint(new Color(0.18f, 0.02f, 0.03f, 0.58f));
            GUI.Label(new Rect(0f, Screen.height * 0.5f - 40f, Screen.width, 80f), failureReason, titleStyle);
        }

        private void DrawSettings()
        {
            DrawMenuBackdrop();
            Rect panel = CenteredPanel(520f, 490f);
            DrawPanel(panel, new Color(0.006f, 0.01f, 0.02f, 0.86f));
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("Settings", titleStyle);
            GUILayout.Space(16f);

            GUILayout.Label($"Master Volume  {Mathf.RoundToInt(masterVolume * 100f)}%", hudStyle);
            masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f, GUILayout.Height(28f));
            GameManager.Instance.SetMasterVolume(masterVolume);

            GUILayout.Space(8f);
            GUILayout.Label($"Effects Volume  {Mathf.RoundToInt(effectsVolume * 100f)}%", hudStyle);
            effectsVolume = GUILayout.HorizontalSlider(effectsVolume, 0f, 1f, GUILayout.Height(28f));
            GameManager.Instance.SetEffectsVolume(effectsVolume);

            GUILayout.Space(8f);
            GUILayout.Label($"Camera Smoothing  {cameraSmoothing:0.00}", hudStyle);
            cameraSmoothing = GUILayout.HorizontalSlider(cameraSmoothing, 0.08f, 0.32f, GUILayout.Height(28f));
            GameManager.Instance.SetCameraSmoothing(cameraSmoothing);

            GUILayout.Space(8f);
            GUILayout.Label($"Graphics Quality  {QualityLabel(graphicsQuality)}", hudStyle);
            graphicsQuality = Mathf.RoundToInt(GUILayout.HorizontalSlider(graphicsQuality, 0f, 2f, GUILayout.Height(28f)));
            QualitySettings.SetQualityLevel(Mathf.Clamp(graphicsQuality, 0, QualitySettings.names.Length - 1), true);

            GUILayout.Space(18f);
            if (DrawButton("Reset Progress"))
            {
                GameManager.Instance.Progression.ResetProgress();
            }

            if (DrawButton("Back"))
            {
                GameManager.Instance.ShowMainMenu();
            }

            GUILayout.EndArea();
        }

        private bool DrawButton(string label)
        {
            return GUILayout.Button(label, buttonStyle, GUILayout.Height(50f));
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 46,
                fontStyle = FontStyle.Bold,
                normal = { textColor = RuntimeVisualFactory.CurrentRingColor }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                normal = { textColor = new Color(0.75f, 0.82f, 0.95f) }
            };

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.whiteTexture }
            };

            buttonStyle = BuildButtonStyle(new Color(0.05f, 0.12f, 0.2f, 0.94f), Color.white);
            disabledButtonStyle = BuildButtonStyle(new Color(0.13f, 0.14f, 0.17f, 0.72f), new Color(0.55f, 0.58f, 0.64f));
            completedButtonStyle = BuildButtonStyle(new Color(0.08f, 0.34f, 0.22f, 0.94f), Color.white);
        }

        private GUIStyle BuildButtonStyle(Color background, Color text)
        {
            Texture2D normal = CreateColorTexture(background);
            Texture2D hover = CreateColorTexture(Color.Lerp(background, RuntimeVisualFactory.CurrentRingColor, 0.28f));
            Texture2D active = CreateColorTexture(Color.Lerp(background, RuntimeVisualFactory.CompletedRingColor, 0.34f));
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { background = normal, textColor = text },
                hover = { background = hover, textColor = Color.white },
                active = { background = active, textColor = Color.white },
                focused = { background = hover, textColor = Color.white }
            };
            style.normal.textColor = text;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }

        private Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            styleTextures.Add(texture);
            return texture;
        }

        private void EnsureLevelSelectManager()
        {
            if (levelSelectManager == null)
            {
                levelSelectManager = GetComponent<LevelSelectManager>();
                if (levelSelectManager == null)
                {
                    levelSelectManager = gameObject.AddComponent<LevelSelectManager>();
                }
            }
        }

        private void DrawPanel(Rect rect)
        {
            DrawPanel(rect, new Color(0.01f, 0.012f, 0.022f, 0.94f));
        }

        private void DrawPanel(Rect rect, Color color)
        {
            DrawFullscreenTint(new Color(0f, 0f, 0f, 0.36f));
            Color previous = GUI.color;
            GUI.color = color;
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.color = previous;
        }

        private void DrawMenuBackdrop()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.01f, 0.025f, 0.05f, 1f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            float centerX = Screen.width * 0.52f;
            for (int i = 0; i < 10; i++)
            {
                float pulse = Mathf.Sin(Time.realtimeSinceStartup * 0.7f + i * 0.55f) * 8f;
                float size = 80f + i * 58f + pulse;
                float y = Screen.height * 0.48f + Mathf.Sin(Time.realtimeSinceStartup * 0.25f) * 16f;
                Rect ring = new Rect(centerX - size * 0.5f, y - size * 0.5f, size, size);
                Color color = Color.Lerp(new Color(0.1f, 0.9f, 1f, 0.08f), new Color(1f, 0.18f, 0.62f, 0.12f), i / 9f);
                DrawOutlineRect(ring, color, 2f);
            }
        }

        private static void DrawOutlineRect(Rect rect, Color color, float thickness)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static string QualityLabel(int quality)
        {
            switch (quality)
            {
                case 0:
                    return "Low";
                case 2:
                    return "High";
                default:
                    return "Medium";
            }
        }

        private static void DrawFullscreenTint(Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static Rect CenteredPanel(float width, float height)
        {
            width = Mathf.Min(width, Screen.width - 48f);
            height = Mathf.Min(height, Screen.height - 48f);
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private static Rect Inset(Rect rect, float inset)
        {
            return new Rect(rect.x + inset, rect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f);
        }

        private static GUIStyle Centered(GUIStyle source)
        {
            GUIStyle copy = new GUIStyle(source);
            copy.alignment = TextAnchor.MiddleCenter;
            return copy;
        }

        private static GUIStyle RightAligned(GUIStyle source)
        {
            GUIStyle copy = new GUIStyle(source);
            copy.alignment = TextAnchor.MiddleRight;
            return copy;
        }
    }
}
