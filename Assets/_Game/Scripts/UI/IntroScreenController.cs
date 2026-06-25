using PatininIzinde.Characters;
using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.UI
{
    public sealed class IntroScreenController : MonoBehaviour
    {
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private KeyCode continueKey = KeyCode.Space;
        [SerializeField] private Texture2D customIntroImage;
        [SerializeField] private FirstPersonCameraController playerController;
        [SerializeField] private PlayerInteractor playerInteractor;

        private const float ReferenceWidth = 1424f;
        private const float ReferenceHeight = 789f;
        private const float ImageReferenceWidth = 1024f;
        private const float ImageReferenceHeight = 576f;

        private bool isVisible;
        private bool isInfoVisible;
        private float startTime;
        private float buttonHoverAmount;
        private Texture2D activeIntroImage;
        private GUIStyle titleStyle;
        private GUIStyle titleAccentStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle missionTitleStyle;
        private GUIStyle missionSubtitleStyle;
        private GUIStyle missionNumberStyle;
        private GUIStyle chipStyle;
        private GUIStyle buttonStyle;
        private GUIStyle buttonHoverStyle;
        private GUIStyle buttonLabelStyle;
        private GUIStyle infoTitleStyle;
        private GUIStyle infoBodyStyle;
        private GUIStyle passwordBoxStyle;
        private GUIStyle passwordIndexStyle;
        private Texture2D pixelTexture;
        private Texture2D pawTexture;
        private Texture2D circleTexture;

        public static bool HasAdventureStarted { get; private set; }
        public static bool IsIntroVisible { get; private set; }

        private void Awake()
        {
            HasAdventureStarted = !showOnStart;
            IsIntroVisible = false;
            activeIntroImage = customIntroImage != null
                ? customIntroImage
                : Resources.Load<Texture2D>("IntroBackground");

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonCameraController>();
            }

            if (playerInteractor == null)
            {
                playerInteractor = FindFirstObjectByType<PlayerInteractor>();
            }
        }

        private void Start()
        {
            if (showOnStart)
            {
                Show();
            }
        }

        private void Update()
        {
            if (!isVisible && !isInfoVisible)
            {
                return;
            }

            if (Input.GetKeyDown(continueKey) || Input.GetKeyDown(KeyCode.Return))
            {
                if (isVisible)
                {
                    ShowInfoScreen();
                }
                else
                {
                    Hide();
                }
            }
        }

        private void OnGUI()
        {
            if (!isVisible && !isInfoVisible)
            {
                return;
            }

            EnsureStyles();

            if (isInfoVisible)
            {
                DrawInfoScreen();
                return;
            }

            activeIntroImage = customIntroImage != null
                ? customIntroImage
                : activeIntroImage != null
                    ? activeIntroImage
                    : Resources.Load<Texture2D>("IntroBackground");

            if (activeIntroImage != null)
            {
                DrawImageIntro(activeIntroImage);
                return;
            }

            float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            scale = Mathf.Clamp(scale, 0.55f, 1.35f);
            Vector2 offset = new Vector2((Screen.width - ReferenceWidth * scale) * 0.5f, (Screen.height - ReferenceHeight * scale) * 0.5f);
            float time = Time.unscaledTime - startTime;

            DrawBackground(time);
            DrawCloud(offset + new Vector2(45f, 40f) * scale, scale, time, 0.55f, 1.15f);
            DrawCloud(offset + new Vector2(595f, 66f) * scale, scale, time, 1.15f, 0.72f);
            DrawCloud(offset + new Vector2(1022f, 34f) * scale, scale, time, 0.2f, 0.82f);
            DrawSun(offset + new Vector2(1364f, 48f) * scale, 40f * scale, time);
            DrawPawPattern(time, scale);

            DrawHeroText(offset, scale);
            DrawDog(offset, scale, time);
            DrawMissionPanel(offset, scale);
            DrawStartButton(offset, scale);
        }

        private void DrawImageIntro(Texture2D image)
        {
            Rect imageRect = GetCoverRect(Screen.width, Screen.height, image.width, image.height);
            GUI.DrawTexture(imageRect, image, ScaleMode.ScaleAndCrop);

            Rect startRect = RectFromImagePixels(imageRect, 363f, 326f, 312f, 74f);
            bool hoveringStart = startRect.Contains(Event.current.mousePosition);
            buttonHoverAmount = Mathf.MoveTowards(buttonHoverAmount, hoveringStart ? 1f : 0f, Time.unscaledDeltaTime * 8f);

            if (buttonHoverAmount > 0.01f)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.18f * buttonHoverAmount);
                GUI.DrawTexture(startRect, pixelTexture);
                GUI.color = oldColor;
            }

            if (GUI.Button(startRect, GUIContent.none, GUIStyle.none))
            {
                ShowInfoScreen();
            }
        }

        private static Rect GetCoverRect(float screenWidth, float screenHeight, float imageWidth, float imageHeight)
        {
            float scale = Mathf.Max(screenWidth / imageWidth, screenHeight / imageHeight);
            float width = imageWidth * scale;
            float height = imageHeight * scale;
            return new Rect((screenWidth - width) * 0.5f, (screenHeight - height) * 0.5f, width, height);
        }

        private static Rect RectFromImagePixels(Rect imageRect, float x, float y, float width, float height)
        {
            float scaleX = imageRect.width / ImageReferenceWidth;
            float scaleY = imageRect.height / ImageReferenceHeight;
            return new Rect(
                imageRect.x + x * scaleX,
                imageRect.y + y * scaleY,
                width * scaleX,
                height * scaleY);
        }

        private void DrawBackground(float time)
        {
            for (int y = 0; y < Screen.height; y += 3)
            {
                float t = Screen.height <= 0 ? 0f : (float)y / Screen.height;
                Color color = Color.Lerp(new Color(0.49f, 0.78f, 0.88f), new Color(1f, 0.88f, 0.58f), t);
                color = Color.Lerp(color, new Color(0.89f, 0.97f, 0.97f), Mathf.Sin(time * 0.35f + t) * 0.025f + 0.025f);
                DrawRect(new Rect(0f, y, Screen.width, 3f), color);
            }
        }

        private void DrawHeroText(Vector2 offset, float scale)
        {
            Rect chip = R(offset, scale, 122f, 84f, 420f, 54f);
            DrawSoftRect(chip, new Color(0.96f, 0.98f, 0.98f, 0.94f));
            DrawRect(new Rect(chip.x, chip.yMax - 2f * scale, chip.width, 2f * scale), new Color(1f, 0.54f, 0.38f, 0.32f));
            GUI.DrawTexture(R(offset, scale, 148f, 100f, 22f, 22f), pawTexture);
            GUI.Label(R(offset, scale, 184f, 96f, 330f, 32f), "DEPREM GUVENLIGI MACERASI", chipStyle);

            GUI.Label(R(offset, scale, 122f, 165f, 440f, 108f), "Tacinin", titleStyle);
            GUI.Label(R(offset, scale, 123f, 268f, 390f, 110f), "izinde", titleAccentStyle);
            GUI.Label(R(offset, scale, 122f, 402f, 500f, 76f), "Taci kayboldu. 4 gorevi tamamla, gizli harfleri\ntopla ve onu geri getir!", labelStyle);

            GUI.Label(R(offset, scale, 122f, 510f, 74f, 30f), "SIFREN:", smallStyle);
            for (int i = 0; i < 4; i++)
            {
                Rect box = R(offset, scale, 207f + i * 70f, 488f, 58f, 60f);
                GUI.Box(box, "?", passwordBoxStyle);
                GUI.Box(R(offset, scale, 251f + i * 70f, 532f, 20f, 20f), (i + 1).ToString(), passwordIndexStyle);
            }

            GUI.Label(R(offset, scale, 122f, 686f, 320f, 24f), "9-11 Yas - Deprem Guvenligi Egitimi", smallStyle);
        }

        private void DrawStartButton(Vector2 offset, float scale)
        {
            Rect buttonRect = R(offset, scale, 122f, 578f, 370f, 78f);
            bool hovering = buttonRect.Contains(Event.current.mousePosition);
            buttonHoverAmount = Mathf.MoveTowards(buttonHoverAmount, hovering ? 1f : 0f, Time.unscaledDeltaTime * 6f);

            float lift = Mathf.Lerp(0f, -4f * scale, buttonHoverAmount);
            Rect animatedRect = new Rect(buttonRect.x, buttonRect.y + lift, buttonRect.width, buttonRect.height);
            DrawSoftRect(new Rect(animatedRect.x, animatedRect.y + 6f * scale, animatedRect.width, animatedRect.height), new Color(0.75f, 0.2f, 0.06f, 0.25f));
            DrawSoftRect(animatedRect, Color.Lerp(new Color(1f, 0.31f, 0.12f), new Color(1f, 0.48f, 0.26f), buttonHoverAmount));

            if (GUI.Button(animatedRect, GUIContent.none, GUIStyle.none))
            {
                ShowInfoScreen();
            }

            GUI.DrawTexture(new Rect(animatedRect.x + 60f * scale, animatedRect.y + 27f * scale, 26f * scale, 26f * scale), pawTexture);
            GUI.Label(new Rect(animatedRect.x + 94f * scale, animatedRect.y, animatedRect.width - 116f * scale, animatedRect.height), "MACERAYA BASLA", buttonLabelStyle);
        }

        private void DrawMissionPanel(Vector2 offset, float scale)
        {
            Rect panel = R(offset, scale, 862f, 366f, 480f, 386f);
            DrawSoftRect(new Rect(panel.x, panel.y + 14f * scale, panel.width, panel.height), new Color(0.82f, 0.62f, 0.28f, 0.12f));
            DrawSoftRect(panel, new Color(0.98f, 0.98f, 0.94f, 0.92f));
            DrawRect(new Rect(panel.x, panel.y, panel.width, 1.5f * scale), Color.white);
            GUI.Label(R(offset, scale, 888f, 394f, 150f, 24f), "GOREVLER", chipStyle);

            DrawMissionRow(offset, scale, 431f, "Deprem Oncesinde", "Hazirlik ve canta", "1", new Color(1f, 0.38f, 0.2f), "!");
            DrawMissionRow(offset, scale, 513f, "Deprem Aninda", "Yer al, korun, tutun", "2", new Color(0.2f, 0.73f, 0.87f), "H");
            DrawMissionRow(offset, scale, 596f, "Deprem Sonrasinda", "Toplanma alani, yardim", "3", new Color(0.65f, 0.32f, 0.88f), "+");
            DrawMissionRow(offset, scale, 678f, "Sifreyi Gir", "4 harfi birlestir, Taci gelsin!", "4", new Color(1f, 0.67f, 0.22f), "?");
        }

        private void DrawMissionRow(Vector2 offset, float scale, float y, string title, string subtitle, string number, Color iconColor, string icon)
        {
            Rect iconBox = R(offset, scale, 888f, y, 50f, 50f);
            DrawSoftRect(iconBox, new Color(0.98f, 1f, 1f, 0.9f));
            DrawRect(new Rect(iconBox.x, iconBox.yMax - 2f * scale, iconBox.width, 2f * scale), new Color(0.72f, 0.8f, 0.82f, 0.24f));
            Color oldColor = GUI.color;
            GUI.color = iconColor;
            GUI.Label(iconBox, icon, missionNumberStyle);
            GUI.color = oldColor;

            GUI.Label(R(offset, scale, 953f, y + 4f, 250f, 24f), title, missionTitleStyle);
            GUI.Label(R(offset, scale, 953f, y + 29f, 250f, 22f), subtitle, missionSubtitleStyle);
            Rect numberRect = R(offset, scale, 1286f, y + 10f, 30f, 30f);
            DrawSoftRect(numberRect, new Color(1f, 0.38f, 0.2f));
            GUI.Label(numberRect, number, missionNumberStyle);
            DrawRect(R(offset, scale, 888f, y + 64f, 428f, 1f), new Color(0.78f, 0.72f, 0.63f, 0.32f));
        }

        private void DrawDog(Vector2 offset, float scale, float time)
        {
            float bob = Mathf.Sin(time * 1.6f) * 4f * scale;
            Vector2 baseOffset = offset + new Vector2(944f * scale, (92f * scale) + bob);

            DrawCircle(baseOffset + new Vector2(213f, 116f) * scale, 68f * scale, new Color(0.92f, 0.65f, 0.27f));
            DrawCircle(baseOffset + new Vector2(98f, 80f) * scale, 68f * scale, new Color(0.96f, 0.75f, 0.36f));
            DrawCircle(baseOffset + new Vector2(56f, 66f) * scale, 34f * scale, new Color(0.96f, 0.75f, 0.36f));
            DrawCircle(baseOffset + new Vector2(64f, 72f) * scale, 23f * scale, new Color(0.99f, 0.79f, 0.48f));
            DrawCircle(baseOffset + new Vector2(138f, 55f) * scale, 31f * scale, new Color(0.78f, 0.48f, 0.18f));
            DrawCircle(baseOffset + new Vector2(138f, 55f) * scale, 20f * scale, new Color(0.96f, 0.7f, 0.38f));
            DrawCircle(baseOffset + new Vector2(48f, 124f) * scale, 20f * scale, new Color(0.96f, 0.75f, 0.36f));
            DrawCircle(baseOffset + new Vector2(22f, 116f) * scale, 14f * scale, new Color(0.18f, 0.08f, 0.04f));
            DrawCircle(baseOffset + new Vector2(60f, 98f) * scale, 17f * scale, Color.black);
            DrawCircle(baseOffset + new Vector2(54f, 90f) * scale, 6f * scale, Color.white);
            DrawCircle(baseOffset + new Vector2(31f, 137f) * scale, 7f * scale, new Color(1f, 0.36f, 0.32f));

            DrawRect(new Rect(baseOffset.x + 100f * scale, baseOffset.y + 145f * scale, 68f * scale, 34f * scale), new Color(1f, 0.36f, 0.2f));
            GUI.Label(new Rect(baseOffset.x + 112f * scale, baseOffset.y + 150f * scale, 55f * scale, 24f * scale), "TACI", missionNumberStyle);

            float tailSwing = Mathf.Sin(time * 2.1f) * 8f * scale;
            DrawRect(new Rect(baseOffset.x + (291f * scale), baseOffset.y + (26f * scale) + tailSwing, 18f * scale, 110f * scale), new Color(0.92f, 0.65f, 0.27f));
            DrawCircle(baseOffset + new Vector2(94f, 222f) * scale, 16f * scale, new Color(0.74f, 0.47f, 0.16f));
            DrawCircle(baseOffset + new Vector2(144f, 228f) * scale, 15f * scale, new Color(0.74f, 0.47f, 0.16f));
            DrawCircle(baseOffset + new Vector2(274f, 218f) * scale, 15f * scale, new Color(0.74f, 0.47f, 0.16f));
        }

        private void DrawCloud(Vector2 position, float scale, float time, float phase, float size)
        {
            float drift = Mathf.Sin(time * 0.55f + phase) * 8f * scale;
            Vector2 p = position + new Vector2(drift, Mathf.Sin(time * 0.4f + phase) * 2f * scale);
            Color cloud = new Color(0.98f, 1f, 1f, 0.9f);
            DrawCircle(p + new Vector2(24f, 30f) * scale * size, 24f * scale * size, cloud);
            DrawCircle(p + new Vector2(76f, 22f) * scale * size, 42f * scale * size, cloud);
            DrawCircle(p + new Vector2(132f, 30f) * scale * size, 28f * scale * size, cloud);
            DrawRect(new Rect(p.x + 22f * scale * size, p.y + 31f * scale * size, 114f * scale * size, 28f * scale * size), cloud);
        }

        private void DrawSun(Vector2 center, float radius, float time)
        {
            Color rayColor = new Color(0.95f, 0.78f, 0.38f, 0.65f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f + Mathf.Sin(time * 0.4f) * 0.08f;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 p = center + dir * (radius + 12f);
                DrawRect(new Rect(p.x - 3f, p.y - 18f, 6f, 36f), rayColor);
            }

            DrawCircle(center, radius, new Color(1f, 0.82f, 0.37f));
        }

        private void DrawPawPattern(float time, float scale)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0.78f, 0.32f, 0.16f);
            for (int i = 0; i < 5; i++)
            {
                float x = 18f * scale + i * 260f * scale;
                float y = Screen.height - (205f + Mathf.Sin(time + i) * 16f) * scale;
                GUI.DrawTexture(new Rect(x, y, 26f * scale, 26f * scale), pawTexture);
            }
            GUI.color = oldColor;
        }

        private void Show()
        {
            isVisible = true;
            isInfoVisible = false;
            IsIntroVisible = true;
            HasAdventureStarted = false;
            startTime = Time.unscaledTime;
            SetPlayerInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ShowInfoScreen()
        {
            isVisible = false;
            isInfoVisible = true;
            IsIntroVisible = true;
            HasAdventureStarted = false;
            SetPlayerInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Hide()
        {
            isVisible = false;
            isInfoVisible = false;
            IsIntroVisible = false;
            HasAdventureStarted = true;
            SetPlayerInputEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void DrawInfoScreen()
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.03f, 0.05f, 0.07f, 0.94f));

            float width = Mathf.Min(760f, Screen.width - 80f);
            float height = Mathf.Min(420f, Screen.height - 80f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            DrawSoftRect(new Rect(panel.x, panel.y + 10f, panel.width, panel.height), new Color(0f, 0f, 0f, 0.28f));
            DrawSoftRect(panel, new Color(0.98f, 0.94f, 0.84f, 0.98f));
            DrawRect(new Rect(panel.x, panel.y, panel.width, 18f), new Color(1f, 0.44f, 0.18f, 1f));

            GUI.Label(new Rect(panel.x + 48f, panel.y + 44f, panel.width - 96f, 58f), "Oyunun Amaci", infoTitleStyle);
            GUI.Label(
                new Rect(panel.x + 64f, panel.y + 126f, panel.width - 128f, 150f),
                "Taci kayboldu. Onu bulmak icin ipucu harfleri topla, deprem oncesi ve sonrasi gorevleri tamamla, guvenli alana ulas ve sifreyi coz.",
                infoBodyStyle);

            Rect buttonRect = new Rect(panel.x + 220f, panel.y + panel.height - 86f, panel.width - 440f, 52f);
            if (GUI.Button(buttonRect, "DEVAM", buttonStyle))
            {
                Hide();
            }

            GUI.Label(new Rect(panel.x + 58f, panel.y + panel.height - 32f, panel.width - 116f, 24f), "Devam etmek icin Enter veya Space tusuna bas", smallStyle);
        }

        private void SetPlayerInputEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.enabled = enabled;
            }

            if (playerInteractor != null)
            {
                playerInteractor.enabled = enabled;
            }
        }

        private Rect R(Vector2 offset, float scale, float x, float y, float width, float height)
        {
            return new Rect(offset.x + x * scale, offset.y + y * scale, width * scale, height * scale);
        }

        private void DrawRoundedRect(Rect rect, float radius, Color color)
        {
            DrawRect(new Rect(rect.x + radius, rect.y, rect.width - radius * 2f, rect.height), color);
            DrawRect(new Rect(rect.x, rect.y + radius, rect.width, rect.height - radius * 2f), color);
            DrawCircle(new Vector2(rect.x + radius, rect.y + radius), radius, color);
            DrawCircle(new Vector2(rect.xMax - radius, rect.y + radius), radius, color);
            DrawCircle(new Vector2(rect.x + radius, rect.yMax - radius), radius, color);
            DrawCircle(new Vector2(rect.xMax - radius, rect.yMax - radius), radius, color);
        }

        private void DrawSoftRect(Rect rect, Color color)
        {
            DrawRect(rect, color);
            DrawRect(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), new Color(color.r * 0.86f, color.g * 0.86f, color.b * 0.86f, color.a * 0.35f));
        }

        private void DrawRoundedOutline(Rect rect, float radius, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x + radius, rect.y, rect.width - radius * 2f, thickness), color);
            DrawRect(new Rect(rect.x + radius, rect.yMax - thickness, rect.width - radius * 2f, thickness), color);
            DrawRect(new Rect(rect.x, rect.y + radius, thickness, rect.height - radius * 2f), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y + radius, thickness, rect.height - radius * 2f), color);
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, pixelTexture);
            GUI.color = oldColor;
        }

        private void DrawCircle(Vector2 center, float radius, Color color)
        {
            DrawCircle(new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), color);
        }

        private void DrawCircle(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, circleTexture);
            GUI.color = oldColor;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            pixelTexture = MakeTexture(Color.white);
            pawTexture = MakePawTexture(Color.white);
            circleTexture = MakeCircleTexture(Color.white);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 74,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.08f, 0.2f, 0.36f) }
            };

            titleAccentStyle = new GUIStyle(titleStyle)
            {
                fontSize = 78,
                normal = { textColor = new Color(1f, 0.38f, 0.2f) }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                normal = { textColor = new Color(0.28f, 0.45f, 0.53f) }
            };

            smallStyle = new GUIStyle(labelStyle)
            {
                fontSize = 16,
                normal = { textColor = new Color(0.31f, 0.48f, 0.57f, 0.92f) }
            };

            chipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.86f, 0.25f, 0.08f) }
            };

            passwordBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(new Color(1f, 0.98f, 0.9f, 0.82f)),
                    textColor = new Color(1f, 0.35f, 0.18f)
                }
            };

            passwordIndexStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(new Color(1f, 0.38f, 0.2f)),
                    textColor = Color.white
                }
            };

            buttonStyle = CreateButtonStyle(new Color(1f, 0.31f, 0.12f));
            buttonHoverStyle = CreateButtonStyle(new Color(1f, 0.44f, 0.23f));
            buttonLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            infoTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.1f, 0.22f, 0.34f) },
                wordWrap = true
            };

            infoBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.18f, 0.16f, 0.12f) },
                wordWrap = true
            };

            missionTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.08f, 0.2f, 0.36f) }
            };

            missionSubtitleStyle = new GUIStyle(missionTitleStyle)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                normal = { textColor = new Color(0.42f, 0.57f, 0.64f) }
            };

            missionNumberStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private GUIStyle CreateButtonStyle(Color color)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(color),
                    textColor = Color.white
                },
                hover =
                {
                    background = MakeTexture(color),
                    textColor = Color.white
                },
                active =
                {
                    background = MakeTexture(new Color(0.9f, 0.2f, 0.08f)),
                    textColor = Color.white
                }
            };
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D MakePawTexture(Color color)
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            DrawCircle(texture, 48, 34, 18, color);
            DrawCircle(texture, 25, 57, 11, color);
            DrawCircle(texture, 39, 71, 11, color);
            DrawCircle(texture, 57, 71, 11, color);
            DrawCircle(texture, 72, 57, 11, color);

            texture.Apply();
            return texture;
        }

        private static Texture2D MakeCircleTexture(Color color)
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            DrawCircle(texture, size / 2, size / 2, size / 2 - 1, color);
            texture.Apply();
            return texture;
        }

        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            int radiusSquared = radius * radius;
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }
    }
}
