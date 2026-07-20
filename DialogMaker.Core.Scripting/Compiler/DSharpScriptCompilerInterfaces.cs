using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private void ValidateInterface(DSharpTypeBuilder type)
        {
            if (type.Constructors.Count > 0)
            {
                throw new ArgumentException($"Interfaces can not contains constructors: {type}");
            }
            if (type.Fields.Count > 0)
            {
                throw new ArgumentException($"Interfaces can not contains fields: {type}");
            }

            void Validate(DSharpVirtualizedMemberInfoBuilder member)
            {
                if (!member.IsStatic && member.Access != DSharpAccessModifier.Public)
                {
                    throw new ArgumentException($"Interfaces should contains only public member definitions. Invalid member \"{member}\" at \"{type}\"");
                }
                if (member.IsVirtual)
                {
                    throw new ArgumentException($"Interfaces can not contains virtual members. Invalid member \"{member}\" at \"{type}\"");
                }
                if (member.IsAbstract)
                {
                    throw new ArgumentException($"Interfaces can not contains abstract members. Invalid member \"{member}\" at \"{type}\"");
                }
            }
            void CheckBaseTypes(IDSharpType type)
            {
                foreach (var baseType in type.GetBaseTypes())
                {
                    if (baseType.ObjectType != DSharpObjectType.Interface)
                    {
                        throw new ArgumentException($"Interfaces can be extended only from other interfaces. Base type: \"{baseType}\", interface: {type}");
                    }

                    CheckBaseTypes(baseType);
                }
            }
            void CheckProperty(DSharpPropertyBuilder property)
            {
                if (!_propertiesWithCustomAccessors.Contains(property))
                {
                    Validate(property);
                }
            }

            if (type.Operators.Count > 0 ||
                type.CastOperators.Count > 0)
            {
                throw new ArgumentException($"Interfaces can not contains custom operators: \"{type}\"");
            }

            foreach (var property in type.Properties)
            {
                CheckProperty(property);
            }
            foreach (var indexer in type.Indexers)
            {
                CheckProperty(indexer);
            }
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                {
                    Validate(method);
                }
                if (!method.IsStatic && method.IsExtern)
                {
                    throw new ArgumentException($"Interfaces can not contains extern method declaration \"{method}\": \"{type}\"");
                }
            }

            CheckBaseTypes(type);
        }
        private void CheckInterfaceImplementation(DSharpTypeBuilder type, IDSharpType interfaceType)
        {
            if (interfaceType.ObjectType != DSharpObjectType.Interface ||
                type.ObjectType == DSharpObjectType.Interface)
            {
                return;
            }

            Dictionary<string, List<IDSharpMemberInfo>> membersToImplement = [];

            foreach (var member in interfaceType.GetInterfaceMembersToImplement())
            {
                if (!membersToImplement.TryGetValue(member.Name, out var list))
                {
                    list = [];
                    membersToImplement.Add(member.Name, list);
                }

                list.Add(member);
            }

            List<IDSharpMemberInfo> implementedDeclarations = [];

            foreach (var member in type.GetAllMembers(false, m => m.DeclaringType?.ObjectType != DSharpObjectType.Interface))
            {
                if (!membersToImplement.TryGetValue(member.Name, out var declarationMembers))
                {
                    continue;
                }

                implementedDeclarations.Clear();

                foreach (var declaration in declarationMembers)
                {
                    if (member.SameSignatureTo(declaration))
                    {
                        implementedDeclarations.Add(declaration);

                        if (member is DSharpPropertyBuilder propertyBuilder && declaration is IDSharpPropertyInfo propertyDeclaration)
                        {
                            propertyBuilder.AddImplementedProperty(propertyDeclaration);
                        }
                        else if (member is DSharpMethodBuilder methodBuilder && declaration is IDSharpMethodInfo methodDeclaration)
                        {
                            methodBuilder.AddImplementedMethod(methodDeclaration);
                        }
                    }
                }

                foreach (var implementedDeclaration in implementedDeclarations)
                {
                    declarationMembers.Remove(implementedDeclaration);
                }

                if (declarationMembers.Count == 0)
                {
                    membersToImplement.Remove(member.Name);
                }
            }

            List<IDSharpMemberInfo> notImplementedMembers = [];

            foreach (var info in membersToImplement)
            {
                foreach (var member in info.Value)
                {
                    notImplementedMembers.Add(member);
                }
            }
            if (notImplementedMembers.Count > 0)
            {
                throw new DSharpTypeNotImplementedMemberException(type, notImplementedMembers, $"\"{type}\" not implements interface \"{interfaceType}\".");
            }
        }
    }
}
