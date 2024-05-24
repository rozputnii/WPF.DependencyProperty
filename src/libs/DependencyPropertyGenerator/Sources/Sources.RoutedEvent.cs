using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
    public static string GenerateRoutedEvent(ClassData @class, EventData @event)
    {
        if (@event.IsAttached)
        {
            return GenerateAttachedRoutedEvent(@class, @event);
        }
        
        
	    return @$" 
#nullable enable

namespace {@class.Namespace}
{{
    {@class.Modifiers}partial class {@class.Name}
    {{
{GenerateXmlDocumentationFrom(@event.XmlDocumentation, @event)}
{GenerateGeneratedCodeAttribute(@class.Version)}
        public static readonly {GenerateRoutedEventType(@class)} {@event.Name}Event =
            {GenerateEventManagerType(@class)}.{GenerateRegisterMethod(@class)}(
                {GenerateRegisterRoutedEventMethodArguments(@class, @event)});

{GenerateXmlDocumentationFrom(@event.EventXmlDocumentation, @event)}
{GenerateCategoryAttribute(@event.Category)}
{GenerateDescriptionAttribute(@event.Description)}
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        public event {GenerateRouterEventType(@class, @event)} {@event.Name}
        {{
            add => AddHandler({@event.Name}Event, value);
            remove => RemoveHandler({@event.Name}Event, value);
        }}

        /// <summary>
        /// A helper method to raise the {@event.Name} event.
        /// </summary>
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        protected {GenerateRoutedEventArgsType(@class)} On{@event.Name}()
        {{
            var args = new {GenerateRoutedEventArgsType(@class)}({@event.Name}Event);
            this.RaiseEvent(args);

            return args;
        }}
    }}
}}".RemoveBlankLinesWhereOnlyWhitespaces();
    }
    
    public static string GenerateAttachedRoutedEvent(ClassData @class, EventData @event)
    {
        return @$"
#nullable enable

namespace {@class.Namespace}
{{
    public{@class.Modifiers} partial class {@class.Name}
    {{
{GenerateXmlDocumentationFrom(@event.XmlDocumentation, @event)}
        public static readonly {GenerateRoutedEventType(@class)} {@event.Name}Event =
            {GenerateEventManagerType(@class)}.{GenerateRegisterMethod(@class)}(
                {GenerateRegisterRoutedEventMethodArguments(@class, @event)});

{GenerateXmlDocumentationFrom(@event.EventXmlDocumentation, @event)}
{GenerateCategoryAttribute(@event.Category)}
{GenerateDescriptionAttribute(@event.Description)}
        public static void Add{@event.Name}Handler({GenerateDependencyObjectType()} element, {GenerateRoutedEventHandlerType(@class)} handler)
        {{
            element = element ?? throw new global::System.ArgumentNullException(nameof(element));

            if (element is {GenerateTypeByPlatform("UIElement")} uiElement)
            {{
                uiElement.AddHandler({@event.Name}Event, handler);
            }}
            else if (element is {GenerateTypeByPlatform("ContentElement")} contentElement)
            {{
                contentElement.AddHandler({@event.Name}Event, handler);
            }}
        }}

{GenerateXmlDocumentationFrom(@event.EventXmlDocumentation, @event)}
{GenerateCategoryAttribute(@event.Category)}
{GenerateDescriptionAttribute(@event.Description)}
        public static void Remove{@event.Name}Handler({GenerateDependencyObjectType()} element, {GenerateRoutedEventHandlerType(@class)} handler)
        {{
            element = element ?? throw new global::System.ArgumentNullException(nameof(element));

            if (element is {GenerateTypeByPlatform("UIElement")} uiElement)
            {{
                uiElement.RemoveHandler({@event.Name}Event, handler);
            }}
            else if (element is {GenerateTypeByPlatform("ContentElement")} contentElement)
            {{
                contentElement.RemoveHandler({@event.Name}Event, handler);
            }}
        }}
    }}
}}".RemoveBlankLinesWhereOnlyWhitespaces();
    }

    private static string GenerateRouterEventType(ClassData @class, EventData @event)
    {
        if (string.IsNullOrWhiteSpace(@event.Type))
        {
            return GenerateRoutedEventHandlerType(@class);
        }

        return @event.Type.WithGlobalPrefix();
    }

    private static string GenerateRoutedEventType(ClassData @class)
    {
        return GenerateTypeByPlatform("RoutedEvent");
    }

    private static string GenerateRoutedEventArgsType(ClassData @class)
    {
        return GenerateTypeByPlatform("RoutedEventArgs");
    }

    private static string GenerateRoutedEventHandlerType(ClassData @class)
    {
        return GenerateTypeByPlatform("RoutedEventHandler");
    }
    
    private static string GenerateRegisterRoutedEventMethodArguments(ClassData @class, EventData @event)
    {
        return @$"
                name: ""{@event.Name}"",
                routingStrategy: {GenerateRoutingStrategyType(@class)}.{@event.Strategy},
                handlerType: typeof({GenerateRouterEventType(@class, @event)}),
                ownerType: typeof({@class.Type})";
    }

    private static string GenerateRoutingStrategyType(ClassData @class)
    {
        return GenerateTypeByPlatform("RoutingStrategy");
    }

    private static string GenerateEventManagerType(ClassData @class)
    {
        return GenerateTypeByPlatform("EventManager");
    }

    private static string GenerateRegisterMethod(ClassData @class)
    {
        return "RegisterRoutedEvent";
    }
}