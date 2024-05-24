using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    private static string GenerateAddOwnerCreateCall(ClassData @class, DependencyPropertyData property)
    {
        return @$"
            {property.FromType}.{property.Name}Property.AddOwner(
                ownerType: typeof({@class.Type}),
                {GeneratePropertyMetadata(@class, property)});
    ".RemoveBlankLinesWhereOnlyWhitespaces();
    }
}