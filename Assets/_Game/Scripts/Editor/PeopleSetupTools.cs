using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using PatininIzinde.QuestSystem;

namespace PatininIzinde.EditorTools
{
    public static class PeopleSetupTools
    {
        private const string FemaleAnimatorPath = "Assets/DenysAlmaral/CityPeople/Animations/City F Animator.controller";
        private const string MotherIdleControllerPath = "Assets/_Game/Art/Mother_Idle.controller";
        private const string MotherIdleClipPath = "Assets/DenysAlmaral/CityPeople/Animations/idle_f_1_150f.fbx";

        [MenuItem("Pati'nin Izinde/Kurulum/Anne Animasyonunu Ayarla")]
        public static void SetupMotherAnimation()
        {
            GameObject mother = GameObject.Find("Anne");
            if (mother == null)
            {
                EditorUtility.DisplayDialog("Anne bulunamadi", "Sahnede Anne isimli karakter bulunamadi.", "Tamam");
                return;
            }

            Animator animator = mother.GetComponent<Animator>();
            if (animator == null)
            {
                animator = mother.AddComponent<Animator>();
            }

            RuntimeAnimatorController controller = GetOrCreateMotherIdleController();
            if (controller == null)
            {
                controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(FemaleAnimatorPath);
            }

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            MonoBehaviour cityPeopleScript = mother.GetComponent("CityPeople") as MonoBehaviour;
            if (cityPeopleScript != null)
            {
                SerializedObject serializedCityPeople = new SerializedObject(cityPeopleScript);
                SerializedProperty autoPlay = serializedCityPeople.FindProperty("AutoPlayAnimations");
                if (autoPlay != null)
                {
                    autoPlay.boolValue = false;
                    serializedCityPeople.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            CapsuleCollider collider = mother.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = mother.AddComponent<CapsuleCollider>();
            }

            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.radius = 0.55f;
            collider.height = 2f;

            if (mother.GetComponent<QuestInteractable>() == null)
            {
                mother.AddComponent<QuestInteractable>();
            }

            Selection.activeGameObject = mother;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static RuntimeAnimatorController GetOrCreateMotherIdleController()
        {
            RuntimeAnimatorController existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(MotherIdleControllerPath);
            if (existing != null)
            {
                return existing;
            }

            AnimationClip idleClip = LoadFirstAnimationClip(MotherIdleClipPath);
            if (idleClip == null)
            {
                return null;
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(MotherIdleControllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = stateMachine.AddState("Anne_Bekleme");
            idleState.motion = idleClip;
            idleState.speed = 1f;
            stateMachine.defaultState = idleState;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return controller;
        }

        private static AnimationClip LoadFirstAnimationClip(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            return null;
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Casual Female G Karakterini Anne Yap")]
        public static void SetupMotherInKitchen()
        {
            GameObject mother = GameObject.Find("casual_Female_G") ?? GameObject.Find("Anne");
            if (mother == null)
            {
                EditorUtility.DisplayDialog(
                    "Anne karakteri bulunamadi",
                    "Sahnede casual_Female_G isimli karakter bulunamadi.",
                    "Tamam");
                return;
            }

            GameObject kitchenReference = GameObject.Find("cabnet") ?? GameObject.Find("cabnet (1)");
            if (kitchenReference == null)
            {
                EditorUtility.DisplayDialog(
                    "Mutfak referansi bulunamadi",
                    "Sahnede cabnet isimli mutfak objesi bulunamadi.",
                    "Tamam");
                return;
            }

            Undo.RecordObject(mother.transform, "Place Mother In Kitchen");
            Undo.RecordObject(mother, "Rename Mother");

            mother.name = "Anne";

            Bounds kitchenBounds = CalculateRendererBounds(kitchenReference);
            Vector3 targetPosition = kitchenBounds.center + new Vector3(1.15f, 0f, 0.95f);
            targetPosition.y = kitchenBounds.min.y + 2.05f;

            mother.transform.position = targetPosition;
            mother.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            Selection.activeGameObject = mother;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Karakteri Anne Yap ve Secili Yere Yerlestir")]
        public static void SetupSelectedCharacterAsMother()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once anne olacak karakteri sec.", "Tamam");
                return;
            }

            Undo.RecordObject(selected, "Rename Mother");
            selected.name = "Anne";
            Selection.activeGameObject = selected;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }
    }
}
