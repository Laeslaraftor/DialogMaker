using DialogMaker.Core.Common;

namespace DialogMaker.Core.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Open()
        {
            var package = DialogPackage.Open(@"C:\Users\Mdely\OneDrive\Documents\DialogsOutput\test.dpack");

            Console.WriteLine(package.Folders.Count);
        }
        [Test]
        public void EmotionCheck()
        {
            var package = DialogPackage.Open(@"C:\Users\Mdely\OneDrive\Documents\DialogsOutput\test.dpack");
            var emotion = package.Resources.Emotions["angry"];

            Console.WriteLine(emotion);
        }
    }
}
