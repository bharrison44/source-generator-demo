using GeneratedNS;

namespace Driver;

internal class Program
{
    static void Main(string[] args)
    {
        var gen = new GeneratedClass { Name = "bob" };

        Console.WriteLine($"Hello, {gen.Name}!");
    }
}