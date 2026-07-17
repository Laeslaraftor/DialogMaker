using DialogMaker.Core.Scripting.Runtime.Executor;
using NUnit.Framework.Constraints;
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
        [Test]
        [TestCase(0u, 1u)]
        [TestCase(1u, 1u)]
        [TestCase(1u, 2u)]
        [TestCase(1u, 20u)]
        public static void StackOffsetRemoveTest(uint offset, uint count)
        {
            DSharpStack stack = new();

            for (int i = 0; i < 10; i++)
            {
                stack.Push(i);
            }

            StackOffsetCount(stack, offset, count);
        }
        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(1, 20)]
        public static void OffsetRemoveTest(int offset, int count)
        {
            List<int> items = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            void PrintItems(string title, List<int> items)
            {
                Console.WriteLine($"({items.Count}) " + title);

                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }
            }

            PrintItems("start items:", items);
            RemoveOffsetCount(items, offset, count);
            PrintItems("now items:", items);
        }

        private static void RemoveOffsetCount(List<int> items, int offset, int count)
        {
            int currentIndex = items.Count - 1;
            int index = currentIndex - offset + 1;
            int startIndex = index - count;

            if (0 > startIndex)
            {
                startIndex = 0;
            }

            int totalCount = items.Count - index;

            for (int i = 0; i < totalCount; i++)
            {
                items[startIndex + i] = items[index + i];
            }
            
            for (int i = 0; i < index - startIndex; i++)
            {
                items.RemoveAt(items.Count - 1);
            }
        }
        private static void StackOffsetCount(DSharpStack stack, uint offset, uint count)
        {
            void PrintStack(string title)
            {
                Console.WriteLine($"({stack.Count}) {title}");

                for (uint i = 0; i < stack.Count; i++)
                {
                    Console.WriteLine(stack.Peek(i).Read<int>());
                }
            }

            PrintStack("Start values: ");
            stack.Pop(offset, count);
            PrintStack("Now values: ");
        }
    }
}
