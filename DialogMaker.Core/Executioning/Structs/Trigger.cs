using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Executioning
{
    public readonly struct Trigger : IEquatable<Trigger>
    {
        private Trigger(TriggerMetadata metadata, DialogExecutionContext context, IDictionary<string, object> parameters)
        {
            Parameters = new(parameters);
            _metadata = metadata;
            _context = context;
        }

        public string Id => _metadata.Id;
        public ReadOnlyDictionary<string, object> Parameters { get; }
        public ICollection<string> OutputKeys => _metadata.Outputs.Keys;

        private readonly TriggerMetadata _metadata;
        private readonly DialogExecutionContext _context;

        #region Управление

        public void SetOutput(string key, OperandValue value)
        {
            _context.Resources.SetValue(_metadata.Outputs[key], value);
        }
        public void SetOutput(string key, IResourceItem resource)
        {
            _context.Resources.SetValue(_metadata.Outputs[key], resource);
        }

        public bool Equals(Trigger other)
        {
            return Id == other.Id &&
                   Parameters == other.Parameters &&
                   OutputKeys == other.OutputKeys;
        }
        public override bool Equals(object obj)
        {
            return obj is Trigger other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Parameters, OutputKeys, _metadata, _context);
        }
        public override string ToString()
        {
            return Id;
        }

        #endregion

        #region Дополнительно

        public OperandValue GetOperandValue(string key)
        {
            TryGetOperandValue(key, out var result);
            return result;
        }
        public bool GetBool(string key)
        {
            return GetOperandValue(key).ToBool();
        }
        public float GetNumber(string key)
        {
            return GetOperandValue(key).ToNumber();
        }
        public string GetString(string key)
        {
            return GetOperandValue(key).ToString();
        }
        public bool TryGetOperandValue(string key, [NotNullWhen(true)] out OperandValue result)
        {
            return TryGetValue(key, out result);
        }
        public bool TryGetResource<T>(string key, [NotNullWhen(true)] out T? result)
            where T : IResourceItem
        {
            return TryGetValue(key, out result);
        }

        public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? result)
        {
            result = default;

            if (Parameters != null &&
                Parameters.TryGetValue(key, out var value) &&
                value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            return false;
        }

        #endregion

        #region Статика

        internal static Trigger Create(TriggerMetadata metadata, DialogExecutionContext context)
        {
            Dictionary<string, object> parameters = [];

            foreach (var input in metadata.Inputs)
            {
                var resource = context.Resources.GetResource(input.Value);
                object parameterValue = resource;

                if (resource is IVariable variable)
                {
                    parameterValue = variable.Value;
                }

                parameters.Add(input.Key, parameterValue);
            }

            return new(metadata, context, parameters);
        }

        #endregion
    }
}
