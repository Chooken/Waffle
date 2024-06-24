using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
