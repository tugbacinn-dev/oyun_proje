using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using PatininIzinde.Core;

namespace PatininIzinde.EditorTools
{
    public sealed class ProjectApprovalWindow : EditorWindow
    {
        private const string ApprovalRule =
            "Buyuk mimari degisiklikler kullanici onayi olmadan yapilmaz.";

        [MenuItem("Pati'nin Izinde/Proje Onay Paneli")]
        public static void Open()
        {
            var window = GetWindow<ProjectApprovalWindow>("Proje Onay Paneli");
            window.minSize = new Vector2(420f, 360f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Pati'nin Izinde", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Proje Onay Paneli", EditorStyles.largeLabel);
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(ApprovalRule, MessageType.Info);

            DrawApprovalItem("Gorev sistemi ana mimarisi", "Onay gerekir");
            DrawApprovalItem("UI yoneticileri ve ekran akisi", "Onay gerekir");
            DrawApprovalItem("Sahne organizasyonu", "Onay gerekir");
            DrawApprovalItem("Karakter kontrol yapisi", "Onay gerekir");
            DrawApprovalItem("Kucuk bug duzeltmeleri", "Onay gerekmez");

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Mevcut Onayli Akis", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(
                "Anne -> Afet Cantasi -> Baba -> Ayse Teyze -> Deprem Ani -> Tahliye/112 -> AFAD Sifresi -> Pati",
                GUILayout.MinHeight(46f));

            if (GUILayout.Button("Tasarim Dokumanini Sec"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(
                    "Assets/_Game/Documentation/DESIGN.md");
            }
        }

        private static void DrawApprovalItem(string title, string status)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(status, GUILayout.Width(120f));
            }
        }
    }

    public sealed class SceneSetupAssistantWindow : EditorWindow
    {
        private const string DemoHouseScenePath = "Assets/contemporary house/Scenes/demo.unity";
        private const string HouseModelPath = "Assets/contemporary house/model/contemporary_house.fbx";

        private Vector3 housePosition = new Vector3(-18f, 0f, 0f);
        private Vector3 houseRotation = Vector3.zero;
        private Vector3 houseScale = Vector3.one;
        private Vector3 ayseHouseOffset = new Vector3(12f, 0f, 0f);

        [MenuItem("Pati'nin Izinde/Sahne Yardimcisi")]
        public static void Open()
        {
            var window = GetWindow<SceneSetupAssistantWindow>("Sahne Yardimcisi");
            window.minSize = new Vector2(440f, 500f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Sahne Yardimcisi", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Bu arac aktif acik sahnede calisir. Asset paketlerinin kendi dosyalarini tasimaz veya silmez.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Contemporary House Aktarma", EditorStyles.boldLabel);
            housePosition = EditorGUILayout.Vector3Field("Pozisyon", housePosition);
            houseRotation = EditorGUILayout.Vector3Field("Rotasyon", houseRotation);
            houseScale = EditorGUILayout.Vector3Field("Scale", houseScale);
            ayseHouseOffset = EditorGUILayout.Vector3Field("Ayse Teyze Ev Offset", ayseHouseOffset);

            if (GUILayout.Button("Iki Dis Evi Kopyala"))
            {
                CopyTwoExteriorHousesIntoActiveScene();
            }

            if (GUILayout.Button("Ev ve Ic Mekan Setini Kopyala"))
            {
                CopyDemoHouseSetIntoActiveScene();
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Isik ve Kamera", EditorStyles.boldLabel);

            if (GUILayout.Button("Fazla Kamera ve Directional Light Objelerini Pasif Yap"))
            {
                DisableDuplicateCamerasAndDirectionalLights();
            }

            if (GUILayout.Button("Cocuk Dostu Gunduz Isigini Uygula"))
            {
                ApplyFriendlyDaylight();
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Hikaye Noktalari", EditorStyles.boldLabel);

            if (GUILayout.Button("Temel Hikaye Noktalarini Olustur"))
            {
                CreateStoryPoints();
            }
        }

        private void CopyDemoHouseSetIntoActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!CanEditActiveScene(activeScene))
            {
                return;
            }

            var demoScene = EditorSceneManager.OpenScene(DemoHouseScenePath, OpenSceneMode.Additive);
            var sourceRoots = demoScene.GetRootGameObjects().Where(IsHouseContentRoot).ToList();

            if (sourceRoots.Count == 0)
            {
                EditorSceneManager.CloseScene(demoScene, true);
                EditorUtility.DisplayDialog("Ic mekan bulunamadi", "Demo sahnesinde ev/oda ana objeleri bulunamadi.", "Tamam");
                return;
            }

            var parent = new GameObject("Bizim_Ev_Ic_Mekan_Seti");
            parent.transform.position = housePosition;
            parent.transform.eulerAngles = houseRotation;
            parent.transform.localScale = houseScale;
            SceneManager.MoveGameObjectToScene(parent, activeScene);

            foreach (var sourceRoot in sourceRoots)
            {
                var clone = Instantiate(sourceRoot);
                clone.name = sourceRoot.name;
                SceneManager.MoveGameObjectToScene(clone, activeScene);
                clone.transform.SetParent(parent.transform, false);
                clone.transform.localPosition = sourceRoot.transform.localPosition;
                clone.transform.localRotation = sourceRoot.transform.localRotation;
                clone.transform.localScale = sourceRoot.transform.localScale;
            }

            Selection.activeGameObject = parent;
            EditorSceneManager.CloseScene(demoScene, true);
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        private void CopyTwoExteriorHousesIntoActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!CanEditActiveScene(activeScene))
            {
                return;
            }

            var sourceRoot = AssetDatabase.LoadAssetAtPath<GameObject>(HouseModelPath);
            if (sourceRoot != null)
            {
                var bizimEvFromModel = CreateExteriorHouseCopy(sourceRoot, activeScene, "Bizim_Ev_Disi", housePosition);
                CreateExteriorHouseCopy(sourceRoot, activeScene, "Ayse_Teyze_Evi_Disi", housePosition + ayseHouseOffset);

                Selection.activeGameObject = bizimEvFromModel;
                EditorSceneManager.MarkSceneDirty(activeScene);
                return;
            }

            var demoScene = EditorSceneManager.OpenScene(DemoHouseScenePath, OpenSceneMode.Additive);
            var demoRoot = demoScene.GetRootGameObjects().FirstOrDefault(root => root.name == "houses");

            if (demoRoot == null)
            {
                EditorSceneManager.CloseScene(demoScene, true);
                EditorUtility.DisplayDialog(
                    "Dis ev bulunamadi",
                    "FBX model ve demo sahnesindeki houses objesi bulunamadi.",
                    "Tamam");
                return;
            }

            var bizimEv = CreateExteriorHouseCopy(demoRoot, activeScene, "Bizim_Ev_Disi", housePosition);
            CreateExteriorHouseCopy(demoRoot, activeScene, "Ayse_Teyze_Evi_Disi", housePosition + ayseHouseOffset);

            Selection.activeGameObject = bizimEv;
            EditorSceneManager.CloseScene(demoScene, true);
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        private GameObject CreateExteriorHouseCopy(GameObject sourceRoot, Scene targetScene, string cloneName, Vector3 position)
        {
            var clone = Instantiate(sourceRoot);
            clone.name = cloneName;
            SceneManager.MoveGameObjectToScene(clone, targetScene);
            clone.transform.position = position;
            clone.transform.eulerAngles = houseRotation;
            clone.transform.localScale = houseScale;
            return clone;
        }

        private static bool CanEditActiveScene(Scene activeScene)
        {
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once hedef sahneyi ac.", "Tamam");
                return false;
            }

            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        private static bool IsHouseContentRoot(GameObject root)
        {
            var name = root.name.ToLowerInvariant();

            if (name.Contains("camera") ||
                name.Contains("light") ||
                name.Contains("player") ||
                name.Contains("skybox"))
            {
                return false;
            }

            return name.Contains("house") ||
                   name.Contains("room") ||
                   name.Contains("windo") ||
                   name.Contains("door") ||
                   name.Contains("fence") ||
                   name.Contains("plane");
        }

        private static void ApplyFriendlyDaylight()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var sun = FindObjectsByType<Light>(FindObjectsSortMode.None)
                .FirstOrDefault(light => light.type == LightType.Directional && light.gameObject.scene == scene);

            if (sun == null)
            {
                var sunObject = new GameObject("Directional Light - Warm Day");
                SceneManager.MoveGameObjectToScene(sunObject, scene);
                sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            Undo.RecordObject(sun.transform, "Apply Friendly Daylight");
            Undo.RecordObject(sun, "Apply Friendly Daylight");

            sun.name = "Directional Light - Warm Day";
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.94f, 0.82f);
            sun.shadows = LightShadows.Soft;

            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.75f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.72f, 0.78f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.42f, 0.36f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = false;

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void DisableDuplicateCamerasAndDirectionalLights()
        {
            var scene = EditorSceneManager.GetActiveScene();

            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .Where(camera => camera.gameObject.scene == scene)
                .ToList();
            var mainCamera = cameras.FirstOrDefault(camera => camera.CompareTag("MainCamera")) ?? cameras.FirstOrDefault();

            foreach (var camera in cameras)
            {
                if (camera == mainCamera)
                {
                    continue;
                }

                Undo.RecordObject(camera.gameObject, "Disable Duplicate Cameras");
                camera.gameObject.SetActive(false);
            }

            if (mainCamera != null)
            {
                Undo.RecordObject(mainCamera.gameObject, "Keep Main Camera");
                mainCamera.gameObject.SetActive(true);
                mainCamera.tag = "MainCamera";
            }

            var directionalLights = FindObjectsByType<Light>(FindObjectsSortMode.None)
                .Where(light => light.gameObject.scene == scene && light.type == LightType.Directional)
                .ToList();
            var primaryLight = directionalLights.OrderByDescending(light => light.intensity).FirstOrDefault();

            foreach (var light in directionalLights)
            {
                if (light == primaryLight)
                {
                    continue;
                }

                Undo.RecordObject(light.gameObject, "Disable Duplicate Directional Lights");
                light.gameObject.SetActive(false);
            }

            if (primaryLight != null)
            {
                Undo.RecordObject(primaryLight.gameObject, "Keep Primary Directional Light");
                primaryLight.gameObject.SetActive(true);
                RenderSettings.sun = primaryLight;
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void CreateStoryPoints()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne gibi hedef sahneyi ac.", "Tamam");
                return;
            }

            var root = GameObject.Find("_StoryPoints");
            if (root == null)
            {
                root = new GameObject("_StoryPoints");
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            CreateStoryPoint(root.transform, "BizimEv_Dis_Kapi", new Vector3(-4f, 0f, -4f));
            CreateStoryPoint(root.transform, "BizimEv_Ic_Spawn", new Vector3(120f, 0f, 0f));
            CreateStoryPoint(root.transform, "BizimEv_Ic_Cikis", new Vector3(122f, 0f, 0f));
            CreateStoryPoint(root.transform, "AyseEv_Dis_Kapi", new Vector3(4f, 0f, -4f));
            CreateStoryPoint(root.transform, "AyseEv_Ic_Spawn", new Vector3(150f, 0f, 0f));
            CreateStoryPoint(root.transform, "AyseEv_Ic_Cikis", new Vector3(152f, 0f, 0f));
            CreateStoryPoint(root.transform, "Toplanma_Alani", new Vector3(0f, 0f, 8f));
            CreateStoryPoint(root.transform, "AFAD_Pano", new Vector3(1.5f, 0f, 8f));

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void CreateStoryPoint(Transform root, string pointName, Vector3 localPosition)
        {
            Transform existing = root.Find(pointName);
            if (existing != null)
            {
                return;
            }

            var point = new GameObject(pointName);
            point.transform.SetParent(root, false);
            point.transform.localPosition = localPosition;
            point.AddComponent<StoryPointMarker>();
        }
    }
}
