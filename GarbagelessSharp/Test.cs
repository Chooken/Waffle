using System.Numerics;

namespace GarbagelessSharp;

public struct Data
{
    public long Number;
    public double NumberTwo;
    public Vector2 Vector;
}

public static class GarbagelessTests
{
    public static void TestUnmanaged()
    {
        Console.WriteLine("Testing Unmanaged");
        
        Unmanaged<Data> data;

        // Test Creating and Freeing 20,000,000 values
        for (int i = 0; i < 20_000_000; i++)
        {
            data = new Data();

            data.Value.Number = i;
    
            data.Dispose();
        }
    }
    
    public static void TestUnmanagedList()
    {
        Console.WriteLine("Testing UnmanagedList");
        
        UnmanagedList<Data> list = new UnmanagedList<Data>(0, 1);

        // Test Add
        list.Add(new Data());

        // Test Change Value member
        list[0].Number = 20;

        // Test Change Value
        var data2 = new Data();
        data2.Number = 33;
        list[0] = data2;

        // Test Add with resize
        list.Add(new Data());
        list.Add(new Data());
        list.Add(new Data());

        list[2].Number = 44;
        
        // Test Slicing
        Span<Data> slice = list[0..3];
        
        // Test Free
        list.Dispose();

        list = new UnmanagedList<Data>();

        // Test Adding 2,000,000 and free
        for (int i = 0; i < 2_000_000; i++)
        {
            list.Add(new Data());
        }
        
        list.Dispose();
        
        // Test Creating and Freeing 2,000,000 Lists
        for (int i = 0; i < 2_000_000; i++)
        {
            list = new UnmanagedList<Data>();
            list.Dispose();
        }
        
        // Test using syntax
        using UnmanagedList<Data> list2 = new();
        
        for (int i = 0; i < 2_000_000; i++)
        {
            list2.Add(new Data());
        }
    }
}