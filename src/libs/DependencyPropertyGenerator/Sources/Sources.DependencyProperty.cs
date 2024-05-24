using System.Globalization;
using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    public static string GenerateDependencyProperty(ClassData @class, DependencyPropertyData property)
    {
        return @$"
#nullable enable

namespace {@class.Namespace}
{{
    {@class.Modifiers}partial class {@class.Name}
    {{
{GenerateXmlDocumentationFrom(property.XmlDocumentation, property, isProperty: false)}
{GenerateGeneratedCodeAttribute(@class.Version)}
        {GeneratePropertyModifier(property)} static readonly {GeneratePropertyType(@class, property)} {GenerateDependencyPropertyName(property)} =
            {GenerateDependencyPropertyCreateCall(@class, property)}

{GenerateAdditionalFieldForDirectProperties(property)}
{GenerateAdditionalPropertyForReadOnlyProperties(property)}
{GenerateXmlDocumentationFrom(property.GetterXmlDocumentation, property, isProperty: true)}
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
        public {GenerateType(property)} {property.Name}
        {{
            {GenerateGetter(property)}
            {GenerateSetter(property)}
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
    
    private static string GenerateGetter(DependencyPropertyData property)
    {
        return @$"get => ({GenerateType(property)})GetValue({property.Name}Property);";
    }

    private static string GenerateSetter(DependencyPropertyData property)
    {
        return
            @$"{GenerateAdditionalSetterModifier(property)}set => SetValue({GenerateDependencyPropertyName(property)}, value);";
    }
    
    private static string GenerateDependencyPropertyCreateCall(ClassData @class, DependencyPropertyData property)
    {
        if (property.IsAddOwner)
        {
            return GenerateAddOwnerCreateCall(@class, property);
        }

        return @$"
            {GenerateManagerType(@class)}.{GenerateRegisterMethod(@class, property)}(
                {GenerateRegisterMethodArguments(@class, property)});
 ".RemoveBlankLinesWhereOnlyWhitespaces();
    }

    // https://docs.avaloniaui.net/docs/authoring-controls/defining-properties
    private static string GenerateAvaloniaRegisterMethodArguments(ClassData @class, DependencyPropertyData property)
    {
        var defaultBindingMode = property.DefaultBindingMode is null or "Default"
            ? "OneWay"
            : property.DefaultBindingMode;

        if (property is { IsDirect: true, IsAddOwner: true })
        {
            return $@"
                getter: static sender => sender.{property.Name},
                setter: {(property.IsReadOnly ? "null" : $"static (sender, value) => sender.{property.Name} = value")},
                unsetValue: {GenerateDefaultValue(property)},
                defaultBindingMode: global::Avalonia.Data.BindingMode.{defaultBindingMode},
                enableDataValidation: {(property.EnableDataValidation ? "true" : "false")}";
        }

        if (property.IsDirect)
        {
            return $@"
                name: ""{property.Name}"",
                getter: static sender => sender.{property.Name},
                setter: {(property.IsReadOnly ? "null" : $"static (sender, value) => sender.{property.Name} = value")},
                unsetValue: {GenerateDefaultValue(property)},
                defaultBindingMode: global::Avalonia.Data.BindingMode.{defaultBindingMode},
                enableDataValidation: {(property.EnableDataValidation ? "true" : "false")}";
        }

        if (property.IsAttached)
        {
            return $@"
                name: ""{property.Name}"",
                defaultValue: {GenerateDefaultValue(property)},
                inherits: {(property.Inherits ? "true" : "false")},
                defaultBindingMode: global::Avalonia.Data.BindingMode.{defaultBindingMode},
                validate: {GenerateValidateValueCallback(@class, property)},
                coerce: {GenerateCoerceValueCallback(@class, property)}";
        }

        return @$"
                name: ""{property.Name}"",
                defaultValue: {GenerateDefaultValue(property)},
                inherits: {(property.Inherits ? "true" : "false")},
                defaultBindingMode: global::Avalonia.Data.BindingMode.{defaultBindingMode},
                validate: {GenerateValidateValueCallback(@class, property)},
                coerce: {GenerateCoerceValueCallback(@class, property)}";
    }

    private static string GeneratePropertyMetadata(ClassData @class, DependencyPropertyData property)
    {
        if (property is { IsAddOwner: true, DefaultValue: null })
        {
            return "null";
        }

        var parameterName = property.IsAttached ? "defaultMetadata: " : "typeMetadata: ";

		if (property.DefaultUpdateSourceTrigger == null)
		{
			return $@"{parameterName}new global::System.Windows.FrameworkPropertyMetadata(
                    defaultValue: {GenerateDefaultValue(property)},
                    flags: {GenerateOptions(property)},
                    propertyChangedCallback: {GeneratePropertyChangedCallback(@class, property)},
                    coerceValueCallback: {GenerateCoerceValueCallback(@class, property)},
                    isAnimationProhibited: {property.IsAnimationProhibited.ToString().ToLower(CultureInfo.InvariantCulture)})";
		}

		return $@"{parameterName}new global::System.Windows.FrameworkPropertyMetadata(
                    defaultValue: {GenerateDefaultValue(property)},
                    flags: {GenerateOptions(property)},
                    propertyChangedCallback: {GeneratePropertyChangedCallback(@class, property)},
                    coerceValueCallback: {GenerateCoerceValueCallback(@class, property)},
                    isAnimationProhibited: {property.IsAnimationProhibited.ToString().ToLower(CultureInfo.InvariantCulture)},
                    defaultUpdateSourceTrigger: global::System.Windows.Data.UpdateSourceTrigger.{property.DefaultUpdateSourceTrigger})";

}

	private static string GenerateManagerType(ClassData @class)
    {
        return GenerateTypeByPlatform("DependencyProperty");
    }
    
    private static string GenerateMauiRegisterMethodArguments(ClassData @class, DependencyPropertyData property)
    {
        var defaultBindingMode = property.DefaultBindingMode is null or "Default"
            ? property.IsReadOnly
                ? "OneWayToSource"
                : "OneWay"
            : property.DefaultBindingMode;

        return @$"
                propertyName: ""{property.Name}"",
                returnType: typeof({property.Type}),
                declaringType: typeof({@class.Type}),
                defaultValue: {GenerateDefaultValue(property)},
                defaultBindingMode: global::Microsoft.Maui.Controls.BindingMode.{defaultBindingMode},
                validateValue: {GenerateValidateValueCallback(@class, property)},
                propertyChanged: {GeneratePropertyChangedCallback(@class, property)},
                propertyChanging: {GeneratePropertyChangingCallback(@class, property)},
                coerceValue: {GenerateCoerceValueCallback(@class, property)},
                defaultValueCreator: {GenerateCreateDefaultValueCallbackValueCallback(property)}";
    }

    private static string GenerateRegisterMethodArguments(ClassData @class, DependencyPropertyData property)
    {
	    return @$"
                name: ""{property.Name}"",
                propertyType: typeof({property.Type}),
                ownerType: typeof({@class.Type}),
                {GeneratePropertyMetadata(@class, property)},
                validateValueCallback: {GenerateValidateValueCallback(@class, property)}";

    }

    private static string GenerateRegisterMethod(ClassData @class, DependencyPropertyData property)
    {
        if (property is { IsReadOnly: true })
        {
            return property.IsAttached
                ? "RegisterAttachedReadOnly"
                : "RegisterReadOnly";
        }

        return property.IsAttached
            ? "RegisterAttached"
            : "Register";
    }

    private static string GenerateCoercePartialMethod(DependencyPropertyData property)
    {
        if (!property.Coerce)
        {
            return " ";
        }

        return property.IsAttached
            ? $"        private static partial {GenerateType(property)} Coerce{property.Name}({GenerateBrowsableForType(property)} {GenerateBrowsableForTypeParameterName(property)}, {GenerateType(property, canBeNull: true)} value);"
            : $"        private partial {GenerateType(property)} Coerce{property.Name}({GenerateType(property, canBeNull: true)} value);";
    }

    private static string GenerateAdditionalFieldForDirectProperties(DependencyPropertyData property)
    {
	    return " ";
    }

    private static string GenerateAdditionalPropertyForReadOnlyProperties(DependencyPropertyData property)
    {
        if (!property.IsReadOnly)
        {
            return " ";
        }

        return $@" 
{GenerateXmlDocumentationFrom(property.XmlDocumentation, property, isProperty: false)}
        public static readonly {GenerateTypeByPlatform("DependencyProperty")} {property.Name}Property
            = {GenerateDependencyPropertyName(property)}.DependencyProperty;
";

    }
}