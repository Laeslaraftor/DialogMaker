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
                if (port is DialogProjectNodeOutput output &&
                    port.Node.TryGetResourceValue(output, out var resource))
                {
                    return new(resource);
                }
                if (port is IValuePort valuePort && 
                    valuePort.CanPresetValue)
                {
                    return new(new OperandValue(valuePort.Value));
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
