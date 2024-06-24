using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaffleEngine
{
    public class UIManager
    {
        private IInteractableUI _selected;

        Stack<UIElement> _stack;

        public void UpdateUI()
        {
            if (_stack.Count == 0) 
                return;

            _stack.Peek().Update();

            UIBounds render_bounds = UIBounds.GetRenderBounds();

            foreach (UIElement element in _stack)
            {
                element.Render(render_bounds);
            }
        }

        public void PopActiveUI() => _stack.Pop();

        public void PushActiveUI(UIElement element) => _stack.Push(element);

        public void SetSelected(IInteractableUI selected) => _selected = selected;
    }
}
