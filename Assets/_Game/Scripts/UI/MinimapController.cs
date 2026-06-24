using UnityEngine;

namespace PatininIzinde.UI
{
    public sealed class MinimapController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform homePoint;
        [SerializeField] private Transform ayseHousePoint;
        [SerializeField] private Transform assemblyAreaPoint;
        [SerializeField] private float mapWorldRadius = 70f;
        [SerializeField] private Vector2 mapSize = new Vector2(240f, 190f);
        [SerializeField] private Vector2 screenMargin = new Vector2(24f, 28f);

        private GUIStyle mapBoxStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallLabelStyle;
        private GUIStyle homeStyle;
        private GUIStyle ayseStyle;
        private GUIStyle assemblyStyle;
        private Texture2D mapTexture;
        private Texture2D headerTexture;
        private Texture2D innerTexture;
        private Texture2D borderTexture;
        private Texture2D routeTexture;
        private Texture2D homeTexture;
        private Texture2D ayseTexture;
        private Texture2D assemblyTexture;
        private Texture2D playerTexture;
        private Texture2D shadowTexture;
        private Camera minimapCamera;
        private RenderTexture minimapRenderTexture;

        private void Awake()
        {
            ResolveMapTargets();
        }

        private void ResolveMapTargets()
        {
            if (player == null)
            {
                GameObject rig = GameObject.Find("PlayerCameraRig");
                if (rig != null)
                {
                    player = rig.transform;
                }
            }

            if (homePoint == null)
            {
                GameObject home = GameObject.Find("BizimEv_Dis_Kapi") ?? FindByNameContains("bizim", "ev");
                if (home != null)
                {
                    homePoint = home.transform;
                }
            }

            if (ayseHousePoint == null)
            {
                GameObject ayse =
                    GameObject.Find("AyseTeyze_Dis_Kapi") ??
                    GameObject.Find("AyseEv_Dis_Donus") ??
                    FindByNameContains("teyze", "ev") ??
                    FindByNameContains("ayse", "ev");
                if (ayse != null)
                {
                    ayseHousePoint = ayse.transform;
                }
            }

            if (assemblyAreaPoint == null)
            {
                GameObject assemblyArea =
                    GameObject.Find("Guvenli_Toplanma_Alani") ??
                    GameObject.Find("Toplanma_Alani") ??
                    GameObject.Find("Pati_Sifre_Notu") ??
                    GameObject.Find("Env_Playground (1)") ??
                    FindByNameContains("toplanma", "alani") ??
                    FindByNameContains("playground", "1");

                if (assemblyArea != null)
                {
                    assemblyAreaPoint = assemblyArea.transform;
                }
            }
        }

        private void OnGUI()
        {
            if (!ShouldShowMinimap())
            {
                return;
            }

            ResolveMapTargets();
            if (player == null || homePoint == null || ayseHousePoint == null || assemblyAreaPoint == null)
            {
                return;
            }

            EnsureStyles();
            EnsureMinimapCamera();
            UpdateMinimapCamera();

            Rect mapRect = new Rect(screenMargin.x, Screen.height - mapSize.y - screenMargin.y, mapSize.x, mapSize.y);
            GUI.DrawTexture(new Rect(mapRect.x + 8f, mapRect.y + 10f, mapRect.width, mapRect.height), shadowTexture);
            GUI.Box(mapRect, GUIContent.none, mapBoxStyle);
            GUI.DrawTexture(new Rect(mapRect.x, mapRect.y, mapRect.width, 30f), headerTexture);
            GUI.Label(new Rect(mapRect.x + 12f, mapRect.y + 4f, mapRect.width - 24f, 22f), "MINI HARITA", titleStyle);

            Rect inner = new Rect(mapRect.x + 12f, mapRect.y + 40f, mapRect.width - 24f, mapRect.height - 54f);
            if (minimapRenderTexture != null)
            {
                GUI.DrawTexture(inner, minimapRenderTexture, ScaleMode.ScaleToFit, false);
            }
            else
            {
                GUI.DrawTexture(inner, innerTexture);
            }

            DrawBorder(inner, 2f, borderTexture);

            Vector2 homeMap = WorldToMap(homePoint.position, inner);
            Vector2 assemblyMap = WorldToMap(assemblyAreaPoint.position, inner);
            Vector2 ayseMap = WorldToMap(ayseHousePoint.position, inner);
            Vector2 playerMap = WorldToMap(player.position, inner);

            DrawMapLabel(homeMap, "Bizim Ev", homeStyle, inner);
            DrawMapLabel(ayseMap, "Ayse Teyze", ayseStyle, inner);
            DrawMapLabel(assemblyMap, "Guvenli Alan", assemblyStyle, inner);
            DrawPlayerMarker(playerMap);

            GUI.Label(new Rect(mapRect.x + 12f, mapRect.yMax - 17f, mapRect.width - 24f, 14f), "Mavi ok: sen", smallLabelStyle);
        }

        private Vector2 WorldToMap(Vector3 worldPosition, Rect inner)
        {
            if (minimapCamera != null)
            {
                Vector3 viewport = minimapCamera.WorldToViewportPoint(worldPosition);
                return new Vector2(
                    inner.x + Mathf.Clamp01(viewport.x) * inner.width,
                    inner.y + (1f - Mathf.Clamp01(viewport.y)) * inner.height);
            }

            return inner.center;
        }

        private void DrawMapLabel(Vector2 center, string label, GUIStyle style, Rect bounds)
        {
            Rect labelRect = new Rect(center.x - 34f, center.y - 21f, 68f, 18f);
            labelRect.x = Mathf.Clamp(labelRect.x, bounds.x + 3f, bounds.xMax - labelRect.width - 3f);
            labelRect.y = Mathf.Clamp(labelRect.y, bounds.y + 3f, bounds.yMax - labelRect.height - 3f);
            GUI.DrawTexture(new Rect(labelRect.x - 2f, labelRect.y + 1f, labelRect.width + 4f, labelRect.height), shadowTexture);
            GUI.DrawTexture(labelRect, mapTexture);
            GUI.Label(labelRect, label, style);
        }

        private void DrawPlayerMarker(Vector2 center)
        {
            float size = 16f;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(player.eulerAngles.y, center);
            GUI.DrawTexture(new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size), playerTexture);
            GUI.matrix = previousMatrix;
        }

        private static void DrawBorder(Rect rect, float thickness, Texture2D texture)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), texture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), texture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), texture);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), texture);
        }

        private void EnsureMinimapCamera()
        {
            if (minimapCamera != null && minimapRenderTexture != null)
            {
                return;
            }

            if (minimapRenderTexture == null)
            {
                minimapRenderTexture = new RenderTexture(512, 384, 16, RenderTextureFormat.ARGB32)
                {
                    name = "MiniMap_RenderTexture"
                };
                minimapRenderTexture.Create();
            }

            if (minimapCamera == null)
            {
                GameObject cameraObject = new GameObject("MiniMap_Camera");
                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                minimapCamera = cameraObject.AddComponent<Camera>();
                minimapCamera.orthographic = true;
                minimapCamera.clearFlags = CameraClearFlags.SolidColor;
                minimapCamera.backgroundColor = new Color(0.45f, 0.68f, 0.34f, 1f);
                minimapCamera.cullingMask = ~0;
                minimapCamera.depth = -20f;
                minimapCamera.nearClipPlane = 0.1f;
                minimapCamera.farClipPlane = 500f;
                minimapCamera.targetTexture = minimapRenderTexture;
                minimapCamera.enabled = true;
            }
        }

        private void UpdateMinimapCamera()
        {
            if (minimapCamera == null)
            {
                return;
            }

            Bounds bounds = new Bounds(assemblyAreaPoint.position, Vector3.zero);
            bounds.Encapsulate(homePoint.position);
            bounds.Encapsulate(ayseHousePoint.position);

            Vector3 center = bounds.center;
            center.y = 160f;
            minimapCamera.transform.SetPositionAndRotation(center, Quaternion.Euler(90f, 0f, 0f));
            float aspect = minimapRenderTexture != null
                ? minimapRenderTexture.width / (float)minimapRenderTexture.height
                : 1.25f;
            float fitSize = Mathf.Max(bounds.extents.z, bounds.extents.x / Mathf.Max(0.1f, aspect));
            minimapCamera.orthographicSize = Mathf.Max(32f, fitSize * 1.35f);
        }

        private void OnDestroy()
        {
            if (minimapRenderTexture != null)
            {
                minimapRenderTexture.Release();
                Destroy(minimapRenderTexture);
            }

            if (minimapCamera != null)
            {
                Destroy(minimapCamera.gameObject);
            }
        }

        private void EnsureStyles()
        {
            if (mapBoxStyle != null)
            {
                return;
            }

            mapTexture = MakeTexture(new Color(0.96f, 0.94f, 0.84f, 0.93f));
            headerTexture = MakeTexture(new Color(0.16f, 0.39f, 0.57f, 0.96f));
            innerTexture = MakeTexture(new Color(0.86f, 0.94f, 0.78f, 0.78f));
            borderTexture = MakeTexture(new Color(0.2f, 0.32f, 0.28f, 0.45f));
            routeTexture = MakeTexture(new Color(1f, 0.83f, 0.32f, 0.78f));
            shadowTexture = MakeTexture(new Color(0f, 0f, 0f, 0.18f));
            homeTexture = MakeCircleTexture(new Color(0.16f, 0.62f, 0.36f, 1f));
            ayseTexture = MakeCircleTexture(new Color(0.94f, 0.36f, 0.22f, 1f));
            assemblyTexture = MakeCircleTexture(new Color(0.38f, 0.4f, 0.9f, 1f));
            playerTexture = MakeTriangleTexture(new Color(0.18f, 0.54f, 0.95f, 1f));

            mapBoxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = mapTexture }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.12f, 0.16f, 0.14f) }
            };

            homeStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = new Color(0.12f, 0.38f, 0.22f) }
            };

            ayseStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = new Color(0.55f, 0.2f, 0.08f) }
            };

            assemblyStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = new Color(0.22f, 0.24f, 0.62f) }
            };

            smallLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.24f, 0.28f, 0.25f) }
            };
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D MakeCircleTexture(Color color)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? color : clear);
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D MakeTriangleTexture(Color color)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float halfWidth = Mathf.Lerp(3f, 13f, y / (float)(size - 1));
                    bool inside = y > 3 && Mathf.Abs(x - size * 0.5f) < halfWidth;
                    texture.SetPixel(x, y, inside ? color : clear);
                }
            }

            texture.Apply();
            return texture;
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

        private static bool ShouldShowMinimap()
        {
            IntroScreenController intro = FindFirstObjectByType<IntroScreenController>();
            return intro == null || IntroScreenController.HasAdventureStarted;
        }
    }
}
