namespace PatininIzinde.Interaction
{
    public interface IInteractable
    {
        string InteractionText { get; }
        bool CanInteract { get; }
        void Interact(PlayerInteractor interactor);
    }
}
