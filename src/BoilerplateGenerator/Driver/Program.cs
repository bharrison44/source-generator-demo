using BoilerplateGenerator.Shared;

namespace Driver;

internal class Program
{
    static void Main(string[] args)
    {
        var thing = new Thing("hello", Guid.NewGuid(), 4, true);

        Console.WriteLine(thing.Handle());
    }
}

[Handler]
public partial class Thing
{
    protected string Name { get; }

    protected Guid Id { get; }

    protected int Count { get; }

    protected bool Enabled { get; }

    protected override string HandleName(string value) => value;

    protected override string HandleId(Guid value) => value.ToString();

    protected override string HandleCount(int value) => new string('a', value);

    protected override string HandleEnabled(bool value) => value.ToString();
}

