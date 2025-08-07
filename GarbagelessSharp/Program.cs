// See https://aka.ms/new-console-template for more information

using GarbagelessSharp;

Console.WriteLine("Start Unmanaged Memory Test");

GarbagelessTests.TestUnmanaged();
GarbagelessTests.TestUnmanagedList();

Console.ReadLine();