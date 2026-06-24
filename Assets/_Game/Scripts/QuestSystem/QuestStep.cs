using System;

namespace PatininIzinde.QuestSystem
{
    [Serializable]
    public sealed class QuestStep
    {
        public string StepId;
        public string Title;
        public string Description;
        public string RewardLetter;
    }
}
