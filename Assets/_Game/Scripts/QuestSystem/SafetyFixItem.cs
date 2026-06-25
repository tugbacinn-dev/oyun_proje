using PatininIzinde.Interaction;
using UnityEngine;

namespace PatininIzinde.QuestSystem
{
    public sealed class SafetyFixItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private SafetyFixManager safetyFixManager;
        [SerializeField] private string itemDisplayName = "Esya";
        [SerializeField] private string interactionText = "E ile sabitle";
        [SerializeField] private AudioClip fixSound;
        [SerializeField, Range(0f, 1f)] private float fixSoundVolume = 0.85f;

        private bool fixedInPlace;
        private static AudioClip sharedFixSound;

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
            PlayFixSound();
        }

        private void PlayFixSound()
        {
            AudioClip clip = fixSound != null ? fixSound : GetSharedFixSound();
            if (clip == null)
            {
                Debug.LogWarning("Sabitleme sesi bulunamadi: Resources/Audio/fix_drill");
                return;
            }

            AudioSource.PlayClipAtPoint(clip, transform.position, fixSoundVolume);
        }

        private static AudioClip GetSharedFixSound()
        {
            if (sharedFixSound == null)
            {
                sharedFixSound = Resources.Load<AudioClip>("Audio/fix_drill");
            }

            return sharedFixSound;
        }
    }
}
