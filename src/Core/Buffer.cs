using System.Collections;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class Buffer<T> where T : unmanaged
{
    public T[] Items;

    public int Count { get; private set; }
    public bool IsReadOnly => false;

    public Buffer(int start_size = 16, BufferUsage usage = BufferUsage.StaticDraw)
    {
        Items = new T[start_size];
        Count = 0;
    }
    
    public T this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    public void Add(T item)
    {
        Count++;
        
        if (Count > Items.Length)
            Array.Resize(ref Items, Items.Length * 2);

        Items[Count - 1] = item;
    }

    public void Sort(IComparer<T> comparer)
    {
        Array.Sort(Items, 0, Count, comparer);
    }

    public void Clear()
    {
        Count = 0;
    }
}