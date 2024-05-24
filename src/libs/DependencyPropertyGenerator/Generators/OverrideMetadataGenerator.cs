using System.Collections.Immutable;
using DependencyPropertyGenerator.Models;
using H;
using H.Generators.Extensions;
using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;

namespace DependencyPropertyGenerator.Generators;

[Generator]
public class OverrideMetadataGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Id = "OMG";

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                hintName: "OverrideMetadataAttribute.g.cs",
                source: Resources.OverrideMetadataAttribute_cs.AsString());
        });

        var version = context.DetectVersion();

        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.OverrideMetadataAttribute")
            .SelectAllAttributes()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.OverrideMetadataAttribute`1")
            .SelectAllAttributes()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
    }

    private static (ClassData Class, EquatableArray<DependencyPropertyData> OverrideMetada)? PrepareData(
        (ClassWithAttributesContext context, string version) tuple)
    {
        var ((_, attributes, _, classSymbol), version) = tuple;
     
        var classData = classSymbol.GetClassData(version);
        var overrideMetadata = attributes
            .Select(attribute => attribute.GetDependencyPropertyData(version))
            .ToImmutableArray()
            .AsEquatableArray();

        return (classData, overrideMetadata);
    }

    private static FileWithName GetSourceCode(
        (ClassData Class, EquatableArray<DependencyPropertyData> OverrideMetada) data)
    {
	    var name = $"{data.Class.FullName}.StaticConstructor.g.cs";
	    var text = Sources.Sources.GenerateStaticConstructor(data.Class, data.OverrideMetada.AsImmutableArray());
           
        return new FileWithName(Name: name, Text: text);
    }

    #endregion
}