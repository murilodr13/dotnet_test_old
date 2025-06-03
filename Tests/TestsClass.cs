using NUnit.Framework;
using System;
using System.IO;
using dotnet_test_old;  // Aponte para o namespace do seu projeto principal

namespace dotnet_test_old.Tests
{
    [TestFixture]
    public class TestsClass
    {
        [Test]
        public void TestHelloWorldOutput()
        {
            // Arrange: redireciona a saída do Console para um StringWriter
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act: chama o Program.Main do projeto principal
            Program.Main(new string[0]);

            // Capture a saída e remova quebras de linha/extras
            var output = stringWriter.ToString().Trim();

            // Assert: verifica se o texto impresso é exatamente "Hello World"
            Assert.AreEqual("Hello World", output);
        }
    }
}
