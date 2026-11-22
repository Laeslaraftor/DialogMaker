using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler
{
    public partial class DSharpScriptCompiler
    {
        private void CompileEvent(DSharpEventBuilder @event, EventNode node)
        {
            var addMethod = @event.AddMethod;
            var removeMethod = @event.RemoveMethod;

            if (node.Add != null || node.Remove != null)
            {
                if (node.Add == null || node.Remove == null)
                {
                    throw new DSharpCompilerException($"Event \"{@event}\" requires both \"add\" and \"remove\" accessors", node);
                }

                CompileMethod(addMethod, node.Add);
                CompileMethod(removeMethod, node.Remove);
                return;
            }
            if (@event.DeclaringType?.ObjectType == DSharpObjectType.Interface)
            {
                return;
            }

            var delegateInfo = Assembly.DelegateTypeInfo;
            
            if (delegateInfo.CombineMethod == null)
            {
                throw new DSharpCompilerException($"There no \"Combine(System.Delegate, System.Delegate)\" method at \"{delegateInfo.Type}\"", node);
            }
            if (delegateInfo.RemoveMethod == null)
            {
                throw new DSharpCompilerException($"There no \"Remove(System.Delegate, System.Delegate)\" method at \"{delegateInfo.Type}\"", node);
            }

            var handlersField = @event.CreateHandlersField();
            var addCode = addMethod.GetBytecodeBuilder();
            var removeCode = removeMethod.GetBytecodeBuilder();

            static void CompileAccessor(DSharpBytecodeBuilder code, DSharpFieldBuilder handlersField, IDSharpMethodInfo delegateHandler, bool skipNull = false)
            {
                DSharpBytecodeBuilder.ReferenceInstruction? skipInstruction = null;

                code.LoadInstance();
                code.LoadPropertyOrField(handlersField);

                if (skipNull)
                {
                    code.Push(null);
                    code.Equals();
                    skipInstruction = code.JumpIfFalse();
                    code.Return();
                }

                skipInstruction?.ReferencedInstruction = code.PopRepeat(2);
                code.LoadLocal(code.Method.Parameters[0]);
                code.Call(delegateHandler);
                code.PopPreviousTwo();
                code.StorePropertyOrField(handlersField);
            }

            CompileAccessor(addCode, handlersField, delegateInfo.CombineMethod);
            CompileAccessor(removeCode, handlersField, delegateInfo.RemoveMethod, true);
        }
    }
}
