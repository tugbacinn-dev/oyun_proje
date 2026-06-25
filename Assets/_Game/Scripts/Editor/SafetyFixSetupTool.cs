using PatininIzinde.QuestSystem;
using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class SafetyFixSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Esyalari Perihan Teyze Sabitleme Gorevi Yap")]
        public static void SetupSelectedSafetyItems()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once Perihan Teyze'nin evinde sabitlenecek esyalari sec.", "Tamam");
                return;
            }

            Scene scene = EditorSceneManager.GetActiveScene();
            QuestManager questManager = Object.FindFirstObjectByType<QuestManager>();
            if (questManager == null)
            {
                GameObject questObject = new GameObject("QuestManager");
                SceneManager.MoveGameObjectToScene(questObject, scene);
                questManager = questObject.AddComponent<QuestManager>();
            }

            SafetyFixManager safetyFixManager = Object.FindFirstObjectByType<SafetyFixManager>();
            if (safetyFixManager == null)
            {
                GameObject managerObject = new GameObject("SafetyFixManager");
                SceneManager.MoveGameObjectToScene(managerObject, scene);
                safetyFixManager = managerObject.AddComponent<SafetyFixManager>();
            }

            SerializedObject serializedManager = new SerializedObject(safetyFixManager);
            serializedManager.FindProperty("questManager").objectReferenceValue = questManager;
            serializedManager.FindProperty("activeStepId").stringValue = "secure_ayse_items";
            serializedManager.FindProperty("totalItems").intValue = selectedObjects.Length;
            serializedManager.FindProperty("fixedItems").intValue = 0;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            foreach (GameObject item in selectedObjects)
            {
                SetupItem(item, safetyFixManager);
            }

            GameUIController ui = Object.FindFirstObjectByType<GameUIController>();
            if (ui != null)
            {
                SerializedObject serializedUi = new SerializedObject(ui);
                serializedUi.FindProperty("safetyFixManager").objectReferenceValue = safetyFixManager;
                serializedUi.ApplyModifiedPropertiesWithoutUndo();
            }

            Selection.objects = selectedObjects;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void SetupItem(GameObject item, SafetyFixManager safetyFixManager)
        {
            Collider collider = item.GetComponent<Collider>();
            if (collider == null)
            {
                collider = item.AddComponent<BoxCollider>();
            }

            SafetyFixItem fixItem = item.GetComponent<SafetyFixItem>();
            if (fixItem == null)
            {
                fixItem = item.AddComponent<SafetyFixItem>();
            }

            SerializedObject serializedItem = new SerializedObject(fixItem);
            serializedItem.FindProperty("safetyFixManager").objectReferenceValue = safetyFixManager;
            serializedItem.FindProperty("itemDisplayName").stringValue = MakeNiceName(item.name);
            serializedItem.FindProperty("interactionText").stringValue = "E ile sabitle";
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string MakeNiceName(string rawName)
        {
            return rawName.Replace("_", " ").Replace("-", " ");
        }
    }
}
