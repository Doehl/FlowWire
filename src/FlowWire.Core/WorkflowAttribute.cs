namespace FlowWire;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowAttribute : Attribute
{
    public int Version { get; init; } = 1;
    public string? Name { get; init; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowRunAttribute : Attribute
{
}
