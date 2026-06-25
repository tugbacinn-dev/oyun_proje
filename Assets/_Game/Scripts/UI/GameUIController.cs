using PatininIzinde.Interaction;
using PatininIzinde.QuestSystem;
using PatininIzinde.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace PatininIzinde.UI
{
    public sealed class GameUIController : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private BagCollectionManager bagCollectionManager;
        [SerializeField] private SafetyFixManager safetyFixManager;
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private Text questText;
        [SerializeField] private Text interactionText;
        [SerializeField] private GameObject notePanel;
        [SerializeField] private Text noteTitleText;
        [SerializeField] private Text noteBodyText;
        [SerializeField] private Texture2D endingScreenTexture;
        [SerializeField] private AudioClip endingScreenSound;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField, Range(0f, 1f)] private float gameMusicVolume = 0.35f;

        private string currentQuestTitle = "";
        private string currentInteractionTitle = "";
        private string bagCounterText = "";
        private string safetyCounterText = "";
        private string currentNoteTitle = "";
        private string currentNoteBody = "";
        private string passwordPromptTitle = "";
        private string passwordPromptBody = "";
        private string passwordInput = "";
        private string passwordFeedback = "";
        private string emergencyInput = "";
        private string emergencyFeedback = "";
        private float earthquakeProgress;
        private float earthquakeShakeTimer;
        private float earthquakeCrouchOffset;
        private float earthquakeCountdown;
        private Transform cameraTransform;
        private Vector3 cameraBaseLocalPosition;
        private bool isNoteVisible;
        private bool earthquakeUiVisible;
        private bool earthquakeCountdownVisible;
        private bool earthquakeSimulationActive;
        private bool gameplayHudVisible = true;
        private bool isPasswordPromptVisible;
        private bool isEmergencyPromptVisible;
        private bool isEmergencyInfoVisible;
        private bool isEndingScreenVisible;
        private bool endingSoundPlayed;
        private AudioSource endingAudioSource;
        private AudioSource gameMusicSource;
        private FirstPersonCameraController playerController;
        private GameObject finalDog;
        private bool finalDogRevealed;
        private bool hasFinalDogInitialTransform;
        private Vector3 finalDogInitialPosition;
        private Quaternion finalDogInitialRotation;
        private Vector3 finalDogInitialScale;
        private const string FinalDogTag = "pati";
        private const float FinalDogAutoRevealDelay = 180f;
        private static readonly Vector3 finalDogFallbackPosition = new Vector3(54.69f, -0.02f, 49.59f);
        private GameObject questTextRoot;
        private GameObject interactionTextRoot;
        private GUIStyle questBoxStyle;
        private GUIStyle questLabelStyle;
        private GUIStyle questSmallStyle;
        private GUIStyle interactionBoxStyle;
        private GUIStyle earthquakeBoxStyle;
        private GUIStyle earthquakeLabelStyle;
        private GUIStyle earthquakeCountdownStyle;
        private GUIStyle noteBoxStyle;
        private GUIStyle noteTitleStyle;
        private GUIStyle noteBodyStyle;
        private GUIStyle noteHintStyle;
        private GUIStyle passwordInputStyle;
        private GUIStyle passwordFeedbackStyle;
        private GUIStyle endingFallbackStyle;
        private GUIStyle counterStyle;
        private Texture2D earthquakeTrackTexture;
        private Texture2D earthquakeFillTexture;
        private Material ayseArrowMaterial;
        private Material ayseArrowTextMaterial;

        private void Awake()
        {
            if (questManager == null)
            {
                questManager = FindFirstObjectByType<QuestManager>();
            }

            if (playerInteractor == null)
            {
                playerInteractor = FindFirstObjectByType<PlayerInteractor>();
            }

            playerController = FindFirstObjectByType<FirstPersonCameraController>();
            ResolveFinalDog();
            SetFinalDogVisible(false);

            if (bagCollectionManager == null)
            {
                bagCollectionManager = FindFirstObjectByType<BagCollectionManager>();
            }

            if (safetyFixManager == null)
            {
                safetyFixManager = FindFirstObjectByType<SafetyFixManager>();
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                cameraBaseLocalPosition = cameraTransform.localPosition;
            }

            questTextRoot = questText != null && questText.transform.parent != null
                ? questText.transform.parent.gameObject
                : questText != null ? questText.gameObject : null;

            interactionTextRoot = interactionText != null && interactionText.transform.parent != null
                ? interactionText.transform.parent.gameObject
                : interactionText != null ? interactionText.gameObject : null;
        }

        private void OnEnable()
        {
            if (questManager != null)
            {
                questManager.StepChanged += UpdateQuestText;
            }

            if (bagCollectionManager != null)
            {
                bagCollectionManager.CountChanged += UpdateBagCounter;
            }

            if (safetyFixManager != null)
            {
                safetyFixManager.CountChanged += UpdateSafetyCounter;
            }
        }

        private void Start()
        {
            ResolveFinalDog();
            SetFinalDogVisible(false);
            EnsureAyseDirectionArrows();

            gameplayHudVisible = !ShouldShowGameplayHud();
            UpdateGameplayHudVisibility();
            UpdateQuestText(questManager != null ? questManager.CurrentStep : null);
            if (bagCollectionManager != null)
            {
                UpdateBagCounter(bagCollectionManager.CollectedItems, bagCollectionManager.TotalItems);
            }

            if (safetyFixManager != null)
            {
                UpdateSafetyCounter(safetyFixManager.FixedItems, safetyFixManager.TotalItems);
            }

            HideNote();
            PlayGameMusic();
        }

        private void Update()
        {
            UpdateInteractionText();
            UpdateEarthquakeFeedback();
            UpdateGameplayHudVisibility();

            if (isPasswordPromptVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitPasswordPrompt();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HidePasswordPrompt();
                }
            }

            if (isEmergencyPromptVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitEmergencyPrompt();
                }
            }

            if (isEmergencyInfoVisible &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)))
            {
                ShowEndingScreen();
            }

            if (isEndingScreenVisible && Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (notePanel != null && notePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
            {
                HideNote();
            }
        }

        private void UpdateFinalDogAutoReveal()
        {
            if (finalDogRevealed || Time.timeSinceLevelLoad < FinalDogAutoRevealDelay)
            {
                return;
            }

            RevealFinalDog();
            currentQuestTitle = "Taci ortaya cikti! Etrafina bak.";
            if (questText != null)
            {
                questText.text = currentQuestTitle;
            }
        }

        private void OnDisable()
        {
            if (questManager != null)
            {
                questManager.StepChanged -= UpdateQuestText;
            }

            if (bagCollectionManager != null)
            {
                bagCollectionManager.CountChanged -= UpdateBagCounter;
            }

            if (safetyFixManager != null)
            {
                safetyFixManager.CountChanged -= UpdateSafetyCounter;
            }
        }

        public void ShowNote(string title, string body)
        {
            currentNoteTitle = title;
            currentNoteBody = body;
            isNoteVisible = true;

            if (notePanel != null)
            {
                notePanel.SetActive(true);
            }

            if (noteTitleText != null)
            {
                noteTitleText.text = title;
            }

            if (noteBodyText != null)
            {
                noteBodyText.text = body;
            }
        }

        public void HideNote()
        {
            isNoteVisible = false;

            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }
        }

        public void ShowPasswordPrompt(string title, string body)
        {
            passwordPromptTitle = title;
            passwordPromptBody = body;
            passwordInput = "";
            passwordFeedback = "";
            currentInteractionTitle = "";
            isPasswordPromptVisible = true;
            SetPlayerInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void HidePasswordPrompt()
        {
            isPasswordPromptVisible = false;
            passwordFeedback = "";
            SetPlayerInputEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UpdateQuestText(QuestStep step)
        {
            if (step == null)
            {
                currentQuestTitle = "Gorevler tamamlandi.";
                earthquakeUiVisible = false;
                earthquakeCountdownVisible = false;
                earthquakeSimulationActive = false;
                if (questText != null)
                {
                    questText.text = currentQuestTitle;
                }

                return;
            }

            currentQuestTitle = step.Title;
            if (step.StepId == "earthquake_moment")
            {
                BeginEarthquakeCountdown();
                EarthquakeSafePoint.EnsureExists(questManager, this);
            }
            else
            {
                earthquakeProgress = 0f;
                earthquakeCrouchOffset = 0f;
                earthquakeUiVisible = false;
                earthquakeCountdownVisible = false;
                earthquakeSimulationActive = false;
                ResetCameraShake();
            }

            if (questText != null)
            {
                questText.text = step.Title;
            }
        }

        public bool IsEarthquakeSimulationActive => earthquakeSimulationActive;

        public void BeginEarthquakeCountdown()
        {
            earthquakeUiVisible = false;
            earthquakeSimulationActive = false;
            earthquakeCountdownVisible = true;
            earthquakeCountdown = 10f;
            earthquakeProgress = 0f;
            earthquakeCrouchOffset = 0f;
            ResetCameraShake();
        }

        public void StartEarthquakeFeedback()
        {
            earthquakeUiVisible = true;
            earthquakeCountdownVisible = false;
            earthquakeSimulationActive = true;
            earthquakeProgress = 0f;
            earthquakeShakeTimer = 999f;
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                cameraBaseLocalPosition = cameraTransform.localPosition;
            }
        }

        public void CompleteEarthquakeFeedback()
        {
            earthquakeProgress = 1f;
            earthquakeShakeTimer = 0f;
            earthquakeCrouchOffset = 0f;
            earthquakeUiVisible = false;
            earthquakeSimulationActive = false;
            earthquakeCountdownVisible = false;
            ResetCameraShake();
        }

        public void SetEarthquakeProgress(float progress)
        {
            earthquakeProgress = Mathf.Clamp01(progress);
        }

        public void SetEarthquakeCrouch(float crouchOffset)
        {
            earthquakeCrouchOffset = Mathf.Max(0f, crouchOffset);
        }

        private void UpdateEarthquakeFeedback()
        {
            if (earthquakeCountdownVisible)
            {
                earthquakeCountdown -= Time.deltaTime;
                if (earthquakeCountdown <= 0f)
                {
                    StartEarthquakeFeedback();
                }
            }

            if (!earthquakeUiVisible)
            {
                return;
            }

            if (earthquakeShakeTimer <= 0f || cameraTransform == null)
            {
                ResetCameraShake();
                return;
            }

            earthquakeShakeTimer -= Time.deltaTime;
            float strength = Mathf.Lerp(0.015f, 0.055f, earthquakeProgress);
            Vector3 crouchOffset = Vector3.down * earthquakeCrouchOffset;
            Vector3 offset = new Vector3(
                Mathf.Sin(Time.time * 31f) * strength,
                Mathf.Sin(Time.time * 37f) * strength * 0.45f,
                0f);
            cameraTransform.localPosition = cameraBaseLocalPosition + crouchOffset + offset;

            if (earthquakeShakeTimer <= 0f)
            {
                ResetCameraShake();
            }
        }

        private void ResetCameraShake()
        {
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = cameraBaseLocalPosition + Vector3.down * earthquakeCrouchOffset;
            }
        }

        private void UpdateInteractionText()
        {
            IInteractable interactable = playerInteractor != null ? playerInteractor.CurrentInteractable : null;
            bool shouldShow = gameplayHudVisible && interactable != null && interactable.CanInteract;
            currentInteractionTitle = shouldShow ? interactable.InteractionText : "";

            if (interactionText == null)
            {
                return;
            }

            interactionText.gameObject.SetActive(shouldShow);

            if (interactionText.transform.parent != null)
            {
                interactionText.transform.parent.gameObject.SetActive(shouldShow);
            }

            if (shouldShow)
            {
                interactionText.text = interactable.InteractionText;
            }
        }

        private void OnGUI()
        {
            if (isEndingScreenVisible)
            {
                DrawEndingScreen();
                return;
            }

            if (!ShouldShowGameplayHud())
            {
                return;
            }

            EnsureStyles();

            DrawShadowLabel(new Rect(42f, 30f, 560f, 22f), "GOREV", questSmallStyle);
            DrawShadowLabel(new Rect(42f, 58f, Screen.width - 84f, 74f), CurrentQuestDisplayTitle(), questLabelStyle);

            if (!string.IsNullOrWhiteSpace(bagCounterText) && bagCollectionManager != null && bagCollectionManager.IsActive)
            {
                DrawShadowLabel(new Rect(Screen.width - 210f, 34f, 180f, 54f), bagCounterText, counterStyle);
            }

            if (!string.IsNullOrWhiteSpace(safetyCounterText) && safetyFixManager != null && safetyFixManager.IsActive)
            {
                DrawShadowLabel(new Rect(Screen.width - 210f, 34f, 180f, 54f), safetyCounterText, counterStyle);
            }

            if (earthquakeUiVisible)
            {
                DrawEarthquakeBar();
            }

            if (!string.IsNullOrWhiteSpace(currentInteractionTitle))
            {
                float width = Mathf.Min(900f, Screen.width - 80f);
                float height = 70f;
                Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 126f, width, height);
                GUI.Box(rect, currentInteractionTitle, interactionBoxStyle);
            }

            if (isNoteVisible)
            {
                float width = Mathf.Min(840f, Screen.width - 80f);
                float height = Mathf.Min(500f, Screen.height - 80f);
                Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
                GUI.Box(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), GUIContent.none, earthquakeBoxStyle);
                GUI.Box(rect, GUIContent.none, noteBoxStyle);
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 18f), earthquakeFillTexture);
                GUI.Label(new Rect(rect.x + 46f, rect.y + 36f, rect.width - 92f, 52f), currentNoteTitle, noteTitleStyle);
                GUI.DrawTexture(new Rect(rect.x + 54f, rect.y + 98f, rect.width - 108f, 2f), earthquakeFillTexture);
                GUI.Label(new Rect(rect.x + 64f, rect.y + 124f, rect.width - 128f, rect.height - 214f), currentNoteBody, noteBodyStyle);
                GUI.Label(new Rect(rect.x + 52f, rect.y + rect.height - 64f, rect.width - 104f, 38f), "Devam etmek icin Space tusuna bas", noteHintStyle);
            }

            if (isPasswordPromptVisible)
            {
                DrawPasswordPrompt();
            }

            if (isEmergencyPromptVisible)
            {
                DrawEmergencyPrompt();
            }

            if (isEmergencyInfoVisible)
            {
                DrawEmergencyInfo();
            }
        }

        private void EnsureStyles()
        {
            if (questBoxStyle != null)
            {
                return;
            }

            questBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(new Color(0.04f, 0.055f, 0.075f, 0.88f)) },
                border = new RectOffset(8, 8, 8, 8)
            };

            questLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                wordWrap = true
            };

            questSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.72f, 0.86f, 1f) },
                wordWrap = true
            };

            interactionBoxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(new Color(0.035f, 0.04f, 0.055f, 0.9f)),
                    textColor = Color.white
                }
            };

            earthquakeBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = MakeTexture(new Color(0.04f, 0.035f, 0.03f, 0.88f)),
                    textColor = Color.white
                }
            };

            earthquakeLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(1f, 0.92f, 0.72f) }
            };
            earthquakeCountdownStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(new Color(0.04f, 0.12f, 0.08f, 0.88f)),
                    textColor = new Color(0.9f, 1f, 0.82f)
                }
            };
            earthquakeTrackTexture = MakeTexture(new Color(0.14f, 0.13f, 0.12f, 0.95f));
            earthquakeFillTexture = MakeTexture(new Color(0.95f, 0.62f, 0.25f, 0.95f));

            noteBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(new Color(0.98f, 0.93f, 0.82f, 0.98f)) }
            };

            noteTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.14f, 0.22f, 0.34f) },
                wordWrap = true
            };

            noteBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = new Color(0.18f, 0.16f, 0.12f) },
                wordWrap = true
            };

            noteHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.86f, 0.25f, 0.08f) }
            };

            passwordInputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = MakeTexture(new Color(1f, 0.98f, 0.9f, 0.98f)),
                    textColor = new Color(0.1f, 0.2f, 0.34f)
                },
                focused =
                {
                    background = MakeTexture(new Color(1f, 1f, 0.96f, 1f)),
                    textColor = new Color(0.1f, 0.2f, 0.34f)
                }
            };

            passwordFeedbackStyle = new GUIStyle(noteHintStyle)
            {
                fontSize = 20,
                normal = { textColor = new Color(0.16f, 0.45f, 0.26f) }
            };

            endingFallbackStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                wordWrap = true
            };

            counterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                normal = { textColor = new Color(0.8f, 1f, 0.92f) }
            };
        }

        private void UpdateGameplayHudVisibility()
        {
            bool shouldShow = ShouldShowGameplayHud();
            if (gameplayHudVisible == shouldShow)
            {
                return;
            }

            gameplayHudVisible = shouldShow;

            if (questTextRoot != null)
            {
                questTextRoot.SetActive(shouldShow);
            }

            if (!shouldShow)
            {
                currentInteractionTitle = "";
                if (interactionTextRoot != null)
                {
                    interactionTextRoot.SetActive(false);
                }
            }
        }

        private static bool ShouldShowGameplayHud()
        {
            IntroScreenController intro = FindFirstObjectByType<IntroScreenController>();
            return intro == null || IntroScreenController.HasAdventureStarted;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void DrawEarthquakeBar()
        {
            float width = Mathf.Min(430f, Screen.width - 80f);
            Rect boxRect = new Rect(Screen.width - width - 34f, 104f, width, 74f);
            GUI.Box(boxRect, GUIContent.none, earthquakeBoxStyle);
            GUI.Label(new Rect(boxRect.x + 18f, boxRect.y + 8f, boxRect.width - 36f, 24f), "SARSINTI", earthquakeLabelStyle);

            Rect trackRect = new Rect(boxRect.x + 18f, boxRect.y + 40f, boxRect.width - 36f, 18f);
            GUI.DrawTexture(trackRect, earthquakeTrackTexture);

            Rect fillRect = new Rect(trackRect.x, trackRect.y, trackRect.width * earthquakeProgress, trackRect.height);
            GUI.DrawTexture(fillRect, earthquakeFillTexture);
        }

        private void DrawPasswordPrompt()
        {
            float width = Mathf.Min(760f, Screen.width - 80f);
            float height = Mathf.Min(430f, Screen.height - 80f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.Box(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), GUIContent.none, earthquakeBoxStyle);
            GUI.Box(rect, GUIContent.none, noteBoxStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 18f), earthquakeFillTexture);

            GUI.Label(new Rect(rect.x + 48f, rect.y + 34f, rect.width - 96f, 48f), passwordPromptTitle, noteTitleStyle);
            GUI.Label(new Rect(rect.x + 64f, rect.y + 104f, rect.width - 128f, 92f), passwordPromptBody, noteBodyStyle);

            GUI.SetNextControlName("PasswordPromptInput");
            passwordInput = GUI.TextField(new Rect(rect.x + 230f, rect.y + 214f, rect.width - 460f, 56f), passwordInput.ToUpperInvariant(), 4, passwordInputStyle);
            GUI.FocusControl("PasswordPromptInput");

            if (GUI.Button(new Rect(rect.x + 230f, rect.y + 292f, rect.width - 460f, 48f), "SIFREYI GIR", interactionBoxStyle))
            {
                SubmitPasswordPrompt();
            }

            string feedback = string.IsNullOrWhiteSpace(passwordFeedback)
                ? "Ipuclarini sirayla dusun: anne, baba, Perihan Teyze, Cem."
                : passwordFeedback;
            GUI.Label(new Rect(rect.x + 58f, rect.y + rect.height - 64f, rect.width - 116f, 38f), feedback, passwordFeedbackStyle);
        }

        private void ShowEmergencyPrompt()
        {
            isEmergencyPromptVisible = true;
            isEmergencyInfoVisible = false;
            emergencyInput = "";
            emergencyFeedback = "";
            currentInteractionTitle = "";
            SetPlayerInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void DrawEmergencyPrompt()
        {
            float width = Mathf.Min(760f, Screen.width - 80f);
            float height = Mathf.Min(430f, Screen.height - 80f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.Box(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), GUIContent.none, earthquakeBoxStyle);
            GUI.Box(rect, GUIContent.none, noteBoxStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 18f), earthquakeFillTexture);

            GUI.Label(new Rect(rect.x + 48f, rect.y + 34f, rect.width - 96f, 54f), "Acil Durum Bilgisi", noteTitleStyle);
            GUI.Label(
                new Rect(rect.x + 64f, rect.y + 112f, rect.width - 128f, 90f),
                "Peki acil bir durum olursa hangi numarayi araman gerek biliyor musun?",
                noteBodyStyle);

            GUI.SetNextControlName("EmergencyPromptInput");
            emergencyInput = GUI.TextField(new Rect(rect.x + 250f, rect.y + 218f, rect.width - 500f, 56f), emergencyInput, 3, passwordInputStyle);
            GUI.FocusControl("EmergencyPromptInput");

            if (GUI.Button(new Rect(rect.x + 230f, rect.y + 296f, rect.width - 460f, 48f), "CEVAPLA", interactionBoxStyle))
            {
                SubmitEmergencyPrompt();
            }

            string feedback = string.IsNullOrWhiteSpace(emergencyFeedback)
                ? "Ipucu: Acil yardim hatti uc rakamlidir."
                : emergencyFeedback;
            GUI.Label(new Rect(rect.x + 58f, rect.y + rect.height - 64f, rect.width - 116f, 38f), feedback, passwordFeedbackStyle);
        }

        private void SubmitEmergencyPrompt()
        {
            string normalized = emergencyInput.Trim();
            if (normalized == "112")
            {
                isEmergencyPromptVisible = false;
                isEmergencyInfoVisible = true;
                emergencyFeedback = "";
                return;
            }

            emergencyFeedback = "Tekrar dusun. Ambulans, itfaiye, polis ve AFAD icin ortak acil cagri numarasi nedir?";
        }

        private void DrawEmergencyInfo()
        {
            float width = Mathf.Min(840f, Screen.width - 80f);
            float height = Mathf.Min(430f, Screen.height - 80f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.Box(new Rect(rect.x + 8f, rect.y + 10f, rect.width, rect.height), GUIContent.none, earthquakeBoxStyle);
            GUI.Box(rect, GUIContent.none, noteBoxStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 18f), earthquakeFillTexture);

            GUI.Label(new Rect(rect.x + 48f, rect.y + 34f, rect.width - 96f, 54f), "112 Acil Cagri", noteTitleStyle);
            GUI.Label(
                new Rect(rect.x + 64f, rect.y + 112f, rect.width - 128f, 150f),
                "Dogru cevap: 112.\n\n112; ambulans, itfaiye, polis, jandarma ve AFAD gibi acil yardim birimlerine ulasmak icindir. Gereksiz yere aranmamali ve mesgul edilmemelidir.",
                noteBodyStyle);

            if (GUI.Button(new Rect(rect.x + 230f, rect.y + 306f, rect.width - 460f, 48f), "DEVAM", interactionBoxStyle))
            {
                ShowEndingScreen();
            }

            GUI.Label(new Rect(rect.x + 58f, rect.y + rect.height - 56f, rect.width - 116f, 34f), "Devam etmek icin Enter veya Space tusuna bas", passwordFeedbackStyle);
        }

        private void ShowEndingScreen()
        {
            isEndingScreenVisible = true;
            isNoteVisible = false;
            isPasswordPromptVisible = false;
            isEmergencyPromptVisible = false;
            isEmergencyInfoVisible = false;
            currentInteractionTitle = "";
            currentQuestTitle = "";

            if (notePanel != null)
            {
                notePanel.SetActive(false);
            }

            if (questTextRoot != null)
            {
                questTextRoot.SetActive(false);
            }

            if (interactionTextRoot != null)
            {
                interactionTextRoot.SetActive(false);
            }

            SetPlayerInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayEndingSound();
            Debug.Log("AFAD dogru. Bitis ekrani acildi.");
        }

        private void PlayEndingSound()
        {
            if (endingSoundPlayed)
            {
                return;
            }

            if (endingScreenSound == null)
            {
                endingScreenSound = Resources.Load<AudioClip>("Audio/ending_woof");
            }

            if (endingScreenSound == null)
            {
                Debug.LogWarning("Bitis ekrani sesi bulunamadi: Resources/Audio/ending_woof");
                return;
            }

            if (endingAudioSource == null)
            {
                endingAudioSource = gameObject.GetComponent<AudioSource>();
                if (endingAudioSource == null)
                {
                    endingAudioSource = gameObject.AddComponent<AudioSource>();
                }

                endingAudioSource.playOnAwake = false;
                endingAudioSource.spatialBlend = 0f;
            }

            endingSoundPlayed = true;
            endingAudioSource.PlayOneShot(endingScreenSound);
        }

        private void PlayGameMusic()
        {
            if (gameMusicSource != null && gameMusicSource.isPlaying)
            {
                return;
            }

            if (gameMusic == null)
            {
                gameMusic = Resources.Load<AudioClip>("Audio/game_music");
            }

            if (gameMusic == null)
            {
                Debug.LogWarning("Oyun muzigi bulunamadi: Resources/Audio/game_music");
                return;
            }

            if (gameMusicSource == null)
            {
                gameMusicSource = gameObject.AddComponent<AudioSource>();
                gameMusicSource.playOnAwake = false;
                gameMusicSource.loop = true;
                gameMusicSource.spatialBlend = 0f;
            }

            gameMusicSource.clip = gameMusic;
            gameMusicSource.volume = gameMusicVolume;
            gameMusicSource.Play();
        }

        private void DrawEndingScreen()
        {
            EnsureStyles();
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.blackTexture, ScaleMode.StretchToFill);

            if (endingScreenTexture != null)
            {
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), endingScreenTexture, ScaleMode.ScaleAndCrop);
                return;
            }

            GUI.Label(
                new Rect(40f, 0f, Screen.width - 80f, Screen.height),
                "Bitis ekrani gorseli atanmadi.\nGameUI uzerindeki Ending Screen Texture alanina resmi surukle.",
                endingFallbackStyle);
        }

        private void SubmitPasswordPrompt()
        {
            string normalized = passwordInput.Trim().ToUpperInvariant();
            if (normalized == "AFAD")
            {
                HidePasswordPrompt();
                ShowEmergencyPrompt();
                return;
            }

            passwordFeedback = "Sifre yanlis. Topladigin 4 ipucu harfini tekrar dusun.";
        }

        private void ResolveFinalDog()
        {
            if (finalDog != null)
            {
                return;
            }

            GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject candidate in sceneObjects)
            {
                if (!candidate.scene.IsValid())
                {
                    continue;
                }

                if (IsFinalDogTagged(candidate))
                {
                    finalDog = candidate;
                    CacheFinalDogInitialTransform();
                    Debug.Log($"Taci tag ile bulundu: {finalDog.name}");
                    return;
                }
            }

            foreach (GameObject candidate in sceneObjects)
            {
                if (!candidate.scene.IsValid())
                {
                    continue;
                }

                if (IsFinalDogCandidate(candidate) && candidate.transform.parent == null)
                {
                    finalDog = candidate;
                    CacheFinalDogInitialTransform();
                    Debug.Log($"Taci bulundu: {finalDog.name}");
                    return;
                }
            }

            foreach (GameObject candidate in sceneObjects)
            {
                if (!candidate.scene.IsValid())
                {
                    continue;
                }

                if (IsFinalDogCandidate(candidate))
                {
                    finalDog = candidate.transform.root.gameObject;
                    CacheFinalDogInitialTransform();
                    Debug.Log($"Taci bulundu: {finalDog.name}");
                    return;
                }
            }
        }

        private static bool IsFinalDogCandidate(GameObject candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (IsFinalDogTagged(candidate))
            {
                return true;
            }

            string candidateName = candidate.name.ToLowerInvariant();
            if (candidateName == "pati" ||
                candidateName.Contains("pati") ||
                candidateName.Contains("cartoonanimal_dog") ||
                candidateName.Contains("dog"))
            {
                return true;
            }

            Animator animator = candidate.GetComponentInChildren<Animator>(true);
            if (animator != null &&
                animator.runtimeAnimatorController != null &&
                animator.runtimeAnimatorController.name.ToLowerInvariant().Contains("cartoonanimal_dog"))
            {
                return true;
            }

            return false;
        }

        private static bool IsFinalDogTagged(GameObject candidate)
        {
            return candidate != null && candidate.tag == FinalDogTag;
        }

        private void CacheFinalDogInitialTransform()
        {
            if (finalDog == null || hasFinalDogInitialTransform)
            {
                return;
            }

            finalDogInitialPosition = finalDogFallbackPosition;
            finalDogInitialRotation = finalDog.transform.rotation;
            finalDogInitialScale = finalDog.transform.localScale;
            hasFinalDogInitialTransform = true;
        }

        private void SetFinalDogVisible(bool visible)
        {
            ResolveFinalDog();
            if (finalDog == null)
            {
                return;
            }

            if (visible)
            {
                Transform parent = finalDog.transform.parent;
                while (parent != null)
                {
                    parent.gameObject.SetActive(true);
                    parent = parent.parent;
                }

                if (hasFinalDogInitialTransform)
                {
                    finalDog.transform.SetPositionAndRotation(finalDogInitialPosition, finalDogInitialRotation);
                    finalDog.transform.localScale = finalDogInitialScale;
                }
            }

            finalDog.SetActive(true);
            if (visible)
            {
                SetChildrenActive(finalDog.transform, true);
            }

            Renderer[] renderers = finalDog.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = visible;
            }

            Collider[] colliders = finalDog.GetComponentsInChildren<Collider>(true);
            foreach (Collider dogCollider in colliders)
            {
                dogCollider.enabled = visible;
            }
        }

        private void RevealFinalDog()
        {
            finalDogRevealed = true;
            SetFinalDogVisible(true);
            if (finalDog == null)
            {
                finalDog = CreateEmergencyPati();
                hasFinalDogInitialTransform = true;
                finalDogInitialScale = finalDog.transform.localScale;
            }

            Vector3 revealPosition = GetFinalDogRevealPosition();
            Quaternion revealRotation = GetFinalDogRevealRotation(revealPosition);
            finalDog.transform.SetPositionAndRotation(revealPosition, revealRotation);
            finalDog.transform.localScale = Vector3.one * 3f;
            ForceFinalDogVisible();
            EnsurePatiMarker();

            Animator animator = finalDog.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.enabled = true;
            }

            Debug.Log($"AFAD dogru. Taci gorunur yapildi: {finalDog.name} / {finalDog.transform.position}");

            currentQuestTitle = "Etrafina iyice bak, Taci geldi buralarda.";
            if (questText != null)
            {
                questText.text = currentQuestTitle;
            }
        }

        private Vector3 GetFinalDogRevealPosition()
        {
            GameObject targetSurface = GameObject.Find("Env_Playground_6 (2)");
            if (targetSurface != null)
            {
                Renderer targetRenderer = targetSurface.GetComponentInChildren<Renderer>();
                if (targetRenderer != null)
                {
                    Bounds bounds = targetRenderer.bounds;
                    return new Vector3(bounds.center.x, bounds.max.y + 0.04f, bounds.center.z);
                }

                return targetSurface.transform.position + Vector3.up * 0.65f;
            }

            Transform viewTransform = Camera.main != null ? Camera.main.transform : null;
            if (viewTransform == null && playerController != null)
            {
                viewTransform = playerController.transform;
            }

            if (viewTransform == null)
            {
                GameObject playground = GameObject.Find("Env_Playground (1)");
                if (playground != null)
                {
                    Vector3 playgroundPosition = playground.transform.position + new Vector3(-1.8f, 0.05f, -0.9f);
                    playgroundPosition.y = playground.transform.position.y + 0.05f;
                    return playgroundPosition;
                }

                return finalDogFallbackPosition;
            }

            Vector3 forward = viewTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = viewTransform.transform.forward;
            }

            forward.Normalize();
            Vector3 position = viewTransform.position + forward * 3f;
            position.y = viewTransform.position.y - 1.25f;

            if (Physics.Raycast(position + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore))
            {
                position.y = hit.point.y;
            }

            return position;
        }

        private Quaternion GetFinalDogRevealRotation(Vector3 revealPosition)
        {
            Transform viewTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 lookDirection = viewTransform != null ? viewTransform.position - revealPosition : Vector3.forward;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude < 0.001f)
            {
                lookDirection = Vector3.forward;
            }

            return Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        }

        private void EnsurePatiMarker()
        {
            if (finalDog == null)
            {
                return;
            }

            Transform existingMarker = finalDog.transform.Find("Taci_Gorunur_Etiket");
            if (existingMarker != null)
            {
                existingMarker.localPosition = new Vector3(0f, 2.2f, 0f);
                existingMarker.gameObject.SetActive(true);
                return;
            }

            GameObject marker = new GameObject("Taci_Gorunur_Etiket");
            marker.transform.SetParent(finalDog.transform, false);
            marker.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            marker.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            marker.transform.localScale = Vector3.one * 0.22f;

            TextMesh label = marker.AddComponent<TextMesh>();
            label.text = "TACI BURADA";
            label.fontSize = 64;
            label.characterSize = 0.18f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(1f, 0.05f, 0.02f);

            MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                renderer.forceRenderingOff = false;
            }
        }

        private void ForceFinalDogVisible()
        {
            if (finalDog == null)
            {
                return;
            }

            Transform parent = finalDog.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }

            finalDog.SetActive(true);
            SetChildrenActive(finalDog.transform, true);

            Renderer[] renderers = finalDog.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
                renderer.forceRenderingOff = false;
            }

            Collider[] colliders = finalDog.GetComponentsInChildren<Collider>(true);
            foreach (Collider dogCollider in colliders)
            {
                dogCollider.enabled = true;
            }
        }

        private static void SetChildrenActive(Transform root, bool active)
        {
            if (root == null)
            {
                return;
            }

            foreach (Transform child in root)
            {
                child.gameObject.SetActive(active);
                SetChildrenActive(child, active);
            }
        }

        private static GameObject CreateEmergencyPati()
        {
            GameObject root = new GameObject("Taci");
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Taci_Govde";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            body.transform.localScale = new Vector3(0.45f, 0.55f, 0.45f);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Taci_Bas";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.8f, 0.48f);
            head.transform.localScale = new Vector3(0.42f, 0.36f, 0.36f);

            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            tail.name = "Taci_Kuyruk";
            tail.transform.SetParent(root.transform, false);
            tail.transform.localPosition = new Vector3(0f, 0.65f, -0.55f);
            tail.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
            tail.transform.localScale = new Vector3(0.1f, 0.35f, 0.1f);

            Color patiColor = new Color(1f, 0.56f, 0.22f);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = patiColor;
            }

            root.transform.localScale = Vector3.one;
            Debug.LogWarning("Sahnedeki kopek bulunamadi; gecici Taci olusturuldu.");
            return root;
        }

        private void EnsureAyseDirectionArrows()
        {
            if (GameObject.Find("AyseTeyze_Yon_Oklari") != null)
            {
                return;
            }

            Transform start = FindGuideStartPoint();
            Transform target = FindAyseHousePoint();
            if (start == null || target == null)
            {
                return;
            }

            Vector3 startPosition = start.position;
            Vector3 targetPosition = target.position;
            startPosition.y = 0.04f;
            targetPosition.y = 0.04f;

            Vector3 direction = targetPosition - startPosition;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < 4f)
            {
                return;
            }

            direction.Normalize();
            GameObject root = new GameObject("AyseTeyze_Yon_Oklari");
            Mesh arrowMesh = CreateGroundArrowMesh();
            Material arrowMaterial = GetAyseArrowMaterial();

            int arrowCount = Mathf.Clamp(Mathf.FloorToInt(distance / 7f), 4, 9);
            for (int i = 0; i < arrowCount; i++)
            {
                float t = (i + 1f) / (arrowCount + 1f);
                Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);
                position.y = 0.055f;

                GameObject arrow = new GameObject($"Ayse_Yon_Oku_{i + 1}");
                arrow.transform.SetParent(root.transform, false);
                arrow.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction, Vector3.up));
                arrow.transform.localScale = new Vector3(2.1f, 1f, 2.1f);

                MeshFilter meshFilter = arrow.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = arrowMesh;

                MeshRenderer meshRenderer = arrow.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = arrowMaterial;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }

            CreateAyseArrowLabel(root.transform, targetPosition, direction);
        }

        private static Transform FindGuideStartPoint()
        {
            GameObject home =
                GameObject.Find("BizimEv_Dis_Kapi") ??
                GameObject.Find("BizimEv_Giris_Tetikleyici") ??
                GameObject.Find("PlayerCameraRig");
            return home != null ? home.transform : null;
        }

        private static Transform FindAyseHousePoint()
        {
            GameObject ayse =
                GameObject.Find("AyseTeyze_Dis_Kapi") ??
                GameObject.Find("AyseEv_Dis_Donus") ??
                GameObject.Find("Ayse_Teyze");
            return ayse != null ? ayse.transform : null;
        }

        private Material GetAyseArrowMaterial()
        {
            if (ayseArrowMaterial != null)
            {
                return ayseArrowMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            ayseArrowMaterial = new Material(shader)
            {
                name = "Ayse_Yon_Oku_Mat",
                color = new Color(1f, 0.78f, 0.12f, 0.92f)
            };
            return ayseArrowMaterial;
        }

        private Material GetAyseArrowTextMaterial()
        {
            if (ayseArrowTextMaterial != null)
            {
                return ayseArrowTextMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            ayseArrowTextMaterial = new Material(shader)
            {
                name = "Ayse_Yon_Yazisi_Mat",
                color = new Color(0.16f, 0.08f, 0.03f, 1f)
            };
            return ayseArrowTextMaterial;
        }

        private static Mesh CreateGroundArrowMesh()
        {
            Mesh mesh = new Mesh
            {
                name = "Ayse_Ground_Arrow_Mesh"
            };

            mesh.vertices = new[]
            {
                new Vector3(-0.28f, 0f, -0.82f),
                new Vector3(0.28f, 0f, -0.82f),
                new Vector3(0.28f, 0f, 0.18f),
                new Vector3(0.55f, 0f, 0.18f),
                new Vector3(0f, 0f, 0.98f),
                new Vector3(-0.55f, 0f, 0.18f),
                new Vector3(-0.28f, 0f, 0.18f)
            };

            mesh.triangles = new[]
            {
                0, 2, 1,
                0, 6, 2,
                6, 5, 3,
                6, 3, 2,
                5, 4, 3
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void CreateAyseArrowLabel(Transform parent, Vector3 targetPosition, Vector3 direction)
        {
            Vector3 labelPosition = targetPosition - direction * 3.2f;
            labelPosition.y = 0.18f;

            GameObject label = new GameObject("AyseTeyze_Yon_Yazisi");
            label.transform.SetParent(parent, false);
            label.transform.SetPositionAndRotation(labelPosition, Quaternion.Euler(90f, 0f, 0f));

            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = "Perihan Teyze Evi";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 32;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.characterSize = 0.22f;
            textMesh.color = new Color(0.16f, 0.08f, 0.03f, 1f);

            MeshRenderer renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetAyseArrowTextMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void SetPlayerInputEnabled(bool enabled)
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonCameraController>();
            }

            if (playerController != null)
            {
                playerController.enabled = enabled;
            }

            if (playerInteractor != null)
            {
                playerInteractor.enabled = enabled;
            }
        }

        private void DrawEarthquakeCountdown()
        {
            float width = Mathf.Min(760f, Screen.width - 90f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, 112f, width, 58f);
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, earthquakeCountdown));
            GUI.Box(rect, $"{seconds} saniye sonra sarsinti simulasyonu baslayacak. Hayat ucgenini bul.", earthquakeCountdownStyle);
        }

        private string CurrentQuestDisplayTitle()
        {
            if (!earthquakeCountdownVisible)
            {
                return currentQuestTitle;
            }

            int seconds = Mathf.CeilToInt(Mathf.Max(0f, earthquakeCountdown));
            return $"{seconds} saniye sonra sarsinti simulasyonu baslayacak. Hayat ucgenini bul.";
        }

        private void DrawShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color originalColor = style.normal.textColor;
            style.normal.textColor = new Color(0f, 0f, 0f, 0.72f);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, style);
            style.normal.textColor = originalColor;
            GUI.Label(rect, text, style);
        }

        private void UpdateBagCounter(int collected, int total)
        {
            bagCounterText = $"{collected}/{total}";
        }

        private void UpdateSafetyCounter(int fixedCount, int total)
        {
            safetyCounterText = $"{total}/{fixedCount}";
        }
    }
}
