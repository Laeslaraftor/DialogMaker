namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpBytecodeBuilder
    {
        public List<DSharpTypeToken> LocalVariables { get; } = [];
        public List<Instruction> Instructions { get; } = [];

        #region Загрузка и выгрузка значений

        public Instruction Push(DSharpLiteralValue value)
        {
            return CreateInstruction(DSharpBytecodeOperation.Push, value);
        }
        public Instruction Pop()
        {
            return CreateInstruction(DSharpBytecodeOperation.Pop);
        }

        public Instruction LoadLocal(int index)
        {
            return CreateInstruction(DSharpBytecodeOperation.LoadLocal, index);
        }
        public Instruction StoreLocal(int index)
        {
            return CreateInstruction(DSharpBytecodeOperation.StoreLocal, index);
        }
        public Instruction LoadField(DSharpTypeToken token)
        {
            return CreateInstruction(DSharpBytecodeOperation.LoadField, token);
        }
        public Instruction StoreField(DSharpTypeToken token)
        {
            return CreateInstruction(DSharpBytecodeOperation.StoreField, token);
        }
        public Instruction LoadProperty(DSharpTypeToken token)
        {
            return CreateInstruction(DSharpBytecodeOperation.LoadProperty, token);
        }
        public Instruction StoreProperty(DSharpTypeToken token)
        {
            return CreateInstruction(DSharpBytecodeOperation.StoreProperty, token);
        }

        #endregion

        #region Вызовы и переходы

        public Instruction Call(DSharpTypeToken token)
        {
            return CreateInstruction(DSharpBytecodeOperation.Call, token);
        }
        public Instruction Jump(Instruction instruction)
        {
            return CreateInstruction(DSharpBytecodeOperation.Jump, instruction);
        }
        public Instruction JumpIfTrue(Instruction instruction)
        {
            return CreateInstruction(DSharpBytecodeOperation.JumpIfTrue, instruction);
        }
        public Instruction JumpIfFalse(Instruction instruction)
        {
            return CreateInstruction(DSharpBytecodeOperation.JumpIfFalse, instruction);
        }
        public Instruction Return()
        {
            return CreateInstruction(DSharpBytecodeOperation.Return);
        }
        public Instruction Throw()
        {
            return CreateInstruction(DSharpBytecodeOperation.Throw);
        }

        #endregion

        #region Логика

        public Instruction Equals()
        {
            return CreateInstruction(DSharpBytecodeOperation.Equals);
        }
        public Instruction NotEquals()
        {
            return CreateInstruction(DSharpBytecodeOperation.NotEquals);
        }
        public Instruction Or()
        {
            return CreateInstruction(DSharpBytecodeOperation.Or);
        }
        public Instruction And()
        {
            return CreateInstruction(DSharpBytecodeOperation.And);
        }
        public Instruction Less()
        {
            return CreateInstruction(DSharpBytecodeOperation.Less);
        }
        public Instruction LessOrEqual()
        {
            return CreateInstruction(DSharpBytecodeOperation.LessOrEqual);
        }
        public Instruction Greater()
        {
            return CreateInstruction(DSharpBytecodeOperation.Greater);
        }
        public Instruction GreaterOrEqual()
        {
            return CreateInstruction(DSharpBytecodeOperation.GreaterOrEqual);
        }
        public Instruction Not()
        {
            return CreateInstruction(DSharpBytecodeOperation.Not);
        }

        #endregion

        #region Математика

        public Instruction Add()
        {
            return CreateInstruction(DSharpBytecodeOperation.Add);
        }
        public Instruction Subtract()
        {
            return CreateInstruction(DSharpBytecodeOperation.Subtract);
        }
        public Instruction Multiply()
        {
            return CreateInstruction(DSharpBytecodeOperation.Multiply);
        }
        public Instruction Divide()
        {
            return CreateInstruction(DSharpBytecodeOperation.Divide);
        }
        public Instruction Mod()
        {
            return CreateInstruction(DSharpBytecodeOperation.Mod);
        }

        #endregion

        #region Дополнительно

        private Instruction CreateInstruction(params object[] parameters)
        {
            var result = (Instruction)Activator.CreateInstance(typeof(Instruction), parameters);
            Instructions.Add(result);

            return result;
        }

        #endregion

        #region Классы

        public class Instruction(DSharpBytecodeOperation operation)
        {
            public Instruction(DSharpBytecodeOperation operation, int index)
            : this(operation)
            {
                Index = index;
            }
            public Instruction(DSharpBytecodeOperation operation, DSharpTypeToken token)
                : this(operation)
            {
                MetadataToken = token;
            }
            public Instruction(DSharpBytecodeOperation operation, DSharpLiteralValue literalValue)
                : this(operation)
            {
                LiteralValue = literalValue;
            }
            public Instruction(DSharpBytecodeOperation operation, Instruction instruction)
                : this(operation)
            {
                ReferencedInstruction = instruction;
            }

            public DSharpBytecodeOperation Operation { get; } = operation;
            public int Index { get; } = -1;
            public DSharpTypeToken? MetadataToken { get; }
            public DSharpLiteralValue? LiteralValue { get; }
            public Instruction? ReferencedInstruction { get; }
        }

        #endregion
    }
}
