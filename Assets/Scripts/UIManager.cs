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
        private float viewTransitionStart;
        private LevelSelectManager levelSelectManager;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle eyebrowStyle;
        private GUIStyle hudStyle;
        private GUIStyle panelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;
        private GUIStyle completedButtonStyle;
        private readonly List<Texture2D> styleTextures = new List<Texture2D>();

        public void ShowMainMenu()
        {
            SetView(View.MainMenu);
        }

        public void ShowLevelSelect(IReadOnlyList<LevelDefinition> levels, ProgressionService progression)
        {
            levelSelectLevels = levels;
            levelSelectProgression = progression;
            EnsureLevelSelectManager();
            SetView(View.LevelSelect);
        }

        public void ShowHud(LevelDefinition level)
        {
            hudLevel = level;
            SetView(View.Hud);
        }

        public void UpdateHudProgress(int currentRing, int totalRings)
        {
            hudCurrentRing = Mathf.Clamp(currentRing, 1, Mathf.Max(totalRings, 1));
            hudTotalRings = totalRings;
        }

        public void ShowPauseMenu()
        {
            SetView(View.Pause);
        }

        public void ShowLevelComplete(int levelNumber, bool hasNextLevel)
        {
            completeLevelNumber = levelNumber;
            completeHasNextLevel = hasNextLevel;
            SetView(View.Complete);
        }

        public void ShowFailure(string reason)
        {
            failureReason = string.IsNullOrWhiteSpace(reason) ? "Missed Ring" : reason;
            SetView(View.Failure);
        }

        public void ShowSettings()
        {
            SetView(View.Settings);
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

            DrawTransitionOverlay();
        }

        private void DrawMainMenu()
        {
            DrawMenuBackdrop();
            DrawHeroDrone();

            Rect panel = MenuPanelRect(460f, 560f);
            DrawPanel(panel, new Color(0.004f, 0.01f, 0.018f, 0.86f));
            DrawOutlineRect(new Rect(panel.x - 1f, panel.y - 1f, panel.width + 2f, panel.height + 2f), new Color(0.1f, 0.95f, 1f, 0.34f), 1f);
            GUILayout.BeginArea(Inset(panel, 28f));
            GUILayout.Label("Aerial", titleStyle);
            GUILayout.Label("AUTOMATIC VELOCITY  /  PORTAL COURSE", eyebrowStyle);
            GUILayout.Space(8f);
            GUILayout.Label("Space Arcade Ring Racing", subtitleStyle);
            GUILayout.Space(28f);
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

            GUILayout.FlexibleSpace();
            GUILayout.Label("WASD position  |  Portals shift the route", subtitleStyle);
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
                GUI.Label(new Rect(24f, 54f, 680f, 30f), "WASD move  |  Portals change speed, gravity, and size", hudStyle);
            }

            if (!string.IsNullOrEmpty(portalEffectLabel) && Time.time < portalEffectUntil)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 190f, 58f, 380f, 34f), portalEffectLabel, Centered(hudStyle));
            }

            GUI.Label(new Rect(Screen.width * 0.5f - 230f, Screen.height - 44f, 460f, 28f), "WASD Move   R Restart   Esc Pause", Centered(hudStyle));
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
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                normal = { textColor = RuntimeVisualFactory.CurrentRingColor }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = new Color(0.75f, 0.82f, 0.95f) }
            };

            eyebrowStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.96f, 0.22f, 0.72f) }
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

            buttonStyle = BuildButtonStyle(new Color(0.035f, 0.1f, 0.18f, 0.96f), Color.white);
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
            float t = Time.realtimeSinceStartup;
            GUI.color = new Color(0.004f, 0.008f, 0.018f, 1f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            DrawStarfield(t);
            DrawCorridorLines(t);

            float centerX = Screen.width * 0.68f + Mathf.Sin(t * 0.22f) * 18f;
            float centerY = Screen.height * 0.49f + Mathf.Sin(t * 0.31f) * 14f;
            for (int i = 0; i < 13; i++)
            {
                float depth = i / 12f;
                float pulse = Mathf.Sin(t * 0.85f + i * 0.55f) * 8f;
                float size = 92f + i * 54f + pulse;
                Rect ring = new Rect(centerX - size * 0.5f, centerY - size * 0.31f, size, size * 0.62f);
                Color color = Color.Lerp(new Color(0.1f, 0.95f, 1f, 0.08f), new Color(1f, 0.18f, 0.62f, 0.18f), depth);
                DrawOutlineRect(ring, color, i == 0 ? 3f : 2f);
            }
        }

        private void DrawHeroDrone()
        {
            if (Screen.width < 900f)
            {
                return;
            }

            float t = Time.realtimeSinceStartup;
            float x = Screen.width * 0.68f;
            float y = Screen.height * 0.49f + Mathf.Sin(t * 1.4f) * 9f;
            float scale = Mathf.Clamp(Screen.width / 1280f, 0.85f, 1.25f);

            DrawRect(new Rect(x - 54f * scale, y - 10f * scale, 108f * scale, 20f * scale), new Color(0.06f, 0.08f, 0.13f, 0.94f));
            DrawRect(new Rect(x - 30f * scale, y - 22f * scale, 60f * scale, 44f * scale), new Color(0.78f, 0.9f, 1f, 0.9f));
            DrawRect(new Rect(x - 15f * scale, y - 8f * scale, 30f * scale, 16f * scale), new Color(0.08f, 0.11f, 0.18f, 0.98f));
            DrawRect(new Rect(x - 3f * scale, y - 30f * scale, 6f * scale, 12f * scale), new Color(0.16f, 1f, 0.95f, 0.9f));

            Vector2[] rotors =
            {
                new Vector2(-78f, -40f),
                new Vector2(78f, -40f),
                new Vector2(-78f, 40f),
                new Vector2(78f, 40f)
            };

            foreach (Vector2 rotor in rotors)
            {
                Rect rotorRect = new Rect(x + rotor.x * scale - 19f * scale, y + rotor.y * scale - 19f * scale, 38f * scale, 38f * scale);
                DrawOutlineRect(rotorRect, new Color(0.16f, 1f, 0.95f, 0.42f), 2f);
                DrawRect(new Rect(rotorRect.center.x - 5f * scale, rotorRect.center.y - 5f * scale, 10f * scale, 10f * scale), new Color(0.16f, 1f, 0.95f, 0.78f));
            }

            DrawRect(new Rect(x - 70f * scale, y + 62f * scale, 140f * scale, 2f * scale), new Color(0.16f, 1f, 0.95f, 0.22f));
            DrawRect(new Rect(x - 40f * scale, y + 72f * scale, 80f * scale, 2f * scale), new Color(1f, 0.2f, 0.7f, 0.22f));
        }

        private static void DrawStarfield(float time)
        {
            for (int i = 0; i < 72; i++)
            {
                float seed = i * 37.719f;
                float x = Mathf.Repeat(Mathf.Sin(seed) * 8273f + time * (4f + i % 5), Screen.width);
                float y = Mathf.Repeat(Mathf.Cos(seed * 1.31f) * 4317f + time * (1.5f + i % 3), Screen.height);
                float size = 1f + (i % 3);
                Color color = i % 5 == 0 ? new Color(1f, 0.2f, 0.75f, 0.42f) : new Color(0.55f, 0.9f, 1f, 0.38f);
                DrawRect(new Rect(x, y, size, size), color);
            }
        }

        private static void DrawCorridorLines(float time)
        {
            float horizonY = Screen.height * 0.52f + Mathf.Sin(time * 0.2f) * 10f;
            float drift = Mathf.Sin(time * 0.25f) * 22f;
            Color cyan = new Color(0.1f, 0.9f, 1f, 0.18f);
            Color magenta = new Color(1f, 0.16f, 0.62f, 0.14f);

            for (int i = 0; i < 7; i++)
            {
                float offset = 90f + i * 92f + Mathf.Repeat(time * 32f, 92f);
                DrawRect(new Rect(Screen.width * 0.5f + drift - offset, horizonY + i * 16f, offset * 1.3f, 2f), i % 2 == 0 ? cyan : magenta);
                DrawRect(new Rect(Screen.width * 0.5f + drift, horizonY + i * 16f, offset * 1.3f, 2f), i % 2 == 0 ? magenta : cyan);
            }

            DrawRect(new Rect(0f, horizonY - 1f, Screen.width, 2f), new Color(0.1f, 0.9f, 1f, 0.12f));
            DrawRect(new Rect(0f, Screen.height * 0.82f, Screen.width, 2f), new Color(1f, 0.16f, 0.62f, 0.1f));
        }

        private static void DrawOutlineRect(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
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

        private void DrawTransitionOverlay()
        {
            float elapsed = Time.realtimeSinceStartup - viewTransitionStart;
            float alpha = Mathf.Clamp01(1f - elapsed / 0.28f) * 0.42f;
            if (alpha > 0.001f)
            {
                DrawFullscreenTint(new Color(0f, 0f, 0f, alpha));
            }
        }

        private static Rect CenteredPanel(float width, float height)
        {
            width = Mathf.Min(width, Screen.width - 48f);
            height = Mathf.Min(height, Screen.height - 48f);
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private static Rect MenuPanelRect(float width, float height)
        {
            width = Mathf.Min(width, Screen.width - 48f);
            height = Mathf.Min(height, Screen.height - 48f);
            float x = Screen.width >= 900f ? 72f : (Screen.width - width) * 0.5f;
            float y = (Screen.height - height) * 0.5f;
            return new Rect(x, y, width, height);
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

        private void SetView(View nextView)
        {
            if (currentView != nextView)
            {
                viewTransitionStart = Time.realtimeSinceStartup;
            }

            currentView = nextView;
        }
    }
}
