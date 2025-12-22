using BenchmarkDotNet.Running;

namespace FlowWire.Benchmarks;

public class Program
{
    static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
