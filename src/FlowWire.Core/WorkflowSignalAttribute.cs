namespace FlowWire;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowSignalAttribute : Attribute
{
    public string? Name { get; init; }
}
