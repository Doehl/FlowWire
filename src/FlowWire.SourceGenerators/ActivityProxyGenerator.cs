using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace FlowWire.SourceGenerators;

[Generator]
public class ActivityProxyGenerator : IIncrementalGenerator
{
    private const string ActivityAttributeName = "FlowWire.ActivityAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaces = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is InterfaceDeclarationSyntax iface && iface.Members.Any(m => m.AttributeLists.Count > 0),
            transform: static (ctx, _) => GetInterfaceModel(ctx)
        )
        .Where(static m => m is not null);

        context.RegisterSourceOutput(interfaces, static (spc, source) => Execute(source!, spc));
    }

    private static ActivityInterfaceModel? GetInterfaceModel(GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(interfaceDecl);

        if (symbol is not INamedTypeSymbol interfaceSymbol) return null;

        List<ActivityMethodModel> activityMethods = [];

        foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            var methodModel = TryGetMethodModel(member);
            if (methodModel is not null)
            {
                activityMethods.Add(methodModel);
            }
        }

        if (activityMethods.Count == 0) return null;

        return new ActivityInterfaceModel(
            Namespace: interfaceSymbol.ContainingNamespace.ToDisplayString(),
            InterfaceName: interfaceSymbol.Name,
            Methods: new EquatableArray<ActivityMethodModel>([.. activityMethods])
        );
    }

    private static ActivityMethodModel? TryGetMethodModel(IMethodSymbol methodSymbol)
    {
        var attr = methodSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ActivityAttributeName);
        if (attr is null) return null;

        string? nameOverride = null;
        if (attr.NamedArguments.Any(na => na.Key == "Name"))
        {
            nameOverride = attr.NamedArguments.First(na => na.Key == "Name").Value.Value?.ToString();
        }

        var parameters = methodSymbol.Parameters.Select(p => new ActivityParameterModel(p.Name, p.Type.ToDisplayString())).ToArray();
        var (hasReturnValue, unwrappedReturnType) = GetReturnInfo(methodSymbol.ReturnType);

        return new ActivityMethodModel(
            Name: methodSymbol.Name,
            ReturnType: methodSymbol.ReturnType.ToDisplayString(),
            Parameters: new EquatableArray<ActivityParameterModel>(parameters),
            ActivityNameAttributeValue: nameOverride,
            HasReturnValue: hasReturnValue,
            UnwrappedReturnType: unwrappedReturnType
        );
    }

    private static (bool HasReturnValue, string UnwrappedReturnType) GetReturnInfo(ITypeSymbol returnType)
    {
        if (returnType is INamedTypeSymbol namedReturn)
        {
            // Check for Task<T> or ValueTask<T>
            if (namedReturn.IsGenericType && (namedReturn.Name == "Task" || namedReturn.Name == "ValueTask"))
            {
                return (true, namedReturn.TypeArguments[0].ToDisplayString());
            }
            else if (namedReturn.Name == "Task" || namedReturn.Name == "ValueTask")
            {
                // Task or ValueTask (void)
                return (false, "void");
            }
            // Fallback for synchronous or other types
            if (namedReturn.SpecialType != SpecialType.System_Void)
            {
                return (true, returnType.ToDisplayString());
            }
        }
        else if (returnType.SpecialType != SpecialType.System_Void)
        {
             return (true, returnType.ToDisplayString());
        }

        return (false, "void");
    }

    private static void Execute(ActivityInterfaceModel model, SourceProductionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace {model.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"public class {model.InterfaceName}Proxy : {model.InterfaceName}");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly FlowWire.IWorkflowContext _context;");
        sb.AppendLine();
        sb.AppendLine($"    public {model.InterfaceName}Proxy(FlowWire.IWorkflowContext context)");
        sb.AppendLine("    {");
        sb.AppendLine("        _context = context;");
        sb.AppendLine("    }");
        sb.AppendLine();

        foreach (var method in model.Methods)
        {
            GenerateMethod(sb, method);
        }

        sb.AppendLine("}");

        context.AddSource($"{model.InterfaceName}Proxy.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateMethod(StringBuilder sb, ActivityMethodModel method)
    {
        var methodActivityName = method.ActivityNameAttributeValue ?? method.Name;

        sb.Append($"    public async {method.ReturnType} {method.Name}(");
        sb.Append(string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}")));
        sb.AppendLine(")");
        sb.AppendLine("    {");

        string inputExp = GetInputExpression(method.Parameters);
        string inputType = GetInputType(method.Parameters);

        // Fix correct call signature
        sb.Append("        ");
        if (method.HasReturnValue)
        {
             sb.Append($"return await _context.CallActivityAsync<{inputType}, {method.UnwrappedReturnType}>(\"{methodActivityName}\", {inputExp});");
        }
        else
        {
             sb.Append($"await _context.CallActivityAsync<{inputType}>(\"{methodActivityName}\", {inputExp});");
        }

        sb.AppendLine();
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string GetInputExpression(IEnumerable<ActivityParameterModel> parameters)
    {
        if (!parameters.Any())
        {
            return "default(global::System.ValueTuple)";
        }
        
        if (parameters.Count() == 1)
        {
            return parameters.First().Name;
        }

        return $"({string.Join(", ", parameters.Select(p => p.Name))})";
    }

    private static string GetInputType(IEnumerable<ActivityParameterModel> parameters)
    {
        if (!parameters.Any())
        {
            return "global::System.ValueTuple";
        }
        
        if (parameters.Count() == 1)
        {
            return parameters.First().Type;
        }

        return $"({string.Join(", ", parameters.Select(p => p.Type))})";
    }
}
