using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Models;

public readonly record struct ClassData(
    string Namespace,
    string Name,
    string FullName,
    string Type,
    string Modifiers,
    string Version,
    bool IsStatic,
    EquatableArray<string> Methods);