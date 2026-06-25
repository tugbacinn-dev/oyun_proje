using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class BagCollectibleItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private BagCollectionManager bagManager;
        [SerializeField] private string itemDisplayName = "Esya";
        [SerializeField] private string itemPurpose = "";
        [SerializeField] private string interactionText = "E ile al";
        [SerializeField] private bool hideWhenCollected = true;

        private bool collected;

        public string InteractionText
        {
            get
            {
                string purpose = GetItemPurpose();
                return string.IsNullOrWhiteSpace(purpose)
                    ? $"{interactionText}: {itemDisplayName}"
                    : $"{interactionText}: {itemDisplayName} - {purpose}";
            }
        }

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

        private string GetItemPurpose()
        {
            if (!string.IsNullOrWhiteSpace(itemPurpose))
            {
                return itemPurpose;
            }

            string itemName = $"{itemDisplayName} {name}".ToLowerInvariant();
            if (itemName.Contains("pusula") || itemName.Contains("compass"))
            {
                return "yon bulmak icin";
            }

            if (itemName.Contains("hap") || itemName.Contains("pill"))
            {
                return "ilac ihtiyaci icin";
            }

            if (itemName.Contains("su") || itemName.Contains("water"))
            {
                return "susuz kalmamak icin";
            }

            if (itemName.Contains("canopen"))
            {
                return "konserve acmak icin";
            }

            if (itemName.Contains("konserve") || itemName.Contains("sardine") || itemName.Contains("canned"))
            {
                return "acil yiyecek";
            }

            if (itemName.Contains("fener") || itemName.Contains("flashlight"))
            {
                return "karanlikta gormek icin";
            }

            if (itemName.Contains("pil") || itemName.Contains("battery"))
            {
                return "cihazlara enerji";
            }

            if (itemName.Contains("ilk") || itemName.Contains("firstaid"))
            {
                return "yaralanmalara ilk yardim";
            }

            if (itemName.Contains("telsiz") || itemName.Contains("radio") || itemName.Contains("walkie"))
            {
                return "haber almak icin";
            }

            if (itemName.Contains("kibrit") || itemName.Contains("match"))
            {
                return "ates yakmak icin";
            }

            return "acil durumda lazim";
        }
    }
}
