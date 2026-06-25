using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class MinimapSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Mini Haritayi Kur")]
        public static void SetupMinimap()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            MinimapController minimap = Object.FindFirstObjectByType<MinimapController>();
            if (minimap == null)
            {
                GameObject minimapObject = new GameObject("MiniMap");
                SceneManager.MoveGameObjectToScene(minimapObject, scene);
                minimap = minimapObject.AddComponent<MinimapController>();
                Undo.RegisterCreatedObjectUndo(minimapObject, "Create Minimap");
            }

            ConfigureMinimap(minimap);
            Selection.activeGameObject = minimap.gameObject;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Mini Haritada Bizim Ev Yap")]
        public static void SetSelectedAsHomePoint()
        {
            SetSelectedPoint("homePoint", "Bizim ev noktasi");
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Mini Haritada Perihan Teyze Evi Yap")]
        public static void SetSelectedAsAysePoint()
        {
            SetSelectedPoint("ayseHousePoint", "Perihan Teyze evi noktasi");
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Mini Haritada Guvenli Toplanma Alani Yap")]
        public static void SetSelectedAsAssemblyAreaPoint()
        {
            SetSelectedPoint("assemblyAreaPoint", "guvenli toplanma alani noktasi");
        }

        private static void SetSelectedPoint(string propertyName, string label)
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
                EditorUtility.DisplayDialog("Secim yok", $"Once {label} olacak objeyi sec.", "Tamam");
                return;
            }

            MinimapController minimap = Object.FindFirstObjectByType<MinimapController>();
            if (minimap == null)
            {
                SetupMinimap();
                minimap = Object.FindFirstObjectByType<MinimapController>();
            }

            if (minimap == null)
            {
                return;
            }

            SerializedObject serializedMinimap = new SerializedObject(minimap);
            serializedMinimap.FindProperty(propertyName).objectReferenceValue = selected.transform;
            serializedMinimap.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = minimap.gameObject;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ConfigureMinimap(MinimapController minimap)
        {
            SerializedObject serializedMinimap = new SerializedObject(minimap);
            serializedMinimap.FindProperty("player").objectReferenceValue = FindTransform("PlayerCameraRig");
            serializedMinimap.FindProperty("homePoint").objectReferenceValue = FindTransform("BizimEv_Dis_Kapi");
            serializedMinimap.FindProperty("ayseHousePoint").objectReferenceValue = FindTransform("AyseTeyze_Dis_Kapi") ?? FindTransform("AyseEv_Dis_Donus") ?? FindTransform("teyzeevi");
            serializedMinimap.FindProperty("assemblyAreaPoint").objectReferenceValue =
                FindTransform("Guvenli_Toplanma_Alani") ??
                FindTransform("Toplanma_Alani") ??
                FindTransform("Pati_Sifre_Notu") ??
                FindTransform("Env_Playground (1)");
            serializedMinimap.FindProperty("mapWorldRadius").floatValue = 70f;
            serializedMinimap.FindProperty("mapSize").vector2Value = new Vector2(240f, 190f);
            serializedMinimap.FindProperty("screenMargin").vector2Value = new Vector2(24f, 28f);
            serializedMinimap.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindTransform(string objectName)
        {
            GameObject found = GameObject.Find(objectName);
            return found != null ? found.transform : null;
        }
    }
}
