using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace FlowWire.SourceGenerators.Tests;

public class ActivityProxyGeneratorTests
{
    [Fact]
    public void CheckSourceGenerator()
    {
        var input = @"
using FlowWire;
using System.Threading.Tasks;

namespace TestNamespace;

public interface IMyActivity
{
    [Activity(Name = ""my-activity"")]
    Task<int> RunAsync(string input);
}
";
        var syntaxTree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.Current.CancellationToken);

        // References
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),   // System.Threading.Tasks
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(typeof(ActivityAttribute).Assembly.Location) // FlowWire.Core
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var generator = new ActivityProxyGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var result = driver.GetRunResult();

        Assert.Empty(result.Diagnostics);
        Assert.Single(result.GeneratedTrees);

        var generatedTree = result.GeneratedTrees[0];
        var generatedText = generatedTree.GetText(TestContext.Current.CancellationToken).ToString();

        var expected = @"namespace TestNamespace;

public class IMyActivityProxy : IMyActivity
{
    private readonly FlowWire.IWorkflowContext _context;

    public IMyActivityProxy(FlowWire.IWorkflowContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<int> RunAsync(string input)
    {
        return await _context.CallActivityAsync<string, int>(""my-activity"", input);
    }

}
";
        // Normalize line endings for comparison
        var normalizedGenerated = generatedText.Replace("\r\n", "\n").Trim();
        var normalizedExpected = expected.Replace("\r\n", "\n").Trim();

        Assert.Equal(normalizedExpected, normalizedGenerated);
    }
}
