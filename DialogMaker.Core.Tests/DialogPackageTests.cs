using DialogMaker.Core.Common;

namespace DialogMaker.Core.Tests
{
    public class DialogPackageTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public static DialogPackage Open()
        {
            var package = DialogPackage.Open(@"C:\Users\Mdely\OneDrive\Documents\DialogsOutput\test.dpack");
            return package;
        }
        [Test]
        public void EmotionCheck()
        {
            var package = Open();
            var emotion = package.Resources.Emotions["angry"];

            Console.WriteLine(emotion);
        }
    }
}
