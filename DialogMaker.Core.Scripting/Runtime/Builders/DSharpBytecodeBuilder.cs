namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    /// <summary>
    /// Class that designed for building bytecode for methods and functions
    /// </summary>
    /// <param name="method">Method or function that contains this bytecode</param>
    public partial class DSharpBytecodeBuilder(DSharpMethodBuilder method)
    {
        /// <summary>
        /// Method or function that contains this bytecode
        /// </summary>
        public DSharpMethodBuilder Method { get; } = method;
        /// <summary>
        /// Local variables of this bytecode
        /// </summary>
        public List<DSharpMethodBuilderParameter> LocalVariables { get; } = [];
        /// <summary>
        /// List of all instructions
        /// </summary>
        public List<Instruction> Instructions { get; } = [];

        #region Управление

        /// <summary>
        /// Write bytecode to stream
        /// </summary>
        /// <param name="stream">Stream for writing built bytecode</param>
        public void Write(Stream stream)
        {
        }

        #endregion

        #region Загрузка и выгрузка значений

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Push"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Instruction Push(DSharpLiteralValue value)
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Push, value);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Pop"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Pop()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Pop);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopOffset"/>
        /// </summary>
        /// <param name="index">Popped item offset</param>
        /// <returns></returns>
        public IndexInstruction PopOffset(int index)
        {
            return CreateInstruction<IndexInstruction>(this, DSharpBytecodeOperation.PopOffset, index);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.New"/>
        /// </summary>
        /// <param name="type">Type of object that needs to instantiate</param>
        /// <returns></returns>
        public TypeInstruction New(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.New, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NewArray"/>
        /// </summary>
        /// <param name="type">Type of array items that needs to instantiate</param>
        /// <param name="size">Size of array</param>
        /// <returns></returns>
        public SizedTypeInstruction NewArray(IDSharpType type, int size)
        {
            return CreateInstruction<SizedTypeInstruction>(this, DSharpBytecodeOperation.New, type, size);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadArgument"/>
        /// </summary>
        /// <param name="parameter">Argument of method or function to load to stack</param>
        /// <returns></returns>
        public ParameterInstruction LoadArgument(DSharpMethodBuilderParameter parameter)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.LoadArgument, parameter);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreArgument"/>
        /// </summary>
        /// <param name="parameter">Argument of method or function for writing value from stack</param>
        /// <returns></returns>
        public ParameterInstruction StoreArgument(DSharpMethodBuilderParameter parameter)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.StoreArgument, parameter);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadLocal"/>
        /// </summary>
        /// <param name="parameter">Local variable to load to stack</param>
        /// <returns></returns>
        public ParameterInstruction LoadLocal(DSharpMethodBuilderParameter parameter)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.LoadLocal, parameter);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreLocal"/>
        /// </summary>
        /// <param name="parameter">Local variable for writing value from stack</param>
        /// <returns></returns>
        public ParameterInstruction StoreLocal(DSharpMethodBuilderParameter parameter)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.StoreLocal, parameter);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadField"/>
        /// </summary>
        /// <param name="member">Field to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadField(IDSharpFieldInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstanceField"/>
        /// </summary>
        /// <param name="member">Field to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadInstanceField(IDSharpFieldInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadInstanceField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreField"/>
        /// </summary>
        /// <param name="member">Field for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreField(IDSharpFieldInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreInstanceField"/>
        /// </summary>
        /// <param name="member">Field for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreInstanceField(IDSharpFieldInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreInstanceField, member);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadProperty"/>
        /// </summary>
        /// <param name="member">Property to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadProperty(IDSharpPropertyInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadInstanceProperty(IDSharpPropertyInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadInstanceProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreProperty(IDSharpPropertyInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreInstanceProperty(IDSharpPropertyInfo member)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreInstanceProperty, member);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadArrayItem"/>
        /// </summary>
        /// <returns></returns>
        public Instruction LoadArrayItem()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.LoadArrayItem);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreArrayItem"/>
        /// </summary>
        /// <returns></returns>
        public Instruction StoreArrayItem()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.StoreArrayItem);
        }

        #endregion

        #region Вызовы и переходы

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Call"/>
        /// </summary>
        /// <param name="method">Method or function that needs to call</param>
        /// <returns></returns>
        public TypeInstruction Call(IDSharpMethodInfo method)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.Call, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.CallInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction CallInstance(IDSharpMethodInfo method)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.CallInstance, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Jump"/>
        /// </summary>
        /// <param name="instruction">Instruction to jumping</param>
        /// <returns></returns>
        public ReferenceInstruction Jump(Instruction? instruction = null)
        {
            var result = CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.Jump);

            if (instruction != null)
            {
                result.ReferencedInstruction = instruction;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.JumpIfTrue"/>
        /// </summary>
        /// <param name="instruction">Instruction to jumping</param>
        /// <returns></returns>
        public ReferenceInstruction JumpIfTrue(Instruction? instruction = null)
        {
            var result = CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.JumpIfTrue);

            if (instruction != null)
            {
                result.ReferencedInstruction = instruction;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.JumpIfFalse"/>
        /// </summary>
        /// <param name="instruction">Instruction to jumping</param>
        /// <returns></returns>
        public ReferenceInstruction JumpIfFalse(Instruction? instruction = null)
        {
            var result = CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.JumpIfFalse);

            if (instruction != null)
            {
                result.ReferencedInstruction = instruction;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Return"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Return()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Return);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Throw"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Throw()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Throw);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Await"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Await()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Await);
        }

        #endregion

        #region Логика

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Equals"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Equals()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Equals);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NotEquals"/>
        /// </summary>
        /// <returns></returns>
        public Instruction NotEquals()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.NotEquals);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Or"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Or()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Or);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.And"/>
        /// </summary>
        /// <returns></returns>
        public Instruction And()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.And);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Less"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Less()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Less);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LessOrEqual"/>
        /// </summary>
        /// <returns></returns>
        public Instruction LessOrEqual()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.LessOrEqual);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Greater"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Greater()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Greater);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GreaterOrEqual"/>
        /// </summary>
        /// <returns></returns>
        public Instruction GreaterOrEqual()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.GreaterOrEqual);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Not"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Not()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Not);
        }

        #endregion

        #region Математика

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Add"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Add()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Add);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Subtract"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Subtract()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Subtract);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Multiply"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Multiply()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Multiply);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Divide"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Divide()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Divide);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Mod"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Mod()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Mod);
        }

        #endregion

        #region Дополнительно

        private T CreateInstruction<T>(params object[] parameters)
            where T : Instruction
        {
            var result = (T)Activator.CreateInstance(typeof(T), parameters);
            Instructions.Add(result);

            return result;
        }

        #endregion
    }
}
