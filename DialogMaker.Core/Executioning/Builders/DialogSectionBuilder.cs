using Acly;
using System;

namespace DialogMaker.Core.Executioning.Builders
{
    public class DialogSectionBuilder
    {
        internal DialogSectionBuilder(DialogCodeBuilder codeBuilder)
        {
            CodeBuilder = codeBuilder;
            Operations = new(_operations);
        }

        public DialogCodeBuilder CodeBuilder { get; }
        public int Index
        {
            get => CodeBuilder.IndexOf(this);
            set { }
        }
        public ReferenceReadOnlyList<OperationBuilder> Operations { get; }

        private readonly ObservableList<OperationBuilder> _operations = [];

        #region Управление

        public OperationBuilder CreateOperation(DialogByteCode code)
        {
            OperationBuilder result = new(this, code);
            _operations.Add(result);

            return result;
        }
        public bool RemoveOperation(OperationBuilder operation)
        {
            return _operations.Remove(operation); 
        }

        public int GetOperationByteCodeIndex(OperationBuilder operation)
        {
            if (!_operations.Contains(operation))
            {
                throw new ArgumentException($"Данная секция не содержит {operation}!", nameof(operation));
            }

            int count = 0;
            int argumentSize = Operation.ArgumentSize;
            int instructionSize = Operation.InstructionSize;

            foreach (var builder in Operations)
            {
                if (builder == operation)
                {
                    return count;
                }

                // Количество аргументов * размер аргумента + размер инструкции
                count = (argumentSize * builder.Arguments.Length) + instructionSize;
            }

            throw new ArgumentException($"Не удалось получить позицию кода для {operation}");
        }

        public void Clear()
        {
            _operations.Clear();
        }

        public override string ToString()
        {
            return $"Сегмент {Index}";
        }

        internal void Compile(CodeCompileContext context)
        {
            foreach (var operation in _operations)
            {
                context.CodeStream.WriteByte((byte)operation.Code);

                foreach (var arg in operation.Arguments)
                {
                    int value = arg.AddToContext(context.ContextBuilder);
                    var data = BitConverter.GetBytes(value);

                    context.CodeStream.Write(data, 0, data.Length);
                }
            }
        }

        #endregion
    }
}
