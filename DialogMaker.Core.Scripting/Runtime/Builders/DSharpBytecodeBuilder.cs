using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

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
            return CreateInstruction<LiteralInstruction>(this, DSharpBytecodeOperation.Push, value);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Push"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TypeInstruction Push(DSharpTypeToken type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.Push, type);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.PopRepeat"/>
        /// </summary>
        /// <param name="count">Repeat count</param>
        /// <returns></returns>
        public IndexInstruction PopRepeat(int count)
        {
            return CreateInstruction<IndexInstruction>(this, DSharpBytecodeOperation.PopRepeat, count);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopOffsetRepeat"/>
        /// </summary>
        /// <param name="offset">Popped item offset</param>
        /// <param name="count">Repeat count</param>
        /// <returns></returns>
        public OffsetCountInstruction PopOffsetRepeat(int offset, int count)
        {
            return CreateInstruction<OffsetCountInstruction>(this, DSharpBytecodeOperation.PopOffsetRepeat, offset, count);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.New"/>
        /// </summary>
        /// <param name="type">Type of object that needs to instantiate</param>
        /// <returns></returns>
        public Instruction New(IDSharpMethodInfo constructor)
        {
            if (constructor.DeclaringType == null)
            {
                throw new ArgumentException($"Constructor must contains declaring type: {constructor}", nameof(constructor));
            }
            if (!constructor.DeclaringType.GetConstructors().Contains(constructor))
            {
                throw new ArgumentException($"Provided method is not constructor: {constructor}", nameof(constructor));
            }

            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.New, constructor);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.New"/>
        /// </summary>
        /// <param name="type">Type of object that needs to instantiate</param>
        /// <returns></returns>
        public Instruction New(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.New, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NewArray"/>
        /// </summary>
        /// <param name="type">Type of array items that needs to instantiate</param>
        /// <returns></returns>
        public TypeInstruction NewArray(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.NewArray, type);
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
        /// Auto load for specified member.
        /// </summary>
        /// <param name="propertyOrField">Property or field</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TypeInstruction LoadPropertyOrField(IDSharpMemberInfo propertyOrField)
        {
            if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanRead)
                {
                    throw new InvalidOperationException($"Unable to read value from property because it have not getter: \"{property}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    return LoadProperty(property);
                }

                return LoadInstanceProperty(property);
            }
            else if (propertyOrField is IDSharpFieldInfo field)
            {
                if (propertyOrField.IsStatic)
                {
                    return LoadField(field);
                }

                return LoadInstanceField(field);
            }

            throw new ArgumentException($"Expected property or field, got: \"{propertyOrField}\"", nameof(propertyOrField));
        }
        /// <summary>
        /// Auto store for specified member.
        /// </summary>
        /// <param name="propertyOrField">Property or field</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TypeInstruction StorePropertyOrField(IDSharpMemberInfo propertyOrField)
        {
            if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanWrite)
                {
                    throw new InvalidOperationException($"Unable to write value to property because it have not setter: \"{property}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    return StoreProperty(property);
                }

                return StoreInstanceProperty(property);
            }
            else if (propertyOrField is IDSharpFieldInfo field)
            {
                if (propertyOrField.IsStatic)
                {
                    return StoreField(field);
                }

                return StoreInstanceField(field);
            }

            throw new ArgumentException($"Expected property or field, got: \"{propertyOrField}\"", nameof(propertyOrField));
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
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstance"/>
        /// </summary>
        /// <returns></returns>
        public Instruction LoadInstance()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.LoadInstance);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitCall"/>
        /// </summary>
        /// <param name="method">Method or function that needs to call</param>
        /// <returns></returns>
        public TypeInstruction AwaitCall(IDSharpMethodInfo method)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.AwaitCall, method);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitCallInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction AwaitCallInstance(IDSharpMethodInfo method)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.AwaitCallInstance, method);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.Empty"/>
        /// </summary>
        /// <param name="forceNew">Create new instruction without checking last</param>
        /// <returns></returns>
        public Instruction Empty(bool forceNew = false)
        {
            if (!forceNew && Instructions.Count > 0 &&
                Instructions[^1].Operation == DSharpBytecodeOperation.Empty)
            {
                return Instructions[^1];
            }

            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Empty);
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
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Increment"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Increment()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Increment);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Decrement"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Decrement()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Decrement);
        }

        #endregion

        #region Дополнительно

        /// <summary>
        /// Resolve expression type
        /// </summary>
        /// <param name="value">Value that contains type info or name</param>
        /// <returns>Resolved type</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IDSharpType? ExpressionTypeResolver(object value)
        {
            string? variableName = null;

            if (value is string str)
            {
                variableName = str;
            }
            else if (value is IdentifierExpressionNode identifier)
            {
                variableName = identifier.GetName(false);
            }
            if (variableName != null)
            {
                var variable = LocalVariables.FirstOrDefault(v => v.Name == variableName);
                
                if (variable.Type == null)
                {
                    throw new InvalidOperationException($"Type of local variable ({variableName}) not specified in {Method}");
                }

                return Method.Assembly.GetType(variable.Type) as IDSharpType;
            }

            return null;
        }

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
