using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class CubeStepColorTool
    {
        private const string GreenMaterialPath = "Assets/_Game/Materials/Yesil_Basamak_Mat.mat";
        private const string AutoColorSessionKey = "PatininIzinde_AutoColorCubeSteps";

        private static readonly HashSet<string> ManualStepNames = new HashSet<string>
        {
            "Cube (5)",
            "Cube (10)",
            "Cube (11)",
            "Cube (12)",
            "Cube (13)",
            "Cube (14)",
            "Cube (15)",
            "Cube (16)",
            "Cube (17)"
        };

        [InitializeOnLoadMethod]
        private static void AutoColorCurrentSelection()
        {
            if (SessionState.GetBool(AutoColorSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(AutoColorSessionKey, true);
            EditorApplication.delayCall += () =>
            {
                int coloredCount = ColorSelectionAndKnownStepCubes(false);
                if (coloredCount > 0)
                {
                    Debug.Log($"{coloredCount} basamak kupu yesile boyandi.");
                }
            };
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Kupleri Yesil Yap")]
        public static void ColorSelectedCubes()
        {
            int coloredCount = ColorRenderers(Selection.gameObjects);
            EditorUtility.DisplayDialog(
                "Kupler yesil yapildi",
                $"{coloredCount} secili kup yesile boyandi.",
                "Tamam");
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Ayse Basamak Kuplerini Yesil Yap")]
        public static void ColorKnownStepCubes()
        {
            int coloredCount = ColorSelectionAndKnownStepCubes(true);
            EditorUtility.DisplayDialog(
                "Basamaklar yesil yapildi",
                $"{coloredCount} basamak kupu yesile boyandi.",
                "Tamam");
        }

        private static int ColorSelectionAndKnownStepCubes(bool includeDialogSelection)
        {
            HashSet<GameObject> targets = new HashSet<GameObject>();

            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                if (selectedObject != null && selectedObject.scene.IsValid())
                {
                    targets.Add(selectedObject);
                }
            }

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject sceneObject in allObjects)
            {
                if (sceneObject == null || !sceneObject.scene.IsValid() || sceneObject.scene != activeScene)
                {
                    continue;
                }

                if (ManualStepNames.Contains(sceneObject.name))
                {
                    targets.Add(sceneObject);
                }
            }

            int coloredCount = ColorRenderers(targets);
            if (includeDialogSelection && coloredCount == 0)
            {
                Debug.LogWarning("Yesile boyanacak kup bulunamadi. Kupleri secip menuyu tekrar calistir.");
            }

            return coloredCount;
        }

        private static int ColorRenderers(IEnumerable<GameObject> targets)
        {
            Material greenMaterial = EnsureGreenMaterial();
            if (greenMaterial == null)
            {
                return 0;
            }

            HashSet<Renderer> renderers = new HashSet<Renderer>();
            foreach (GameObject target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                Renderer[] childRenderers = target.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in childRenderers)
                {
                    if (renderer != null)
                    {
                        renderers.Add(renderer);
                    }
                }
            }

            foreach (Renderer renderer in renderers)
            {
                Undo.RecordObject(renderer, "Color Cube Step Green");
                renderer.sharedMaterial = greenMaterial;
                EditorUtility.SetDirty(renderer);
            }

            if (renderers.Count > 0)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            return renderers.Count;
        }

        private static Material EnsureGreenMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(GreenMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                Debug.LogWarning("Yesil materyal icin uygun shader bulunamadi.");
                return null;
            }

            material = new Material(shader)
            {
                name = "Yesil_Basamak_Mat",
                color = new Color(0.18f, 0.82f, 0.2f, 1f)
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", new Color(0.18f, 0.82f, 0.2f, 1f));
            }

            AssetDatabase.CreateAsset(material, GreenMaterialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return material;
        }
    }
}
