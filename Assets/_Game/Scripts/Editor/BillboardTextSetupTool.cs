using PatininIzinde.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class BillboardTextSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Tabelaya Guvenli Toplanma Yazisi Ekle")]
        public static void AddAssemblyAreaTextToSelectedBillboard()
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
                EditorUtility.DisplayDialog("Secim yok", "Once yaziyi eklemek istedigin tabelayi sec.", "Tamam");
                return;
            }

            BillboardCustomText overlay = selected.GetComponent<BillboardCustomText>();
            if (overlay == null)
            {
                overlay = Undo.AddComponent<BillboardCustomText>(selected);
            }

            SerializedObject serializedOverlay = new SerializedObject(overlay);
            serializedOverlay.FindProperty("text").stringValue = "GUVENLI\nTOPLANMA ALANI";
            serializedOverlay.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(selected);
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
