using PatininIzinde.Interaction;
using PatininIzinde.QuestSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class HomeReturnSetupTool
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Bizim Ev Simulasyon Ic Giris Noktasi Yap")]
        public static void MakeSelectedObjectHomeSimulationEntryPoint()
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
                EditorUtility.DisplayDialog("Secim yok", "Once bizim evin icinde oyuncunun dogacagi zemin/noktayi sec.", "Tamam");
                return;
            }

            GameObject point = FindOrCreateHomeSimulationEntryPoint(scene);
            Undo.RecordObject(point.transform, "Move Home Simulation Entry Point");

            Bounds bounds = CalculateRendererBounds(selected);
            Vector3 position = bounds.center;
            position.y = selected.transform.position.y + 0.05f;

            point.transform.position = position;
            point.transform.rotation = selected.transform.rotation;

            Selection.activeGameObject = point;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Kapiyi Bizim Eve Simulasyon Girisi Yap")]
        public static void MakeSelectedDoorHomeSimulationEntrance()
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
                EditorUtility.DisplayDialog("Secim yok", "Once sehirdeki bizim ev dis kapisini sec.", "Tamam");
                return;
            }

            selectedDoor.name = "BizimEv_Giris_Kapisi";
            ConfigureHomeSimulationDoor(selectedDoor, scene);

            Selection.activeGameObject = selectedDoor;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Yere Bizim Ev Giris Tetikleyicisi Koy")]
        public static void PlaceHomeSimulationEntranceTriggerAtSelection()
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
                EditorUtility.DisplayDialog("Secim yok", "Once bizim ev kapisinin onundeki kapi/zemin/porch objesini sec.", "Tamam");
                return;
            }

            GameObject trigger = GameObject.Find("BizimEv_Giris_Tetikleyici");
            if (trigger == null)
            {
                trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trigger.name = "BizimEv_Giris_Tetikleyici";
                SceneManager.MoveGameObjectToScene(trigger, scene);
                Undo.RegisterCreatedObjectUndo(trigger, "Create Home Entrance Trigger");
            }

            Bounds bounds = CalculateRendererBounds(selected);
            Vector3 position = bounds.center;
            position.y = bounds.min.y + 1.1f;

            trigger.transform.position = position;
            trigger.transform.rotation = selected.transform.rotation;
            trigger.transform.localScale = new Vector3(2.2f, 2.4f, 2.2f);

            Renderer renderer = trigger.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            ConfigureHomeSimulationDoor(trigger, scene);

            Selection.activeGameObject = trigger;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Giris Tetikleyicisini Biraz Buyut")]
        public static void EnlargeHomeSimulationEntranceTrigger()
        {
            GameObject trigger = GameObject.Find("BizimEv_Giris_Tetikleyici") ?? GameObject.Find("BizimEv_Giris_Kapisi");
            if (trigger == null)
            {
                EditorUtility.DisplayDialog("Tetikleyici yok", "Once bizim ev giris tetikleyicisini kur.", "Tamam");
                return;
            }

            Undo.RecordObject(trigger.transform, "Enlarge Home Entrance Trigger");
            trigger.transform.localScale += new Vector3(0.8f, 0.2f, 0.8f);
            Selection.activeGameObject = trigger;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static void ConfigureHomeSimulationDoor(GameObject doorObject, Scene scene)
        {
            BoxCollider collider = doorObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = doorObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;

            TeleportDoor teleportDoor = doorObject.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = doorObject.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile eve gir";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindHomeSimulationEntryTarget(scene).transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = Object.FindFirstObjectByType<QuestManager>();
            serializedDoor.FindProperty("requiredStepId").stringValue = "go_back_home";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = true;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindHomeSimulationEntryTarget(Scene scene)
        {
            GameObject interior = GameObject.Find("BizimEv_IcMekan");
            GameObject interiorDoor = interior != null
                ? FindChildByName(interior.transform, "Disari_Cikis_Kapisi")
                : null;

            if (interiorDoor != null)
            {
                return interiorDoor;
            }

            GameObject entryPoint = interior != null
                ? FindChildByName(interior.transform, "BizimEv_Ic_Kapi_Giris_Noktasi")
                : null;

            if (entryPoint != null)
            {
                return entryPoint;
            }

            GameObject start = interior != null
                ? FindChildByName(interior.transform, "BizimEv_Ic_Baslangic")
                : null;

            if (start != null)
            {
                return start;
            }

            return FindOrCreateHomeSimulationEntryPoint(scene);
        }

        private static GameObject CreateHomeInteriorDoorEntryPoint(Scene scene, Transform door, Transform parent)
        {
            GameObject point = new GameObject("BizimEv_Ic_Kapi_Giris_Noktasi");
            SceneManager.MoveGameObjectToScene(point, scene);

            Vector3 entryDirection = -door.forward;
            if (entryDirection.sqrMagnitude < 0.01f)
            {
                entryDirection = Vector3.back;
            }

            point.transform.position = door.position + entryDirection.normalized * 1f;
            point.transform.rotation = door.rotation;

            if (parent != null)
            {
                point.transform.SetParent(parent, true);
            }

            return point;
        }

        private static GameObject FindChildByName(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root.gameObject;
            }

            foreach (Transform child in root)
            {
                GameObject match = FindChildByName(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static GameObject FindOrCreateHomeSimulationEntryPoint(Scene scene)
        {
            GameObject point = GameObject.Find("BizimEv_Simulasyon_Ic_Giris");
            if (point != null)
            {
                return point;
            }

            point = new GameObject("BizimEv_Simulasyon_Ic_Giris");
            SceneManager.MoveGameObjectToScene(point, scene);

            GameObject start = GameObject.Find("BizimEv_Ic_Baslangic");
            if (start != null)
            {
                point.transform.position = start.transform.position;
                point.transform.rotation = start.transform.rotation;
            }
            else
            {
                GameObject interior = GameObject.Find("BizimEv_IcMekan");
                if (interior != null)
                {
                    Bounds bounds = CalculateRendererBounds(interior);
                    point.transform.position = new Vector3(bounds.center.x, bounds.min.y + 0.08f, bounds.center.z);
                    point.transform.rotation = interior.transform.rotation;
                }
                else
                {
                    point.transform.position = new Vector3(120f, 0.08f, 0f);
                }
            }

            return point;
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
