using DependencyPropertyGenerator.Models;
using H;
using H.Generators.Extensions;
using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;

namespace DependencyPropertyGenerator.Generators;

[Generator]
public class RoutedEventGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Id = "REG";

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                hintName: "RoutedEventAttribute.g.cs",
                source: Resources.RoutedEventAttribute_cs.AsString());
            context.AddSource(
                hintName: "RoutedEventStrategy.g.cs",
                source: Resources.RoutedEventStrategy_cs.AsString());
        });

        var version = context.DetectVersion();

        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.RoutedEventAttribute")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.RoutedEventAttribute`1")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
    }

    private static (ClassData Class, EventData Event)? PrepareData(
        (ClassWithAttributesContext context, string version) tuple)
    {
        var ((_, attributes, _, classSymbol), version) = tuple;
        if (attributes.FirstOrDefault() is not { } attribute)
        {
            return null;
        }

        var eventData = attribute.GetEventData(isStaticClass: false);
        
        var classData = classSymbol.GetClassData(version);

        return (classData, eventData);
    }

    private static FileWithName GetSourceCode((ClassData Class, EventData Event) data)
    {
        var category = data.Event.IsAttached
            ? "AttachedEvents"
            : "Events";

        return new FileWithName(
            Name: $"{data.Class.FullName}.{category}.{data.Event.Name}.g.cs",
            Text: Sources.Sources.GenerateRoutedEvent(data.Class, data.Event));
    }

    #endregion
}