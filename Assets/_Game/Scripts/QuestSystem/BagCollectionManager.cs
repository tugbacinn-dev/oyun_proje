using System;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class BagCollectionManager : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private string activeStepId = "prepare_bag";
        [SerializeField] private int totalItems = 10;
        [SerializeField] private int collectedItems;

        public event Action<int, int> CountChanged;

        public int CollectedItems => collectedItems;
        public int TotalItems => totalItems;
        public bool IsActive => questManager != null && questManager.IsCurrentStep(activeStepId);

        private void Awake()
        {
            if (questManager == null)
            {
                questManager = FindFirstObjectByType<QuestManager>();
            }
        }

        private void Start()
        {
            CountChanged?.Invoke(collectedItems, totalItems);
        }

        public void ConfigureTotal(int total)
        {
            totalItems = Mathf.Max(0, total);
            collectedItems = 0;
            CountChanged?.Invoke(collectedItems, totalItems);
        }

        public bool TryCollect()
        {
            if (!IsActive || collectedItems >= totalItems)
            {
                return false;
            }

            collectedItems++;
            CountChanged?.Invoke(collectedItems, totalItems);

            if (collectedItems >= totalItems && questManager != null)
            {
                questManager.CompleteStep(activeStepId);
            }

            return true;
        }
    }
}
