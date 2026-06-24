using UnityEngine;

namespace PatininIzinde.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private float interactionViewAngle = 65f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool logCurrentInteractable;

        private IInteractable currentInteractable;

        public IInteractable CurrentInteractable => currentInteractable;
        public Transform PlayerTransform => transform;

        private void Update()
        {
            currentInteractable = FindClosestInteractable();

            if (currentInteractable != null && currentInteractable.CanInteract && Input.GetKeyDown(interactionKey))
            {
                if (logCurrentInteractable)
                {
                    Debug.Log($"Interacting with: {currentInteractable.InteractionText}", this);
                }

                currentInteractable.Interact(this);
            }
        }

        private IInteractable FindClosestInteractable()
        {
            Transform viewTransform = Camera.main != null ? Camera.main.transform : transform;
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayers, QueryTriggerInteraction.Collide);
            IInteractable closest = null;
            float closestScore = float.MaxValue;

            foreach (Collider hit in hits)
            {
                IInteractable interactable = FindUsableInteractable(hit);
                if (interactable == null || !interactable.CanInteract)
                {
                    continue;
                }

                Vector3 targetPoint = hit.bounds.ClosestPoint(viewTransform.position);
                Vector3 toTarget = targetPoint - viewTransform.position;
                float distance = toTarget.magnitude;
                if (distance <= 0.01f)
                {
                    continue;
                }

                float angle = Vector3.Angle(viewTransform.forward, toTarget);
                if (angle > interactionViewAngle)
                {
                    continue;
                }

                float score = distance + angle * 0.03f;
                if (score < closestScore)
                {
                    closestScore = score;
                    closest = interactable;
                }
            }

            return closest;
        }

        private static IInteractable FindUsableInteractable(Collider hit)
        {
            IInteractable[] localInteractables = hit.GetComponents<IInteractable>();
            foreach (IInteractable interactable in localInteractables)
            {
                if (interactable.CanInteract)
                {
                    return interactable;
                }
            }

            IInteractable[] parentInteractables = hit.GetComponentsInParent<IInteractable>();
            foreach (IInteractable interactable in parentInteractables)
            {
                if (interactable.CanInteract)
                {
                    return interactable;
                }
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
