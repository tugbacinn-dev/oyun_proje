using UnityEngine;

namespace PatininIzinde.Core
{
    public sealed class StoryPointMarker : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.85f);
        [SerializeField] private float gizmoRadius = 0.35f;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
        }
    }
}
