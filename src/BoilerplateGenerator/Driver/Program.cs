using BoilerplateGenerator.Shared;

namespace Driver;

internal class Program
{
    static void Main(string[] args)
    {
        var thing = new Thing(Guid.NewGuid(), "hello");

        Console.WriteLine(thing.Handle());
    }
}

[Handler]
public partial class Thing
{
    protected Guid Id { get; }

    protected string Name { get; }

    protected override string HandleName(string value) => value;

    protected override string HandleId(Guid value) => value.ToString();
}

