using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpMethodBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder? declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpVirtualizedMemberInfoBuilder(assembly, name, metadataToken), IDSharpMethodInfo
    {
        private DSharpMethodBuilder(DSharpPropertyBuilder property, bool isSetter, string name, DSharpTypeToken metadataToken)
            : this(property.Assembly, property.DeclaringType, name, metadataToken)
        {
            LinkedProperty = property;
            MethodType = isSetter ? DSharpMethodType.Setter : DSharpMethodType.Getter;
        }
        private DSharpMethodBuilder(DSharpOperatorBuilder @operator, DSharpTypeToken metadataToken)
            : this(@operator.Assembly, @operator.DeclaringType, @operator.Name, metadataToken)
        {
            LinkedOperator = @operator;
            MethodType = DSharpMethodType.Operator;
        }
        private DSharpMethodBuilder(DSharpMethodType methodType, string name, DSharpTypeBuilder type, DSharpTypeToken metadataToken)
            : this(type.Assembly, type, name, metadataToken)
        {
            LinkedType = type;
            MethodType = methodType;
        }

        public DSharpMethodType MethodType { get; } = DSharpMethodType.Default;
        /// <summary>
        /// This method is getter or setter only if this property not null 
        /// </summary>
        public DSharpPropertyBuilder? LinkedProperty { get; }
        /// <summary>
        /// This method is operator only if this property not null 
        /// </summary>
        public DSharpOperatorBuilder? LinkedOperator { get; }
        /// <summary>
        /// This method is constructor only if this property not null 
        /// </summary>
        public DSharpTypeBuilder? LinkedType { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string Name
        {
            get
            {
                if (LinkedOperator != null)
                {
                    return LinkedOperator.Name;
                }
                if (MethodType == DSharpMethodType.Finalizer)
                {
                    return DSharpTypeBuilder.FinalizerName;
                }
                if (LinkedType != null)
                {
                    return DSharpTypeBuilder.ConstructorName;
                }
                else if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Getter)
                    {
                        return LinkedProperty.GetterMethodName;
                    }

                    return LinkedProperty.SetterMethodName;
                }

                return base.Name;
            }
            set => base.Name = value;
        }
        /// <summary>
        /// Namespace that contains this function. 
        /// This property should be null when method is child of some type
        /// </summary>
        public virtual string? Namespace { get; set; }
        public override DSharpTypeBuilder? DeclaringType { get; } = declaringType;
        public DSharpTypeToken? ReturnType
        {
            get
            {
                if (field == null && ReturnTypeResolver != null)
                {
                    field = ReturnTypeResolver();
                    ReturnTypeResolver = null;
                }
                if (field == null && OriginalMethod?.ReturnType != null)
                {
                    field = GetReplacedType(OriginalMethod.ReturnType);
                }
                if (LinkedOperator != null)
                {
                    return LinkedOperator.ReturnType;
                }
                if (LinkedType != null || MethodType == DSharpMethodType.Finalizer)
                {
                    return null;
                }
                if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Setter)
                    {
                        return null;
                    }

                    return LinkedProperty.PropertyType;
                }

                return field;
            }
            set;
        }
        public List<DSharpMethodBuilderParameter> Parameters { get; } = [];
        public List<DSharpTypeToken> GenericParameters { get; } = [];
        public IDSharpMethodInfo? OverrideMethod
        {
            get
            {
                if (MethodType == DSharpMethodType.Finalizer)
                {
                    if (DeclaringType?.TryGetInheritedFinalizer(out var inheritedFinalizer) == true)
                    {
                        return inheritedFinalizer;
                    }

                    return null;
                }
                if (LinkedType != null)
                {
                    return null;
                }
                if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Getter)
                    {
                        return LinkedProperty.OverrideProperty?.Getter;
                    }

                    return LinkedProperty.OverrideProperty?.Setter;
                }

                return field;
            }
            set
            {
                if (field != value)
                {
                    if (value != null)
                    {
                        if (value.IsSealed)
                        {
                            throw new ArgumentException($"Unable to override sealed method \"{value}\" by \"{this}\"", nameof(value));
                        }
                        if (!value.IsVirtual && !value.IsAbstract)
                        {
                            throw new ArgumentException($"Unable to override method that not virtual or abstract \"{value}\" by \"{this}\"", nameof(value));
                        }
                        if (value.ReturnType != (IDSharpType?)Assembly.GetTypeOrDefault(ReturnType))
                        {
                            throw new ArgumentException($"Unable to override method \"{value}\" by \"{this}\" because it's has different return types");
                        }
                        if (!CompareParameters(value, this))
                        {
                            throw new ArgumentException($"Unable to override method \"{value}\" by \"{this}\" because it's has different parameters");
                        }
                        if (!CompareGenericParameters(value, this))
                        {
                            throw new ArgumentException($"Unable to override method \"{value}\" by \"{this}\" because it's has different generic parameters");
                        }
                    }

                    field = value;
                }
            }
        }
        public override bool IsStatic
        {
            get
            {
                if (MethodType == DSharpMethodType.Finalizer)
                {
                    return false;
                }
                else if (MethodType == DSharpMethodType.Operator)
                {
                    return true;
                }

                return LinkedProperty?.IsStatic ?? base.IsStatic;
            }
            set => base.IsStatic = value;
        }
        public override bool IsAbstract
        {
            get
            {
                if (LinkedType != null || MethodType == DSharpMethodType.Finalizer)
                {
                    return false;
                }

                return LinkedProperty?.IsAbstract ?? base.IsAbstract;
            }
            set => base.IsAbstract = value;
        }
        public override bool IsSealed
        {
            get
            {
                if (LinkedType != null || MethodType == DSharpMethodType.Finalizer)
                {
                    return false;
                }

                return LinkedProperty?.IsSealed ?? base.IsSealed;
            }
            set => base.IsSealed = value;
        }
        public override bool IsVirtual
        {
            get
            {
                if (OverrideMethod != null || MethodType == DSharpMethodType.Finalizer)
                {
                    return true;
                }
                if (LinkedType != null)
                {
                    return false;
                }
                if (LinkedProperty != null)
                {
                    return LinkedProperty.IsVirtual;
                }

                return base.IsVirtual;
            }
            set => base.IsVirtual = value;
        }
        public override DSharpAccessModifier Access
        {
            get
            {
                if (MethodType == DSharpMethodType.Finalizer)
                {
                    return DSharpAccessModifier.Protected;
                }
                if (LinkedProperty != null)
                {
                    if (MethodType == DSharpMethodType.Getter)
                    {
                        return LinkedProperty.GetterAccess;
                    }

                    return LinkedProperty.SetterAccess;
                }
                if (LinkedOperator != null)
                {
                    return LinkedOperator.Access;
                }

                return base.Access;
            }
            set => base.Access = value;
        }
        public bool IsExtern
        {
            get
            {
                if (LinkedProperty != null || LinkedType != null ||
                    MethodType == DSharpMethodType.Finalizer)
                {
                    return false;
                }

                return field;
            }
            set;
        }
        public override bool IsDeclaration => IsAbstract || IsExtern || !HasBody;
        public bool HasBody
        {
            get
            {
                if (IsExtern || IsAbstract)
                {
                    return false;
                }

                return GetBytecodeBuilder().Instructions.Count > 0;
            }
        }
        internal IDSharpMethodInfo? OriginalMethod { get; set; }
        internal Func<DSharpTypeToken>? ReturnTypeResolver { get; set; }

        IDSharpType? IDSharpMethodInfo.ReturnType
        {
            get
            {
                if (ReturnType == null)
                {
                    return null;
                }

                return (IDSharpType)Assembly.GetType(ReturnType);
            }
        }

        private DSharpBytecodeBuilder? _bytecodeBuilder;

        #region Управление

        internal override void Update()
        {
            base.Update();
            _ = ReturnType;
        }

        /// <summary>
        /// Get bytecode builder for this method
        /// </summary>
        /// <returns>Bytecode builder</returns>
        /// <exception cref="InvalidOperationException">Abstract method can not contains bytecode</exception>
        /// <exception cref="InvalidOperationException">Extern method can not contains bytecode</exception>
        public DSharpBytecodeBuilder GetBytecodeBuilder()
        {
            if (IsAbstract)
            {
                throw new InvalidOperationException($"Abstract method can not contains bytecode: {this}");
            }
            if (IsExtern)
            {
                throw new InvalidOperationException($"Extern method can not contains bytecode: {this}");
            }
            if (_bytecodeBuilder == null)
            {
                _bytecodeBuilder = new(this);

                // ЗДЕСЬ НАДО ПРОПИСАТЬ КОПИРОВАНИЕ КОДА С ЗАМЕНОЙ ТИПОВ!!!
                if (DeclaringType?.GenericTemplate != null)
                {
                    var templatedMembers = DeclaringType.GetTemplatedMembers();

                    if (!templatedMembers.TryGetKey(this, out var methodTemplateMember))
                    {
                        throw new InvalidOperationException($"Unable to get bytecode builder for templated method \"{this}\" because it's template not found");
                    }
                    if (methodTemplateMember is not IDSharpMethodInfo methodTemplate)
                    {
                        throw new InvalidOperationException($"Template for method \"{this}\" must be a method, got \"{methodTemplateMember}\"");
                    }

                    methodTemplate.CopyBytecodeTo(_bytecodeBuilder);
                    _bytecodeBuilder.ReplaceMembers(templatedMembers);
                }
            }

            return _bytecodeBuilder;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpParameterInfo[] GetParameters() => [.. Parameters];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpType[] GetGenericParameters() => [.. GenericParameters.Select(t => (IDSharpType)Assembly.GetType(t))];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="builder"><inheritdoc/></param>
        public void CopyBytecodeTo(DSharpBytecodeBuilder builder)
        {
            var code = GetBytecodeBuilder();
            code.CopyTo(builder);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            string returnType = string.Empty;
            string name = Name + '(';
            int parameterIndex = 0;

            if (ReturnType != null)
            {
                returnType = Assembly.GetType(ReturnType) + " ";
            }

            foreach (var parameter in Parameters)
            {
                if (parameterIndex > 0)
                {
                    name += ", ";
                }

                if (parameter.Type != null)
                {
                    var type = Assembly.GetType(parameter.Type);
                    name += $"{type} ";
                }

                name += $"{parameter.Name}";

                parameterIndex++;
            }

            name += ')';

            if (DeclaringType == null)
            {
                return returnType + name;
            }

            return $"{returnType}{DeclaringType.FullName}.{name}";
        }

        #endregion

        #region Статика

        /// <summary>
        /// Create constructor method
        /// </summary>
        /// <param name="type">Type for constructing</param>
        /// <param name="metadataToken">Token for new method</param>
        /// <returns>Constructor method</returns>
        public static DSharpMethodBuilder CreateConstructor(DSharpTypeBuilder type, DSharpTypeToken metadataToken)
        {
            return new(DSharpMethodType.Constructor, DSharpTypeBuilder.ConstructorName, type, metadataToken);
        }
        /// <summary>
        /// Create finalize method
        /// </summary>
        /// <param name="type">Type for finalizing</param>
        /// <param name="metadataToken">Token for new method</param>
        /// <returns>Finalize method</returns>
        public static DSharpMethodBuilder CreateFinalizer(DSharpTypeBuilder type, DSharpTypeToken metadataToken)
        {
            return new(DSharpMethodType.Finalizer, DSharpTypeBuilder.FinalizerName, type, metadataToken);
        }
        /// <summary>
        /// Create getter method
        /// </summary>
        /// <param name="property">Property that must contains getter</param>
        /// <param name="metadataToken">Token for new method</param>
        /// <returns>Getter method</returns>
        public static DSharpMethodBuilder CreateGetter(DSharpPropertyBuilder property, string name, DSharpTypeToken metadataToken)
        {
            return new(property, false, name, metadataToken);
        }
        /// <summary>
        /// Create setter method
        /// </summary>
        /// <param name="property">Property that must contains setter</param>
        /// <param name="metadataToken">Token for new method</param>
        /// <returns>Setter method</returns>
        public static DSharpMethodBuilder CreateSetter(DSharpPropertyBuilder property, string name, DSharpTypeToken metadataToken)
        {
            return new(property, true, name, metadataToken);
        }
        /// <summary>
        /// Create operator method
        /// </summary>
        /// <param name="operator">Operator that must contains method</param>
        /// <param name="metadataToken">Token for new method</param>
        /// <returns>Operator method</returns>
        public static DSharpMethodBuilder CreateOperator(DSharpOperatorBuilder @operator, DSharpTypeToken metadataToken)
        {
            return new(@operator, metadataToken);
        }


        /// <summary>
        /// Compare parameters of two methods. 
        /// Parameters always compares by it's type, names does not counts
        /// </summary>
        /// <param name="m1">First method to compare parameters</param>
        /// <param name="m2">Second method to compare parameters</param>
        /// <returns>Is parameters equals</returns>
        public static bool CompareParameters(IDSharpMethodInfo m1, IDSharpMethodInfo m2)
        {
            var params1 = m1.GetParameters();
            var params2 = m2.GetParameters();

            if (params1.Length != params2.Length)
            {
                return false;
            }

            for (int i = 0; i < params1.Length; i++)
            {
                if (params1[i].Type != params2[i].Type)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Compare generic parameters of two methods
        /// </summary>
        /// <param name="m1">First method to compare generic parameters</param>
        /// <param name="m2">Second method to compare generic parameters</param>
        /// <returns>Is generic parameters equals</returns>
        public static bool CompareGenericParameters(IDSharpMethodInfo m1, IDSharpMethodInfo m2)
        {
            var params1 = m1.GetGenericParameters();
            var params2 = m2.GetGenericParameters();

            if (params1.Length != params2.Length)
            {
                return false;
            }

            for (int i = 0; i < params1.Length; i++)
            {
                if (params1[i] != params2[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
