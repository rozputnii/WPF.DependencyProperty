﻿using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    public static string GenerateAttachedDependencyProperty(ClassData @class, DependencyPropertyData property)
    {
        return @$"
#nullable enable

namespace {@class.Namespace}
{{
    {GenerateModifiers(@class)}partial class {@class.Name}{GenerateBaseType(@class)}
    {{
{GenerateXmlDocumentationFrom(property.XmlDocumentation, property, isProperty: false)}
{GenerateGeneratedCodeAttribute(@class.Version)}
        {GeneratePropertyModifier(property)} static readonly {GeneratePropertyType(@class, property)} {GenerateDependencyPropertyName(property)} =
            {GenerateManagerType(@class)}.{GenerateRegisterMethod(@class, property)}(
                {GenerateRegisterAttachedMethodArguments(@class, property)});

{GenerateAdditionalPropertyForReadOnlyProperties(property)}
{GenerateXmlDocumentationFrom(property.SetterXmlDocumentation, property, isProperty: true)}
{GenerateCategoryAttribute(property.Category)}
{GenerateDescriptionAttribute(property.Description)}
{GenerateTypeConverterAttribute(property.TypeConverter)}
{GenerateBindableAttribute(property.Bindable)}
{GenerateBrowsableAttribute(property.Browsable)}
{GenerateDesignerSerializationVisibilityAttribute(property.DesignerSerializationVisibility)}
{GenerateClsCompliantAttribute(property.ClsCompliant)}
{GenerateLocalizabilityAttribute(property.Localizability)}
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        {(property.IsReadOnly ? "internal" : "public")} static void Set{property.Name}({GenerateDependencyObjectType()} element, {GenerateType(property)} value)
        {{
            element = element ?? throw new global::System.ArgumentNullException(nameof(element));

            element.SetValue({GenerateDependencyPropertyName(property)}, value);
        }}

{GenerateXmlDocumentationFrom(property.GetterXmlDocumentation, property, isProperty: true)}
{GenerateCategoryAttribute(property.Category)}
{GenerateDescriptionAttribute(property.Description)}
{GenerateTypeConverterAttribute(property.TypeConverter)}
{GenerateBindableAttribute(property.Bindable)}
{GenerateBrowsableAttribute(property.Browsable)}
{GenerateDesignerSerializationVisibilityAttribute(property.DesignerSerializationVisibility)}
{GenerateBrowsableForTypeAttribute(property)}
{GenerateClsCompliantAttribute(property.ClsCompliant)}
{GenerateLocalizabilityAttribute(property.Localizability)}
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        public static {GenerateType(property)} Get{property.Name}({GenerateDependencyObjectType()} element)
        {{
            element = element ?? throw new global::System.ArgumentNullException(nameof(element));

            return ({GenerateType(property)})element.GetValue({property.Name}Property);
        }}

{GenerateOnChangedMethods(property)}
{GenerateOnChangingMethods(property)}
{GenerateCoercePartialMethod(property)}
{GenerateValidatePartialMethod(@class, property)}
{GenerateCreateDefaultValueCallbackPartialMethod(property)}
{GenerateBindEventMethod(property)}
    }}
}}".RemoveBlankLinesWhereOnlyWhitespaces();
    }
    
    private static string GenerateRegisterAttachedMethodArguments(ClassData @class, DependencyPropertyData property)
    {
	    return @$"
                name: ""{property.Name}"",
                propertyType: typeof({property.Type}),
                ownerType: typeof({@class.Type}),
                {GeneratePropertyMetadata(@class, property)},
                validateValueCallback: {GenerateValidateValueCallback(@class, property)}";

    }

    private static string GenerateModifiers(ClassData @class)
    {
        return @class.Modifiers;
    }

    private static string GenerateBaseType(ClassData @class)
    {
        return string.Empty;
    }
}