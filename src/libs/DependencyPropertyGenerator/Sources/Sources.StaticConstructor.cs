﻿using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
	public static string GenerateStaticConstructor(
		ClassData @class,
		IReadOnlyCollection<DependencyPropertyData> properties)
	{
		return @$" 
#nullable enable

namespace {@class.Namespace}
{{
    {GenerateModifiers(@class)}partial class {@class.Name}
    {{
        static {@class.Name}()
        {{
{properties.Where(static property => property.IsReadOnly).Select(property => @$"
            {property.Name}Property.OverrideMetadata(
                forType: typeof({@class.Type}),
                {GeneratePropertyMetadata(@class, property)},
                key: {property.Name}PropertyKey);
").Inject()}
{properties.Where(static property => !property.IsReadOnly).Select(property => @$"
            {property.Name}Property.OverrideMetadata(
                forType: typeof({@class.Type}),
                {GeneratePropertyMetadata(@class, property)});
").Inject()}
        }}

{properties.Select(GenerateOnChangedMethods).Inject()}
    }}
}}".RemoveBlankLinesWhereOnlyWhitespaces();
	}

	private static string GenerateAvaloniaStaticConstructorPropertyChanged(
        ClassData @class,
        DependencyPropertyData property)
    {
        var (name, isChanged0, isChanged1, isChanged2, isChanged3) = CheckOnChangedMethods(@class, property);
        if (!isChanged0 &&
            !isChanged1 &&
            !isChanged2 &&
            !isChanged3)
        {
            return string.Empty;
        }

        return property.IsAttached
            ? @$"
            {property.Name}Property.Changed.Subscribe(new global::Avalonia.Reactive.AnonymousObserver<global::Avalonia.AvaloniaPropertyChangedEventArgs<{GenerateType(property)}>>(static x =>
            {{
                {(isChanged0 ? @$"{name}();" : "")}
                {(isChanged1 ? @$"{name}(
                    ({GenerateBrowsableForType(property)})x.Sender);" : "")}
                {(isChanged2 ? @$"{name}(
                    ({GenerateBrowsableForType(property)})x.Sender,
                    ({GenerateType(property)})x.NewValue.GetValueOrDefault());" : "")}
                {(isChanged3 ? @$"{name}(
                    ({GenerateBrowsableForType(property)})x.Sender,
                    ({GenerateType(property)})x.OldValue.GetValueOrDefault(),
                    ({GenerateType(property)})x.NewValue.GetValueOrDefault());" : "")}
            }}));
".RemoveBlankLinesWhereOnlyWhitespaces()
            : @$"
            {property.Name}Property.Changed.Subscribe(new global::Avalonia.Reactive.AnonymousObserver<global::Avalonia.AvaloniaPropertyChangedEventArgs<{GenerateType(property)}>>(static x =>
            {{
                {(isChanged0 ? @$"(({@class.Type})x.Sender).{name}();" : "")}
                {(isChanged1 ? @$"(({@class.Type})x.Sender).{name}(
                    ({GenerateType(property)})x.NewValue.GetValueOrDefault());" : "")}
                {(isChanged2 ? @$"(({@class.Type})x.Sender).{name}(
                    ({GenerateType(property)})x.OldValue.GetValueOrDefault(),
                    ({GenerateType(property)})x.NewValue.GetValueOrDefault());" : "")}
            }}));
".RemoveBlankLinesWhereOnlyWhitespaces();
    }
}