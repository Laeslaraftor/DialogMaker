using DialogMaker.Core.Scripting.Runtime.Executor;
using System;
namespace DialogMaker.Core.Tests
{
    public class StackTests
    {
        [Test]
        public static void PushTest()
        {
            DSharpStack stack = new();

            stack.Push();
            stack.Push(1.22d);
            stack.Push(10);

            void PrintStack()
            {
                for (uint i = 0; i < stack.Count; i++)
                {
                    var frame = stack.Peek(i);
                    Console.WriteLine(frame.ValueType);
                }
            }

            PrintStack();

            Console.WriteLine();
            Console.WriteLine("Pop with offset 1");
            stack.Pop(1);

            Console.WriteLine();
            PrintStack();
        }
    }
}
