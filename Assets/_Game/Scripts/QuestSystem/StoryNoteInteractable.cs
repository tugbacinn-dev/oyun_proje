using PatininIzinde.Interaction;
using PatininIzinde.UI;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class StoryNoteInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private GameUIController uiController;
        [SerializeField] private string requiredStepId;
        [SerializeField] private string interactionText = "E ile konus";
        [SerializeField] private string noteTitle = "Not";
        [TextArea(3, 8)]
        [SerializeField] private string noteBody;
        [SerializeField] private bool completeStepAfterReading = true;

        public string InteractionText => interactionText;
        public bool CanInteract => questManager != null && questManager.IsCurrentStep(requiredStepId);

        private void Awake()
        {
            if (questManager == null)
            {
                questManager = FindFirstObjectByType<QuestManager>();
            }

            if (uiController == null)
            {
                uiController = FindFirstObjectByType<GameUIController>();
            }
        }

        public void Configure(
            QuestManager manager,
            GameUIController ui,
            string stepId,
            string prompt,
            string title,
            string body,
            bool completeAfterReading = true)
        {
            questManager = manager != null ? manager : FindFirstObjectByType<QuestManager>();
            uiController = ui != null ? ui : FindFirstObjectByType<GameUIController>();
            requiredStepId = stepId;
            interactionText = prompt;
            noteTitle = title;
            noteBody = body;
            completeStepAfterReading = completeAfterReading;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (uiController != null)
            {
                uiController.ShowNote(noteTitle, noteBody);
            }

            if (completeStepAfterReading && questManager != null)
            {
                questManager.CompleteStep(requiredStepId);
            }
        }
    }
}
