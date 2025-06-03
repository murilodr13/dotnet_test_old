// using NUnit.Framework;
// using System;
// using System.IO;
// using System.IO.Compression;

// namespace MeuProjeto.Tests
// {
//     [TestFixture]
//     public class MeuProjetoTests
//     {
//         [Test]
//         public void TestHelloWorld()
//         {
//             // Arrange
//             StringWriter stringWriter = new StringWriter();
//             Console.SetOut(stringWriter);

//             // Act
//             Program.Main(null);
//             string output = stringWriter.ToString().Trim();

//             // Assert
//             Assert.AreEqual("Hello, World!", output);
//         }

//         [Test]
//         public void TestArtifactGeneration()
//         {
//             // Arrange
//             string artifactPath = Path.Combine(Environment.CurrentDirectory, "artifacts");
//             string zipPath = Path.Combine(artifactPath, "MeuProjeto.zip");

//             // Act
//             Program.Main(null);

//             // Assert
//             Assert.IsTrue(File.Exists(zipPath));
//         }
//     }
// }

using System;

namespace dotnet_test_old
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
        }
    }
}
