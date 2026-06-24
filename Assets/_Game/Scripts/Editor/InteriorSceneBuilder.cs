using PatininIzinde.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PatininIzinde.EditorTools
{
    public static class InteriorSceneBuilder
    {
        [MenuItem("Pati'nin Izinde/Kurulum/Secili Objeyi Bizim Ev Cikis Kapisi Yap")]
        public static void MakeSelectedObjectHomeExitDoor()
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
                EditorUtility.DisplayDialog("Secim yok", "Once cikis kapisi olacak objeyi sec.", "Tamam");
                return;
            }

            selected.name = "Disari_Cikis_Kapisi";

            BoxCollider collider = selected.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = selected.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = new Vector3(1.25f, 2.15f, 0.7f);
            collider.center = new Vector3(0f, 1.1f, 0f);

            TeleportDoor teleportDoor = selected.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = selected.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile disari cik";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindOrCreateExternalHomePoint(scene).transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = null;
            serializedDoor.FindProperty("requiredStepId").stringValue = "";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = false;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = selected;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Ic Isiklarini Duzelt")]
        public static void FixHomeInteriorLighting()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject interior = GameObject.Find("BizimEv_IcMekan");
            if (interior == null)
            {
                EditorUtility.DisplayDialog("Ic mekan bulunamadi", "BizimEv_IcMekan objesi sahnede bulunamadi.", "Tamam");
                return;
            }

            Light[] childLights = interior.GetComponentsInChildren<Light>(true);
            foreach (Light childLight in childLights)
            {
                Undo.RecordObject(childLight, "Fix Home Interior Lighting");

                if (childLight.type == LightType.Directional)
                {
                    childLight.gameObject.SetActive(false);
                    continue;
                }

                childLight.enabled = true;
                childLight.color = new Color(1f, 0.9f, 0.76f);
                childLight.intensity = childLight.type == LightType.Point ? 0.55f : 0.7f;
                childLight.range = Mathf.Clamp(childLight.range, 3f, 7f);
                childLight.shadows = LightShadows.None;
            }

            Transform lightingRoot = interior.transform.Find("_InteriorLighting");
            if (lightingRoot == null)
            {
                GameObject lightingObject = new GameObject("_InteriorLighting");
                Undo.RegisterCreatedObjectUndo(lightingObject, "Create Interior Lighting");
                lightingObject.transform.SetParent(interior.transform, false);
                lightingRoot = lightingObject.transform;
            }

            Bounds bounds = CalculateRendererBounds(interior);
            CreateOrUpdateInteriorPointLight(
                lightingRoot,
                "Soft_Warm_Room_Light",
                new Vector3(bounds.center.x, bounds.max.y - 0.7f, bounds.center.z),
                1.15f,
                8f,
                new Color(1f, 0.88f, 0.68f));

            CreateOrUpdateInteriorPointLight(
                lightingRoot,
                "Soft_Fill_Room_Light",
                new Vector3(bounds.center.x + 2.4f, bounds.min.y + 1.7f, bounds.center.z - 1.6f),
                0.45f,
                6f,
                new Color(0.72f, 0.84f, 1f));

            Light sceneSun = Object.FindFirstObjectByType<Light>();
            Light[] sceneLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in sceneLights)
            {
                if (light.type != LightType.Directional || light.gameObject.scene != scene || !light.gameObject.activeInHierarchy)
                {
                    continue;
                }

                sceneSun = light;
                break;
            }

            if (sceneSun != null && sceneSun.type == LightType.Directional)
            {
                Undo.RecordObject(sceneSun, "Soften Scene Sun");
                sceneSun.intensity = 0.75f;
                sceneSun.color = new Color(1f, 0.93f, 0.82f);
                RenderSettings.sun = sceneSun;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.62f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.52f, 0.46f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.29f, 0.25f);
            RenderSettings.ambientIntensity = 0.75f;

            Selection.activeGameObject = interior;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Secili Room1'i Bizim Ev Ic Mekan Yap")]
        public static void ConvertSelectedRoomToHomeInterior()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject selectedRoom = Selection.activeGameObject;
            if (selectedRoom == null)
            {
                EditorUtility.DisplayDialog("Secim yok", "Once sahnedeki room1 objesini sec.", "Tamam");
                return;
            }

            GameObject oldGeneratedInterior = GameObject.Find("BizimEv_IcMekan");
            if (oldGeneratedInterior != null && oldGeneratedInterior != selectedRoom)
            {
                Undo.DestroyObjectImmediate(oldGeneratedInterior);
            }

            selectedRoom.name = "BizimEv_IcMekan";
            Bounds roomBounds = CalculateRendererBounds(selectedRoom);

            Transform startPoint = FindOrCreateChildMarker(
                selectedRoom.transform,
                "BizimEv_Ic_Baslangic",
                new Vector3(roomBounds.center.x, roomBounds.min.y + 1.05f, roomBounds.center.z));

            Transform exitPoint = FindOrCreateChildMarker(
                selectedRoom.transform,
                "BizimEv_Ic_Cikis",
                new Vector3(roomBounds.center.x, roomBounds.min.y + 1.05f, roomBounds.min.z + 0.8f));

            GameObject exitTrigger = GameObject.Find("Disari_Cikis_Kapisi");
            if (exitTrigger == null || exitTrigger.transform.root.name != selectedRoom.name)
            {
                exitTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
                exitTrigger.name = "Disari_Cikis_Kapisi";
                Undo.RegisterCreatedObjectUndo(exitTrigger, "Create Home Exit Trigger");
                exitTrigger.transform.SetParent(selectedRoom.transform, true);
            }

            exitTrigger.transform.position = exitPoint.position;
            exitTrigger.transform.localScale = new Vector3(1.8f, 2.2f, 1.2f);

            Renderer triggerRenderer = exitTrigger.GetComponent<Renderer>();
            if (triggerRenderer != null)
            {
                triggerRenderer.sharedMaterial = CreateMaterial("Cikis_Trigger_Mat", new Color(0.15f, 0.55f, 1f, 0.55f));
            }

            BoxCollider collider = exitTrigger.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = exitTrigger.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = Vector3.one;

            TeleportDoor teleportDoor = exitTrigger.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = exitTrigger.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile disari cik";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindOrCreateExternalHomePoint(scene).transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = null;
            serializedDoor.FindProperty("requiredStepId").stringValue = "";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = false;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            MovePlayerToInteriorStart(startPoint);

            Selection.activeGameObject = selectedRoom;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Cikis Kapisini Onar")]
        public static void RepairHomeExitDoor()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            GameObject door = GameObject.Find("Disari_Cikis_Kapisi");
            GameObject target = FindOrCreateExternalHomePoint(scene);

            if (door == null)
            {
                EditorUtility.DisplayDialog("Kapi bulunamadi", "Disari_Cikis_Kapisi objesi sahnede bulunamadi.", "Tamam");
                return;
            }

            BoxCollider collider = door.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = door.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = new Vector3(2.5f, 2.2f, 2.2f);
            collider.center = new Vector3(0f, 1.1f, 0f);

            TeleportDoor teleportDoor = door.GetComponent<TeleportDoor>();
            if (teleportDoor == null)
            {
                teleportDoor = door.AddComponent<TeleportDoor>();
            }

            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile disari cik";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = target.transform;
            serializedDoor.FindProperty("questManager").objectReferenceValue = null;
            serializedDoor.FindProperty("requiredStepId").stringValue = "";
            serializedDoor.FindProperty("completeStepAfterTeleport").boolValue = false;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = door;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Dis Kapi Noktasini Evin Onune Al")]
        public static void PlaceHomeExitPointAtExteriorDoor()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject home = FindHomeExteriorObject();
            if (home == null)
            {
                EditorUtility.DisplayDialog(
                    "Bizim ev bulunamadi",
                    "Sahnede adinda Bizim ve Ev gecen dis ev objesi bulunamadi. Ornek ad: Bizim_Ev_Disi",
                    "Tamam");
                return;
            }

            GameObject point = FindOrCreateExternalHomePoint(scene);
            Bounds bounds = CalculateRendererBounds(home);
            Vector3 forward = home.transform.forward;
            Vector3 targetPosition = bounds.center - forward * (bounds.extents.z + 2.5f);
            targetPosition.y = bounds.min.y + 1.1f;

            point.transform.position = targetPosition;
            point.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            Selection.activeGameObject = point;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Dis Kapi Noktasini Secili Objeye Al")]
        public static void PlaceHomeExitPointAtSelection()
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
                EditorUtility.DisplayDialog("Secim yok", "Once kapinin onundeki zemin/porch objesini sec.", "Tamam");
                return;
            }

            GameObject point = FindOrCreateExternalHomePoint(scene);
            Bounds bounds = CalculateRendererBounds(selected);
            Vector3 pointPosition = bounds.center;
            pointPosition.y = bounds.max.y + 1.25f;

            point.transform.position = pointPosition;
            point.transform.rotation = selected.transform.rotation;

            Selection.activeGameObject = point;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Sehir Cikis Kamera Yuksekligini Artir")]
        public static void RaiseCityExitCameraHeight()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            GameObject point = FindOrCreateExternalHomePoint(scene);
            Vector3 position = point.transform.position;
            position.y += 1.25f;
            point.transform.position = position;

            Selection.activeGameObject = point;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Pati'nin Izinde/Kurulum/Bizim Ev Ic Mekanini Kur")]
        public static void BuildHomeInterior()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Sahne bulunamadi", "Once anasahne acik olmali.", "Tamam");
                return;
            }

            if (GameObject.Find("BizimEv_IcMekan") != null)
            {
                EditorUtility.DisplayDialog("Zaten var", "BizimEv_IcMekan sahnede zaten bulunuyor.", "Tamam");
                return;
            }

            var root = new GameObject("BizimEv_IcMekan");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = new Vector3(120f, 0f, 0f);

            CreateCube(root.transform, "Zemin", new Vector3(0f, -0.05f, 0f), new Vector3(12f, 0.1f, 9f), new Color(0.72f, 0.64f, 0.52f));
            CreateCube(root.transform, "Arka_Duvar", new Vector3(0f, 1.5f, 4.5f), new Vector3(12f, 3f, 0.15f), new Color(0.95f, 0.9f, 0.78f));
            CreateCube(root.transform, "Sol_Duvar", new Vector3(-6f, 1.5f, 0f), new Vector3(0.15f, 3f, 9f), new Color(0.95f, 0.9f, 0.78f));
            CreateCube(root.transform, "Sag_Duvar", new Vector3(6f, 1.5f, 0f), new Vector3(0.15f, 3f, 9f), new Color(0.95f, 0.9f, 0.78f));
            CreateCube(root.transform, "On_Duvar_Sol", new Vector3(-3.8f, 1.5f, -4.5f), new Vector3(4.4f, 3f, 0.15f), new Color(0.95f, 0.9f, 0.78f));
            CreateCube(root.transform, "On_Duvar_Sag", new Vector3(3.8f, 1.5f, -4.5f), new Vector3(4.4f, 3f, 0.15f), new Color(0.95f, 0.9f, 0.78f));

            CreateCube(root.transform, "Masa_Guvenli_Nokta", new Vector3(-3f, 0.45f, 1.5f), new Vector3(2f, 0.9f, 1.2f), new Color(0.45f, 0.28f, 0.18f));
            CreateCube(root.transform, "Afet_Cantasi", new Vector3(3.7f, 0.35f, 2.6f), new Vector3(0.9f, 0.7f, 0.5f), new Color(0.9f, 0.12f, 0.12f));
            CreateNpc(root.transform, "Anne", new Vector3(0f, 0.9f, 1.5f), new Color(1f, 0.62f, 0.72f));
            CreateNpc(root.transform, "Baba", new Vector3(2f, 0.9f, 1.2f), new Color(0.35f, 0.55f, 1f));

            var insideSpawn = CreateMarker(root.transform, "BizimEv_Ic_Baslangic", new Vector3(0f, 0.1f, -2.8f));
            var exitPoint = CreateMarker(root.transform, "BizimEv_Ic_Cikis", new Vector3(0f, 0.1f, -4.1f));
            var door = CreateCube(root.transform, "Disari_Cikis_Kapisi", new Vector3(0f, 1f, -4.55f), new Vector3(1.4f, 2f, 0.1f), new Color(0.55f, 0.32f, 0.18f));
            var doorCollider = door.GetComponent<BoxCollider>();
            doorCollider.isTrigger = true;
            doorCollider.size = new Vector3(2.5f, 2.2f, 2.2f);
            var teleportDoor = door.AddComponent<TeleportDoor>();
            SerializedObject serializedDoor = new SerializedObject(teleportDoor);
            serializedDoor.FindProperty("interactionText").stringValue = "E ile disari cik";
            serializedDoor.FindProperty("targetPoint").objectReferenceValue = FindOrCreateExternalHomePoint(scene).transform;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            MovePlayerToInteriorStart(insideSpawn.transform);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", color);
            return cube;
        }

        private static void CreateNpc(Transform parent, string name, Vector3 localPosition, Color color)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = name;
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = localPosition;
            npc.transform.localScale = new Vector3(0.45f, 0.9f, 0.45f);
            npc.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "_Mat", color);
        }

        private static GameObject CreateMarker(Transform parent, string name, Vector3 localPosition)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
            return marker;
        }

        private static Transform FindOrCreateChildMarker(Transform parent, string markerName, Vector3 worldPosition)
        {
            Transform existing = parent.Find(markerName);
            if (existing != null)
            {
                existing.position = worldPosition;
                return existing;
            }

            GameObject marker = new GameObject(markerName);
            Undo.RegisterCreatedObjectUndo(marker, "Create Interior Marker");
            marker.transform.SetParent(parent, true);
            marker.transform.position = worldPosition;
            return marker.transform;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        private static void CreateOrUpdateInteriorPointLight(Transform parent, string lightName, Vector3 worldPosition, float intensity, float range, Color color)
        {
            Transform existing = parent.Find(lightName);
            GameObject lightObject;
            if (existing == null)
            {
                lightObject = new GameObject(lightName);
                Undo.RegisterCreatedObjectUndo(lightObject, "Create Interior Light");
                lightObject.transform.SetParent(parent, true);
            }
            else
            {
                lightObject = existing.gameObject;
            }

            lightObject.transform.position = worldPosition;

            Light light = lightObject.GetComponent<Light>();
            if (light == null)
            {
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static GameObject FindOrCreateExternalHomePoint(Scene scene)
        {
            GameObject point = GameObject.Find("BizimEv_Dis_Kapi");
            if (point != null)
            {
                return point;
            }

            point = new GameObject("BizimEv_Dis_Kapi");
            SceneManager.MoveGameObjectToScene(point, scene);
            point.transform.position = new Vector3(0f, 0.1f, -8f);
            return point;
        }

        private static GameObject FindHomeExteriorObject()
        {
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject candidate in objects)
            {
                string lowerName = candidate.name.ToLowerInvariant();
                if (lowerName.Contains("bizim") && lowerName.Contains("ev") && !lowerName.Contains("ic"))
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

        private static void MovePlayerToInteriorStart(Transform startPoint)
        {
            GameObject rig = GameObject.Find("PlayerCameraRig");
            if (rig == null)
            {
                return;
            }

            rig.transform.position = startPoint.position;
            rig.transform.rotation = startPoint.rotation;
        }
    }
}
