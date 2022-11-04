using MapperGenerator;

namespace Driver;

internal class Program
{
    static void Main(string[] args)
    {
        var thing = new Thing { Name = "John", Size = 34 };

        Mappings.Register<Thing, Stuff>();
        Mappings.Register<Stuff, Thing>();

        var stuff = thing.MapToStuff();
        var thing2 = stuff.MapToThing();
    }
}

public record Thing
{
    public string Name { get; set; }

    public int? Size { get; set; }

    public bool Enabled { get; set; }
}

public record Stuff
{
    public string Name { get; set; }

    public int Size { get; set; }

    public bool Enabled { get; set; }
}