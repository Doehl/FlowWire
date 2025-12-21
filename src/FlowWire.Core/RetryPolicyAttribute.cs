namespace FlowWire;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RetryPolicyAttribute : Attribute
{
    public int MaxAttempts { get; init; } = 3;
    public double InitialIntervalSeconds { get; init; } = 1;
    public double BackoffCoefficient { get; init; } = 2;
    public double MaxIntervalSeconds { get; init; } = 100;
}
