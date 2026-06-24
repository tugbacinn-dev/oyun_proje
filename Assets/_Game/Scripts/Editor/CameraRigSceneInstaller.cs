using PatininIzinde.Characters;
using PatininIzinde.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    [InitializeOnLoad]
    public static class CameraRigSceneInstaller
    {
        private const string TargetSceneName = "anasahne";
        private const string RigName = "PlayerCameraRig";
        private const string CameraName = "PlayerViewCamera";

        static CameraRigSceneInstaller()
        {
            EditorApplication.delayCall += InstallIfNeeded;
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Kamera Oyuncuyu Kur")]
        public static void InstallIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid() || activeScene.name != TargetSceneName)
            {
                return;
            }

            GameObject rig = GameObject.Find(RigName);
            if (rig == null)
            {
                rig = new GameObject(RigName);
                SceneManager.MoveGameObjectToScene(rig, activeScene);
                rig.transform.position = new Vector3(0f, 1.1f, -8f);
                rig.transform.rotation = Quaternion.identity;
            }

            CharacterController controller = rig.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = rig.AddComponent<CharacterController>();
            }

            controller.height = 1.55f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.75f, 0f);
            controller.stepOffset = 0.25f;

            FirstPersonCameraController movement = rig.GetComponent<FirstPersonCameraController>();
            if (movement == null)
            {
                movement = rig.AddComponent<FirstPersonCameraController>();
            }

            if (rig.GetComponent<PlayerInteractor>() == null)
            {
                rig.AddComponent<PlayerInteractor>();
            }

            Camera camera = Camera.main;
            GameObject cameraObject;
            if (camera == null)
            {
                cameraObject = new GameObject(CameraName);
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 500f;
            }
            else
            {
                cameraObject = camera.gameObject;
                cameraObject.name = CameraName;
                cameraObject.tag = "MainCamera";
            }

            cameraObject.transform.SetParent(rig.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            cameraObject.transform.localRotation = Quaternion.identity;

            var movementSerialized = new SerializedObject(movement);
            movementSerialized.FindProperty("cameraPivot").objectReferenceValue = cameraObject.transform;
            movementSerialized.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = rig;
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Kamerayi Bizim Evin Onune Al")]
        public static void PlaceCameraAtHomeFront()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            GameObject rig = GameObject.Find(RigName);
            if (rig == null)
            {
                InstallIfNeeded();
                rig = GameObject.Find(RigName);
            }

            GameObject home = FindHomeObject();
            if (rig == null || home == null)
            {
                EditorUtility.DisplayDialog(
                    "Ev veya kamera bulunamadi",
                    "PlayerCameraRig ve adinda Bizim/Bizim_Ev gecen ev objesi sahnede olmali.",
                    "Tamam");
                return;
            }

            Bounds bounds = CalculateRendererBounds(home);
            Vector3 forward = home.transform.forward;
            Vector3 frontPosition = bounds.center - forward * (bounds.extents.z + 2.2f);
            frontPosition.y = bounds.min.y + 1.1f;

            rig.transform.position = frontPosition;
            rig.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            Selection.activeGameObject = rig;
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Kamera Yuksekligini Insan Seviyesine Ayarla")]
        public static void SetHumanViewHeight()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            GameObject rig = GameObject.Find(RigName);
            if (rig == null)
            {
                InstallIfNeeded();
                rig = GameObject.Find(RigName);
            }

            if (rig == null)
            {
                EditorUtility.DisplayDialog("Kamera bulunamadi", "PlayerCameraRig sahnede bulunamadi.", "Tamam");
                return;
            }

            CharacterController controller = rig.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = rig.AddComponent<CharacterController>();
            }

            controller.height = 1.8f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            Camera camera = rig.GetComponentInChildren<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject(CameraName);
                cameraObject.transform.SetParent(rig.transform, false);
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.gameObject.name = CameraName;
            camera.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            camera.transform.localRotation = Quaternion.identity;
            camera.nearClipPlane = 0.03f;

            Selection.activeGameObject = rig;
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Kamerayi Secili Objeye Yakin Baslat")]
        public static void PlaceCameraNearSelectedObject()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once masanin yanindaki referans objeyi sec.", "Tamam");
                return;
            }

            GameObject rig = GameObject.Find(RigName);
            if (rig == null)
            {
                InstallIfNeeded();
                rig = GameObject.Find(RigName);
            }

            if (rig == null)
            {
                EditorUtility.DisplayDialog("Kamera bulunamadi", "PlayerCameraRig sahnede bulunamadi.", "Tamam");
                return;
            }

            Bounds bounds = CalculateRendererBounds(selected);
            Vector3 startPosition = bounds.center + new Vector3(0f, -0.85f, -1.65f);
            startPosition.y = bounds.min.y - 0.65f;

            rig.transform.position = startPosition;
            rig.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject marker = GameObject.Find("BizimEv_Ic_Baslangic");
            if (marker == null)
            {
                marker = new GameObject("BizimEv_Ic_Baslangic");
                SceneManager.MoveGameObjectToScene(marker, activeScene);
            }

            marker.transform.position = startPosition;
            marker.transform.rotation = rig.transform.rotation;

            SetHumanViewHeight();

            Selection.activeGameObject = rig;
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        private static GameObject FindHomeObject()
        {
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject candidate in objects)
            {
                string lowerName = candidate.name.ToLowerInvariant();
                if (lowerName.Contains("bizim") && lowerName.Contains("ev"))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Bounds CalculateRendererBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one * 4f);
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
