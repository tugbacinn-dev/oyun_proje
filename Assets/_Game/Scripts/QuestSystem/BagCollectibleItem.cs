using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class BagCollectibleItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private BagCollectionManager bagManager;
        [SerializeField] private string itemDisplayName = "Esya";
        [SerializeField] private string interactionText = "E ile al";
        [SerializeField] private bool hideWhenCollected = true;

        private bool collected;

        public string InteractionText => $"{interactionText}: {itemDisplayName}";
        public bool CanInteract => !collected && bagManager != null && bagManager.IsActive;

        private void Awake()
        {
            if (bagManager == null)
            {
                bagManager = FindFirstObjectByType<BagCollectionManager>();
            }
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (collected || bagManager == null || !bagManager.TryCollect())
            {
                return;
            }

            collected = true;

            if (hideWhenCollected)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
