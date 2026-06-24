using System.Collections;
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
        private FirstPersonCameraController playerController;
        private GameObject finalDog;
        private Coroutine finalDogRunRoutine;
        private bool hasFinalDogInitialTransform;
        private Vector3 finalDogInitialPosition;
        private Quaternion finalDogInitialRotation;
        private Vector3 finalDogInitialScale;
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
        private GUIStyle counterStyle;
        private Texture2D earthquakeTrackTexture;
        private Texture2D earthquakeFillTexture;

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
            SetFinalDogVisible(true);

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
            SetFinalDogVisible(true);

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
        }

        private void Update()
        {
            UpdateInteractionText();
            UpdateEarthquakeFeedback();
            UpdateGameplayHudVisibility();

            if (isPasswordPromptVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SubmitPasswordPrompt();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HidePasswordPrompt();
                }
            }

            if (notePanel != null && notePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
            {
                HideNote();
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
                float width = 520f;
                float height = 62f;
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
                ? "Ipuclarini sirayla dusun: anne, baba, Ayse Teyze, Can."
                : passwordFeedback;
            GUI.Label(new Rect(rect.x + 58f, rect.y + rect.height - 64f, rect.width - 116f, 38f), feedback, passwordFeedbackStyle);
        }

        private void SubmitPasswordPrompt()
        {
            string normalized = passwordInput.Trim().ToUpperInvariant();
            if (normalized == "AFAD")
            {
                passwordFeedback = "Sifre dogru! Pati cok yakinda.";
                HidePasswordPrompt();
                RevealFinalDog();
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

                if (candidate.name == "SM_CartoonAnimal_Dog" && candidate.transform.parent == null)
                {
                    finalDog = candidate;
                    CacheFinalDogInitialTransform();
                    return;
                }
            }

            foreach (GameObject candidate in sceneObjects)
            {
                if (!candidate.scene.IsValid())
                {
                    continue;
                }

                if (candidate.name == "SM_CartoonAnimal_Dog")
                {
                    finalDog = candidate.transform.root.gameObject;
                    CacheFinalDogInitialTransform();
                    return;
                }
            }
        }

        private void CacheFinalDogInitialTransform()
        {
            if (finalDog == null || hasFinalDogInitialTransform)
            {
                return;
            }

            finalDogInitialPosition = finalDog.transform.position;
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

            finalDog.SetActive(visible);
        }

        private void RevealFinalDog()
        {
            SetFinalDogVisible(true);
            if (finalDog == null)
            {
                currentQuestTitle = "Sifre dogru! Pati modeli sahnede bulunamadi.";
                if (questText != null)
                {
                    questText.text = currentQuestTitle;
                }

                return;
            }

            Vector3 targetPosition = ResolveFinalDogTargetPosition();
            if (finalDogRunRoutine != null)
            {
                StopCoroutine(finalDogRunRoutine);
            }

            finalDogRunRoutine = StartCoroutine(RunFinalDogToTarget(targetPosition));
            currentQuestTitle = "Sifre dogru! Pati bulundu.";
            if (questText != null)
            {
                questText.text = currentQuestTitle;
            }
        }

        private Vector3 ResolveFinalDogTargetPosition()
        {
            float dogY = finalDog != null ? finalDog.transform.position.y : 0f;
            GameObject passwordNote = GameObject.Find("Pati_Sifre_Notu");
            if (passwordNote != null)
            {
                Vector3 notePosition = passwordNote.transform.position;
                return new Vector3(notePosition.x + 3.9f, dogY, notePosition.z + 0.2f);
            }

            GameObject umbrella = GameObject.Find("Env_Playground (1)");
            if (umbrella != null)
            {
                Vector3 umbrellaPosition = umbrella.transform.position;
                return new Vector3(umbrellaPosition.x + 3.9f, dogY, umbrellaPosition.z + 0.02f);
            }

            return new Vector3(54.69f, dogY, 45.97f);
        }

        private IEnumerator RunFinalDogToTarget(Vector3 targetPosition)
        {
            Animator animator = finalDog.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.enabled = true;
            }

            const float runSpeed = 7f;
            const float stopDistance = 0.08f;

            while (finalDog != null)
            {
                Vector3 currentPosition = finalDog.transform.position;
                Vector3 flatDirection = targetPosition - currentPosition;
                flatDirection.y = 0f;

                if (flatDirection.sqrMagnitude <= stopDistance * stopDistance)
                {
                    finalDog.transform.position = targetPosition;
                    break;
                }

                if (flatDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
                    finalDog.transform.rotation = Quaternion.Slerp(finalDog.transform.rotation, targetRotation, Time.deltaTime * 8f);
                }

                finalDog.transform.position = Vector3.MoveTowards(
                    currentPosition,
                    targetPosition,
                    runSpeed * Time.deltaTime);

                yield return null;
            }

            finalDogRunRoutine = null;
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
