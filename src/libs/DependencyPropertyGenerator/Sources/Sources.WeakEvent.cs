using DependencyPropertyGenerator.Models;
using H.Generators.Extensions;

namespace DependencyPropertyGenerator.Sources;

internal static partial class Sources
{
	public static string GenerateWeakEvent(ClassData @class, EventData @event)
	{
		var additionalParameters = string.IsNullOrWhiteSpace(@event.Type)
			? string.Empty
			: $", {GenerateEventArgsType(@event)} args";
		var args = string.IsNullOrWhiteSpace(@event.Type)
			? "System.EventArgs.Empty".WithGlobalPrefix()
			: "args";
		var modifiers = @event.IsAttached
			? " static"
			: string.Empty;


		var source = @event.IsAttached
			? @class.Name
			: $"(source as {@class.Name})!";

		return @$" 
#nullable enable

namespace {@class.Namespace}
{{
    {@class.Modifiers}partial class {@class.Name}
    {{
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        private class {@event.Name}WeakEventManager : global::System.Windows.WeakEventManager
        {{
            private {@event.Name}WeakEventManager()
            {{
            }}

            public static void AddHandler(object? source, {GenerateEventHandlerType(@event)} handler)
            {{
                if (source == null)
                    throw new global::System.ArgumentNullException(nameof(source));
                if (handler == null)
                    throw new global::System.ArgumentNullException(nameof(handler));

                CurrentManager.ProtectedAddHandler(source, handler);
            }}

            public static void RemoveHandler(object? source, {GenerateEventHandlerType(@event)} handler)
            {{
                if (source == null)
                    throw new global::System.ArgumentNullException(nameof(source));
                if (handler == null)
                    throw new global::System.ArgumentNullException(nameof(handler));

                CurrentManager.ProtectedRemoveHandler(source, handler);
            }}

            internal static {@event.Name}WeakEventManager CurrentManager
            {{
                get
                {{
                    var managerType = typeof({@event.Name}WeakEventManager);
                    var manager = ({@event.Name}WeakEventManager)GetCurrentManager(managerType);
                    if (manager == null)
                    {{
                        manager = new {@event.Name}WeakEventManager();
                        SetCurrentManager(managerType, manager);
                    }}

                    return manager;
                }}
            }}

            protected override void StartListening(object? source)
            {{
                {source}.{@event.Name} += On{@event.Name};
            }}

            protected override void StopListening(object? source)
            {{
                {source}.{@event.Name} -= On{@event.Name};
            }}

            internal void On{@event.Name}(object? sender, {GenerateEventArgsType(@event)} args)
            {{
                DeliverEvent(sender, args);
            }}
        }}

{GenerateXmlDocumentationFrom(@event.EventXmlDocumentation, @event)}
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        public{modifiers} event {GenerateEventHandlerType(@event)} {@event.Name}
        {{
            add => {@event.Name}WeakEventManager.AddHandler(null, value);
            remove => {@event.Name}WeakEventManager.RemoveHandler(null, value);
        }}

        /// <summary>
        /// A helper method to raise the {@event.Name} event.
        /// </summary>
{GenerateGeneratedCodeAttribute(@class.Version)}
{GenerateExcludeFromCodeCoverageAttribute()}
        internal{modifiers} void Raise{@event.Name}Event(object? sender{additionalParameters})
        {{
            {@event.Name}WeakEventManager.CurrentManager.On{@event.Name}(sender, {args});
        }}
    }}
}}".RemoveBlankLinesWhereOnlyWhitespaces();
	}

	private static string GenerateEventHandlerType(EventData @event, bool nullable = true, bool nullableType = true)
    {
        var eventHandler = (string.IsNullOrWhiteSpace(@event.Type)
            ? "System.EventHandler"
            : $"System.EventHandler<{GenerateType(@event, nullable: nullableType)}>").WithGlobalPrefix();
        if (nullable)
        {
            eventHandler += "?";
        }

        return eventHandler;
    }
}