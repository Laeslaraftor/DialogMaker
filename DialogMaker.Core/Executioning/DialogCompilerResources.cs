using DialogMaker.Core.Common;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Core.Executioning.Builders;
using System.Collections.Generic;

namespace DialogMaker.Core.Executioning
{
    public class DialogCompilerResources
    {
        private readonly Dictionary<DialogProjectNodePort, DialogExecutionParameter> _variables = [];

        #region Управление

        public bool CreateVariable(DialogProjectNodePort port, DialogExecutionParameter variable)
        {
            if (!_variables.TryAdd(port, variable))
            {
                return false;
            }

            foreach (var connection in port)
            {
                _variables.TryAdd(connection, variable);
            }

            return true;
        }
        public DialogExecutionParameter GetOrCreateVariable(DialogProjectNodePort port)
        {
            if (_variables.TryGetValue(port, out var variable))
            {
                return variable;
            }

            static DialogExecutionParameter GetValue(DialogProjectNodePort port)
            {
                if (port is DialogProjectNodeOutput output)
                {
                    if (port.Node.TryGetResourceValue(output, out var resource))
                    {
                        return new(resource);
                    }
                    if (port.DataType == DialogNodePortType.Action)
                    {
                        return DialogExecutionParameter.Empty;
                    }
                    else if (port.DataType == DialogNodePortType.String)
                    {
                        return new(string.Empty);
                    }

                    return new(0);
                }
                if (port is IValuePort valuePort &&
                    valuePort.CanPresetValue)
                {
                    var value = valuePort.Value;

                    if (value is IResourceItem resource)
                    {
                        return new(resource);
                    }

                    return new(new OperandValue(value));
                }

                return DialogExecutionParameter.Empty;
            }

            variable = DialogExecutionParameter.Empty;

            foreach (var connection in port)
            {
                variable = GetValue(connection);

                if (variable != DialogExecutionParameter.Empty)
                {
                    break;
                }
            }

            if (variable == DialogExecutionParameter.Empty)
            {
                variable = GetValue(port);
            }

            _variables.TryAdd(port, variable);

            foreach (var connection in port)
            {
                _variables.TryAdd(connection, variable);
            }

            return variable;
        }

        #endregion
    }
}
