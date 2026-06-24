using System;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class SafetyFixManager : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private string activeStepId = "secure_ayse_items";
        [SerializeField] private int totalItems = 9;
        [SerializeField] private int fixedItems;

        public event Action<int, int> CountChanged;

        public int FixedItems => fixedItems;
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
            CountChanged?.Invoke(fixedItems, totalItems);
        }

        public void ConfigureTotal(int total)
        {
            totalItems = Mathf.Max(0, total);
            fixedItems = 0;
            CountChanged?.Invoke(fixedItems, totalItems);
        }

        public bool TryFix()
        {
            if (!IsActive || fixedItems >= totalItems)
            {
                return false;
            }

            fixedItems++;
            CountChanged?.Invoke(fixedItems, totalItems);

            if (fixedItems >= totalItems && questManager != null)
            {
                questManager.CompleteStep(activeStepId);
            }

            return true;
        }
    }
}
