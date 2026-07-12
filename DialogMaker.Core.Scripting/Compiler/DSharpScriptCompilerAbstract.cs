using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private void ValidateVirtualization(DSharpTypeBuilder type)
        {
            void Validate(DSharpVirtualizedMemberInfoBuilder member)
            {
                if (!member.IsVirtual && !member.IsAbstract)
                {
                    return;
                }
                if (type.IsValueType() && (member.IsAbstract || !member.IsSealed))
                {
                    throw new InvalidOperationException($"Invalid member \"{member}\" at \"{type}\". Value type can not contains abstract and virtual members.");
                }
                if (member.IsAbstract && !type.IsAbstract)
                {
                    throw new ArgumentException($"Abstract member \"{member}\" can not be declared in non abstract type \"{type}\"");
                }
                if (member.Access == DSharpAccessModifier.Private)
                {
                    throw new ArgumentException($"Invalid access modifier at \"{member}\" in \"{type}\". Abstract and virtual members can not be private");
                }
            }

            foreach (var property in type.Properties)
            {
                Validate(property);
            }
            foreach (var indexer in type.Indexers)
            {
                Validate(indexer);
            }
            foreach (var method in type.Methods)
            {
                Validate(method);
            }
        }
        private void CheckImplementation(DSharpTypeBuilder type)
        {
            if (type.BaseTypes.Count > 0)
            {
                bool haveEmptyConstructor = false;

                foreach (var baseType in type.GetBaseTypes().Where(t => t.ObjectType == DSharpObjectType.Class))
                {
                    int constructorsCount = 0;

                    foreach (var constructor in baseType.GetConstructors())
                    {
                        if (constructor.GetParameters().Length == 0)
                        {
                            haveEmptyConstructor = true;
                            break;
                        }

                        constructorsCount++;
                    }

                    if (constructorsCount == 0)
                    {
                        haveEmptyConstructor = true;
                    }
                    if (haveEmptyConstructor)
                    {
                        break;
                    }
                }

                if (!haveEmptyConstructor)
                {
                    bool allInvokesBaseConstructor = true;

                    foreach (var constructor in type.Constructors)
                    {
                        if (!_createdConstructors.TryGetValue(constructor, out var node) ||
                            node.Type == DSharpConstructorType.Default)
                        {
                            allInvokesBaseConstructor = false;
                            break;
                        }
                    }

                    if (!allInvokesBaseConstructor)
                    {
                        throw new InvalidOperationException($"Type \"{type}\" should call inherited constructors in all of it constructors because base type not contains empty constructor");
                    }
                }
            }
            if (type.IsAbstract)
            {
                return;
            }

            Dictionary<string, List<IDSharpMemberInfo>> membersToImplement = [];

            foreach (var baseType in type.BaseTypes.Where(t => t.ObjectType == DSharpObjectType.Class))
            {
                foreach (var member in baseType.GetAbstractMembersToImplement())
                {
                    if (!membersToImplement.TryGetValue(member.Name, out var list))
                    {
                        list = [];
                        membersToImplement.Add(member.Name, list);
                    }

                    list.Add(member);
                }
            }

            if (membersToImplement.Count == 0)
            {
                return;
            }

            List<IDSharpMemberInfo> implementedDeclarations = [];

            foreach (var member in type.GetAllMembers(false, m => m.DeclaringType?.ObjectType != DSharpObjectType.Interface &&
                                                                  !m.IsDeclaration))
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
                throw new DSharpTypeNotImplementedMemberException(type, notImplementedMembers, $"\"{type}\" not implements all inherited abstract members.");
            }
        }
    }
}
