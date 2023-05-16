using System;
using System.IO;
using System.IO.Compression;

namespace MeuProjeto
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Gere um arquivo ZIP contendo o executável como artefato
            string artifactPath = Path.Combine(Environment.CurrentDirectory, "artifacts");
            Directory.CreateDirectory(artifactPath);
            string zipPath = Path.Combine(artifactPath, "MeuProjeto.zip");
            File.Copy("MeuProjeto.dll", Path.Combine(artifactPath, "MeuProjeto.dll"));

            ZipFile.CreateFromDirectory(artifactPath, zipPath);
            Console.WriteLine("Artefato gerado em: " + zipPath);
        }
    }
}
