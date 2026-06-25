using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PatininIzinde.EditorTools
{
    public static class EndingScreenSetupTool
    {
        [MenuItem("Pati'nin Izinde/Secili Resmi Bitis Ekrani Yap")]
        public static void AssignSelectedImageAsEndingScreen()
        {
            Texture2D texture = Selection.activeObject as Texture2D;
            if (texture == null && Selection.activeObject is Sprite sprite)
            {
                texture = sprite.texture;
            }

            if (texture == null)
            {
                EditorUtility.DisplayDialog("Gorsel secilmedi", "Project panelinden bir Texture2D veya Sprite sec.", "Tamam");
                return;
            }

            GameUIController uiController = Object.FindFirstObjectByType<GameUIController>();
            if (uiController == null)
            {
                EditorUtility.DisplayDialog("GameUI bulunamadi", "Sahnede GameUIController olan GameUI objesini bulamadim.", "Tamam");
                return;
            }

            SerializedObject serializedUi = new SerializedObject(uiController);
            SerializedProperty endingTexture = serializedUi.FindProperty("endingScreenTexture");
            endingTexture.objectReferenceValue = texture;
            serializedUi.ApplyModifiedProperties();

            EditorUtility.SetDirty(uiController);
            EditorSceneManager.MarkSceneDirty(uiController.gameObject.scene);
            EditorUtility.DisplayDialog("Hazir", "Secili gorsel bitis ekrani olarak ayarlandi.", "Tamam");
        }

        [MenuItem("Pati'nin Izinde/Secili Resmi Bitis Ekrani Yap", true)]
        public static bool CanAssignSelectedImageAsEndingScreen()
        {
            return Selection.activeObject is Texture2D || Selection.activeObject is Sprite;
        }
    }
}
