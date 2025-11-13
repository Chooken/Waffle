namespace WaffleEngine.UI;

public static partial class Ui
{
    public static Flex Flex => new Flex();
}

public struct Flex : ILayout
{
    public void CalculateFitSize(UiElement element, bool width)
    {
        if (width)
        {
            element.Bounds.ContentWidth = 0;
        }
        else
        {
            element.Bounds.ContentHeight = 0;
        }
        
        // Calculate Fit Content Size
        foreach (var child in element.Children)
        {
            child.Layout.CalculateFitSize(child, width);

            switch (element.Settings.Direction)
            {
                case UiDirection.LeftToRight:
                    
                    if (width)
                    {
                        element.Bounds.ContentWidth += child.Bounds.CalculatedWidth;
                    }
                    else
                    {
                        element.Bounds.ContentHeight = MathF.Max(element.Bounds.ContentHeight, child.Bounds.CalculatedHeight);
                    }
                    break;
                
                case UiDirection.TopToBottom:
                    
                    if (width)
                    {
                        element.Bounds.ContentWidth = MathF.Max(element.Bounds.ContentWidth, child.Bounds.CalculatedWidth);
                    }
                    else
                    {
                        element.Bounds.ContentHeight += child.Bounds.CalculatedHeight;
                    }
                    break;
            }
        }
        
        // Add Gap to Content Size
        if (width)
        {
            if (element.Settings.Direction == UiDirection.LeftToRight)
                element.Bounds.ContentWidth += MathF.Max((element.Children.Count - 1) * element.Settings.Gap, 0);
        }
        else
        {
            if (element.Settings.Direction == UiDirection.TopToBottom)
                element.Bounds.ContentHeight += MathF.Max((element.Children.Count - 1) * element.Settings.Gap, 0);
        }

        // Set Calculated Size based off type.
        if (width)
        {
            switch (element.Settings.Width.SizeType)
            {
                case UiSizeType.Fixed:
                    element.Bounds.CalculatedWidth = element.Settings.Width.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    element.Bounds.CalculatedWidth = 
                        element.Settings.Width.ComputeValue(element.Bounds.ContentWidth + element.Settings.Padding.TotalHorizontal);
                    break;
                case UiSizeType.RatioOfX:
                    element.Bounds.CalculatedWidth = 
                        element.Settings.Width.ComputeValue(element.Bounds.ContentWidth + element.Settings.Padding.TotalHorizontal) * element.Settings.Width.Value;
                    break;
                default:
                    element.Bounds.CalculatedWidth = 0;
                    break;
            }
        }
        else
        {
            switch (element.Settings.Height.SizeType)
            {
                case UiSizeType.Fixed:
                    element.Bounds.CalculatedHeight = element.Settings.Height.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    element.Bounds.CalculatedHeight =
                        element.Settings.Height.ComputeValue(element.Bounds.ContentHeight + element.Settings.Padding.TotalVertical);
                    break;
                case UiSizeType.RatioOfX:
                    // Not sure if I should make this manipulated by min, max and step.
                    element.Bounds.CalculatedHeight = element.Bounds.CalculatedWidth * element.Settings.Height.Value;
                    break;
                default:
                    element.Bounds.CalculatedHeight = 0;
                    break;
            }
        }
    }
    
    public void CalculatePercentages(UiElement element, bool width)
    {
        foreach (var child in element.Children)
        {
            if (width)
            {
                if (child.Settings.Width.SizeType == UiSizeType.Percentage)
                {
                    if (element.Settings.Direction == UiDirection.LeftToRight)
                    {
                        child.Bounds.CalculatedWidth = (element.Bounds.CalculatedWidth - element.Settings.Padding.TotalHorizontal) * child.Settings.Width.Value;
                        element.Bounds.ContentWidth += child.Bounds.CalculatedWidth;
                    }
                    else
                    {
                        child.Bounds.CalculatedWidth = (element.Bounds.CalculatedWidth - element.Settings.Padding.TotalHorizontal) * child.Settings.Width.Value;
                        element.Bounds.ContentWidth = MathF.Max(element.Bounds.ContentWidth, child.Bounds.CalculatedWidth);
                    }

                    child.Bounds.CalculatedWidth = child.Settings.Width.ComputeValue(child.Bounds.CalculatedWidth);
                }
            }
            else
            {
                if (child.Settings.Height.SizeType == UiSizeType.Percentage)
                {
                    if (element.Settings.Direction == UiDirection.LeftToRight)
                    {
                        child.Bounds.CalculatedHeight = (element.Bounds.CalculatedHeight - element.Settings.Padding.TotalVertical) * child.Settings.Height.Value;
                        element.Bounds.ContentHeight = MathF.Max(element.Bounds.ContentHeight, child.Bounds.CalculatedHeight);
                    }
                    else
                    {
                        child.Bounds.CalculatedHeight = (element.Bounds.CalculatedHeight - element.Settings.Padding.TotalVertical) * child.Settings.Height.Value;
                        element.Bounds.ContentHeight += child.Bounds.CalculatedHeight;
                    }
                    
                    child.Bounds.CalculatedHeight = child.Settings.Height.ComputeValue(child.Bounds.CalculatedHeight);
                }
            }
            
            child.Layout.CalculatePercentages(child, width);
        }
        
        RecalculateContentSize(element, width);
    }
    
    public void GrowChildren(UiElement element, bool width)
    {
        float remainder = width ? 
            element.Bounds.CalculatedWidth - element.Settings.Padding.TotalHorizontal - element.Bounds.ContentWidth : 
            element.Bounds.CalculatedHeight - element.Settings.Padding.TotalVertical - element.Bounds.ContentHeight;

        int childGrowCount = 0;
        
        foreach (var child in element.Children)
        {
            if (width)
            {
                if (child.Settings.Width.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
            else
            {
                if (child.Settings.Height.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
        }

        bool fillPass = (width && element.Settings.Direction == UiDirection.TopToBottom) ||
                        (!width && element.Settings.Direction == UiDirection.LeftToRight);

        if (remainder > 0)
        {
            while (remainder > 0 || fillPass)
            {
                if (childGrowCount == 0 || element.Children.Count == 0)
                    break;

                float growValue = remainder / childGrowCount;

                foreach (var child in element.Children)
                {
                    if (width)
                    {
                        if (child.Settings.Width.SizeType == UiSizeType.Grow)
                        {
                            if (element.Settings.Direction == UiDirection.LeftToRight)
                            {
                                (float value, float overflow) =
                                    child.Settings.Width.GetRemainder(child.Bounds.CalculatedWidth + growValue);

                                child.Bounds.CalculatedWidth = value;
                                remainder -= growValue - overflow;
                            }
                            else
                            {
                                child.Bounds.CalculatedWidth =
                                    child.Settings.Width.ComputeValue(element.Bounds.CalculatedWidth -
                                                                      element.Settings.Padding.TotalHorizontal);
                            }
                        }
                    }
                    else
                    {
                        if (child.Settings.Height.SizeType == UiSizeType.Grow)
                        {
                            if (element.Settings.Direction == UiDirection.LeftToRight)
                            {
                                child.Bounds.CalculatedHeight =
                                    child.Settings.Height.ComputeValue(element.Bounds.CalculatedHeight -
                                                                       element.Settings.Padding.TotalVertical);
                            }
                            else
                            {
                                (float value, float overflow) =
                                    child.Settings.Height.GetRemainder(child.Bounds.CalculatedHeight + growValue);

                                child.Bounds.CalculatedHeight = value;
                                remainder -= growValue - overflow;
                            }
                        }
                    }
                }

                if (fillPass)
                    break;
            }
        }
        else
        {
            foreach (var child in element.Children)
            {
                if (width)
                {
                    if (child.Settings.Width.SizeType == UiSizeType.Grow)
                    {
                        if (element.Settings.Direction == UiDirection.TopToBottom)
                        {
                            child.Bounds.CalculatedWidth =
                                child.Settings.Width.ComputeValue(element.Bounds.CalculatedWidth -
                                                                  element.Settings.Padding.TotalHorizontal);
                        }
                    }
                }
                else
                {
                    if (child.Settings.Height.SizeType == UiSizeType.Grow)
                    {
                        if (element.Settings.Direction == UiDirection.LeftToRight)
                        {
                            child.Bounds.CalculatedHeight =
                                child.Settings.Height.ComputeValue(element.Bounds.CalculatedHeight -
                                                                   element.Settings.Padding.TotalVertical);
                        }
                    }
                }
            }
        }

        foreach (var child in element.Children)
        {
            child.Layout.GrowChildren(child, width);
        }
        
        RecalculateContentSize(element, width);
    }
    
    public void CalculatePositions(UiElement element, Vector2 position)
    {
        element.Bounds.CalulatedPosition = position;
        
        // Get Start Position and Size Without Padding
        Vector2 childStartPosition = new Vector2(
            position.x + element.Settings.Padding.Left, 
            position.y + element.Settings.Padding.Top);
        Vector2 sizeWithoutPadding = new Vector2(
            element.Bounds.CalculatedWidth - element.Settings.Padding.TotalHorizontal, 
            element.Bounds.CalculatedHeight - element.Settings.Padding.TotalVertical);

        Vector2 childOffset = Vector2.Zero;

        foreach (var child in element.Children)
        {
            // Get Alignment Offset
            Vector2 alignmentOffset = Vector2.Zero;

            float horizontalRemainder = element.Settings.Direction == UiDirection.LeftToRight
                ? sizeWithoutPadding.x - element.Bounds.ContentWidth
                : sizeWithoutPadding.x - child.Bounds.CalculatedWidth;

            switch (element.Settings.Alignment.Horizontal)
            {
                case UiAlignmentHorizontal.Center:
                    alignmentOffset.x = horizontalRemainder / 2;
                    break;
                case UiAlignmentHorizontal.Right:
                    alignmentOffset.x = horizontalRemainder;
                    break;
            }
            
            float verticalRemainder = element.Settings.Direction == UiDirection.TopToBottom
                ? sizeWithoutPadding.y - element.Bounds.ContentHeight
                : sizeWithoutPadding.y - child.Bounds.CalculatedHeight;

            switch (element.Settings.Alignment.Vertical)
            {
                case UiAlignmentVertical.Center:
                    alignmentOffset.y = verticalRemainder / 2;
                    break;
                case UiAlignmentVertical.Bottom:
                    alignmentOffset.y = verticalRemainder;
                    break;
            }

            // Propagate to child.
            child.Layout.CalculatePositions(child, childStartPosition + alignmentOffset + childOffset);
            
            // Offset next child.
            switch (element.Settings.Direction)
            {
                case UiDirection.LeftToRight:
                    childOffset.x += child.Bounds.CalculatedWidth + element.Settings.Gap;
                    break; 
                case UiDirection.TopToBottom:
                    childOffset.y += child.Bounds.CalculatedHeight + element.Settings.Gap;
                    break;
            }
        }
    }

    private void RecalculateContentSize(UiElement element, bool width)
    {
        float contentSize = 0;

        foreach (var child in element.Children)
        {
            if (element.Settings.Direction == UiDirection.LeftToRight)
            {
                contentSize = width ? 
                    contentSize + child.Bounds.CalculatedWidth : 
                    MathF.Max(contentSize, child.Bounds.CalculatedHeight);
            }
            else
            {
                contentSize = width ? 
                    MathF.Max(contentSize, child.Bounds.CalculatedWidth) : 
                    contentSize + child.Bounds.CalculatedHeight;
            }
        }
        
        // Add Gap to Content Size
        if (width)
        {
            if (element.Settings.Direction == UiDirection.LeftToRight)
                contentSize += MathF.Max((element.Children.Count - 1) * element.Settings.Gap, 0);
        }
        else
        {
            if (element.Settings.Direction == UiDirection.TopToBottom)
                contentSize += MathF.Max((element.Children.Count - 1) * element.Settings.Gap, 0);
        }

        if (width)
        {
            element.Bounds.ContentWidth = contentSize;
        }
        else
        {
            element.Bounds.ContentHeight = contentSize;
        }
    }
}