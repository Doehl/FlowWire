namespace FlowWire;


[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class WorkflowQueryAttribute : Attribute
{
    public string? Name { get; init; }
}
