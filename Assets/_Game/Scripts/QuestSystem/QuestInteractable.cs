using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class QuestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private string requiredStepId;
        [SerializeField] private string interactionText = "E ile etkiles";
        [SerializeField] private bool completeStepOnInteract = true;

        public string InteractionText => interactionText;
        public bool CanInteract => questManager != null && questManager.IsCurrentStep(requiredStepId);

        public void Interact(PlayerInteractor interactor)
        {
            if (completeStepOnInteract)
            {
                questManager.CompleteStep(requiredStepId);
            }
        }
    }
}
