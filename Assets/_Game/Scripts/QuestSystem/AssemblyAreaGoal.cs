using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class AssemblyAreaGoal : MonoBehaviour
    {
        private const string GoalName = "Guvenli_Toplanma_Alani";
        private const string StepId = "go_to_assembly_area";

        [SerializeField] private QuestManager questManager;
        private Transform labelTransform;

        private void Awake()
        {
            Transform existingLabel = transform.Find("GuvenliToplanma_Label");
            if (existingLabel != null)
            {
                labelTransform = existingLabel;
            }
        }

        private void Update()
        {
            if (labelTransform == null || Camera.main == null)
            {
                return;
            }

            Vector3 toCamera = labelTransform.position - Camera.main.transform.position;
            if (toCamera.sqrMagnitude > 0.01f)
            {
                labelTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
        }

        public static void EnsureExists(QuestManager manager)
        {
            GameObject goalObject = GameObject.Find(GoalName);
            if (goalObject == null)
            {
                goalObject = CreateGoalObject();
            }

            AssemblyAreaGoal goal = goalObject.GetComponent<AssemblyAreaGoal>();
            if (goal == null)
            {
                goal = goalObject.AddComponent<AssemblyAreaGoal>();
            }

            goal.questManager = manager != null ? manager : FindFirstObjectByType<QuestManager>();
            goalObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (questManager == null)
            {
                questManager = FindFirstObjectByType<QuestManager>();
            }

            if (questManager == null || !questManager.IsCurrentStep(StepId))
            {
                return;
            }

            if (other.GetComponentInParent<PlayerInteractor>() == null)
            {
                return;
            }

            questManager.CompleteStep(StepId);
            gameObject.SetActive(false);
        }

        private static GameObject CreateGoalObject()
        {
            GameObject goalObject = new GameObject(GoalName);
            goalObject.name = GoalName;
            goalObject.transform.position = ResolveGoalPosition();

            SphereCollider trigger = goalObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.4f, 0f);
            trigger.radius = 4.5f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "ToplanmaAlani_Gorsel";
            visual.transform.SetParent(goalObject.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(4.5f, 0.06f, 4.5f);

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateGoalMaterial();
            }

            GameObject label = new GameObject("GuvenliToplanma_Label");
            label.transform.SetParent(goalObject.transform, false);
            label.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            label.transform.localScale = Vector3.one * 0.08f;

            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = "Guvenli Toplanma Alani";
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 32;
            textMesh.color = Color.white;

            return goalObject;
        }

        private static Vector3 ResolveGoalPosition()
        {
            GameObject can = FinalQuestCoordinator.FindCanForAssemblyArea();
            if (can != null)
            {
                Vector3 forward = can.transform.forward;
                if (forward.sqrMagnitude < 0.01f)
                {
                    forward = Vector3.forward;
                }

                Vector3 position = can.transform.position + forward.normalized * 10f;
                position.y = can.transform.position.y + 0.05f;
                return position;
            }

            return new Vector3(30f, 0.05f, 30f);
        }

        private static Material CreateGoalMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader)
            {
                name = "Guvenli_Toplanma_Alani_Mat",
                color = new Color(0.1f, 0.65f, 0.32f, 0.82f)
            };
        }
    }
}
