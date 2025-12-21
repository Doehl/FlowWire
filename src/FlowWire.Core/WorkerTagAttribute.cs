namespace FlowWire;

public enum TagCondition
{
    Or = 0,
    And = 1
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
public sealed class WorkerTagAttribute : Attribute
{
    public string[] Tags { get; }
    public TagCondition Condition { get; }

    public WorkerTagAttribute(string tag)
    {
        Tags = new[] { tag };
        Condition = TagCondition.Or;
    }

    public WorkerTagAttribute(string[] tags, TagCondition condition = TagCondition.Or)
    {
        Tags = tags;
        Condition = condition;
    }
}
