using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    private static string GenerateType(DependencyPropertyData property, bool canBeNull = false)
    {
        var value = property.Type;
        if (canBeNull ||
            property is { IsValueType: false, DefaultValue: null })
        {
            value += "?";
        }

        return value;
    }

    private static string GenerateType(EventData @event, bool nullable = true)
    {
        var value = @event.Type;
        if (nullable && !@event.IsValueType)
        {
            value += "?";
        }

        return value;
    }

    private static string GenerateDependencyPropertyName(DependencyPropertyData property)
    {
        if (property is { IsReadOnly: true })
        {
            return $"{property.Name}PropertyKey";
        }

        return $"{property.Name}Property";
    }

    private static string GenerateTypeByPlatform(string name)
    {
        return $"System.Windows.{name}".WithGlobalPrefix();
    }

    private static string GenerateDependencyObjectType()
    {
        return GenerateTypeByPlatform("DependencyObject");
    }

    private static string GenerateDefaultValue(DependencyPropertyData property)
    {
        var type = property.Type;
        if (property is { IsSpecialType: true, DefaultValueDocumentation: { } })
        {
            return $"({type}){property.DefaultValueDocumentation}";
        }

        return property.DefaultValue != null
            ? $"({type}){property.DefaultValue}"
            : $"default({type})";
    }

    private static string GenerateBrowsableForType(DependencyPropertyData property)
    {
        return property.BrowsableForType ?? GenerateDependencyObjectType();
    }

    private static string GenerateBrowsableForTypeParameterName(DependencyPropertyData property)
    {
        return (property.BrowsableForType ?? GenerateDependencyObjectType())
            .ExtractSimpleName()
            .ToParameterName();
    }

    private static string GenerateAdditionalSetterModifier(DependencyPropertyData property)
    {
        return property.IsReadOnly
                ? "protected "
                : string.Empty;
    }

    private static string GeneratePropertyModifier(DependencyPropertyData property)
    {
        if (property is { IsReadOnly: true })
        {
            return "internal";
        }

        return "public";
    }

    private static string GenerateValidatePartialMethod(ClassData @class, DependencyPropertyData property)
    {
        if (!property.Validate)
        {
            return " ";
        }
        
        return $"        private static partial bool Is{property.Name}Valid({GenerateType(property, canBeNull: true)} value);";
    }

    private static string GenerateCreateDefaultValueCallbackPartialMethod(DependencyPropertyData property)
    {
        if (!property.CreateDefaultValueCallback)
        {
            return " ";
        }

        return $"        private static partial {GenerateType(property)} Get{property.Name}DefaultValue();";
    }

    private static string GenerateOnChangedMethodDeclaration(string name, DependencyPropertyData property)
    {
        var modifiers = property.IsAttached ? "static " : string.Empty;

        return $@" 
        {modifiers}partial void {name}(
{(property.IsAttached ? @$" 
            {GenerateBrowsableForType(property)} {GenerateBrowsableForTypeParameterName(property)}," : " ")}
            {GenerateType(property)} oldValue,
            {GenerateType(property)} newValue)".RemoveBlankLinesWhereOnlyWhitespaces();
    }

    private static string GenerateOnChangedMethodCall(string name, DependencyPropertyData property)
    {
        return $@" 
            {name}(
{(property.IsAttached ? @$" 
                {GenerateBrowsableForTypeParameterName(property)}," : " ")}
                oldValue,
                newValue);".RemoveBlankLinesWhereOnlyWhitespaces();
    }

    private static string GenerateBindEventMethod(DependencyPropertyData property)
    {
        if (property.BindEvents.IsEmpty)
        {
            return " ";
        }

        var type = property.Type;
        var sender = property.IsAttached ? GenerateBrowsableForTypeParameterName(property) : "this";

        return $@"
{GenerateOnChangedMethodDeclaration($"On{property.Name}Changed_BeforeBind", property)};
{GenerateOnChangedMethodDeclaration($"On{property.Name}Changed_AfterBind", property)};

{GenerateOnChangedMethodDeclaration($"On{property.Name}Changed", property)}
        {{
{GenerateOnChangedMethodCall($"On{property.Name}Changed_BeforeBind", property)}

            if (oldValue is not default({type}))
            {{
{property.BindEvents.Select(@event => $@" 
                {sender}.{@event} -= On{property.Name}Changed_{@event};
 ").Inject()}
            }}
            if (newValue is not default({type}))
            {{
{property.BindEvents.Select(@event => $@" 
                {sender}.{@event} += On{property.Name}Changed_{@event};
 ").Inject()}
            }}

{GenerateOnChangedMethodCall($"On{property.Name}Changed_AfterBind", property)}
        }}".RemoveBlankLinesWhereOnlyWhitespaces();
    }

    private static string GenerateOptions(DependencyPropertyData property)
    {
        var values = new List<string>();
        if (property.AffectsMeasure)
        {
            values.Add(nameof(property.AffectsMeasure));
        }

        if (property.AffectsArrange)
        {
            values.Add(nameof(property.AffectsArrange));
        }

        if (property.AffectsParentMeasure)
        {
            values.Add(nameof(property.AffectsParentMeasure));
        }

        if (property.AffectsParentArrange)
        {
            values.Add(nameof(property.AffectsParentArrange));
        }

        if (property.AffectsRender)
        {
            values.Add(nameof(property.AffectsRender));
        }

        if (property.Inherits)
        {
            values.Add(nameof(property.Inherits));
        }

        if (property.OverridesInheritanceBehavior)
        {
            values.Add(nameof(property.OverridesInheritanceBehavior));
        }

        if (property.NotDataBindable)
        {
            values.Add(nameof(property.NotDataBindable));
        }

        if (property.DefaultBindingMode == "TwoWay")
        {
            values.Add("BindsTwoWayByDefault");
        }

        if (property.Journal)
        {
            values.Add(nameof(property.Journal));
        }

        if (property.SubPropertiesDoNotAffectRender)
        {
            values.Add(nameof(property.SubPropertiesDoNotAffectRender));
        }

        if (values.Count == 0)
        {
            values.Add("None");
        }

        return string.Join(" | ", values
            .Select(static value => $"global::System.Windows.FrameworkPropertyMetadataOptions.{value}"));
    }

    private static string GeneratePropertyType(ClassData @class, DependencyPropertyData property)
    {
        if (property is { IsReadOnly: true })
        {
            return GenerateTypeByPlatform("DependencyPropertyKey");
        }

        return GenerateTypeByPlatform("DependencyProperty");
    }
    
    private static string GenerateEventArgsType(EventData @event)
    {
        if (string.IsNullOrWhiteSpace(@event.Type))
        {
            return "global::System.EventArgs";
        }

        return GenerateType(@event);
    }
}