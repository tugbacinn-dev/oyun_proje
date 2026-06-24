using PatininIzinde.Interaction;
using PatininIzinde.QuestSystem;
using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PatininIzinde.EditorTools
{
    public static class InitialQuestSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Ilk Gorev UI ve Anne Konusmasini Kur")]
        public static void SetupInitialQuest()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            QuestManager questManager = FindOrCreateQuestManager(scene);
            SetupQuestSteps(questManager);

            GameUIController uiController = FindOrCreateGameUI(scene, questManager);
            SetupMotherInteraction(questManager, uiController);
            SetupFatherInteraction(questManager, uiController);
            SetupAyseInteraction(questManager, uiController);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Anne Notunu Guncelle")]
        public static void UpdateMotherNoteOnly()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            QuestManager questManager = FindOrCreateQuestManager(scene);
            GameUIController uiController = Object.FindFirstObjectByType<GameUIController>();
            SetupMotherInteraction(questManager, uiController);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Karakteri Ayse Teyze Yap")]
        public static void MakeSelectedCharacterAyseTeyze()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once Ayse Teyze olacak karakteri sec.", "Tamam");
                return;
            }

            selected.name = "Ayse_Teyze";
            QuestManager questManager = FindOrCreateQuestManager(scene);
            GameUIController uiController = Object.FindFirstObjectByType<GameUIController>();
            SetupAyseInteraction(questManager, uiController);

            Selection.activeGameObject = selected;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Ayse Teyze Notunu Guncelle")]
        public static void UpdateAyseNoteOnly()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            QuestManager questManager = FindOrCreateQuestManager(scene);
            GameUIController uiController = Object.FindFirstObjectByType<GameUIController>();
            SetupAyseInteraction(questManager, uiController);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static QuestManager FindOrCreateQuestManager(Scene scene)
        {
            QuestManager questManager = Object.FindFirstObjectByType<QuestManager>();
            if (questManager != null)
            {
                return questManager;
            }

            GameObject questObject = new GameObject("QuestManager");
            SceneManager.MoveGameObjectToScene(questObject, scene);
            return questObject.AddComponent<QuestManager>();
        }

        private static void SetupQuestSteps(QuestManager questManager)
        {
            SerializedObject serializedQuest = new SerializedObject(questManager);
            SerializedProperty steps = serializedQuest.FindProperty("steps");
            steps.arraySize = 12;

            SetStep(steps.GetArrayElementAtIndex(0), "talk_to_mother", "Pati'yi bulmak icin annenle konus.", "Annen Pati'nin bir not biraktigini biliyor olabilir.", "A");
            SetStep(steps.GetArrayElementAtIndex(1), "prepare_bag", "Afet cantani hazirla.", "Dogru esyalari bul ve cantaya yerlestir.", "");
            SetStep(steps.GetArrayElementAtIndex(2), "talk_to_father", "Babaya git ve ikinci gorevi al.", "Afet cantasini tamamladin. Simdi babandan siradaki notu al.", "F");
            SetStep(steps.GetArrayElementAtIndex(3), "go_to_ayse", "Ayse Teyze'nin evine git.", "Ayse Teyze'nin evindeki riskli esyalari guvenli hale getir.", "");
            SetStep(steps.GetArrayElementAtIndex(4), "secure_ayse_items", "Ayse Teyze'nin evindeki riskli esyalari sabitle.", "Devrilebilecek veya dusme riski olan esyalari E ile sabitle.", "");
            SetStep(steps.GetArrayElementAtIndex(5), "leave_ayse_house", "Ayse Teyze'nin evinden cik.", "Tum esyalari sabitledin. Dis kapiya gidip Ayse Teyze'nin yanina don.", "");
            SetStep(steps.GetArrayElementAtIndex(6), "talk_to_ayse", "Ayse Teyze ile konus ve siradaki ipucunu al.", "Ayse Teyze sana yeni gorevi ve A harfini verecek.", "A");
            SetStep(steps.GetArrayElementAtIndex(7), "go_back_home", "Bizim eve geri don.", "Cok-Kapan-Tutun simulasyonu icin kendi evine geri git.", "");
            SetStep(steps.GetArrayElementAtIndex(8), "enter_home_for_simulation", "Bizim evin kapisindan iceri gir.", "Evin kapisinda E tusuna basarak iceri gir.", "");
            SetStep(steps.GetArrayElementAtIndex(9), "earthquake_moment", "Deprem aninda guvenli noktaya gec.", "Cok-Kapan-Tutun davranisini uygulamak icin guvenli noktayi bul.", "");
            SetStep(steps.GetArrayElementAtIndex(10), "talk_to_can", "Evin onunde seni bekleyen Can'dan son gorevi al.", "Arkadasin Can ile konus ve son guvenli toplanma gorevini ogren.", "D");
            SetStep(steps.GetArrayElementAtIndex(11), "go_to_assembly_area", "Guvenli toplanma alanina git.", "Depremden sonra binadan uzak, acik ve guvenli toplanma alanina ulas.", "");

            serializedQuest.FindProperty("currentStepIndex").intValue = 0;
            serializedQuest.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetStep(SerializedProperty step, string id, string title, string description, string rewardLetter)
        {
            step.FindPropertyRelative("StepId").stringValue = id;
            step.FindPropertyRelative("Title").stringValue = title;
            step.FindPropertyRelative("Description").stringValue = description;
            step.FindPropertyRelative("RewardLetter").stringValue = rewardLetter;
        }

        private static GameUIController FindOrCreateGameUI(Scene scene, QuestManager questManager)
        {
            GameUIController existing = Object.FindFirstObjectByType<GameUIController>();
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject oldGameUi = GameObject.Find("GameUI");
            if (oldGameUi != null)
            {
                Undo.DestroyObjectImmediate(oldGameUi);
            }

            GameObject canvasObject = new GameObject("GameUI");
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.enabled = false;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject questPanel = CreateAnchoredPanel(
                canvasObject.transform,
                "QuestPanel",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(26f, -26f),
                new Vector2(640f, 84f),
                new Color(0.06f, 0.08f, 0.11f, 0.82f));

            Text questLabel = CreateText(questPanel.transform, "QuestLabel", new Vector2(24f, -10f), new Vector2(590f, 22f), 15, TextAnchor.UpperLeft, new Color(0.58f, 0.78f, 1f));
            questLabel.text = "GOREV";

            Text questText = CreateText(questPanel.transform, "QuestText", new Vector2(24f, -34f), new Vector2(590f, 44f), 24, TextAnchor.UpperLeft, Color.white);
            questText.text = "Pati'yi bulmak icin annenle konus.";

            GameObject interactionPanel = CreateAnchoredPanel(
                canvasObject.transform,
                "InteractionPanel",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 105f),
                new Vector2(520f, 64f),
                new Color(0.04f, 0.05f, 0.07f, 0.86f));

            Text interactionText = CreateText(interactionPanel.transform, "InteractionText", Vector2.zero, new Vector2(500f, 54f), 28, TextAnchor.MiddleCenter, Color.white);
            interactionText.text = "E ile etkiles";
            interactionPanel.SetActive(false);

            GameObject notePanel = CreatePanel(canvasObject.transform, "NotePanel");
            Text noteTitle = CreateText(notePanel.transform, "NoteTitle", new Vector2(0f, -44f), new Vector2(620f, 58f), 34, TextAnchor.MiddleCenter, new Color(0.18f, 0.11f, 0.06f));
            Text noteBody = CreateText(notePanel.transform, "NoteBody", new Vector2(0f, -122f), new Vector2(610f, 190f), 25, TextAnchor.UpperCenter, new Color(0.16f, 0.12f, 0.09f));
            Text closeHint = CreateText(notePanel.transform, "CloseHint", new Vector2(0f, -282f), new Vector2(620f, 44f), 18, TextAnchor.MiddleCenter, new Color(0.38f, 0.25f, 0.15f));
            closeHint.text = "Devam etmek icin Space tusuna bas";

            GameUIController ui = canvasObject.AddComponent<GameUIController>();
            SerializedObject serializedUi = new SerializedObject(ui);
            serializedUi.FindProperty("questManager").objectReferenceValue = questManager;
            serializedUi.FindProperty("playerInteractor").objectReferenceValue = Object.FindFirstObjectByType<PlayerInteractor>();
            serializedUi.FindProperty("bagCollectionManager").objectReferenceValue = Object.FindFirstObjectByType<BagCollectionManager>();
            serializedUi.FindProperty("safetyFixManager").objectReferenceValue = Object.FindFirstObjectByType<SafetyFixManager>();
            serializedUi.FindProperty("questText").objectReferenceValue = questText;
            serializedUi.FindProperty("interactionText").objectReferenceValue = interactionText;
            serializedUi.FindProperty("notePanel").objectReferenceValue = notePanel;
            serializedUi.FindProperty("noteTitleText").objectReferenceValue = noteTitle;
            serializedUi.FindProperty("noteBodyText").objectReferenceValue = noteBody;
            serializedUi.ApplyModifiedPropertiesWithoutUndo();

            return ui;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            return text;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.96f, 0.86f, 0.66f, 0.97f);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(720f, 430f);

            GameObject accent = new GameObject("TopAccent");
            accent.transform.SetParent(panel.transform, false);
            Image accentImage = accent.AddComponent<Image>();
            accentImage.color = new Color(0.72f, 0.36f, 0.14f, 1f);
            RectTransform accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(0f, 18f);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateAnchoredPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            return panel;
        }

        private static void SetupMotherInteraction(QuestManager questManager, GameUIController uiController)
        {
            GameObject mother = GameObject.Find("Anne");
            if (mother == null)
            {
                EditorUtility.DisplayDialog("Anne bulunamadi", "Once Anne karakterini sahneye yerlestir.", "Tamam");
                return;
            }

            Collider collider = mother.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsule = mother.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 0.9f, 0f);
                capsule.radius = 0.55f;
                capsule.height = 2f;
                capsule.isTrigger = true;
            }

            StoryNoteInteractable note = mother.GetComponent<StoryNoteInteractable>();
            if (note == null)
            {
                note = mother.AddComponent<StoryNoteInteractable>();
            }

            SerializedObject serializedNote = new SerializedObject(note);
            serializedNote.FindProperty("questManager").objectReferenceValue = questManager;
            serializedNote.FindProperty("uiController").objectReferenceValue = uiController;
            serializedNote.FindProperty("requiredStepId").stringValue = "talk_to_mother";
            serializedNote.FindProperty("interactionText").stringValue = "E ile annenle konus";
            serializedNote.FindProperty("noteTitle").stringValue = "Annenin Notu";
            serializedNote.FindProperty("noteBody").stringValue =
                "Kizim, Pati'yi bulman icin ilk ipucu harfin A.\n\nBu harfleri aklinda tut; gorevler bittiginde isine yarayacak.\n\nSimdi afet gorevlerinin ilkini veriyorum: deprem cantani hazirla.";
            serializedNote.FindProperty("completeStepAfterReading").boolValue = true;
            serializedNote.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupFatherInteraction(QuestManager questManager, GameUIController uiController)
        {
            GameObject father = GameObject.Find("Baba");
            if (father == null)
            {
                return;
            }

            Collider collider = father.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsule = father.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 0.9f, 0f);
                capsule.radius = 0.55f;
                capsule.height = 2f;
                capsule.isTrigger = true;
            }

            StoryNoteInteractable note = father.GetComponent<StoryNoteInteractable>();
            if (note == null)
            {
                note = father.AddComponent<StoryNoteInteractable>();
            }

            SerializedObject serializedNote = new SerializedObject(note);
            serializedNote.FindProperty("questManager").objectReferenceValue = questManager;
            serializedNote.FindProperty("uiController").objectReferenceValue = uiController;
            serializedNote.FindProperty("requiredStepId").stringValue = "talk_to_father";
            serializedNote.FindProperty("interactionText").stringValue = "E ile babanla konus";
            serializedNote.FindProperty("noteTitle").stringValue = "Babanin Notu";
            serializedNote.FindProperty("noteBody").stringValue =
                "Aferin kizim, afet cantani hazirlamayi ogrendin.\n\nIkinci ipucu harfin F. Aman diyeyim, harfleri unutma.\n\nSimdi ucuncu gorevin geliyor: Ayse Teyze'nin evine git ve sabitlenmesi gereken esyalari sabitle.";
            serializedNote.FindProperty("completeStepAfterReading").boolValue = true;
            serializedNote.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupAyseInteraction(QuestManager questManager, GameUIController uiController)
        {
            GameObject ayse = GameObject.Find("Ayse_Teyze") ?? GameObject.Find("AyseTeyze") ?? GameObject.Find("elder_Female_A");
            if (ayse == null)
            {
                return;
            }

            Collider collider = ayse.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsule = ayse.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 0.9f, 0f);
                capsule.radius = 0.55f;
                capsule.height = 2f;
                capsule.isTrigger = true;
            }

            StoryNoteInteractable note = ayse.GetComponent<StoryNoteInteractable>();
            if (note == null)
            {
                note = ayse.AddComponent<StoryNoteInteractable>();
            }

            SerializedObject serializedNote = new SerializedObject(note);
            serializedNote.FindProperty("questManager").objectReferenceValue = questManager;
            serializedNote.FindProperty("uiController").objectReferenceValue = uiController;
            serializedNote.FindProperty("requiredStepId").stringValue = "talk_to_ayse";
            serializedNote.FindProperty("interactionText").stringValue = "E ile Ayse Teyze ile konus";
            serializedNote.FindProperty("noteTitle").stringValue = "Ayse Teyze'nin Notu";
            serializedNote.FindProperty("noteBody").stringValue =
                "Kuzum, yardim ettigin icin cok tesekkur ederim.\n\nIpucu harfin A. Bu harfleri unutma.\n\nSimdi yeni gorevin: evine geri gidip Cok-Kapan-Tutun simulasyonunu ogren bakalim.";
            serializedNote.FindProperty("completeStepAfterReading").boolValue = true;
            serializedNote.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
