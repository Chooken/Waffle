
namespace WaffleEngine
{
    public interface IInteractableUI
    {
        public IInteractableUI InteractUp();
        public IInteractableUI InteractDown();
        public IInteractableUI InteractLeft();
        public IInteractableUI InteractRight();

        public void OnSelect();
        public void OnDeselect();

        public void OnInteract();
    }
}
