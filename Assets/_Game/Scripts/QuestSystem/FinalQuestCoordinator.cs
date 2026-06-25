using PatininIzinde.UI;
using PatininIzinde.World;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class FinalQuestCoordinator : MonoBehaviour
    {
        private const string TalkToCanStepId = "talk_to_can";
        private const string AssemblyAreaStepId = "go_to_assembly_area";

        private static FinalQuestCoordinator instance;

        [SerializeField] private QuestManager questManager;
        [SerializeField] private GameUIController uiController;
        private bool subscribedToQuestManager;

        public static void EnsureExists(QuestManager manager)
        {
            if (instance != null)
            {
                instance.questManager = manager;
                instance.SubscribeToQuestManager();
                return;
            }

            GameObject coordinatorObject = new GameObject("FinalQuestCoordinator");
            instance = coordinatorObject.AddComponent<FinalQuestCoordinator>();
            instance.questManager = manager;
            instance.SubscribeToQuestManager();
            DontDestroyOnLoad(coordinatorObject);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToQuestManager();
        }

        private void Start()
        {
            ResolveReferences();
            SubscribeToQuestManager();
            ConfigureCanInteraction();
            EnsureFinalPasswordNote();
            HandleStepChanged(questManager != null ? questManager.CurrentStep : null);
        }

        private void OnDisable()
        {
            if (questManager != null)
            {
                questManager.StepChanged -= HandleStepChanged;
            }

            subscribedToQuestManager = false;
        }

        private void ResolveReferences()
        {
            if (questManager == null)
            {
                questManager = FindFirstObjectByType<QuestManager>();
            }

            if (uiController == null)
            {
                uiController = FindFirstObjectByType<GameUIController>();
            }
        }

        private void HandleStepChanged(QuestStep step)
        {
            if (step == null)
            {
                return;
            }

            if (step.StepId == TalkToCanStepId)
            {
                ConfigureCanInteraction();
            }

            if (step.StepId == AssemblyAreaStepId)
            {
                BillboardCustomText.EnsureOnBillboard("Env_Billboard_Small_2 (1)", "GUVENLI\nTOPLANMA ALANI");
                AssemblyAreaGoal.EnsureExists(questManager);
                EnsureFinalPasswordNote();
            }
        }

        private void SubscribeToQuestManager()
        {
            if (subscribedToQuestManager || questManager == null)
            {
                return;
            }

            questManager.StepChanged += HandleStepChanged;
            subscribedToQuestManager = true;
        }

        private void ConfigureCanInteraction()
        {
            GameObject can = FindCanObject();
            if (can == null)
            {
                return;
            }

            Collider collider = can.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsule = can.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 0.85f, 0f);
                capsule.radius = 0.55f;
                capsule.height = 1.8f;
                capsule.isTrigger = true;
            }
            else
            {
                collider.isTrigger = true;
            }

            StoryNoteInteractable note = can.GetComponent<StoryNoteInteractable>();
            if (note == null)
            {
                note = can.AddComponent<StoryNoteInteractable>();
            }

            note.Configure(
                questManager,
                uiController,
                TalkToCanStepId,
                "E ile Can ile konus",
                "Can'in Notu",
                "Kanka, son ipucu harfin D.\n\nBuraya kadar cok iyi geldin.\n\nSimdi yesil basamaklari takip ederek guvenli toplanma alanina git, son gorevini yerine getir ve Pati'yi bul.",
                true);
        }

        private void EnsureFinalPasswordNote()
        {
            GameObject existing = GameObject.Find("Pati_Sifre_Notu");
            if (existing != null)
            {
                ConfigurePasswordNote(existing);
                return;
            }

            GameObject umbrella = GameObject.Find("Env_Playground (1)") ?? FindByNameContains("env_playground", "(1)");
            if (umbrella == null)
            {
                return;
            }

            GameObject noteObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noteObject.name = "Pati_Sifre_Notu";
            noteObject.transform.SetParent(umbrella.transform, false);
            noteObject.transform.localPosition = new Vector3(0f, 1.35f, -0.18f);
            noteObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            noteObject.transform.localScale = new Vector3(0.48f, 0.32f, 0.035f);

            Renderer renderer = noteObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateNoteMaterial();
            }

            GameObject textObject = new GameObject("Sifre_Notu_Yazisi");
            textObject.transform.SetParent(noteObject.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.56f);
            textObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            textObject.transform.localScale = Vector3.one * 0.08f;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = "SIFRE";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 40;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = new Color(0.12f, 0.18f, 0.3f);

            ConfigurePasswordNote(noteObject);
        }

        private void ConfigurePasswordNote(GameObject noteObject)
        {
            BoxCollider collider = noteObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.size = new Vector3(5f, 5f, 60f);
            }

            FinalPasswordNoteInteractable interactable = noteObject.GetComponent<FinalPasswordNoteInteractable>();
            if (interactable == null)
            {
                interactable = noteObject.AddComponent<FinalPasswordNoteInteractable>();
            }
        }

        private static Material CreateNoteMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader)
            {
                name = "Pati_Sifre_Notu_Mat",
                color = new Color(1f, 0.88f, 0.32f, 1f)
            };
        }

        private static GameObject FindCanObject()
        {
            return GameObject.Find("arkadas_can") ??
                   GameObject.Find("Can") ??
                   GameObject.Find("little_boy_B") ??
                   FindByNameContains("arkadas", "can") ??
                   FindByNameContains("little", "boy");
        }

        private static GameObject FindByNameContains(string first, string second)
        {
            GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject candidate in objects)
            {
                string lowerName = candidate.name.ToLowerInvariant();
                if (lowerName.Contains(first) && lowerName.Contains(second))
                {
                    return candidate;
                }
            }

            return null;
        }

        public static GameObject FindCanForAssemblyArea()
        {
            return FindCanObject();
        }
    }
}
