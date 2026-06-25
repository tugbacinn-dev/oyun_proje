using PatininIzinde.Interaction;
using PatininIzinde.QuestSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class AyseHouseTeleportSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Evin Onune Perihan Teyze Giris Kapisi Koy")]
        public static void PlaceAyseExteriorDoorTrigger()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selectedHouse = Selection.activeGameObject;
            if (selectedHouse == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once Perihan Teyze'nin sehirdeki evini veya kapisini sec.", "Tamam");
                return;
            }

            Transform interiorTarget = FindOrCreateAyseInteriorEntry(scene).transform;
            GameObject doorTrigger = GameObject.Find("AyseTeyze_Dis_Kapi");
            if (doorTrigger == null)
            {
                doorTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
                doorTrigger.name = "AyseTeyze_Dis_Kapi";
                Undo.RegisterCreatedObjectUndo(doorTrigger, "Create Ayse Door Trigger");
                SceneManager.MoveGameObjectToScene(doorTrigger, scene);
            }
            else
            {
                Undo.RecordObject(doorTrigger.transform, "Move Ayse Door Trigger");
            }

            Bounds bounds = CalculateRendererBounds(selectedHouse);
            Vector3 triggerPosition = selectedHouse.transform.position;
            if (bounds.size.sqrMagnitude > 0.01f)
            {
                triggerPosition = bounds.center;
                triggerPosition.y = bounds.min.y + 1.15f;
            }

            Vector3 frontDirection = selectedHouse.transform.forward;
            if (frontDirection.sqrMagnitude < 0.01f)
            {
                frontDirection = Vector3.forward;
            }

            float forwardExtent = Mathf.Max(1.25f, Vector3.Dot(bounds.extents, Abs(frontDirection.normalized)));
            triggerPosition += frontDirection.normalized * (forwardExtent + 0.35f);

            doorTrigger.transform.position = triggerPosition;
            doorTrigger.transform.rotation = Quaternion.LookRotation(-frontDirection.normalized, Vector3.up);
            doorTrigger.transform.localScale = new Vector3(1.4f, 2.2f, 1f);

            Renderer renderer = doorTrigger.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateTriggerMaterial();
                renderer.enabled = false;
            }

            BoxCollider collider = doorTrigger.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = doorTrigger.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = Vector3.one;
            collider.center = Vector3.zero;

            TeleportDoor teleportDoor = doorTrigger.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = doorTrigger.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile Perihan Teyze'nin evine gir";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = interiorTarget;
            serializedDoor.FindProperty("questManager").objectReferenceValue = Object.FindFirstObjectByType<QuestManager>();
            serializedDoor.FindProperty("requiredStepId").stringValue = "go_to_ayse";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = true;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = doorTrigger;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Kapiyi Perihan Teyze Giris Kapisi Yap")]
        public static void MakeSelectedDoorAyseEntrance()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selectedDoor = Selection.activeGameObject;
            if (selectedDoor == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once Perihan Teyze evinin onundeki kapi/trigger objesini sec.", "Tamam");
                return;
            }

            ConfigureAyseDoor(selectedDoor, scene);
            Selection.activeGameObject = selectedDoor;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Perihan Evinden Cikisi Perihan Teyzenin Yanina Al")]
        public static void MoveAyseReturnPointNearAyse()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject point = FindOrCreateAyseExteriorReturnPoint(scene);
            MoveReturnPointNearAyse(point);

            Selection.activeGameObject = point;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Yanlis Ayse Kapi Isinlamalarini Onar")]
        public static void RepairWrongAyseDoorTeleports()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            int repairedCount = 0;
            TeleportDoor[] doors = Object.FindObjectsByType<TeleportDoor>(FindObjectsSortMode.None);
            foreach (TeleportDoor door in doors)
            {
                if (door == null || door.gameObject.scene != scene)
                {
                    continue;
                }

                if (door.name != "Disari_Cikis_Kapisi" || IsInsideHomeInterior(door.transform))
                {
                    continue;
                }

                ConfigureAyseDoor(door.gameObject, scene);
                repairedCount++;
            }

            EditorUtility.DisplayDialog("Onarim tamam", $"{repairedCount} yanlis Ayse kapi isinlamasi onarildi.", "Tamam");
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Perihan Teyze Ic Kapi Noktasi Yap")]
        public static void MakeSelectedObjectAyseInteriorEntry()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selectedPoint = Selection.activeGameObject;
            if (selectedPoint == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once teyze evi ic mekandaki dis kapi/zemin noktasini sec.", "Tamam");
                return;
            }

            GameObject entryPoint = FindOrCreateAyseInteriorEntry(scene);
            Undo.RecordObject(entryPoint.transform, "Move Ayse Interior Entry");

            Bounds bounds = CalculateRendererBounds(selectedPoint);
            Vector3 position = bounds.center;
            position.y = selectedPoint.transform.position.y + 0.05f;

            entryPoint.transform.position = position;
            entryPoint.transform.rotation = selectedPoint.transform.rotation;

            Selection.activeGameObject = entryPoint;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Kapiyi Perihan Teyze Evinden Cikis Kapisi Yap")]
        public static void MakeSelectedDoorAyseInteriorExit()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selectedDoor = Selection.activeGameObject;
            if (selectedDoor == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once Perihan Teyze evindeki cikis kapisini sec.", "Tamam");
                return;
            }

            selectedDoor.name = "AyseEv_Ic_Cikis_Kapisi";

            BoxCollider collider = selectedDoor.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = selectedDoor.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = new Vector3(1.4f, 2.2f, 1f);
            collider.center = new Vector3(0f, 1f, 0f);

            TeleportDoor teleportDoor = selectedDoor.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = selectedDoor.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile disari cik";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindOrCreateAyseExteriorReturnPoint(scene).transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = Object.FindFirstObjectByType<QuestManager>();
            serializedDoor.FindProperty("requiredStepId").stringValue = "leave_ayse_house";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = true;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = selectedDoor;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Perihan Teyze Ic Girisini Biraz Yukari Al")]
        public static void RaiseAyseInteriorEntry()
        {
            MoveAyseInteriorEntryHeight(0.35f);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Perihan Teyze Ic Girisini Biraz Asagi Al")]
        public static void LowerAyseInteriorEntry()
        {
            MoveAyseInteriorEntryHeight(-0.35f);
        }

        private static GameObject FindOrCreateAyseInteriorEntry(Scene scene)
        {
            GameObject entryPoint = GameObject.Find("AyseEv_Ic_Giris");
            if (entryPoint != null)
            {
                return entryPoint;
            }

            entryPoint = new GameObject("AyseEv_Ic_Giris");
            SceneManager.MoveGameObjectToScene(entryPoint, scene);

            GameObject interior = GameObject.Find("Teyze_Ev_IcMekan");
            if (interior == null)
            {
                interior = GameObject.Find("TeyzeEv_IcMekan");
            }

            if (interior != null)
            {
                Bounds bounds = CalculateRendererBounds(interior);
                entryPoint.transform.position = new Vector3(bounds.center.x, bounds.min.y + 0.08f, bounds.min.z + 1f);
                entryPoint.transform.rotation = interior.transform.rotation;
                entryPoint.transform.SetParent(interior.transform, true);
            }
            else
            {
                entryPoint.transform.position = new Vector3(150f, 0.08f, 0f);
            }

            return entryPoint;
        }

        private static GameObject FindOrCreateAyseExteriorReturnPoint(Scene scene)
        {
            GameObject point = GameObject.Find("AyseEv_Dis_Donus");
            if (point != null)
            {
                return point;
            }

            point = new GameObject("AyseEv_Dis_Donus");
            SceneManager.MoveGameObjectToScene(point, scene);

            if (MoveReturnPointNearAyse(point))
            {
                return point;
            }

            GameObject exteriorDoor = GameObject.Find("AyseTeyze_Dis_Kapi");
            if (exteriorDoor != null)
            {
                Vector3 outsideDirection = -exteriorDoor.transform.forward;
                if (outsideDirection.sqrMagnitude < 0.01f)
                {
                    outsideDirection = Vector3.forward;
                }

                point.transform.position = exteriorDoor.transform.position + outsideDirection.normalized * 2.2f;
                point.transform.rotation = Quaternion.LookRotation(exteriorDoor.transform.forward, Vector3.up);
            }
            else
            {
                point.transform.position = new Vector3(95.9f, 1.1f, 55.2f);
            }

            return point;
        }

        private static bool MoveReturnPointNearAyse(GameObject point)
        {
            GameObject ayse = GameObject.Find("Ayse_Teyze") ?? GameObject.Find("AyseTeyze") ?? GameObject.Find("elder_Female_A");
            if (ayse == null)
            {
                return false;
            }

            Vector3 forward = ayse.transform.forward;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.forward;
            }

            Vector3 position = ayse.transform.position + forward.normalized * 1.8f;
            position.y = ayse.transform.position.y + 0.08f;
            point.transform.position = position;
            point.transform.rotation = Quaternion.LookRotation(-forward.normalized, Vector3.up);
            return true;
        }

        private static void MoveAyseInteriorEntryHeight(float amount)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject entryPoint = FindOrCreateAyseInteriorEntry(scene);
            Undo.RecordObject(entryPoint.transform, "Move Ayse Interior Entry Height");
            entryPoint.transform.position += Vector3.up * amount;

            Selection.activeGameObject = entryPoint;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ConfigureAyseDoor(GameObject doorObject, Scene scene)
        {
            doorObject.name = "AyseTeyze_Dis_Kapi";

            BoxCollider collider = doorObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = doorObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = new Vector3(1.4f, 2.2f, 1f);
            collider.center = new Vector3(0f, 1f, 0f);

            TeleportDoor teleportDoor = doorObject.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = doorObject.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile Perihan Teyze'nin evine gir";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindOrCreateAyseInteriorEntry(scene).transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = Object.FindFirstObjectByType<QuestManager>();
            serializedDoor.FindProperty("requiredStepId").stringValue = "go_to_ayse";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = true;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool IsInsideHomeInterior(Transform target)
        {
            Transform current = target;
            while (current != null)
            {
                if (current.name == "BizimEv_IcMekan")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
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

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private static Material CreateTriggerMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/Ayse_Kapi_Trigger_Mat.mat");
            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = "Ayse_Kapi_Trigger_Mat",
                color = new Color(0.95f, 0.72f, 0.22f, 0.35f)
            };

            if (!AssetDatabase.IsValidFolder("Assets/_Game/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Materials");
            }

            AssetDatabase.CreateAsset(material, "Assets/_Game/Materials/Ayse_Kapi_Trigger_Mat.mat");
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
