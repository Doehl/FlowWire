using BenchmarkDotNet.Attributes;
using FlowWire.Engine.Runtime;

namespace FlowWire.Benchmarks;

[MemoryDiagnoser]
public class ArenaBenchmarks
{
    private Arena _arena = null!;

    [GlobalSetup]
    public void Setup()
    {
        _arena = new Arena(65536); // 64KB Default
    }

    [Benchmark]
    public void Alloc_Small_Fit()
    {
        _arena.Reset();
        var span = _arena.Allocate(1024); // 1KB
        span[0] = 1;
    }

    [Benchmark]
    public void Alloc_Medium_Fit()
    {
        _arena.Reset();
        // 4 x 10KB
        _arena.Allocate(10240);
        _arena.Allocate(10240);
        _arena.Allocate(10240);
        _arena.Allocate(10240);
    }

    // This is expected to FAIL before the fix
    // [Benchmark] 
    // Commented out to prevent crash during baseline run, strictly speaking.
    // However, user asked for pre/post benchmark. 
    // I will enable it, knowing it might crash the benchmark runner or just throw an Exception report.
    [Benchmark]
    public void Alloc_Large_Grow()
    {
        _arena.Reset();
        _arena.Allocate(40000); // 40KB
        _arena.Allocate(40000); // 40KB (Total 80KB > 64KB) - Should Trigger Growth
    }

    [Benchmark]
    public void Alloc_And_Trim()
    {
         _arena.Reset();
         // Force growth
         _arena.Allocate(70000);
         
         // Reset should trim if implemented
         _arena.Reset();
    }
}
