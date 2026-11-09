using DialogMaker.Core.Tests.Collections;

namespace DialogMaker.Core.Tests
{
    internal class CustomCollectionsTests
    {
        [Test]
        [TestCase("value,value1,value,union,union2")]
        public static void TestHashSet(string values)
        {
            CustomHashSet<string> hashSet = new();

            foreach (var value in values.Split(','))
            {
                if (hashSet.Add(value))
                {
                    Console.WriteLine(value);
                }
            }
        }
    }
}
