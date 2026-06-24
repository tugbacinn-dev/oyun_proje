using PatininIzinde.Characters;
using PatininIzinde.Interaction;
using PatininIzinde.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class IntroScreenSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Oyun Baslangic Amac Ekranini Kur")]
        public static void SetupIntroScreen()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            IntroScreenController intro = Object.FindFirstObjectByType<IntroScreenController>();
            if (intro == null)
            {
                GameObject introObject = new GameObject("IntroScreen");
                SceneManager.MoveGameObjectToScene(introObject, scene);
                intro = introObject.AddComponent<IntroScreenController>();
                Undo.RegisterCreatedObjectUndo(introObject, "Create Intro Screen");
            }

            SerializedObject serializedIntro = new SerializedObject(intro);
            serializedIntro.FindProperty("showOnStart").boolValue = true;
            serializedIntro.FindProperty("continueKey").enumValueIndex = (int)KeyCode.Space;
            serializedIntro.FindProperty("playerController").objectReferenceValue = Object.FindFirstObjectByType<FirstPersonCameraController>();
            serializedIntro.FindProperty("playerInteractor").objectReferenceValue = Object.FindFirstObjectByType<PlayerInteractor>();
            serializedIntro.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = intro.gameObject;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Resmi Baslangic Ekrani Yap")]
        public static void AssignSelectedImageToIntroScreen()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            Texture2D selectedTexture = Selection.activeObject as Texture2D;
            if (selectedTexture == null)
            {
                EditorUtility.DisplayDialog("Resim secilmedi", "Project panelinden baslangic ekrani olarak kullanacagin PNG/JPG resmi sec.", "Tamam");
                return;
            }

            IntroScreenController intro = Object.FindFirstObjectByType<IntroScreenController>();
            if (intro == null)
            {
                SetupIntroScreen();
                intro = Object.FindFirstObjectByType<IntroScreenController>();
            }

            if (intro == null)
            {
                EditorUtility.DisplayDialog("Intro bulunamadi", "IntroScreen olusturulamadi.", "Tamam");
                return;
            }

            SerializedObject serializedIntro = new SerializedObject(intro);
            serializedIntro.FindProperty("customIntroImage").objectReferenceValue = selectedTexture;
            serializedIntro.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = intro.gameObject;
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
