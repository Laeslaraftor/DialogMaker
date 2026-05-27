using DialogMaker.Core.Executioning;

namespace DialogMaker.Core.Tests
{
    public class ByteCodeTests
    {
        [Test]
        public void ReadByteCode()
        {
            var pack = DialogPackageTests.Open();
            var dialog = pack["quest 3"]["perCharacterQest"];
            var dialogCode = dialog.GetCode();

            Console.WriteLine($"Code length: {dialogCode.Length}");

            var byteCodeData = DialogByteCodeData.Read(dialogCode);

            foreach (var section in byteCodeData.Sections)
            {
                int index = 0;
                Console.WriteLine($"Section {section.Index}:");

                foreach (var opcode in section.Operations)
                {
                    Console.WriteLine($"    {index}: {opcode}");
                    index++;
                }

            }
        }
    }
}
