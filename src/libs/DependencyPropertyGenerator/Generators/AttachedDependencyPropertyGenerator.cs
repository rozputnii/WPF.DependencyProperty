using DependencyPropertyGenerator.Models;
using H;
using H.Generators.Extensions;
using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;

namespace DependencyPropertyGenerator.Generators;

[Generator]
public class AttachedDependencyPropertyGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Id = "ADPG";

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                hintName: "AttachedDependencyPropertyAttribute.g.cs",
                source: Resources.AttachedDependencyPropertyAttribute_cs.AsString());
        });

        var version = context.DetectVersion();

        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.AttachedDependencyPropertyAttribute")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.AttachedDependencyPropertyAttribute`1")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.AttachedDependencyPropertyAttribute`2")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
    }

    private static (ClassData Class, DependencyPropertyData DependencyProperty)? PrepareData(
        (ClassWithAttributesContext context, string version) tuple)
    {
        var ((_, attributes, classSyntax, classSymbol), version) = tuple;
        if (attributes.FirstOrDefault() is not { } attribute)
        {
            return null;
        }
        
        var classData = classSymbol.GetClassData(version);
        var dependencyPropertyData = attribute.GetDependencyPropertyData(version,
            classSyntax.TryFindAttributeSyntax(attribute), isAttached: true);

        return (classData, dependencyPropertyData);
    }

    private static FileWithName GetSourceCode((ClassData Class, DependencyPropertyData DependencyProperty) data)
    {
        return new FileWithName(
            Name: $"{data.Class.FullName}.AttachedProperties.{data.DependencyProperty.Name}.g.cs",
            Text: Sources.Sources.GenerateAttachedDependencyProperty(data.Class, data.DependencyProperty));
    }

    #endregion
}