using DependencyPropertyGenerator.Models;
using H;
using H.Generators.Extensions;
using H.Generators.Extensions.Models;
using Microsoft.CodeAnalysis;

namespace DependencyPropertyGenerator.Generators;

[Generator]
public class AddOwnerGenerator : IIncrementalGenerator
{
    #region Constants

    private const string Id = "AOG";

    #endregion

    #region Methods

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                hintName: "AddOwnerAttribute.g.cs",
                source: Resources.AddOwnerAttribute_cs.AsString());
        });

        var version = context.DetectVersion();

        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.AddOwnerAttribute")
            .SelectManyAllAttributesOfCurrentClassSyntax()
            .Combine(version)
            .SelectAndReportExceptions(PrepareData, context, Id)
            .WhereNotNull()
            .SelectAndReportExceptions(GetSourceCode, context, Id)
            .AddSource(context);
        context.SyntaxProvider
            .ForAttributeWithMetadataNameOfClassesAndRecords("DependencyPropertyGenerator.AddOwnerAttribute`2")
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
        var ((_, attributes, _, classSymbol), version) = tuple;
        if (attributes.FirstOrDefault() is not { } attribute)
        {
            return null;
        }

        var classData = classSymbol.GetClassData(version);
        var dependencyPropertyData = attribute.GetDependencyPropertyData(version, isAddOwner: true);

        return (classData, dependencyPropertyData);
    }

    private static FileWithName GetSourceCode((ClassData Class, DependencyPropertyData DependencyProperty) data)
    {
        return new FileWithName(
            Name: $"{data.Class.FullName}.AddOwner.{data.DependencyProperty.Name}.g.cs",
            Text: Sources.Sources.GenerateDependencyProperty(data.Class, data.DependencyProperty));
    }

    #endregion
}