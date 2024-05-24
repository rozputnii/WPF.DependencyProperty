using DependencyPropertyGenerator.Models;
using H;
using H.Generators.Extensions;
using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;

namespace DependencyPropertyGenerator.Generators;

[Generator]
public class WeakEventGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Id = "WEG";

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                hintName: "WeakEventAttribute.g.cs",
                source: Resources.WeakEventAttribute_cs.AsString());
        });

        var version = context.DetectVersion();

        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.WeakEventAttribute")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.WeakEventAttribute`1")
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

        var classData = classSymbol.GetClassData(version);
        var eventData = attribute.GetEventData(isStaticClass: classData.IsStatic);

        return (classData, eventData);
    }

    private static FileWithName GetSourceCode((ClassData Class, EventData Event) data)
    {
        return new FileWithName(
            Name: $"{data.Class.FullName}.WeakEvents.{data.Event.Name}.g.cs",
            Text: Sources.Sources.GenerateWeakEvent(data.Class, data.Event));
    }

    #endregion
}