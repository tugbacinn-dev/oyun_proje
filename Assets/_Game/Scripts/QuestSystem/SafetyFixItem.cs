using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class SafetyFixItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private SafetyFixManager safetyFixManager;
        [SerializeField] private string itemDisplayName = "Esya";
        [SerializeField] private string interactionText = "E ile sabitle";

        private bool fixedInPlace;

        public string InteractionText => $"{interactionText}: {itemDisplayName}";
        public bool CanInteract => !fixedInPlace && safetyFixManager != null && safetyFixManager.IsActive;

        private void Awake()
        {
            if (safetyFixManager == null)
            {
                safetyFixManager = FindFirstObjectByType<SafetyFixManager>();
            }
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (fixedInPlace || safetyFixManager == null || !safetyFixManager.TryFix())
            {
                return;
            }

            fixedInPlace = true;
        }
    }
}
