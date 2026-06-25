using PatininIzinde.QuestSystem;
using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class BagQuestSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Esyalari Afet Cantasi Gorev Esyasi Yap")]
        public static void SetupSelectedBagItems()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once afet cantasi icin toplanacak esyalari sec.", "Tamam");
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

            BagCollectionManager bagManager = Object.FindFirstObjectByType<BagCollectionManager>();
            if (bagManager == null)
            {
                GameObject managerObject = new GameObject("BagCollectionManager");
                SceneManager.MoveGameObjectToScene(managerObject, scene);
                bagManager = managerObject.AddComponent<BagCollectionManager>();
            }

            SerializedObject serializedManager = new SerializedObject(bagManager);
            serializedManager.FindProperty("questManager").objectReferenceValue = questManager;
            serializedManager.FindProperty("activeStepId").stringValue = "prepare_bag";
            serializedManager.FindProperty("totalItems").intValue = selectedObjects.Length;
            serializedManager.FindProperty("collectedItems").intValue = 0;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            foreach (GameObject item in selectedObjects)
            {
                SetupItem(item, bagManager);
            }

            GameUIController ui = Object.FindFirstObjectByType<GameUIController>();
            if (ui != null)
            {
                SerializedObject serializedUi = new SerializedObject(ui);
                serializedUi.FindProperty("bagCollectionManager").objectReferenceValue = bagManager;
                serializedUi.ApplyModifiedPropertiesWithoutUndo();
            }

            Selection.objects = selectedObjects;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void SetupItem(GameObject item, BagCollectionManager bagManager)
        {
            Collider collider = item.GetComponent<Collider>();
            if (collider == null)
            {
                collider = item.AddComponent<BoxCollider>();
            }

            BagCollectibleItem collectible = item.GetComponent<BagCollectibleItem>();
            if (collectible == null)
            {
                collectible = item.AddComponent<BagCollectibleItem>();
            }

            SerializedObject serializedItem = new SerializedObject(collectible);
            serializedItem.FindProperty("bagManager").objectReferenceValue = bagManager;
            serializedItem.FindProperty("itemDisplayName").stringValue = MakeNiceName(item.name);
            serializedItem.FindProperty("itemPurpose").stringValue = MakePurpose(item.name);
            serializedItem.FindProperty("interactionText").stringValue = "E ile al";
            serializedItem.FindProperty("hideWhenCollected").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string MakeNiceName(string rawName)
        {
            return rawName.Replace("_", " ").Replace("-", " ");
        }

        private static string MakePurpose(string rawName)
        {
            string itemName = rawName.ToLowerInvariant();
            if (itemName.Contains("pusula") || itemName.Contains("compass"))
            {
                return "yon bulmak icin";
            }

            if (itemName.Contains("hap") || itemName.Contains("pill"))
            {
                return "ilac ihtiyaci icin";
            }

            if (itemName.Contains("su") || itemName.Contains("water"))
            {
                return "susuz kalmamak icin";
            }

            if (itemName.Contains("canopen"))
            {
                return "konserve acmak icin";
            }

            if (itemName.Contains("konserve") || itemName.Contains("sardine") || itemName.Contains("canned"))
            {
                return "acil yiyecek";
            }

            if (itemName.Contains("fener") || itemName.Contains("flashlight"))
            {
                return "karanlikta gormek icin";
            }

            if (itemName.Contains("pil") || itemName.Contains("battery"))
            {
                return "cihazlara enerji";
            }

            if (itemName.Contains("ilk") || itemName.Contains("firstaid"))
            {
                return "yaralanmalara ilk yardim";
            }

            if (itemName.Contains("telsiz") || itemName.Contains("radio") || itemName.Contains("walkie"))
            {
                return "haber almak icin";
            }

            if (itemName.Contains("kibrit") || itemName.Contains("match"))
            {
                return "ates yakmak icin";
            }

            return "acil durumda lazim";
        }
    }
}
