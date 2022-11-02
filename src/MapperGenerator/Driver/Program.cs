using GeneratedNS;

namespace Driver;

internal class Program
{
    static void Main(string[] args)
    {
        var thing = new Thing { Name = "John", Size = 34 };

        SimpleMapper.Register<Thing, Stuff>();
        SimpleMapper.Register<Stuff, Thing>();
        SimpleMapper.Register<Thing, Junk>();

        var stuff = thing.MapToStuff();
        var thing2 = stuff.MapToThing();
        var junk = thing2.MapToJunk();

        Console.WriteLine($"Hello, {stuff}!");
        Console.WriteLine($"Hello, {thing2}!");
        Console.WriteLine($"Hello, {junk}!");
    }
}

public record Thing
{
    public string Name { get; set; }

    public int Size { get; set; }
}

public record Stuff
{
    public string Name { get; set; }

    public int Size { get; set; }
}

public record Junk
{
    public string Name { get; set; }

    public int Size { get; set; }
}