using PatininIzinde.Interaction;
using PatininIzinde.UI;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class FinalPasswordNoteInteractable : MonoBehaviour, IInteractable
    {
        private const string AssemblyAreaStepId = "go_to_assembly_area";

        [SerializeField] private QuestManager questManager;
        [SerializeField] private GameUIController uiController;
        [SerializeField] private string interactionText = "E ile sifreyi gir";

        public string InteractionText => interactionText;

        public bool CanInteract
        {
            get
            {
                ResolveReferences();
                return questManager == null ||
                       questManager.CurrentStep == null ||
                       questManager.IsCurrentStep(AssemblyAreaStepId);
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void Interact(PlayerInteractor interactor)
        {
            ResolveReferences();
            if (uiController == null)
            {
                return;
            }

            uiController.ShowPasswordPrompt(
                "Taci'nin Sifresi",
                "Taci'yi bulmak icin sana verilen ipuclarindan topladigin sifreyi gir.");
        }

        private void ResolveReferences()
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
    }
}
