using UnityEngine;

namespace PatininIzinde.World
{
    public sealed class BillboardCustomText : MonoBehaviour
    {
        private const string OverlayName = "Custom_Billboard_Text_Overlay";

        [SerializeField] private string text = "GUVENLI TOPLANMA ALANI";
        [SerializeField] private Vector2 panelSize = new Vector2(3.15f, 1.05f);
        [SerializeField] private float textScale = 0.055f;

        private Transform overlayTransform;
        private Transform panelTransform;

        public static void EnsureOnBillboard(string billboardName, string customText)
        {
            GameObject billboard = GameObject.Find(billboardName) ?? FindByNameContains("billboard", "small");
            if (billboard == null)
            {
                return;
            }

            BillboardCustomText overlay = billboard.GetComponent<BillboardCustomText>();
            if (overlay == null)
            {
                overlay = billboard.AddComponent<BillboardCustomText>();
            }

            overlay.text = customText;
            overlay.Rebuild();
        }

        private void Start()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                Rebuild();
            }
        }

        private void LateUpdate()
        {
            FaceTextToReadableSide();
        }

        private void Rebuild()
        {
            Transform oldOverlay = transform.Find(OverlayName);
            if (oldOverlay != null)
            {
                DestroyObject(oldOverlay.gameObject);
            }

            GameObject overlayObject = new GameObject(OverlayName);
            overlayObject.transform.SetParent(transform, false);
            overlayObject.transform.localPosition = new Vector3(0f, 1.82f, 0f);
            overlayObject.transform.localRotation = Quaternion.identity;
            overlayObject.transform.localScale = Vector3.one;
            overlayTransform = overlayObject.transform;

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(overlayTransform, false);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localScale = new Vector3(panelSize.x, panelSize.y, 0.06f);
            panelTransform = panel.transform;

            Collider panelCollider = panel.GetComponent<Collider>();
            if (panelCollider != null)
            {
                DestroyObject(panelCollider);
            }

            Renderer panelRenderer = panel.GetComponent<Renderer>();
            if (panelRenderer != null)
            {
                panelRenderer.material = CreatePanelMaterial();
            }

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(overlayTransform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            textObject.transform.localScale = Vector3.one * textScale;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 34;
            textMesh.lineSpacing = 0.92f;
            textMesh.color = Color.white;

            FaceTextToReadableSide();
        }

        private void FaceTextToReadableSide()
        {
            if (overlayTransform == null || Camera.main == null)
            {
                return;
            }

            Vector3 toCamera = Camera.main.transform.position - overlayTransform.position;
            Vector3 localToCamera = transform.InverseTransformDirection(toCamera);
            float yRotation = localToCamera.z >= 0f ? 180f : 0f;
            overlayTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            if (panelTransform != null)
            {
                panelTransform.localPosition = new Vector3(0f, 0f, 0.04f);
            }
        }

        private static GameObject FindByNameContains(string first, string second)
        {
            GameObject[] objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject candidate in objects)
            {
                string lowerName = candidate.name.ToLowerInvariant();
                if (lowerName.Contains(first) && lowerName.Contains(second))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Material CreatePanelMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader)
            {
                name = "Billboard_Custom_Text_Panel_Mat",
                color = new Color(0.02f, 0.08f, 0.07f, 0.95f)
            };
        }

        private static void DestroyObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
