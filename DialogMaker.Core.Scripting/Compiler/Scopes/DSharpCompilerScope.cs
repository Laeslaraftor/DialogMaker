using DialogMaker.Core.Scripting.Runtime;
using DialogMaker.Core.Scripting.Runtime.Builders;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Scopes
{
    /// <summary>
    /// Base type of scope
    /// </summary>
    /// <param name="assembly">Compiling assembly</param>
    /// <param name="parent">Parent scope</param>
    public abstract class DSharpCompilerScope(DSharpAssemblyBuilder assembly, DSharpCompilerScope? parent)
    {
        /// <summary>
        /// Compiling assembly
        /// </summary>
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        /// <summary>
        /// Parent scope
        /// </summary>
        public DSharpCompilerScope? Parent { get; } = parent;

        /// <summary>
        /// Try to resolve type in current and parent scope
        /// </summary>
        /// <param name="namespace">Type's namespace</param>
        /// <param name="name">Name of type</param>
        /// <param name="result">Resolved type</param>
        /// <param name="genericTypes">List of types for searching generic types</param>
        /// <returns>Is type resolved</returns>
        /// <exception cref="ArgumentException">Specified type can not replace generic type</exception>
        public bool TryResolveType(string name, [NotNullWhen(true)] out IDSharpType? result, params IList<IDSharpType>? genericTypes)
        {
            result = null;
            int genericTypesCount = 0;
            IDSharpType? foundAssignableTemplate = null;

            bool? IsValid(IDSharpType type)
            {
                if (type.GenericTemplate != null)
                {
                    var genericParameters = type.GetGenericParameters();

                    if (genericParameters.Length == 0 && genericTypesCount == 0)
                    {
                        return true;
                    }
                    if (genericParameters.Length != genericTypesCount)
                    {
                        return false;
                    }

                    return genericParameters.SequenceEqual(genericTypes);
                }

                var typeGenerics = type.GetGenericTypes();

                if (typeGenerics.Length == 0 && genericTypesCount == 0)
                {
                    return true;
                }
                if (typeGenerics.Length != genericTypesCount)
                {
                    return false;
                }
                if (typeGenerics.SequenceEqual(genericTypes))
                {
                    return true;
                }

                for (int i = 0; i < typeGenerics.Length; i++)
                {
                    if (!typeGenerics[i].CanReplaceGenericType(genericTypes![i]))
                    {
                        throw new ArgumentException($"\"{genericTypes[i]}\" can not replace generic type in \"{type}\" at {i} index");
                    }
                }

                return null;
            }
            IDSharpType? Check(IEnumerable<IDSharpType> types)
            {
                foreach (var type in types)
                {
                    var validateResult = IsValid(type);

                    if (validateResult == false)
                    {
                        continue;
                    }
                    if (validateResult == null)
                    {
                        foundAssignableTemplate = type;
                        continue;
                    }

                    return type;
                }

                return null;
            }

            result = RecursiveCheck(scope =>
            {
                return Check(scope.GetTypes(name));
            });

            if (result != null)
            {
                return true;
            }
            else if (foundAssignableTemplate != null)
            {
                result = Assembly.FillGeneric(foundAssignableTemplate, genericTypes!);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try to resolve all members with specified name in current and parent scope
        /// </summary>
        /// <param name="name">Name of members</param>
        /// <param name="result">List of resolved members</param>
        /// <returns>Is any member resolved</returns>
        public bool TryResolveMultipleMembers(string name, [NotNullWhen(true)] out List<IDSharpMemberInfo>? result)
        {
            return TryResolveMultipleMembers<IDSharpMemberInfo>(name, out result);
        }
        /// <summary>
        /// Try to resolve all members with specified name in current and parent scope
        /// </summary>
        /// <param name="name">Name of members</param>
        /// <param name="result">List of resolved members</param>
        /// <returns>Is any member resolved</returns>
        public bool TryResolveMultipleMembers<T>(string name, [NotNullWhen(true)] out List<T>? result)
            where T : IDSharpMemberInfo
        {
            List<T>? members = null;

            RecursiveCheck<T>(scope =>
            {
                foreach (var member in GetMembers())
                {
                    if (member is T typedMember &&
                        member.Name == name)
                    {
                        members ??= [];
                        members.Add(typedMember);
                    }
                }

                return default;
            });

            result = members;
            return result != null && result.Count > 0;
        }
        /// <summary>
        /// Try to resolve member with specified name in current and parent scope
        /// </summary>
        /// <param name="name">Name of member</param>
        /// <param name="result">Resolved member</param>
        /// <returns>Is member resolved</returns>
        public bool TryResolveMember(string name, [NotNullWhen(true)] out IDSharpMemberInfo? result)
        {
            return TryResolveMember<IDSharpMemberInfo>(name, out result);
        }
        /// <summary>
        /// Try to resolve member with specified name in current and parent scope
        /// </summary>
        /// <param name="name">Name of member</param>
        /// <param name="result">Resolved member</param>
        /// <returns>Is member resolved</returns>
        public bool TryResolveMember<T>(string name, [NotNullWhen(true)] out T? result)
            where T : IDSharpMemberInfo
        {
            result = RecursiveCheck(scope =>
            {
                foreach (var member in GetMembers())
                {
                    if (member is T typedMember &&
                        member.Name == name)
                    {
                        return typedMember;
                    }
                }

                return default;
            });

            return result != null;
        }
        /// <summary>
        /// Try to resolve variable in current and parent scope
        /// </summary>
        /// <param name="name">Name of variable</param>
        /// <param name="result">Resolved variable</param>
        /// <returns>Is variable resolved</returns>
        public bool TryGetVariable(string name, [NotNullWhen(true)] out IDSharpParameterInfo? result)
        {
            result = RecursiveCheck(scope =>
            {
                foreach (var variable in GetVariables())
                {
                    if (variable.Name == name)
                    {
                        return variable;
                    }
                }

                return null;
            });

            return result != null;
        }
        /// <summary>
        /// Try to create variable in current scope
        /// </summary>
        /// <param name="name">Name of variable</param>
        /// <param name="type">Type of variable</param>
        /// <param name="result">Created variable</param>
        /// <returns>Is variable created</returns>
        public bool TryCreateVariable(string name, IDSharpType type, [NotNullWhen(true)] out IDSharpParameterInfo? result)
        {
            result = RecursiveCheck(scope =>
            {
                return scope.CreateLocalVariable(name, type);
            });

            return result != null;
        }

        /// <summary>
        /// Try to resolve first property or field
        /// </summary>
        /// <param name="name">Name of property or field</param>
        /// <param name="result">Resolved property or field</param>
        /// <returns>Is property or field resolved</returns>
        public bool TryResolvePropertyOrField(string name, [NotNullWhen(true)] out IDSharpMemberInfo? result)
        {
            if (TryResolveMember<IDSharpPropertyInfo>(name, out var property))
            {
                result = property;
                return true;
            }
            if (TryResolveMember<IDSharpFieldInfo>(name, out var field))
            {
                result = field;
                return true;
            }

            result = null;
            return false;
        }
        /// <summary>
        /// Try to resolve first property, field or variable
        /// </summary>
        /// <param name="name">Name of property, field or variable</param>
        /// <param name="propertyOrFieldHandler">Handler for resolved property or field</param>
        /// <param name="variableHandler">Handler for resolved variable</param>
        /// <returns>Is property, field or variable resolved</returns>
        public bool TryResolvePropertyOrFieldOrVariable(string name, Action<IDSharpMemberInfo> propertyOrFieldHandler, Action<IDSharpParameterInfo> variableHandler)
        {
            if (TryGetVariable(name, out var variable))
            {
                variableHandler(variable);
                return true;
            }
            if (TryResolvePropertyOrField(name, out var propertyOrField))
            {
                propertyOrFieldHandler(propertyOrField);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try to resolve methods with specified generic parameters.
        /// </summary>
        /// <param name="name">Name of method</param>
        /// <param name="genericParameters">Method generic parameters</param>
        /// <param name="result">Resolved methods</param>
        /// <returns>Is methods resolved</returns>
        public bool TryResolveMethods(string name, IList<IDSharpType>? genericParameters, [NotNullWhen(true)] out List<IDSharpMethodInfo>? result)
        {
            result = null;

            if (!TryResolveMultipleMembers<IDSharpMethodInfo>(name, out var methods))
            {
                return false;
            }

            if (genericParameters == null)
            {
                result = methods;
                return true;
            }

            foreach (var method in methods)
            {
                var methodGenericParameters = method.GetGenericParameters();

                if (methodGenericParameters.SequenceEqual(genericParameters))
                {
                    result ??= [];
                    result.Add(method);
                }
            }

            return result != null;
        }

        public IDSharpType ResolveType(string name, params IList<IDSharpType>? genericTypes)
        {
            if (TryResolveType(name, out var result, genericTypes))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve type \"{name}\"", nameof(name));
        }
        public List<T> ResolveMultipleMembers<T>(string name)
            where T : IDSharpMemberInfo
        {
            if (TryResolveMultipleMembers<T>(name, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve any member with name \"{name}\"", nameof(name));
        }
        public List<IDSharpMemberInfo> ResolveMultipleMembers(string name)
        {
            return ResolveMultipleMembers<IDSharpMemberInfo>(name);
        }
        public T ResolveMember<T>(string name)
            where T : IDSharpMemberInfo
        {
            if (TryResolveMember<T>(name, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve member \"{name}\"", nameof(name));
        }
        public IDSharpMemberInfo ResolveMember(string name)
        {
            return ResolveMember<IDSharpMemberInfo>(name);
        }
        public IDSharpParameterInfo GetVariable(string name)
        {
            if (TryGetVariable(name, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to get variable \"{name}\"", nameof(name));
        }
        public IDSharpParameterInfo CreateVariable(string name, IDSharpType type)
        {
            if (TryCreateVariable(name, type, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to create variable \"{name}\"", nameof(name));
        }
        public IDSharpMemberInfo ResolvePropertyOrField(string name)
        {
            if (TryResolvePropertyOrField(name, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Unable to resolve property or field \"{name}\"", nameof(name));
        }
        public void ResolvePropertyOrFieldOrVariable(string name, Action<IDSharpMemberInfo> propertyOrFieldHandler, Action<IDSharpParameterInfo> variableHandler)
        {
            if (!TryResolvePropertyOrFieldOrVariable(name, propertyOrFieldHandler, variableHandler))
            {
                throw new ArgumentException($"Unable to resolve property, field or variable \"{name}\"", nameof(name));
            }
        }
        public List<IDSharpMethodInfo> ResolveMethods(string name, IList<IDSharpType>? genericParameters)
        {
            if (TryResolveMethods(name, genericParameters, out var result))
            {
                return result;
            }

            string message = $"Unable to resolve any method \"{name}\"";

            if (genericParameters != null && genericParameters.Count > 0)
            {
                message += $" with {genericParameters.Count} generic parameters";
            }

            throw new ArgumentException(message, nameof(name));
        }

        /// <summary>
        /// Get all types in current scope with specified name
        /// </summary>
        /// <param name="name">Name of type. It may includes namespace and declaring type</param>
        /// <returns>Enumeration of all types in current scope</returns>
        protected abstract IEnumerable<IDSharpType> GetTypes(string name);
        /// <summary>
        /// Get all members in current scope
        /// </summary>
        /// <returns>Enumeration of all members in current scope</returns>
        protected abstract IEnumerable<IDSharpMemberInfo> GetMembers();
        /// <summary>
        /// Get all variables in current scope
        /// </summary>
        /// <returns>Enumeration of all variables in current scope</returns>
        protected abstract IEnumerable<IDSharpParameterInfo> GetVariables();
        /// <summary>
        /// Create variable in current scope. If variable not created it was return <c>null</c>
        /// </summary>
        /// <param name="name">Name of new variable</param>
        /// <param name="type">Type of new variable</param>
        /// <returns>Created variable or <c>null</c> if variable with same name already exists in current scope</returns>
        protected abstract IDSharpParameterInfo? CreateLocalVariable(string name, IDSharpType type);

        private T? RecursiveCheck<T>(Func<DSharpCompilerScope, T?> handler)
        {
            DSharpCompilerScope? scope = this;
            T? result = default;

            while (scope != null && result == null)
            {
                result = handler(scope);
                scope = scope.Parent;
            }

            return result;
        }
    }
}
