using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Executor;
using System.Data.Common;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Class that designed for building bytecode for methods and functions
    /// </summary>
    /// <param name="method">Method or function that contains this bytecode</param>
    public partial class DSharpBytecodeBuilder(DSharpMethodBuilder method) : IDSharpMethodBytecode
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
        public List<Instruction> Instructions => _instructions;
        public int Size
        {
            get
            {
                using MemoryStream memory = new();
                Write(memory);
                return (int)memory.Length;
            }
        }

        int IDSharpMethodBytecode.InstructionsCount => Instructions.Count;

        private readonly DSharpCompilerContext _context = new()
        {
            Assembly = method.Assembly,
            CurrentMember = method
        };
        private List<Instruction> _instructions = [];
        private bool _isWriting;

        #region Управление

        /// <summary>
        /// Write bytecode to stream
        /// </summary>
        /// <remarks>First 4 bytes represents total amount of instructions, 
        /// then follows instructions one by one. Instruction size - 2 bytes, 
        /// some instructions may contains parameters, which follows after instruction bytes.
        /// Each instruction have static amount of parameters
        /// <code>
        ///  0  : 1 instruction byte
        ///  1  : 2 instruction byte
        /// ... : parameters
        /// </code>
        /// Example for <see cref="PopOffset(int)"/>:
        /// <code>
        ///  0  : 1 instruction byte
        ///  1  : 2 instruction byte
        ///  2  : 1 offset byte
        ///  3  : 2 offset byte
        ///  4  : 3 offset byte
        ///  5  : 4 offset byte
        /// </code>
        /// </remarks>
        /// <param name="stream">Stream for writing built bytecode</param>
        public void Write(Stream stream)
        {
            if (_isWriting)
            {
                throw new InvalidOperationException("Bytecode builder already writing code to stream");
            }

            _isWriting = true;
            var overrideMethod = Method.OverrideMethod;
            List<Instruction>? extraInstructions = null;

            stream.Write(Method.Parameters.Count + LocalVariables.Count);

            foreach (var parameter in Method.Parameters)
            {
                DSharpParameterInfo.Write(stream, parameter);
            }
            foreach (var variable in LocalVariables)
            {
                DSharpParameterInfo.Write(stream, variable);
            }

            if (Method.MethodType == DSharpMethodType.Finalizer && overrideMethod != null)
            {
                var defaultInstructions = _instructions;
                extraInstructions = [];
                _instructions = extraInstructions;

                LoadInstance();
                CallBaseInstance(overrideMethod);

                _instructions = defaultInstructions;
            }

            try
            {
                int instructionsCount = Instructions.Count(i => i is not CommentInstruction) + (extraInstructions?.Count ?? 0);
                stream.Write(instructionsCount);

                if (extraInstructions != null)
                {
                    foreach (var extraInstruction in extraInstructions)
                    {
                        extraInstruction.Write(stream);
                    }
                }
                foreach (var instruction in Instructions)
                {
                    if (instruction is not CommentInstruction)
                    {
                        instruction.Write(stream);
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _isWriting = false;
            }
        }

        public void CopyTo(DSharpBytecodeBuilder builder)
        {
            builder.LocalVariables.Clear();

            foreach (var localVariable in LocalVariables)
            {
                builder.LocalVariables.Add(new(localVariable));
            }

            var otherInstructions = builder.Instructions;
            otherInstructions.Clear();

            foreach (var instruction in Instructions)
            {
                var newInstruction = instruction.Copy(builder);
                otherInstructions.Add(newInstruction);
            }
            for (int i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];
                var otherInstruction = otherInstructions[i];

                if (instruction is ReferenceInstruction reference &&
                    otherInstruction is ReferenceInstruction otherReference &&
                    reference.ReferencedInstruction != null)
                {
                    int index = Instructions.IndexOf(reference.ReferencedInstruction);
                    otherReference.ReferencedInstruction = otherInstructions[index];
                } 
            }
        }
        public void CopyTo(UnmanagedArray<byte> byteArray)
        {
            using MemoryStream memory = new();
            Write(memory);

            memory.Position = 0;
            Span<byte> buffer = stackalloc byte[1024];
            int index = 0;

            while (index < byteArray.Length)
            {
                int bytesRead = memory.Read(buffer);

                if (bytesRead == 0)
                {
                    break;
                }

                for (int i = 0; i < bytesRead; i++)
                {
                    byteArray[index + i] = buffer[i];
                }

                index += bytesRead;
            }
        }
        /// <summary>
        /// Replace members in instructions arguments
        /// </summary>
        /// <param name="replacedMembers">Dictionary of replaced members</param>
        public void ReplaceMembers(IReadOnlyDictionary<IDSharpMemberInfo, IDSharpMemberInfo> replacedMembers)
        {
            var assembly = Method.Assembly;

            IDSharpMemberInfo ReplaceMember(IDSharpMemberInfo member)
            {
                if (replacedMembers.TryGetValue(member, out var replacedMember))
                {
                    return replacedMember;
                }

                return member;
            }

            foreach (var variable in LocalVariables)
            {
                if (variable.Type != null)
                {
                    var member = assembly.GetType(variable.Type);
                    variable.Type = assembly.GetTypeToken(ReplaceMember(member));
                }
            }
            foreach (var instruction in Instructions)
            {
                if (instruction is TypeInstruction typeInstruction)
                {
                    typeInstruction.MemberInfo = ReplaceMember(typeInstruction.MemberInfo);
                }
            }
        }
        /// <summary>
        /// Find all instructions that references to specified instruction
        /// </summary>
        /// <param name="instruction">Instruction that referenced by other</param>
        /// <returns>List of instructions that references to specified instruction</returns>
        public List<ReferenceInstruction>? FindReferences(Instruction instruction)
        {
            List<ReferenceInstruction>? result = null;

            foreach (var i in Instructions)
            {
                if (i is ReferenceInstruction referenceInstruction &&
                    referenceInstruction.ReferencedInstruction == instruction)
                {
                    result ??= [];
                    result.Add(referenceInstruction);
                }
            }

            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Instructions.Count == 0)
            {
                return "[No code]";
            }

            StringBuilder builder = new();
            var lineNumberLength = (Instructions.Count - 1).ToString().Length;
            int line = 0;

            string GetLine()
            {
                string result = line.ToString();

                if (lineNumberLength > result.Length)
                {
                    for (int i = 0; i < lineNumberLength - result.Length; i++)
                    {
                        result = ' ' + result;
                    }
                }

                return result;
            }

            foreach (var instruction in Instructions)
            {
                builder.AppendLine($"{GetLine()}: {instruction}");
                line++;
            }

            return builder.ToString().TrimEnd();
        }

        #endregion

        #region Загрузка и выгрузка значений

        /// <summary>
        /// Add comment to bytecode. Comments will not writes to result bytecode
        /// </summary>
        /// <param name="text">Text of comment</param>
        /// <returns>Comment instruction</returns>
        public CommentInstruction Comment(string? text)
        {
            return CreateInstruction<CommentInstruction>(this, text);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Push"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public LiteralInstruction Push(DSharpLiteralValue value)
        {
            return CreateInstruction<LiteralInstruction>(this, DSharpBytecodeOperation.Push, value);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Push"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SizeInstruction PushSize(IDSharpType type)
        {
            return CreateInstruction<SizeInstruction>(this, DSharpBytecodeOperation.Push, type);
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
        public IndexInstruction PopOffset(int index) => PopOffset((uint)index);
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopOffset"/>
        /// </summary>
        /// <param name="index">Popped item offset</param>
        /// <returns></returns>
        public IndexInstruction PopOffset(uint index)
        {
            return CreateInstruction<IndexInstruction>(this, DSharpBytecodeOperation.PopOffset, index);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopRepeat"/>
        /// </summary>
        /// <param name="count">Repeat count</param>
        /// <returns></returns>
        public IndexInstruction PopRepeat(int count) => PopRepeat((uint)count);
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopRepeat"/>
        /// </summary>
        /// <param name="count">Repeat count</param>
        /// <returns></returns>
        public IndexInstruction PopRepeat(uint count)
        {
            return CreateInstruction<IndexInstruction>(this, DSharpBytecodeOperation.PopRepeat, count);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.PopPreviousTwo"/>
        /// </summary>
        /// <returns></returns>
        public Instruction PopPreviousTwo()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.PopPreviousTwo);
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

            CheckAccess(constructor);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.New, constructor);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.New"/>
        /// </summary>
        /// <param name="type">Type of object that needs to instantiate</param>
        /// <returns></returns>
        public Instruction New(IDSharpType type)
        {
            if (type.IsStatic)
            {
                throw new ArgumentException($"Unable to create new instance of static type");
            }

            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.New, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NewArray"/>
        /// </summary>
        /// <param name="type">Type of array items that needs to instantiate</param>
        /// <returns></returns>
        public TypeInstruction NewArray(IDSharpType type)
        {
            if (type.IsStatic)
            {
                throw new ArgumentException($"Unable to create array of static types");
            }

            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.NewArray, type);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadLocal"/>
        /// </summary>
        /// <param name="parameter">Local variable to load to stack</param>
        /// <returns></returns>
        public ParameterInstruction LoadLocal(IDSharpParameterInfo parameter)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.LoadLocal, parameter);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreLocal"/>
        /// </summary>
        /// <param name="parameter">Local variable for writing value from stack</param>
        /// <returns></returns>
        public ParameterInstruction StoreLocal(IDSharpParameterInfo parameter)
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
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstanceField"/>
        /// </summary>
        /// <param name="member">Field to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadInstanceField(IDSharpFieldInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadInstanceField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreField"/>
        /// </summary>
        /// <param name="member">Field for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreField(IDSharpFieldInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreField, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreInstanceField"/>
        /// </summary>
        /// <param name="member">Field for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreInstanceField(IDSharpFieldInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreInstanceField, member);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadProperty"/>
        /// </summary>
        /// <param name="member">Property to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadInstanceProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadInstanceProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadBaseInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadBaseInstanceProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadBaseInstanceProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreInstanceProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreInstanceProperty, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreBaseInstanceProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreBaseInstanceProperty(IDSharpPropertyInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreBaseInstanceProperty, member);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadIndexer"/>
        /// </summary>
        /// <param name="member">Indexer to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadIndexer(IDSharpIndexerInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadIndexer, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadBaseIndexer"/>
        /// </summary>
        /// <param name="member">Indexer to load to stack</param>
        /// <returns></returns>
        public TypeInstruction LoadBaseIndexer(IDSharpIndexerInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadBaseIndexer, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreIndexer"/>
        /// </summary>
        /// <param name="member">Indexer for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreIndexer(IDSharpIndexerInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreIndexer, member);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreBaseIndexer"/>
        /// </summary>
        /// <param name="member">Indexer for writing value from stack</param>
        /// <returns></returns>
        public TypeInstruction StoreBaseIndexer(IDSharpIndexerInfo member)
        {
            CheckAccess(member);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreBaseIndexer, member);
        }

        /// <summary>
        /// Auto load for specified member.
        /// </summary>
        /// <param name="propertyOrField">Property or field</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TypeInstruction LoadPropertyOrField(IDSharpMemberInfo propertyOrField, bool isBase = false)
        {
            if (propertyOrField is IDSharpIndexerInfo indexer)
            {
                if (!indexer.CanRead)
                {
                    throw new InvalidOperationException($"Unable to read value from indexer because it have not getter: \"{indexer}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    throw new InvalidOperationException($"Invalid indexer \"{indexer}\"! Indexers can not be static!");
                }
                if (isBase)
                {
                    return LoadBaseIndexer(indexer);
                }

                return LoadIndexer(indexer);
            }
            else if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanRead)
                {
                    throw new InvalidOperationException($"Unable to read value from property because it have not getter: \"{property}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    return LoadProperty(property);
                }
                if (isBase)
                {
                    return LoadBaseInstanceProperty(property);
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
        public TypeInstruction StorePropertyOrField(IDSharpMemberInfo propertyOrField, bool isBase = false)
        {
            if (propertyOrField is IDSharpIndexerInfo indexer)
            {
                if (!indexer.CanWrite)
                {
                    throw new InvalidOperationException($"Unable to write value to indexer because it have not setter: \"{indexer}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    throw new InvalidOperationException($"Invalid indexer \"{indexer}\"! Indexers can not be static!");
                }
                if (isBase)
                {
                    return StoreBaseIndexer(indexer);
                }

                return StoreIndexer(indexer);
            }
            else if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanWrite)
                {
                    throw new InvalidOperationException($"Unable to write value to property because it have not setter: \"{property}\"");
                }
                if (propertyOrField.IsStatic)
                {
                    return StoreProperty(property);
                }
                if (isBase)
                {
                    return StoreBaseInstanceProperty(property);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadInstance"/>
        /// </summary>
        /// <returns></returns>
        public Instruction LoadInstance()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.LoadInstance);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadTypeInformation"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction LoadTypeInformation(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadTypeInformation, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadTypeSize"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction LoadTypeSize(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadTypeSize, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadCurrentInstructionIndex"/>
        /// </summary>
        /// <returns></returns>
        public Instruction LoadCurrentInstructionIndex()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.LoadCurrentInstructionIndex);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StartScope"/>
        /// </summary>
        /// <returns></returns>
        public Instruction StartScope()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.StartScope);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.EndScope"/>
        /// </summary>
        /// <returns></returns>
        public Instruction EndScope()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.EndScope);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Cast"/>
        /// </summary>
        /// <param name="type">Type to cast</param>
        /// <returns></returns>
        public TypeInstruction Cast(IDSharpType type)
        {
            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.Cast, type);
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
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.Call, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitCall"/>
        /// </summary>
        /// <param name="method">Method or function that needs to call</param>
        /// <returns></returns>
        public TypeInstruction AwaitCall(IDSharpMethodInfo method)
        {
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.AwaitCall, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.CallInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction CallInstance(IDSharpMethodInfo method)
        {
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.CallInstance, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.CallBaseInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction CallBaseInstance(IDSharpMethodInfo method)
        {
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.CallBaseInstance, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitCallInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction AwaitCallInstance(IDSharpMethodInfo method)
        {
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.AwaitCallInstance, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitCallBaseInstance"/>
        /// </summary>
        /// <param name="method">Method that needs to call</param>
        /// <returns></returns>
        public TypeInstruction AwaitCallBaseInstance(IDSharpMethodInfo method)
        {
            CheckAccess(method);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.AwaitCallBaseInstance, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GenericCall"/>
        /// </summary>
        /// <param name="info">Method or function that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction GenericCall(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.GenericCall, info);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitGenericCall"/>
        /// </summary>
        /// <param name="info">Method or function that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction AwaitGenericCall(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.AwaitGenericCall, info);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GenericCallInstance"/>
        /// </summary>
        /// <param name="info">Method that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction GenericCallInstance(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.GenericCallInstance, info);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GenericCallBaseInstance"/>
        /// </summary>
        /// <param name="info">Method that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction GenericCallBaseInstance(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.GenericCallBaseInstance, info);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitGenericCallInstance"/>
        /// </summary>
        /// <param name="info">Method that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction AwaitGenericCallInstance(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.AwaitGenericCallInstance, info);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AwaitGenericCallBaseInstance"/>
        /// </summary>
        /// <param name="info">Method that needs to call</param>
        /// <returns></returns>
        public GenericCallingInstruction AwaitGenericCallBaseInstance(DSharpMethodCallingInfo info)
        {
            CheckAccess(info.Method);
            return CreateInstruction<GenericCallingInstruction>(this, DSharpBytecodeOperation.AwaitGenericCallBaseInstance, info);
        }

        public TypeInstruction CallAuto(IDSharpMethodInfo method, bool isAwait, ref DSharpMethodCompileSettings settings)
        {
            return CallAuto(method, isAwait, settings.NextNonVirtualizedAccess);
        }
        public TypeInstruction CallAuto(IDSharpMethodInfo method, bool isAwait = false, bool nextNonVirtualizedAccess = false)
        {
            bool isStatic = method.IsStatic || 
                            method.DeclaringType == null;

            if (isAwait)
            {
                if (isStatic)
                {
                    return AwaitCall(method);
                }
                else
                {
                    if (nextNonVirtualizedAccess)
                    {
                        return AwaitCallBaseInstance(method);
                    }
                    else
                    {
                        return AwaitCallInstance(method);
                    }
                }
            }
            else
            {
                if (isStatic)
                {
                    return Call(method);
                }
                else
                {
                    if (nextNonVirtualizedAccess)
                    {
                        return CallBaseInstance(method);
                    }
                    else
                    {
                        return CallInstance(method);
                    }
                }
            }
        }
        public Instruction CallAuto(DSharpMethodCallingInfo info, bool isAwait, ref DSharpMethodCompileSettings settings)
        {
            return CallAuto(info, isAwait, settings.NextNonVirtualizedAccess);
        }
        public Instruction CallAuto(DSharpMethodCallingInfo info, bool isAwait = false, bool nextNonVirtualizedAccess = false)
        {
            var method = info.Method;
            bool isStatic = method.IsStatic ||
                            method.DeclaringType == null;

            if (info.GenericParameters.Count == 0)
            {
                return CallAuto(method, isAwait, nextNonVirtualizedAccess);
            }
            else
            {
                if (isAwait)
                {
                    if (isStatic)
                    {
                        return AwaitGenericCall(info);
                    }
                    else
                    {
                        if (nextNonVirtualizedAccess)
                        {
                            return AwaitGenericCallBaseInstance(info);
                        }
                        else
                        {
                            return AwaitGenericCallInstance(info);
                        }
                    }
                }
                else
                {
                    if (isStatic)
                    {
                        return GenericCall(info);
                    }
                    else
                    {
                        if (nextNonVirtualizedAccess)
                        {
                            return GenericCallBaseInstance(info);
                        }
                        else
                        {
                            return GenericCallInstance(info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.JumpIndexed"/>
        /// </summary>
        /// <returns></returns>
        public Instruction JumpIndexed()
        {
            return CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.JumpIndexed);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.SkipNext"/>
        /// </summary>
        /// <returns></returns>
        public Instruction SkipNext()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.SkipNext);
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

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StartTrying"/>
        /// </summary>
        /// <returns></returns>
        public Instruction StartTrying()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.StartTrying);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StopTrying"/>
        /// </summary>
        /// <returns></returns>
        public Instruction StopTrying()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.StopTrying);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.RegisterCatch"/>
        /// </summary>
        /// <returns></returns>
        public ReferenceInstruction RegisterCatch(Instruction? reference = null)
        {
            var result = CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.RegisterCatch);

            if (reference != null)
            {
                result.ReferencedInstruction = reference;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.RegisterTypedCatch"/>
        /// </summary>
        /// <returns></returns>
        public TypedReferenceInstruction RegisterTypedCatch(IDSharpType type, Instruction? reference = null)
        {
            var result = CreateInstruction<TypedReferenceInstruction>(this, DSharpBytecodeOperation.RegisterTypedCatch, type);

            if (reference != null)
            {
                result.ReferencedInstruction = reference;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.RegisterFinally"/>
        /// </summary>
        /// <returns></returns>
        public ReferenceInstruction RegisterFinally(Instruction? reference = null)
        {
            var result = CreateInstruction<ReferenceInstruction>(this, DSharpBytecodeOperation.RegisterFinally);

            if (reference != null)
            {
                result.ReferencedInstruction = reference;
            }

            return result;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Finally"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Finally()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Finally);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftLeft"/>
        /// </summary>
        /// <returns></returns>
        public Instruction ShiftLeft()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.ShiftLeft);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftRight"/>
        /// </summary>
        /// <returns></returns>
        public Instruction ShiftRight()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.ShiftRight);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Xor"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Xor()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Xor);
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

        #region Операторы

        /// <summary>
        /// Add unary operation
        /// </summary>
        /// <param name="unaryOperator">Unary operator</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public Instruction UnaryOperation(DSharpUnaryOperator unaryOperator)
        {
            switch (unaryOperator)
            {
                case DSharpUnaryOperator.Not:
                    return Not();
                case DSharpUnaryOperator.Minus:
                    Push(-1);
                    Multiply();
                    return PopOffsetRepeat(1, 2);
                case DSharpUnaryOperator.Increment:
                    return Increment();
                case DSharpUnaryOperator.Decrement:
                    return Decrement();
                default:
                    throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)unaryOperator}\"", nameof(unaryOperator));
            }
        }
        /// <summary>
        /// Add binary operation
        /// </summary>
        /// <param name="binaryOperator">Binary operator</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public Instruction BinaryOperation(DSharpBinaryOperator binaryOperator)
        {
            return binaryOperator switch
            {
                DSharpBinaryOperator.Plus => Add(),
                DSharpBinaryOperator.Minus => Subtract(),
                DSharpBinaryOperator.Multiply => Multiply(),
                DSharpBinaryOperator.Divide => Divide(),
                DSharpBinaryOperator.Mod => Mod(),
                DSharpBinaryOperator.ShiftLeft => ShiftLeft(),
                DSharpBinaryOperator.ShiftRight => ShiftRight(),
                DSharpBinaryOperator.LogicalAnd => And(),
                DSharpBinaryOperator.LogicalOr => Or(),
                DSharpBinaryOperator.LogicalLess => Less(),
                DSharpBinaryOperator.LogicalLessOrEquals => LessOrEqual(),
                DSharpBinaryOperator.LogicalGreater => Greater(),
                DSharpBinaryOperator.LogicalGreaterOrEquals => GreaterOrEqual(),
                DSharpBinaryOperator.LogicalEquals => Equals(),
                DSharpBinaryOperator.LogicalNotEquals => NotEquals(),
                DSharpBinaryOperator.LogicalXor => Xor(),
                _ => throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)binaryOperator}\"", nameof(binaryOperator)),
            };
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
                
                if (variable == null)
                {
                    return null;
                }
                if (variable.Type == null)
                {
                    throw new InvalidOperationException($"Type of local variable ({variableName}) not specified in {Method}");
                }

                return Method.Assembly.GetType(variable.Type) as IDSharpType;
            }

            return null;
        }
        /// <summary>
        /// Resolve expression member
        /// </summary>
        /// <param name="value">Value that contains type info or name</param>
        /// <returns>Resolved member</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IDSharpMemberInfo? ExpressionMemberResolver(DSharpCompilerContext context, object value)
        {
            if ((context.CurrentMember == null || context.CurrentMember == Method) &&
                Method.DeclaringType != null)
            {
                return ExpressionMemberResolver(Method.DeclaringType, value);
            }

            IDSharpMemberInfo? result = null;

            if (context.CurrentMember is IDSharpType currentType)
            {
                result = ExpressionMemberResolver(currentType, value);
            }
            else if (context.CurrentMember?.DeclaringType != null)
            {
                result = ExpressionMemberResolver(context.CurrentMember.DeclaringType, value);
            }

            if (context.ParentExpression == null && Method.DeclaringType != null && result == null)
            {
                result = ExpressionMemberResolver(Method.DeclaringType, value);
            }

            return result;
        }

        private T CreateInstruction<T>(params object?[] parameters)
            where T : Instruction
        {
            var result = (T)Activator.CreateInstance(typeof(T), parameters);
            Instructions.Add(result);

            return result;
        }
        private void CheckAccess(IDSharpMemberInfo member)
        {
            if (!_context.CanAccessTo(member))
            {
                _context.ThrowCanNotAccessException(member);
            }
        }
        private IDSharpMemberInfo? ExpressionMemberResolver(IDSharpType type, object value)
        {
            string? name = null;

            if (value is string str)
            {
                name = str;
            }
            else if (value is IdentifierExpressionNode identifier)
            {
                name = identifier.GetName(false);
            }
            if (name != null)
            {
                var field = type.GetFieldOrDefault(name);

                if (field != null)
                {
                    return field;
                }

                var property = type.GetPropertyOrDefault(name);

                if (property != null)
                {
                    return property;
                }

                var method = type.GetMethodOrDefault(name);

                if (method != null)
                {
                    return method;
                }
            }

            return null;
        }

        #endregion
    }
}
