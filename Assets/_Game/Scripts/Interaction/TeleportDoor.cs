using UnityEngine;
using PatininIzinde.QuestSystem;

namespace PatininIzinde.Interaction
{
    public sealed class TeleportDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionText = "E ile gir";
        [SerializeField] private Transform targetPoint;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private string requiredStepId;
        [SerializeField] private bool completeStepAfterTeleport;
        [SerializeField] private bool faceTargetForward = true;
        [SerializeField] private bool logTeleport = true;
        [SerializeField] private float homeReturnCameraLowerOffset = 0.28f;

        public string InteractionText => interactionText;
        public bool CanInteract =>
            ResolveTeleportTarget() != null &&
            !IsMisplacedHomeExit() &&
            (questManager == null ||
             string.IsNullOrWhiteSpace(requiredStepId) ||
             questManager.IsCurrentStep(requiredStepId) ||
             (requiredStepId == "go_to_ayse" && questManager.IsCurrentStep("secure_ayse_items")) ||
             (requiredStepId == "leave_ayse_house" && questManager.IsCurrentStep("talk_to_ayse")) ||
             (requiredStepId == "enter_home_for_simulation" && questManager.IsCurrentStep("go_back_home")));

        public void Interact(PlayerInteractor interactor)
        {
            Transform resolvedTarget = ResolveTeleportTarget();
            if (resolvedTarget == null)
            {
                Debug.LogWarning($"{name} icin targetPoint atanmamis.", this);
                return;
            }

            Transform player = ResolvePlayerTransform(interactor.PlayerTransform);
            TeleportPlayer(player, resolvedTarget);

            if (IsHomeReturnDoor() && interactor.PlayerTransform != player)
            {
                TeleportPlayer(interactor.PlayerTransform, resolvedTarget);
            }

            bool shouldCompleteStep = completeStepAfterTeleport || requiredStepId == "go_to_ayse";
            if (shouldCompleteStep && questManager != null && !string.IsNullOrWhiteSpace(requiredStepId))
            {
                if (requiredStepId == "enter_home_for_simulation" && questManager.IsCurrentStep("go_back_home"))
                {
                    questManager.CompleteStep("go_back_home");
                }
                else
                {
                    questManager.CompleteStep(requiredStepId);
                }

                if ((requiredStepId == "go_back_home" || requiredStepId == "enter_home_for_simulation") &&
                    questManager.IsCurrentStep("enter_home_for_simulation"))
                {
                    questManager.CompleteStep("enter_home_for_simulation");
                }
            }

            if (logTeleport)
            {
                Debug.Log($"{name} oyuncuyu {resolvedTarget.name} noktasina tasidi.", this);
            }
        }

        private Transform ResolveTeleportTarget()
        {
            if (!IsHomeReturnDoor())
            {
                return targetPoint;
            }

            if (targetPoint != null)
            {
                return targetPoint;
            }

            GameObject homeInterior = GameObject.Find("BizimEv_IcMekan");
            Transform homeRoot = homeInterior != null ? homeInterior.transform : null;

            Transform interiorDoor = FindChild(homeRoot, "Disari_Cikis_Kapisi");
            if (interiorDoor != null)
            {
                return interiorDoor;
            }

            Transform interiorDoorEntry = FindChild(homeRoot, "BizimEv_Ic_Kapi_Giris_Noktasi");
            if (interiorDoorEntry != null)
            {
                return interiorDoorEntry;
            }

            Transform interiorStart = FindChild(homeRoot, "BizimEv_Ic_Baslangic");
            if (interiorStart != null)
            {
                return interiorStart;
            }

            return targetPoint;
        }

        private Vector3 ResolveTeleportPosition(Transform resolvedTarget, Transform player)
        {
            Vector3 position = resolvedTarget.position;
            if (IsHomeReturnDoor() && player != null && player.name == "PlayerCameraRig")
            {
                position.y -= homeReturnCameraLowerOffset;
            }

            return position;
        }

        private void TeleportPlayer(Transform player, Transform resolvedTarget)
        {
            if (player == null)
            {
                return;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            player.position = ResolveTeleportPosition(resolvedTarget, player);

            if (faceTargetForward)
            {
                player.rotation = resolvedTarget.rotation;
            }

            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        private Transform ResolvePlayerTransform(Transform fallback)
        {
            if (!IsHomeReturnDoor())
            {
                return fallback;
            }

            GameObject playerRig = GameObject.Find("PlayerCameraRig");
            return playerRig != null ? playerRig.transform : fallback;
        }

        private bool IsHomeReturnDoor()
        {
            return requiredStepId == "go_back_home" ||
                   name == "BizimEv_Giris_Tetikleyici" ||
                   name == "BizimEv_Giris_Kapisi";
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform match = FindChild(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private bool IsMisplacedHomeExit()
        {
            if (name != "Disari_Cikis_Kapisi")
            {
                return false;
            }

            if (targetPoint != null && targetPoint.name == "BizimEv_Dis_Kapi")
            {
                return false;
            }

            Transform current = transform;
            while (current != null)
            {
                if (current.name == "BizimEv_IcMekan")
                {
                    return false;
                }

                current = current.parent;
            }

            GameObject homeInterior = GameObject.Find("BizimEv_IcMekan");
            if (homeInterior == null)
            {
                return true;
            }

            Renderer[] renderers = homeInterior.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return true;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            bounds.Expand(new Vector3(3f, 3f, 3f));
            return !bounds.Contains(transform.position);
        }
    }
}
