namespace DialogMaker.Core.Tests
{
    public class OperandValueTests
    {
        [Test]
        public static void BoolTest()
        {
            OperandValue operand = "-1";

            Console.WriteLine(operand.ToNumber());
            Console.WriteLine(operand.ToBool());
        }
    }
}
