﻿using System.ComponentModel;
using System.Security;
using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    private static string GenerateAttribute(string name)
    {
        return $"        [global::{name}]";
    }
    
    private static string GenerateAttribute(string name, string? value)
    {
        if (value == null)
        {
            return " ";
        }

        return $"        [global::{name}({value})]";
    }

    private static string GenerateComponentModelAttribute(string name, string? value)
    {
        return GenerateAttribute($"System.ComponentModel.{name}", value);
    }

    private static string GenerateCategoryAttribute(string? value)
    {
        if (value == null)
        {
            return " ";
        }

        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.Category),
            $"\"{value}\"");
    }

    private static string GenerateDescriptionAttribute(string? value)
    {
        if (value == null)
        {
            return " ";
        }

        var isMultilineString =
            value.Contains('\r') ||
            value.Contains('\n');

        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.Description),
            isMultilineString
                ? $"@\"{SecurityElement.Escape(value)}\""
                : $"\"{SecurityElement.Escape(value)}\"");
    }

    private static string GenerateTypeConverterAttribute(string? value)
    {
        if (value == null)
        {
            return " ";
        }

        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.TypeConverter),
            $"typeof({value.WithGlobalPrefix()})");
    }

    private static string ToBooleanKeyword(this bool value)
    {
        return value
            ? "true"
            : "false";
    }

    private static string GenerateBindableAttribute(bool? value)
    {
        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.Bindable),
            value?.ToBooleanKeyword());
    }

    private static string GenerateBrowsableAttribute(bool? value)
    {
        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.Browsable),
            value?.ToBooleanKeyword());
    }

    private static string GenerateDesignerSerializationVisibilityAttribute(string? value)
    {
        if (value == null)
        {
            return " ";
        }

        return GenerateComponentModelAttribute(
            nameof(DependencyPropertyData.DesignerSerializationVisibility),
            $"global::System.ComponentModel.{nameof(DesignerSerializationVisibility)}.{value}");
    }

    private static string GenerateClsCompliantAttribute(bool? value)
    {
        return GenerateAttribute("System.CLSCompliant", value?.ToBooleanKeyword());
    }

    private static string GenerateGeneratedCodeAttribute(string version)
    {
        return GenerateAttribute("System.CodeDom.Compiler.GeneratedCode", $"\"DependencyPropertyGenerator\", \"{version}\"");
    }

    private static string GenerateExcludeFromCodeCoverageAttribute()
    {
        return GenerateAttribute("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage");
    }
    
    private static string GenerateLocalizabilityAttribute(string? value)
    {
        if (value == null)
        {
            return " ";
        }

        return GenerateAttribute(
            "System.Windows.Localizability",
            $"global::System.Windows.LocalizationCategory.{value}");
    }

    private static string GenerateBrowsableForTypeAttribute(DependencyPropertyData property)
    {
        return GenerateAttribute(
            "System.Windows.AttachedPropertyBrowsableForType",
            $"typeof({GenerateBrowsableForType(property)})");
    }
}