using System;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestStep[] steps;
        [SerializeField] private int currentStepIndex;
        [SerializeField] private AudioClip questCompleteSound;
        [SerializeField, Range(0f, 1f)] private float questCompleteSoundVolume = 0.85f;

        private AudioSource questAudioSource;

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

            PlayQuestCompleteSound();
            currentStepIndex++;
            NotifyStepChanged();
        }

        private void PlayQuestCompleteSound()
        {
            if (questCompleteSound == null)
            {
                questCompleteSound = Resources.Load<AudioClip>("Audio/quest_complete");
            }

            if (questCompleteSound == null)
            {
                Debug.LogWarning("Gorev tamamlama sesi bulunamadi: Resources/Audio/quest_complete");
                return;
            }

            if (questAudioSource == null)
            {
                questAudioSource = gameObject.GetComponent<AudioSource>();
                if (questAudioSource == null)
                {
                    questAudioSource = gameObject.AddComponent<AudioSource>();
                }

                questAudioSource.playOnAwake = false;
                questAudioSource.spatialBlend = 0f;
            }

            questAudioSource.PlayOneShot(questCompleteSound, questCompleteSoundVolume);
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
                "Perihan Teyze'nin evindeki riskli esyalari sabitle.",
                "Devrilebilecek veya dusme riski olan esyalari E ile sabitle.",
                "");

            AppendStepIfMissing(
                "leave_ayse_house",
                "Perihan Teyze'nin evinden cik.",
                "Tum esyalari sabitledin. Dis kapiya gidip Perihan Teyze'nin yanina don.",
                "");

            AppendStepIfMissing(
                "talk_to_ayse",
                "Perihan Teyze ile konus ve siradaki ipucunu al.",
                "Perihan Teyze sana yeni gorevi ve A harfini verecek.",
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
                "Evin onunde seni bekleyen Cem'den son gorevi al.",
                "Arkadasin Cem ile konus ve son guvenli toplanma gorevini ogren.",
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
