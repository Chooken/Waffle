using System.Diagnostics.CodeAnalysis;

namespace WaffleEngine.UI;

public abstract class INode
{
    public NodeTree Tree { get; private set; }
    public IRect Rect { get; private set; }
    public bool Clipped { get; private set; }
    public bool Enabled { get; private set; }
    public bool IsHovered { get; private set; }
    public List<INode> Children { get; private set; }
    public Dictionary<string, INode> TaggedNodes { get; private set; }

    public virtual void OnInit() {}
    public virtual void OnUpdate() { SetChildrenToParentSize(); }
    public virtual void OnEvent(NodeEvent node_event) {}
    public virtual void OnDraw() { DrawAllChildren(); }
    public virtual void OnDeinit() {}
    
    public bool IsFocused => Tree.Focused == this;
    public bool IsActive => Tree.Active == this;
    
    public INode SetRect(IRect rect)
    {
        Rect = rect;
        return this;
    }
    
    public INode SetClipped(bool state)
    {
        Clipped = state;
        return this;
    }
    
    public INode SetEnabled(bool state)
    {
        Enabled = state;
        return this;
    }

    public INode AddNode(INode child)
    {
        Children.Add(child);
        return this;
    }
    
    public INode AddTaggedNode(string tag, INode child)
    {
        Children.Add(child);
        TaggedNodes.Add(tag, child);
        return this;
    }

    public INode RemoveNode(INode child)
    {
        Children.Remove(child);
        return this;
    }

    public INode RemoveTaggedNode(string tag)
    {
        if (TryGetTaggedNode(tag, out INode? child))
        {
            Children.Remove(child);
            TaggedNodes.Remove(tag);
        }
        return this;
    }

    public bool TryGetTaggedNode(string tag, [NotNullWhen(true)] out INode? child)
    {
        return TaggedNodes.TryGetValue(tag, out child);
    }

    public bool TryGetNode(int index, [NotNullWhen(true)] out INode? child)
    {
        if (Children.Count <= index)
        {
            child = null;
            return false;
        }
        
        child = Children[index];
        
        return true;
    }

    public bool TryFindTaggedNode(string tag, [NotNullWhen(true)] out INode? child)
    {
        if (TryGetTaggedNode(tag, out child))
        {
            return true;
        }

        foreach (var node in Children)
        {
            return TryFindTaggedNode(tag, out child);
        }
        
        return false;
    }

    public void PropagateUpdate()
    {
        OnUpdate();

        foreach (INode child in Children)
        {
            child.PropagateUpdate();
        }
    }
    
    public void PropagateStates(ref bool process_events)
    {
        if (!Enabled) return;

        foreach (INode child in Children)
        {
            child.PropagateStates(ref process_events);
        }

        IsHovered = false;

        if (process_events)
        {
            if (Rect.Contains(Input.Mouse.Position))
            {
                process_events = false;
                
                IsHovered = true;

                if (Input.Mouse.IsLeftPressed || Input.Mouse.IsRightPressed)
                {
                    Tree.SetFocused(this);
                    Tree.SetActive(this);
                }
            }
        }

        if (IsActive)
        {
            if (Input.Mouse.IsLeftDown || Input.Mouse.IsRightDown)
            {
                OnEvent(NodeEvent.MouseHold);
            }
            else
            {
                OnEvent(NodeEvent.MouseClick);
                Tree.SetActive(null);
            }
        }
    }

    public void Draw()
    {
        if (!Enabled || Rect.Width == 0 || Rect.Height == 0)
        {
            return;
        }

        IRect clip = Tree.Clipstack.Peek();

        if (Clipped)
        {
            IRect clippedRect = Rect;

            if (Tree.Clipstack.TryPeek(out clippedRect))
            {
                clippedRect = clippedRect.GetOverlap(Rect) ?? IRect.Zero;
            }
            
            Tree.Clipstack.Push(clippedRect);
        }
        
        OnDraw();

        if (Clipped)
        {
            Tree.Clipstack.Pop();
        }
    }
    
    public void SetChildrenToParentSize()
    {
        foreach (INode child in Children)
        {
            child.SetRect(Rect);
        }
    }
    
    public void DrawAllChildren()
    {
        foreach (INode child in Children)
        {
            child.Draw();
        }
    }
}