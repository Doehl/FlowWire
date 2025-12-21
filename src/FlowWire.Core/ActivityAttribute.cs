namespace FlowWire;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ActivityAttribute : Attribute
{
    public string? Name { get; init; }
}
