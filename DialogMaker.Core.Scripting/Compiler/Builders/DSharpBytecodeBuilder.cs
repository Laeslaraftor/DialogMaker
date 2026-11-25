using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Executor;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Class that designed for building bytecode for methods and functions
    /// </summary>
    /// <param name="method">Method or function that contains this bytecode</param>
    public partial class DSharpBytecodeBuilder(DSharpMethodBuilder method) : IDSharpMethodBytecode
    {
        ~DSharpBytecodeBuilder()
        {
            _sizeMemoryStream?.Dispose();
        }

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
        /// <summary>
        /// Instructions adding mode
        /// </summary>
        public DSharpBytecodeInstructionAddMode InstructionsAddMode { get; set; }
        /// <summary>
        /// Index that will be used for inserting instructions when <see cref="InstructionsAddMode"/> setted as 
        /// <see cref="DSharpBytecodeInstructionAddMode.Index"/> or <see cref="DSharpBytecodeInstructionAddMode.IndexWithCounting"/>
        /// </summary>
        public int InstructionsInsertIndex { get; set; } = -1;
        /// <summary>
        /// Bytecode size
        /// </summary>
        public int Size
        {
            get
            {
                _sizeMemoryStream ??= new();

                lock (_sizeMemoryStream)
                {
                    _sizeMemoryStream.SetLength(0);
                    Write(_sizeMemoryStream);
                    return (int)_sizeMemoryStream.Length;
                }
            }
        }
        /// <summary>
        /// Flag for disabling member access checking/
        /// </summary>
        public bool DisableAccessCheck { get; set; }
        int IDSharpMethodBytecode.InstructionsCount => Instructions.Count;

        private readonly DSharpCompilerContext _context = new()
        {
            Assembly = method.Assembly,
            CurrentMember = method
        };
        private List<Instruction> _instructions = [];
        private bool _isWriting;
        private MemoryStream? _sizeMemoryStream;

        #region Controls

        /// <summary>
        /// Clear bytecode and variables
        /// </summary>
        /// <param name="startInstructionsIndex">Start index for clearing bytecode. 0 - all instruction will be removed</param>
        public void Clear(int startInstructionsIndex = 0)
        {
            if (0 >= startInstructionsIndex)
            {
                Instructions.Clear();
            }
            else
            {
                Instructions.RemoveRange(startInstructionsIndex, Instructions.Count - startInstructionsIndex);
            }

            LocalVariables.Clear();
        }

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
                Call(overrideMethod, DSharpMethodCallingType.Default);

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
                return member.Replace(Method.Assembly, replacedMembers);
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
                else if (instruction is TwoMembersInstruction twoMembersInstruction)
                {
                    twoMembersInstruction.FirstMember = ReplaceMember(twoMembersInstruction.FirstMember);
                    twoMembersInstruction.SecondMember = ReplaceMember(twoMembersInstruction.SecondMember);
                }
                else if (instruction is CallingInstruction genericCallInstruction)
                {
                    var callingInfo = genericCallInstruction.CallingInfo;

                    if (callingInfo != null)
                    {
                        genericCallInstruction.CallingInfo = callingInfo.ReplaceTypes(replacedMembers);
                        genericCallInstruction.AccessedMember = genericCallInstruction.CallingInfo.Callable;
                    }
                    else
                    {
                        genericCallInstruction.AccessedMember = ReplaceMember(genericCallInstruction.AccessedMember);
                    }
                }
                else if (instruction is GetFieldAddressInstruction getFieldAddressInstruction)
                {
                    getFieldAddressInstruction?.Field = (IDSharpFieldInfo)ReplaceMember(getFieldAddressInstruction.Field);
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
        /// Get variable or parameter index
        /// </summary>
        /// <param name="variable">Variable or parameter</param>
        /// <returns>Index of variable or parameter</returns>
        public int GetVariableIndex(IDSharpParameterInfo variable)
        {
            int index = Method.Parameters.IndexOf(variable);

            if (index == -1)
            {
                index = LocalVariables.IndexOf(variable);

                if (index != -1)
                {
                    index += Method.Parameters.Count;
                }
            }

            return index;
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

        #region Loading and storing values

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
        /// <inheritdoc cref="DSharpBytecodeOperation.PushSizedNull"/>
        /// </summary>
        /// <param name="type">Type of object</param>
        /// <returns></returns>
        public TypeInstruction PushSizedNull(IDSharpType type)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.PushSizedNull, type);
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
        public OffsetCountInstruction PopOffsetRepeat(uint offset, int count)
        {
            return CreateInstruction<OffsetCountInstruction>(this, DSharpBytecodeOperation.PopOffsetRepeat, offset, count);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StackMove"/>
        /// </summary>
        /// <param name="offset">Popped item offset</param>
        /// <param name="moveOffset">Move offset</param>
        /// <returns></returns>
        public OffsetCountInstruction StackMove(uint offset, int moveOffset)
        {
            return CreateInstruction<OffsetCountInstruction>(this, DSharpBytecodeOperation.StackMove, offset, moveOffset);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.NewStackArray"/>
        /// </summary>
        /// <param name="type">Type of array items that needs to instantiate</param>
        /// <returns></returns>
        public TypeInstruction NewStackArray(IDSharpType type)
        {
            if (type.IsStatic)
            {
                throw new ArgumentException($"Unable to create array of static types");
            }

            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.NewStackArray, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.MakeObject"/>
        /// </summary>
        /// <param name="type">Type of creating object</param>
        /// <returns></returns>
        public TypeInstruction MakeObject(IDSharpType type)
        {
            if (type.IsStatic)
            {
                throw new ArgumentException($"Unable to create array of static types");
            }

            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.MakeObject, type);
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
        public CallingInstruction LoadProperty(IDSharpPropertyInfo member, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(member);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.LoadProperty, member);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreProperty"/>
        /// </summary>
        /// <param name="member">Property for writing value from stack</param>
        /// <returns></returns>
        public CallingInstruction StoreProperty(IDSharpPropertyInfo member, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(member);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.StoreProperty, member);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadIndexer"/>
        /// </summary>
        /// <param name="member">Indexer to load to stack</param>
        /// <returns></returns>
        public CallingInstruction LoadIndexer(IDSharpIndexerInfo member, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(member);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.LoadIndexer, member);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreIndexer"/>
        /// </summary>
        /// <param name="member">Indexer for writing value from stack</param>
        /// <returns></returns>
        public CallingInstruction StoreIndexer(IDSharpIndexerInfo member, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(member);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.StoreIndexer, member);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }

        /// <summary>
        /// Auto load for specified member.
        /// </summary>
        /// <param name="propertyOrField">Property or field</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Instruction LoadPropertyOrField(IDSharpMemberInfo propertyOrField, bool isBase = false)
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

                return LoadIndexer(indexer, isBase ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
            }
            else if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanRead)
                {
                    throw new InvalidOperationException($"Unable to read value from property because it have not getter: \"{property}\"");
                }

                return LoadProperty(property, isBase ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
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
        public Instruction StorePropertyOrField(IDSharpMemberInfo propertyOrField, bool isBase = false)
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

                return StoreIndexer(indexer, isBase ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
            }
            else if (propertyOrField is IDSharpPropertyInfo property)
            {
                if (!property.CanWrite)
                {
                    throw new InvalidOperationException($"Unable to write value to property because it have not setter: \"{property}\"");
                }

                return StoreProperty(property, isBase ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
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
            if (Method.IsStatic)
            {
                if (Method.IsAutoGenerated)
                {
                    Method.IsStatic = false;
                }
                else
                {
                    throw new InvalidOperationException($"Unable to load instance from static method \"{Method}\"");
                }
            }

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
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadMethodInformation"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction LoadMethodInformation(IDSharpMethodInfo method)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.LoadMethodInformation, method);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LoadTypeSize"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction LoadTypeSize(IDSharpType type, DSharpLoadTypeSizeMode mode = DSharpLoadTypeSizeMode.TotalSize)
        {
            return CreateInstruction<TypeSizeInstruction>(this, DSharpBytecodeOperation.LoadTypeSize, type, mode);
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
        /// <inheritdoc cref="DSharpBytecodeOperation.Cast"/>
        /// </summary>
        /// <param name="type">Type to cast</param>
        /// <returns></returns>
        public TypeInstruction Cast(IDSharpType type)
        {
            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.Cast, type);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Box"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Box()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Box);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.As"/>
        /// </summary>
        /// <param name="type">Type to cast</param>
        /// <returns></returns>
        public TypeInstruction As(IDSharpType type)
        {
            CheckAccess(type);
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.As, type);
        }

        #endregion

        #region Calling and jumping

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.DynamicCall"/>
        /// </summary>
        /// <returns></returns>
        public Instruction DynamicCall()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.DynamicCall);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Call"/>
        /// </summary>
        /// <param name="method">Method or function that needs to call</param>
        /// <returns></returns>
        public CallingInstruction Call(IDSharpMethodInfo method, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(method);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.Call, method);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Call"/>
        /// </summary>
        /// <param name="info">Method or function that needs to call</param>
        /// <returns></returns>
        public CallingInstruction Call(DSharpMethodCallingInfo info, DSharpMethodCallingType callingType = DSharpMethodCallingType.Virtual)
        {
            CheckAccess(info.Callable);
            var instruction = CreateInstruction<CallingInstruction>(this, DSharpBytecodeOperation.Call, info);
            instruction.RequestDefaultCalling = callingType == DSharpMethodCallingType.Default;

            return instruction;
        }

        public CallingInstruction CallAuto(IDSharpMethodInfo method, bool isAwait, ref DSharpMethodCompileSettings settings)
        {
            return CallAuto(method, isAwait, settings.NextNonVirtualizedAccess);
        }
        public CallingInstruction CallAuto(IDSharpMethodInfo method, bool isAwait = false, bool nextNonVirtualizedAccess = false)
        {
            return Call(method, nextNonVirtualizedAccess ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
        }
        public CallingInstruction CallAuto(DSharpMethodCallingInfo info, bool isAwait, ref DSharpMethodCompileSettings settings)
        {
            return CallAuto(info, isAwait, settings.NextNonVirtualizedAccess);
        }
        public CallingInstruction CallAuto(DSharpMethodCallingInfo info, bool isAwait = false, bool nextNonVirtualizedAccess = false)
        {
            if (!info.Callable.CanBeCalledDirectly)
            {
                throw new ArgumentException($"Direct calling is unavailable for \"{info.Callable}\"");
            }

            var method = info.Callable.GetMethods().First();

            if (info.GenericParameters.Count == 0)
            {
                return CallAuto(method, isAwait, nextNonVirtualizedAccess);
            }
            else
            {
                return Call(info, nextNonVirtualizedAccess ? DSharpMethodCallingType.Default : DSharpMethodCallingType.Virtual);
            }
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Lock"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Lock()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Lock);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.Unlock"/>
        /// </summary>
        /// <returns></returns>
        public Instruction Unlock()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.Unlock);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.JumpIndexed"/>
        /// </summary>
        /// <returns></returns>
        public Instruction JumpIndexed()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.JumpIndexed);
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

        #region Logic

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

        #region Math

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
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.BitInvert"/>
        /// </summary>
        /// <returns></returns>
        public Instruction BitInvert()
        {
            return CreateInstruction<Instruction>(this, DSharpBytecodeOperation.BitInvert);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GetAddress"/>
        /// </summary>
        /// <returns></returns>
        public GetVariableAddressInstruction GetAddress(IDSharpParameterInfo variable)
        {
            return CreateInstruction<GetVariableAddressInstruction>(this, variable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GetAddress"/>
        /// </summary>
        /// <returns></returns>
        public GetFieldAddressInstruction GetAddress(IDSharpFieldInfo field)
        {
            return CreateInstruction<GetFieldAddressInstruction>(this, field);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ReadOnAddress"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction ReadOnAddress(IDSharpType readType)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.ReadOnAddress, readType);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.StoreOnAddress"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction StoreOnAddress(IDSharpType valueType)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.StoreOnAddress, valueType);
        }

        #endregion

        #region Extra math and comparison

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AddVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Add(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.AddVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.SubtractVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Subtract(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.SubtractVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.MultiplyVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Multiply(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.MultiplyVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.DivideVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Divide(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.DivideVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ModVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Mod(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.ModVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.XorVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Xor(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.XorVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.OrVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Or(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.OrVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AndVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction And(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.AndVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftLeftVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction ShiftLeft(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.ShiftLeftVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftRightVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction ShiftRight(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.ShiftRightVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.EqualsVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Equals(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.EqualsVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NotEqualsVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction NotEquals(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.NotEqualsVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LessVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Less(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.LessVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LessOrEqualVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction LessOrEqual(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.LessOrEqualVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GreaterVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction Greater(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.GreaterVariables, leftVariable, rightVariable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GreaterOrEqualVariables"/>
        /// </summary>
        /// <returns></returns>
        public TwoParametersInstruction GreaterOrEqual(IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return CreateInstruction<TwoParametersInstruction>(this, DSharpBytecodeOperation.GreaterOrEqualVariables, leftVariable, rightVariable);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AddFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Add(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.AddFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.SubtractFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Subtract(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.SubtractFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.MultiplyFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Multiply(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.MultiplyFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.DivideFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Divide(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.DivideFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ModFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Mod(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.ModFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.XorFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Xor(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.XorFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.OrFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Or(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.OrFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.AndFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction And(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.AndFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftLeftFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction ShiftLeft(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.ShiftLeftFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.ShiftRightFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction ShiftRight(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.ShiftRightFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.EqualsFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Equals(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.EqualsFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.NotEqualsFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction NotEquals(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.NotEqualsFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LessFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Less(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.LessFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.LessOrEqualFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction LessOrEqual(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.LessOrEqualFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GreaterFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction Greater(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.GreaterFields, leftField, rightField);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.GreaterOrEqualFields"/>
        /// </summary>
        /// <returns></returns>
        public TwoMembersInstruction GreaterOrEqual(IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return CreateInstruction<TwoMembersInstruction>(this, DSharpBytecodeOperation.GreaterOrEqualFields, leftField, rightField);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.InvertVariable"/>
        /// </summary>
        /// <returns></returns>
        public ParameterInstruction Invert(IDSharpParameterInfo variable)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.InvertVariable, variable);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.IncrementVariable"/>
        /// </summary>
        /// <returns></returns>
        public BoolArgParameterInstruction Increment(IDSharpParameterInfo variable, bool pushResultIntoStack)
        {
            return CreateInstruction<BoolArgParameterInstruction>(this, DSharpBytecodeOperation.IncrementVariable, variable, pushResultIntoStack);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.DecrementVariable"/>
        /// </summary>
        /// <returns></returns>
        public BoolArgParameterInstruction Decrement(IDSharpParameterInfo variable, bool pushResultIntoStack)
        {
            return CreateInstruction<BoolArgParameterInstruction>(this, DSharpBytecodeOperation.DecrementVariable, variable, pushResultIntoStack);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.BitInvertVariable"/>
        /// </summary>
        /// <returns></returns>
        public ParameterInstruction BitInvert(IDSharpParameterInfo variable)
        {
            return CreateInstruction<ParameterInstruction>(this, DSharpBytecodeOperation.BitInvertVariable, variable);
        }

        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.InvertField"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction Invert(IDSharpFieldInfo field)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.InvertField, field);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.IncrementField"/>
        /// </summary>
        /// <returns></returns>
        public BoolArgTypeInstruction Increment(IDSharpFieldInfo field, bool pushResultIntoStack)
        {
            return CreateInstruction<BoolArgTypeInstruction>(this, DSharpBytecodeOperation.IncrementField, field, pushResultIntoStack);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.DecrementField"/>
        /// </summary>
        /// <returns></returns>
        public BoolArgTypeInstruction Decrement(IDSharpFieldInfo field, bool pushResultIntoStack)
        {
            return CreateInstruction<BoolArgTypeInstruction>(this, DSharpBytecodeOperation.DecrementField, field, pushResultIntoStack);
        }
        /// <summary>
        /// <inheritdoc cref="DSharpBytecodeOperation.BitInvertField"/>
        /// </summary>
        /// <returns></returns>
        public TypeInstruction BitInvert(IDSharpFieldInfo field)
        {
            return CreateInstruction<TypeInstruction>(this, DSharpBytecodeOperation.BitInvertField, field);
        }

        #endregion

        #region Operators

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
                case DSharpUnaryOperator.BitInvert:
                    return BitInvert();
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
                DSharpBinaryOperator.And => And(),
                DSharpBinaryOperator.Or => Or(),
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
        /// <summary>
        /// Add unary operation for variable
        /// </summary>
        /// <param name="unaryOperator">Unary operator</param>
        /// <param name="variable">Variable for that unary operation is need to be performed</param>
        /// <param name="pushResultIntoStack">
        /// Flag (bool) that indicates when result is need to be pushed to stack.
        /// Works only with increment and decrement instructions
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public Instruction UnaryOperation(DSharpUnaryOperator unaryOperator, IDSharpParameterInfo variable, bool pushResultIntoStack = true)
        {
            return unaryOperator switch
            {
                DSharpUnaryOperator.Not => Invert(variable),
                DSharpUnaryOperator.Minus => Invert(variable),
                DSharpUnaryOperator.Increment => Increment(variable, pushResultIntoStack),
                DSharpUnaryOperator.Decrement => Decrement(variable, pushResultIntoStack),
                DSharpUnaryOperator.BitInvert => BitInvert(variable),
                _ => throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)unaryOperator}\"", nameof(unaryOperator)),
            };
        }
        /// <summary>
        /// Add binary operation between variables
        /// </summary>
        /// <param name="binaryOperator">Binary operator</param>
        /// <param name="leftVariable">Left operand variable</param>
        /// <param name="rightVariable">Right operand variable</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public TwoParametersInstruction BinaryOperation(DSharpBinaryOperator binaryOperator, IDSharpParameterInfo leftVariable, IDSharpParameterInfo rightVariable)
        {
            return binaryOperator switch
            {
                DSharpBinaryOperator.Plus => Add(leftVariable, rightVariable),
                DSharpBinaryOperator.Minus => Subtract(leftVariable, rightVariable),
                DSharpBinaryOperator.Multiply => Multiply(leftVariable, rightVariable),
                DSharpBinaryOperator.Divide => Divide(leftVariable, rightVariable),
                DSharpBinaryOperator.Mod => Mod(leftVariable, rightVariable),
                DSharpBinaryOperator.ShiftLeft => ShiftLeft(leftVariable, rightVariable),
                DSharpBinaryOperator.ShiftRight => ShiftRight(leftVariable, rightVariable),
                DSharpBinaryOperator.And => And(leftVariable, rightVariable),
                DSharpBinaryOperator.Or => Or(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalAnd => And(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalOr => Or(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalLess => Less(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalLessOrEquals => LessOrEqual(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalGreater => Greater(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalGreaterOrEquals => GreaterOrEqual(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalEquals => Equals(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalNotEquals => NotEquals(leftVariable, rightVariable),
                DSharpBinaryOperator.LogicalXor => Xor(leftVariable, rightVariable),
                _ => throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)binaryOperator}\"", nameof(binaryOperator)),
            };
        }
        /// <summary>
        /// Add unary operation for field
        /// </summary>
        /// <param name="unaryOperator">Unary operator</param>
        /// <param name="field">Field for that unary operation is need to be performed</param>
        /// <param name="pushResultIntoStack">
        /// Flag (bool) that indicates when result is need to be pushed to stack.
        /// Works only with increment and decrement instructions
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public TypeInstruction UnaryOperation(DSharpUnaryOperator unaryOperator, IDSharpFieldInfo field, bool pushResultIntoStack = true)
        {
            return unaryOperator switch
            {
                DSharpUnaryOperator.Not => Invert(field),
                DSharpUnaryOperator.Minus => Invert(field),
                DSharpUnaryOperator.Increment => Increment(field, pushResultIntoStack),
                DSharpUnaryOperator.Decrement => Decrement(field, pushResultIntoStack),
                DSharpUnaryOperator.BitInvert => BitInvert(field),
                _ => throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)unaryOperator}\"", nameof(unaryOperator)),
            };
        }
        /// <summary>
        /// Add binary operation between variables
        /// </summary>
        /// <param name="binaryOperator">Binary operator</param>
        /// <param name="leftVariable">Left operand variable</param>
        /// <param name="rightVariable">Right operand variable</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unknown operator</exception>
        public TwoMembersInstruction BinaryOperation(DSharpBinaryOperator binaryOperator, IDSharpFieldInfo leftField, IDSharpFieldInfo rightField)
        {
            return binaryOperator switch
            {
                DSharpBinaryOperator.Plus => Add(leftField, rightField),
                DSharpBinaryOperator.Minus => Subtract(leftField, rightField),
                DSharpBinaryOperator.Multiply => Multiply(leftField, rightField),
                DSharpBinaryOperator.Divide => Divide(leftField, rightField),
                DSharpBinaryOperator.Mod => Mod(leftField, rightField),
                DSharpBinaryOperator.ShiftLeft => ShiftLeft(leftField, rightField),
                DSharpBinaryOperator.ShiftRight => ShiftRight(leftField, rightField),
                DSharpBinaryOperator.And => And(leftField, rightField),
                DSharpBinaryOperator.Or => Or(leftField, rightField),
                DSharpBinaryOperator.LogicalAnd => And(leftField, rightField),
                DSharpBinaryOperator.LogicalOr => Or(leftField, rightField),
                DSharpBinaryOperator.LogicalLess => Less(leftField, rightField),
                DSharpBinaryOperator.LogicalLessOrEquals => LessOrEqual(leftField, rightField),
                DSharpBinaryOperator.LogicalGreater => Greater(leftField, rightField),
                DSharpBinaryOperator.LogicalGreaterOrEquals => GreaterOrEqual(leftField, rightField),
                DSharpBinaryOperator.LogicalEquals => Equals(leftField, rightField),
                DSharpBinaryOperator.LogicalNotEquals => NotEquals(leftField, rightField),
                DSharpBinaryOperator.LogicalXor => Xor(leftField, rightField),
                _ => throw new ArgumentException($"Unknown operator \"{(DSharpTokenType)binaryOperator}\"", nameof(binaryOperator)),
            };
        }

        #endregion

        #region Extra

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

        private T CreateInstruction<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object?[] parameters)
            where T : Instruction
        {
            var result = (T)Activator.CreateInstance(typeof(T), parameters)!;

            if (InstructionsAddMode == DSharpBytecodeInstructionAddMode.End ||
                InstructionsAddMode != DSharpBytecodeInstructionAddMode.Start &&
                InstructionsInsertIndex == -1)
            {
                Instructions.Add(result);
            }
            else if (InstructionsAddMode == DSharpBytecodeInstructionAddMode.Start)
            {
                Instructions.Insert(0, result);
            }
            else
            {
                Instructions.Insert(InstructionsInsertIndex, result);

                if (InstructionsAddMode == DSharpBytecodeInstructionAddMode.IndexWithCounting)
                {
                    InstructionsInsertIndex++;
                }
            }

            return result;
        }
        private void CheckAccess(IDSharpMemberInfo member)
        {
            if (!DisableAccessCheck && !_context.CanAccessTo(member))
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
