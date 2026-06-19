using Acly.Tokens;
using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Runtime.Builders
{
    public class DSharpAssemblyBuilder(string name, IList<DSharpAssembly> references) : IDSharpAssembly
    {
        public string Name { get; } = name;
        public ReadOnlyCollection<DSharpAssembly> References { get; } = new(references);
        public ReferenceReadOnlyList<DSharpTypeBuilder> Types
        {
            get
            {
                field ??= new(_types);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpFieldBuilder> GlobalVariables
        {
            get
            {
                field ??= new(_globalVariables);
                return field;
            }
        }
        public ReferenceReadOnlyList<DSharpMethodBuilder> GlobalFunctions
        {
            get
            {
                field ??= new(_globalFunctions);
                return field;
            }
        }
        public DSharpTypeToken StringToken
        {
            get
            {
                field ??= GetTypeToken(StringTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken NumberToken
        {
            get
            {
                field ??= GetTypeToken(NumberTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken CharToken
        {
            get
            {
                field ??= GetTypeToken(CharTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken BoolToken
        {
            get
            {
                field ??= GetTypeToken(BoolTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken ObjectToken
        {
            get
            {
                field ??= GetTypeToken(ObjectTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken EnumToken
        {
            get
            {
                field ??= GetTypeToken(EnumTypeFullName);
                return field;
            }
        }
        public DSharpTypeToken ArrayBaseToken
        {
            get
            {
                field ??= GetTypeToken(ArrayTypeFullName);
                return field;
            }
        }
        public IDSharpType StringType
        {
            get
            {
                field ??= (IDSharpType)GetType(StringToken);
                return field;
            }
        }
        public IDSharpType NumberType
        {
            get
            {
                field ??= (IDSharpType)GetType(NumberToken);
                return field;
            }
        }
        public IDSharpType CharType
        {
            get
            {
                field ??= (IDSharpType)GetType(CharToken);
                return field;
            }
        }
        public IDSharpType BoolType
        {
            get
            {
                field ??= (IDSharpType)GetType(BoolToken);
                return field;
            }
        }
        public IDSharpType ObjectType
        {
            get
            {
                field ??= (IDSharpType)GetType(ObjectToken);
                return field;
            }
        }
        public IDSharpType EnumType
        {
            get
            {
                field ??= (IDSharpType)GetType(EnumToken);
                return field;
            }
        }
        public IDSharpType ArrayBaseType
        {
            get
            {
                field ??= (IDSharpType)GetType(ArrayBaseToken);
                return field;
            }
        }
        IReadOnlyCollection<IDSharpType> IDSharpAssembly.Types => Types;

        private readonly ObservableCollection<DSharpTypeBuilder> _types = [];
        private readonly ObservableCollection<DSharpFieldBuilder> _globalVariables = [];
        private readonly ObservableCollection<DSharpMethodBuilder> _globalFunctions = [];
        private readonly List<DSharpTypeToken> _typeDefinitions = [];
        private readonly List<DSharpTypeToken> _methodsDefinitions = [];
        private readonly List<DSharpTypeToken> _fieldsDefinitions = [];
        private readonly List<DSharpTypeToken> _propertiesDefinitions = [];

        #region Управление

        internal DSharpTypeToken AllocateMetadataToken(DSharpMetadataTokenType type)
        {
            List<DSharpTypeToken> tokensBuffer;

            if (type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokensBuffer = _typeDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Property)
            {
                tokensBuffer = _propertiesDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Field)
            {
                tokensBuffer = _fieldsDefinitions;
            }
            else if (type == DSharpMetadataTokenType.Method)
            {
                tokensBuffer = _methodsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Invalid token type: {type}");
            }

            DSharpTypeToken token = new(type, tokensBuffer.Count, 0);
            tokensBuffer.Add(token);

            return token;
        }
        internal bool RemoveMember(DSharpMemberInfoBuilder member)
        {
            List<DSharpTypeToken> tokens;

            if (member.MetadataToken.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                tokens = _typeDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Method)
            {
                tokens = _methodsDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Property)
            {
                tokens = _propertiesDefinitions;
            }
            else if (member.MetadataToken.Type == DSharpMetadataTokenType.Field)
            {
                tokens = _fieldsDefinitions;
            }
            else
            {
                throw new ArgumentException($"Member with invalid metadata token type: {member.MetadataToken}");
            }

            bool result = RemoveMember(tokens, member.MetadataToken);

            if (result && member is DSharpTypeBuilder typeBuilder)
            {
                _types.Remove(typeBuilder);

                foreach (var type in _types)
                {
                    type.BaseTypes.Remove(typeBuilder.MetadataToken);
                }
            }

            return result;
        }
        internal DSharpTypeBuilder CreateType(string name, bool isGeneric, DSharpTypeBuilder? parent)
        {
            if (isGeneric && parent == null)
            {
                throw new ArgumentException("Parent type should be specified for generic type");
            }

            return CreateMember(DSharpMetadataTokenType.TypeDefinition, _types, t => new(this, isGeneric, parent, name, t));
        }

        public bool RemoveGlobalVariable(DSharpFieldBuilder variable)
        {
            if (_globalVariables.Remove(variable))
            {
                return RemoveMember(_fieldsDefinitions, variable.MetadataToken);
            }

            return false;
        }
        public bool RemoveGlobalFunction(DSharpMethodBuilder function)
        {
            if (_globalFunctions.Remove(function))
            {
                return RemoveMember(_methodsDefinitions, function.MetadataToken);
            }

            return false;
        }
        public bool RemoveType(DSharpTypeBuilder type) => RemoveMember(type);
        public DSharpTypeBuilder CreateType(string name, DSharpTypeBuilder? parent = null)
        {
            return CreateType(name, false, parent);
        }
        public DSharpFieldBuilder CreateGlobalVariable(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Field, _globalVariables, t => new(this, null, name, t));
        }
        public DSharpMethodBuilder CreateGlobalFunction(string name)
        {
            return CreateMember(DSharpMetadataTokenType.Method, _globalFunctions, t => new(this, null, name, t));
        }
        public IDSharpType FillGeneric(IDSharpType genericType, params IEnumerable<IDSharpType> genericParameters)
        {
            if (this.TryFindGenericImplementationType(genericType, out var result, genericParameters))
            {
                return result;
            }

            var genericsCount = genericParameters.Count();
            var genericTypes = genericType.GetGenericTypes();
            if (genericTypes.Length != genericsCount)
            {
                throw new ArgumentException($"Generic parameters count must match to amount of generic types in filling type", nameof(genericParameters));
            }

            DSharpAssemblyBuilder assemblyBuilder = this;

            if (genericType.Assembly is DSharpAssemblyBuilder genericTypeAssemblyBuilder)
            {
                assemblyBuilder = genericTypeAssemblyBuilder;
            }

            Dictionary<IDSharpType, IDSharpType> replacedTypes = [];
            Dictionary<IDSharpMemberInfo, IDSharpMemberInfo> replacedMembers = [];
            List<DSharpBytecodeBuilder> bytecodeBuilders = [];
            var newType = assemblyBuilder.CreateType(genericType.Name);
            newType.IsStatic = genericType.IsStatic;
            newType.IsAbstract = genericType.IsAbstract;
            newType.ObjectType = genericType.ObjectType;
            newType.IsSealed = genericType.IsSealed;
            newType.Namespace = genericType.Namespace;
            newType.GenericTemplate = genericType;

            int i = 0;

            foreach (var parameter in genericParameters)
            {
                var genericTypeParameter = genericTypes[i];

                if (!genericTypeParameter.CanReplaceGenericType(parameter))
                {
                    throw new ArgumentException($"Generic type {genericTypeParameter} can not be replaced by {parameter}");
                }

                newType.GenericParameters.Add(GetTypeToken(parameter));
                replacedTypes.Add(genericTypeParameter, parameter);
                replacedMembers.Add(genericTypeParameter, parameter);
                i++;
            }
            foreach (var genericTypeBaseType in genericType.GetBaseTypes())
            {
                var currentBaseType = ReplaceGenericParameters(genericTypeBaseType, replacedTypes);
                newType.BaseTypes.Add(GetTypeToken(currentBaseType));
            }
            foreach (var field in genericType.GetFields())
            {
                var newField = newType.CreateField(field.Name);
                newField.IsReadOnly = field.IsReadOnly;
                newField.IsStatic = field.IsStatic;
                newField.Access = field.Access;
                newField.RawValue = field.RawValue;
                newField.FieldType = GetTypeToken(ReplaceGenericParameters(field.FieldType, replacedTypes));

                replacedMembers.Add(field, newField);
            }
            foreach (var property in genericType.GetProperties())
            {
                var newProperty = newType.CreateProperty(property.Name);
                newProperty.Access = property.Access;
                newProperty.IsStatic = property.IsStatic;
                newProperty.IsSealed = property.IsSealed;
                newProperty.IsAbstract = property.IsAbstract;
                newProperty.IsVirtual = property.IsVirtual;
                newProperty.OverrideProperty = property.OverrideProperty;
                newProperty.PropertyType = GetTypeToken(ReplaceGenericParameters(property.PropertyType, replacedTypes));

                if (property.Getter != null)
                {
                    var getter = newProperty.CreateGetter();
                    ProcessMethod(getter, property.Getter);
                }
                if (property.Setter != null)
                {
                    var setter = newProperty.CreateSetter();
                    ProcessMethod(setter, property.Setter);
                }

                replacedMembers.Add(property, newProperty);
            }
            foreach (var method in genericType.GetMethods())
            {
                if (method.MethodType != DSharpMethodType.Default)
                {
                    continue;
                }

                var newMethod = newType.CreateMethod(name);
                ProcessMethod(newMethod, method);
            }
            foreach (var constructor in genericType.GetConstructors())
            {
                if (replacedMembers.ContainsKey(constructor))
                {
                    continue;
                }

                var newConstructor = newType.CreateConstructor();
                ProcessMethod(newConstructor, constructor);
            }

            void ProcessMethod(DSharpMethodBuilder newMethod, IDSharpMethodInfo method)
            {
                newMethod.Access = method.Access;
                newMethod.IsStatic = method.IsStatic;
                newMethod.IsAbstract = method.IsAbstract;
                newMethod.IsVirtual = method.IsVirtual;
                newMethod.IsExtern = method.IsExtern;
                newMethod.OverrideMethod = method.OverrideMethod;

                if (method.MethodType == DSharpMethodType.Default &&
                    method.ReturnType != null)
                {
                    newMethod.ReturnType = GetTypeToken(ReplaceGenericParameters(method.ReturnType, replacedTypes));
                }
                
                foreach (var genericParameter in method.GetGenericParameters())
                {
                    newMethod.GenericParameters.Add(GetTypeToken(genericParameter));
                }
                foreach (var parameter in method.GetParameters())
                {
                    newMethod.Parameters.Add(new(this)
                    {
                        Name = parameter.Name,
                        Type = GetTypeToken(ReplaceGenericParameters(parameter.Type, replacedTypes))
                    });
                }

                //if (!method.IsExtern && !method.IsAbstract)
                //{
                //    var code = newMethod.GetBytecodeBuilder();
                //    method.CopyBytecodeTo(code);
                //    bytecodeBuilders.Add(code);
                //}

                replacedMembers.Add(method, newMethod);
            }

            //foreach (var bytecodeBuilder in bytecodeBuilders)
            //{
            //    bytecodeBuilder.ReplaceMembers(replacedMembers);
            //}

            return newType;
        }
        public IDSharpType ReplaceGenericParameters(IDSharpType genericType, Dictionary<IDSharpType, IDSharpType> replacedTypes)
        {
            if (replacedTypes.Count == 0)
            {
                return genericType;
            }
            if (replacedTypes.TryGetValue(genericType, out var directReplacedType))
            {
                return directReplacedType;
            }

            var baseTypeGenericParameters = genericType.GetGenericParameters();
            bool refillType = false;

            for (int i = 0; i < baseTypeGenericParameters.Length; i++)
            {
                var parameter = baseTypeGenericParameters[i];

                if (replacedTypes.TryGetValue(parameter, out var replacedType))
                {
                    baseTypeGenericParameters[i] = replacedType;
                    refillType = true;
                }
            }

            if (refillType)
            {
                genericType = FillGeneric(genericType, baseTypeGenericParameters);
            }

            return genericType;
        }
        public IDSharpType CreateArray(IDSharpType elementType)
        {
            return FillGeneric(ArrayBaseType, elementType);
        }

        private T CreateMember<T>(DSharpMetadataTokenType tokenType, IList<T> members, Func<DSharpTypeToken, T> fabric)
        {
            var token = AllocateMetadataToken(tokenType);
            var type = fabric(token);
            members.Add(type);

            return type;
        }
        private bool RemoveMember(List<DSharpTypeToken> membersToken, DSharpTypeToken token)
        {
            var index = membersToken.IndexOf(token);

            if (index == -1)
            {
                return false;
            }

            membersToken.RemoveAt(index);
            token.Index = -1;

            for (int i = index; i < membersToken.Count; i++)
            {
                membersToken[i].Index--;
            }

            return true;
        }

        #endregion

        #region Поиск

        public DSharpTypeToken GetTypeToken(string fullName)
        {
            if (TryGetTypeToken(fullName, out var token))
            {
                return token;
            }

            throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }
        public DSharpTypeToken GetTypeToken(IDSharpMemberInfo type)
        {
            if (type is DSharpMemberInfoBuilder builder)
            {
                return builder.MetadataToken;
            }

            int assemblyIndex = References.IndexOf(type.Assembly);

            if (assemblyIndex == -1)
            {
                throw new ArgumentException($"Unable to get type token because specified type contains in assembly that not referenced by builder: {type}", nameof(type));
            }

            return new(type.MetadataToken, assemblyIndex);
        }
        public bool TryGetStandardType(string name, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            if (name == StringTypeFullName || name == StringName)
            {
                result = StringToken;
                return true;
            }
            if (name == NumberTypeFullName || name == NumberName)
            {
                result = NumberToken;
                return true;
            }
            if (name == BoolTypeFullName || name == BoolName)
            {
                result = BoolToken;
                return true;
            }
            if (name == ObjectTypeFullName || name == ObjectName)
            {
                result = ObjectToken;
                return true;
            }
            if (name == CharTypeFullName || name == CharName)
            {
                result = CharToken;
                return true;
            }

            result = null;
            return false;
        }
        public bool TryGetTypeToken(string fullName, [NotNullWhen(true)] out DSharpTypeToken? result)
        {
            result = null;
            var type = _types.FirstOrDefault(t => t.FullName == fullName);

            if (type != null)
            {
                result = type;
                return true;
            }

            int referenceIndex = 0;

            foreach (var reference in References)
            {
                if (reference.TryGetType(fullName, out var referencedType))
                {
                    result = new(referencedType.MetadataToken, referenceIndex);
                    return true;
                }

                referenceIndex++;
            }

            return false;
        }
        public IDSharpType GetType(string fullName)
        {
            IDSharpType type = _types.FirstOrDefault(t => t.FullName == fullName);

            if (type != null)
            {
                return type;
            }

            foreach (var assembly in References)
            {
                type = assembly.Types.FirstOrDefault(t => t.FullName == fullName);

                if (type != null)
                {
                    return type;
                }
            }

            return type ?? throw new ArgumentException($"Unknown type: {fullName}", nameof(fullName));
        }
        public IDSharpType GetType(DSharpLiteralType literalType)
        {
            return literalType switch
            {
                DSharpLiteralType.Null => throw new ArgumentException("Can not get type for null literal value", nameof(literalType)),
                DSharpLiteralType.String => (IDSharpType)GetType(StringToken),
                DSharpLiteralType.Char => (IDSharpType)GetType(CharToken),
                DSharpLiteralType.Number => (IDSharpType)GetType(NumberToken),
                DSharpLiteralType.Bool => (IDSharpType)GetType(BoolToken),
                _ => throw new ArgumentException($"Invalid literal type: {literalType}", nameof(literalType))
            };
        }
        public IDSharpMemberInfo GetType(DSharpTypeToken token)
        {
            return GetType((DSharpMetadataToken)token);
        }
        public IDSharpMemberInfo GetType(DSharpMetadataToken metadata)
        {
            if (metadata.AssemblyIndex > 0)
            {
                DSharpMetadataToken originalToken = metadata;
                originalToken = new(metadata, 0);
                return References[metadata.AssemblyIndex - 1].GetType(originalToken);
            }

            if (metadata.Type == DSharpMetadataTokenType.TypeDefinition)
            {
                return _types.First(t => t.MetadataToken == metadata);
            }
            else if (metadata.Type == DSharpMetadataTokenType.Field)
            {
                var globalVariable = _globalVariables.FirstOrDefault(v => v.MetadataToken == metadata);

                if (globalVariable != null)
                {
                    return globalVariable;
                }

                foreach (var type in _types)
                {
                    var field = type.Fields.FirstOrDefault(f => f.MetadataToken == metadata);

                    if (field != null)
                    {
                        return field;
                    }
                }

                throw new ArgumentException($"Unable to find field for token: {metadata}", nameof(metadata));
            }
            else if (metadata.Type == DSharpMetadataTokenType.Method)
            {
                var globalFunction = _globalFunctions.FirstOrDefault(f => f.MetadataToken == metadata);

                if (globalFunction != null)
                {
                    return globalFunction;
                }

                foreach (var type in _types)
                {
                    var method = type.Methods.FirstOrDefault(m => m.MetadataToken == metadata);

                    if (method != null)
                    {
                        return method;
                    }
                }

                throw new ArgumentException($"Unable to find method for token: {metadata}", nameof(metadata));
            }
            else if (metadata.Type == DSharpMetadataTokenType.Property)
            {
                foreach (var type in _types)
                {
                    var property = type.Properties.FirstOrDefault(p => p.MetadataToken == metadata);

                    if (property != null)
                    {
                        return property;
                    }
                }

                throw new ArgumentException($"Unable to find method for token: {metadata}", nameof(metadata));
            }

            throw new ArgumentException($"Invalid token type: {metadata}", nameof(metadata));
        }
        public IDSharpType GetType(TypeInfoNode typeInfo)
        {
            return GetType(typeInfo.GetFullName(true, false));
        }
        public IDSharpFieldInfo[] GetGlobalVariables() => [.. _globalVariables];
        public IDSharpMethodInfo[] GetGlobalFunctions() => [.. GlobalFunctions];
        public List<IDSharpType> GetTypes(string fullName)
        {
            List<IDSharpType> result = [.. Types.Where(t => t.FullName == fullName)];

            foreach (var reference in References)
            {
                var types = reference.Types.Where(t => t.FullName == fullName);
                result.AddRange(types);
            }

            return result;
        }

        #endregion

        #region Константы

        public const string ObjectTypeFullName = "System.Object";
        public const string StringTypeFullName = "System.String";
        public const string NumberTypeFullName = "System.Number";
        public const string BoolTypeFullName = "System.Boolean";
        public const string CharTypeFullName = "System.Char";
        public const string EnumTypeFullName = "System.Enum";
        public const string ArrayTypeFullName = "System.Array`1";
        public const string ObjectName = "object";
        public const string StringName = "string";
        public const string NumberName = "number";
        public const string BoolName = "bool";
        public const string CharName = "char";
        public const string VarName = "var";

        #endregion
    }
}
