using System;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestStep[] steps;
        [SerializeField] private int currentStepIndex;

        public event Action<QuestStep> StepChanged;
        public event Action<string> LetterCollected;

        public QuestStep CurrentStep =>
            steps != null && currentStepIndex >= 0 && currentStepIndex < steps.Length
                ? steps[currentStepIndex]
                : null;

        private void Awake()
        {
            EnsureExtendedQuestStepsExist();
            FinalQuestCoordinator.EnsureExists(this);
        }

        private void Start()
        {
            NotifyStepChanged();
        }

        public bool IsCurrentStep(string stepId)
        {
            return CurrentStep != null && CurrentStep.StepId == stepId;
        }

        public void CompleteStep(string stepId)
        {
            if (!IsCurrentStep(stepId))
            {
                return;
            }

            QuestStep completedStep = CurrentStep;
            if (!string.IsNullOrWhiteSpace(completedStep.RewardLetter))
            {
                LetterCollected?.Invoke(completedStep.RewardLetter);
            }

            currentStepIndex++;
            NotifyStepChanged();
        }

        private void NotifyStepChanged()
        {
            StepChanged?.Invoke(CurrentStep);
        }

        private void EnsureExtendedQuestStepsExist()
        {
            if (!HasStep("go_to_ayse"))
            {
                return;
            }

            AppendStepIfMissing(
                "secure_ayse_items",
                "Ayse Teyze'nin evindeki riskli esyalari sabitle.",
                "Devrilebilecek veya dusme riski olan esyalari E ile sabitle.",
                "");

            AppendStepIfMissing(
                "leave_ayse_house",
                "Ayse Teyze'nin evinden cik.",
                "Tum esyalari sabitledin. Dis kapiya gidip Ayse Teyze'nin yanina don.",
                "");

            AppendStepIfMissing(
                "talk_to_ayse",
                "Ayse Teyze ile konus ve siradaki ipucunu al.",
                "Ayse Teyze sana yeni gorevi ve A harfini verecek.",
                "A");

            AppendStepIfMissing(
                "go_back_home",
                "Bizim eve geri don.",
                "Cok-Kapan-Tutun simulasyonu icin kendi evine geri git.",
                "");

            AppendStepIfMissing(
                "enter_home_for_simulation",
                "Bizim evin kapisindan iceri gir.",
                "Evin kapisinda E tusuna basarak iceri gir.",
                "");

            AppendStepIfMissing(
                "earthquake_moment",
                "Deprem aninda guvenli noktaya gec.",
                "Cok-Kapan-Tutun davranisini uygulamak icin guvenli noktayi bul.",
                "");

            AppendStepIfMissing(
                "talk_to_can",
                "Evin onunde seni bekleyen Can'dan son gorevi al.",
                "Arkadasin Can ile konus ve son guvenli toplanma gorevini ogren.",
                "D");

            AppendStepIfMissing(
                "go_to_assembly_area",
                "Guvenli toplanma alanina git.",
                "Depremden sonra binadan uzak, acik ve guvenli toplanma alanina ulas.",
                "");
        }

        private void AppendStepIfMissing(string stepId, string title, string description, string rewardLetter)
        {
            if (HasStep(stepId))
            {
                return;
            }

            int oldLength = steps != null ? steps.Length : 0;
            QuestStep[] expandedSteps = new QuestStep[oldLength + 1];
            for (int i = 0; i < oldLength; i++)
            {
                expandedSteps[i] = steps[i];
            }

            expandedSteps[oldLength] = new QuestStep
            {
                StepId = stepId,
                Title = title,
                Description = description,
                RewardLetter = rewardLetter
            };

            steps = expandedSteps;
        }

        private bool HasStep(string stepId)
        {
            if (steps == null)
            {
                return false;
            }

            foreach (QuestStep step in steps)
            {
                if (step != null && step.StepId == stepId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
