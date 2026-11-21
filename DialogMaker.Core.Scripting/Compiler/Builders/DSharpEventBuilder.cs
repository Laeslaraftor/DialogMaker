using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpEventBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpMemberInfoBuilder(assembly, name, metadataToken), IDSharpEventInfo
    {
        public override IDSharpType DeclaringType { get; } = declaringType;
        public override bool IsDeclaration => DeclaringType?.ObjectType == DSharpObjectType.Interface;
        public DSharpTypeToken? EventType
        {
            get
            {
                if (field == null)
                {
                    if (EventTypeResolver != null)
                    {
                        field = EventTypeResolver();
                        EventTypeResolver = null;
                    }
                    else if (OriginalEvent != null)
                    {
                        field = GetReplacedType(OriginalEvent.EventType);
                    }
                }

                return field;
            }
            set;
        }
        public DSharpMethodBuilder AddMethod
        {
            get
            {
                field ??= GetOrCreateAccessor(AddMethodNameFormat, ref _triedToFindAdd, (name, type) => type.CreateMethod(t => DSharpMethodBuilder.CreateEventAdd(this, name, t)));
                return field;
            }
        }
        public DSharpMethodBuilder RemoveMethod
        {
            get
            {
                field ??= GetOrCreateAccessor(RemoveMethodNameFormat, ref _triedToFindRemove, (name, type) => type.CreateMethod(t => DSharpMethodBuilder.CreateEventRemove(this, name, t)));
                return field;
            }
        }
        public DSharpFieldBuilder? HandlersField { get; private set; }
        public Func<DSharpTypeToken>? EventTypeResolver { get; set; }
        public IDSharpEventInfo? OriginalEvent { get; set; }
        public ReadOnlyCollection<IDSharpEventInfo> ImplementedEvents
        {
            get
            {
                field ??= new(_implementedEvents);
                return field;
            }
        }

        IDSharpType IDSharpEventInfo.EventType
        {
            get
            {
                if (EventType == null)
                {
                    throw new InvalidOperationException($"Event type not specified for \"{this}\"");
                }

                return (IDSharpType)Assembly.GetType(EventType);
            }
        }
        IDSharpMethodInfo IDSharpEventInfo.AddMethod => AddMethod;
        IDSharpMethodInfo IDSharpEventInfo.RemoveMethod => RemoveMethod;
        IDSharpFieldInfo? IDSharpEventInfo.HandlersField => HandlersField;

        private readonly List<IDSharpEventInfo> _implementedEvents = [];
        private bool _triedToFindAdd;
        private bool _triedToFindRemove;

        #region Controls

        internal override void Update()
        {
            base.Update();
            _ = EventType;
        }

        public void AddImplementedEvent(IDSharpEventInfo @event)
        {
            if (@event.DeclaringType.ObjectType != DSharpObjectType.Interface)
            {
                throw new ArgumentException($"Event should be declared in interface: {@event}", nameof(@event));
            }
            if (@event == this)
            {
                throw new ArgumentException($"Event can not implement itself", nameof(@event));
            }
            if (!_implementedEvents.Contains(@event))
            {
                _implementedEvents.Add(@event);
            }
        }
        public bool RemoveImplementedEvent(IDSharpEventInfo @event) => _implementedEvents.Remove(@event);

        public DSharpFieldBuilder CreateHandlersField()
        {
            if (HandlersField != null)
            {
                return HandlersField;
            }
            if (DeclaringType is not DSharpTypeBuilder builder)
            {
                throw new InvalidOperationException("Declaring type should be as builder");
            }

            string name = string.Format(HandlersFieldNameFormat, Name);
            var field = builder.CreateField(name);
            field.Access = DSharpAccessModifier.Private;
            field.FieldType = EventType;

            HandlersField = field;

            return field;
        }

        public override string ToString()
        {
            string result = "event ";

            if (EventType != null)
            {
                result += Assembly.GetType(EventType) + " ";
            }

            return result + Name;
        }

        public IDSharpEventInfo[] GetImplementedEvents() => [.. _implementedEvents];

        private DSharpMethodBuilder GetOrCreateAccessor(string nameFormat, ref bool triedToFind, Func<string, DSharpTypeBuilder, DSharpMethodBuilder> fabric)
        {
            if (DeclaringType is not DSharpTypeBuilder declaringTypeBuilder)
            {
                throw new InvalidOperationException("Declaring type should be as builder");
            }

            var name = string.Format(nameFormat, Name);
            DSharpMethodBuilder? result = null;

            if (!triedToFind)
            {
                triedToFind = true;
                result = declaringTypeBuilder.Methods.FirstOrDefault(m => m.Name == name);
            }
            if (result == null)
            {
                result = fabric(name, declaringTypeBuilder);
                result.Parameters.Add(new(Assembly)
                {
                    Name = "value",
                    TypeGetter = () => EventType
                });
            }

            return result;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Format for event handler add method name
        /// </summary>
        public const string AddMethodNameFormat = "<>{0}_Add";
        /// <summary>
        /// Format for event handler remove method name
        /// </summary>
        public const string RemoveMethodNameFormat = "<>{0}_Remove";
        /// <summary>
        /// Format for event handlers field name
        /// </summary>
        public const string HandlersFieldNameFormat = "<>{0}_Handlers";

        #endregion
    }
}
