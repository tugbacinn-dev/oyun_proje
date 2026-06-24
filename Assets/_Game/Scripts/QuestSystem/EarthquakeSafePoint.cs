using PatininIzinde.Interaction;
using PatininIzinde.UI;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class EarthquakeSafePoint : MonoBehaviour, IInteractable
    {
        private const string StepId = "earthquake_moment";
        private static EarthquakeSafePoint activePoint;

        [SerializeField] private QuestManager questManager;
        [SerializeField] private GameUIController uiController;
        [SerializeField] private string interactionText = "E basili tut: Cok-Kapan-Tutun";
        [SerializeField] private float holdDuration = 4.5f;
        [SerializeField] private float crouchCameraOffset = 1.05f;
        [SerializeField] private float interactionDistance = 2.2f;

        private float holdProgress;
        private float currentCrouch;
        private Transform labelTransform;
        private Transform triangleTransform;

        public string InteractionText => uiController != null && uiController.IsEarthquakeSimulationActive
            ? interactionText
            : "Hayat ucgeninde bekle";
        public bool CanInteract => questManager != null && questManager.IsCurrentStep(StepId);

        public static void EnsureExists(QuestManager questManager, GameUIController uiController)
        {
            if (activePoint != null)
            {
                activePoint.Configure(questManager, uiController);
                activePoint.gameObject.SetActive(true);
                activePoint.transform.position = LifeTrianglePosition();
                activePoint.RebuildMarkerVisual();
                return;
            }

            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
            point.name = "HayatUcgeni_Koltuk_Yani";
            point.transform.position = LifeTrianglePosition();
            point.transform.localScale = Vector3.one;

            BoxCollider collider = point.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.size = new Vector3(1.8f, 2f, 1.8f);
                collider.center = new Vector3(0f, 0.9f, 0f);
            }

            Renderer renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            activePoint = point.AddComponent<EarthquakeSafePoint>();
            activePoint.Configure(questManager, uiController);
            activePoint.RebuildMarkerVisual();
        }

        public void Interact(PlayerInteractor interactor)
        {
            // Hold behavior is handled in Update so releasing E can restore camera height.
        }

        private void Configure(QuestManager manager, GameUIController ui)
        {
            questManager = manager != null ? manager : FindFirstObjectByType<QuestManager>();
            uiController = ui != null ? ui : FindFirstObjectByType<GameUIController>();
            holdProgress = 0f;
            currentCrouch = 0f;
        }

        private void Update()
        {
            if (triangleTransform != null)
            {
                triangleTransform.Rotate(Vector3.up, 95f * Time.deltaTime, Space.Self);
            }

            if (labelTransform == null || Camera.main == null)
            {
                return;
            }

            Vector3 toCamera = labelTransform.position - Camera.main.transform.position;
            if (toCamera.sqrMagnitude > 0.01f)
            {
                labelTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }

            UpdateHoldInteraction();
        }

        private void UpdateHoldInteraction()
        {
            if (!CanInteract || uiController == null || !uiController.IsEarthquakeSimulationActive)
            {
                MoveCrouchToward(0f);
                return;
            }

            Transform player = FindPlayer();
            bool playerIsNear = player != null && Vector3.Distance(player.position, transform.position) <= interactionDistance;
            bool isHolding = playerIsNear && Input.GetKey(KeyCode.E);

            if (isHolding)
            {
                holdProgress = Mathf.MoveTowards(holdProgress, 1f, Time.deltaTime / holdDuration);
                MoveCrouchToward(crouchCameraOffset);
                uiController.SetEarthquakeProgress(holdProgress);

                if (holdProgress >= 1f)
                {
                    uiController.CompleteEarthquakeFeedback();
                    questManager.CompleteStep(StepId);
                    gameObject.SetActive(false);
                }

                return;
            }

            MoveCrouchToward(0f);
            uiController.SetEarthquakeProgress(holdProgress);
        }

        private void MoveCrouchToward(float target)
        {
            currentCrouch = Mathf.MoveTowards(currentCrouch, target, Time.deltaTime * 2.8f);
            uiController?.SetEarthquakeCrouch(currentCrouch);
        }

        private static Transform FindPlayer()
        {
            GameObject player = GameObject.Find("PlayerCameraRig");
            return player != null ? player.transform : null;
        }

        private void RebuildMarkerVisual()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            EnsureTriangleMarker();
            EnsureTeachingLabel();
        }

        private void EnsureTeachingLabel()
        {
            GameObject labelObject = new GameObject("HayatUcgeni_Label");
            labelObject.transform.SetParent(transform, false);
            ConfigureLabel(labelObject);

            labelTransform = labelObject.transform;
        }

        private static void ConfigureLabel(GameObject labelObject)
        {
            labelObject.transform.localPosition = new Vector3(0f, 0.82f, 0f);
            labelObject.transform.localScale = Vector3.one * 0.045f;

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(labelObject.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            panel.transform.localScale = new Vector3(6.2f, 1.35f, 0.08f);

            Collider panelCollider = panel.GetComponent<Collider>();
            if (panelCollider != null)
            {
                Destroy(panelCollider);
            }

            Renderer panelRenderer = panel.GetComponent<Renderer>();
            if (panelRenderer != null)
            {
                panelRenderer.material = CreatePanelMaterial();
            }

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();

            textMesh.text = "Hayat ucgeni burada";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 32;
            textMesh.color = Color.white;
        }

        private void EnsureTriangleMarker()
        {
            GameObject triangleObject = new GameObject("HayatUcgeni_Ucgen");
            triangleObject.transform.SetParent(transform, false);
            triangleObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            triangleObject.transform.localRotation = Quaternion.identity;
            triangleObject.transform.localScale = Vector3.one * 1.35f;

            MeshFilter meshFilter = triangleObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateTrianglePyramidMesh();

            MeshRenderer meshRenderer = triangleObject.AddComponent<MeshRenderer>();
            meshRenderer.material = CreateMarkerMaterial();
            triangleTransform = triangleObject.transform;
        }

        private static Mesh CreateTrianglePyramidMesh()
        {
            Mesh mesh = new Mesh
            {
                name = "HayatUcgeni_3D_Ucgen_Mesh",
                vertices = new[]
                {
                    new Vector3(0f, 0.55f, 0f),
                    new Vector3(0f, 0f, 0.62f),
                    new Vector3(-0.58f, 0f, -0.42f),
                    new Vector3(0.58f, 0f, -0.42f),
                    new Vector3(0f, -0.02f, 0f)
                },
                triangles = new[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 1,
                    4, 2, 1,
                    4, 3, 2,
                    4, 1, 3
                }
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Vector3 LifeTrianglePosition()
        {
            Transform sceneMarker = FindSceneMarker();
            if (sceneMarker != null)
            {
                return sceneMarker.position;
            }

            return new Vector3(404.3592f, -0.3916f, 9.625f);
        }

        private static Transform FindSceneMarker()
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (Transform candidate in transforms)
            {
                string candidateName = candidate.name.ToLowerInvariant();
                if (candidateName.Contains("kapan") && candidateName.Contains("tutun"))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Material CreateMarkerMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = "CokKapanTutun_Guvenli_Nokta_Mat",
                color = new Color(0.2f, 1f, 0.45f, 0.72f)
            };
            return material;
        }

        private static Material CreatePanelMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = "HayatUcgeni_Label_Panel_Mat",
                color = new Color(0.02f, 0.38f, 0.12f, 0.92f)
            };
            return material;
        }
    }
}
