namespace Weaver
{
    using System;
    using System.IO;

    class Program
    {
        private const string PatchedAssemblyName = "NestClient.Patched";

        private static readonly string InitialAssemblyPath = Path.GetFullPath(@"..\..\..\NestClient\bin\Debug\NestClient.dll");
        private static readonly string PatchedAssemblyPath = Path.GetFullPath(@"..\..\..\NestClient\bin\Debug\NestClient.patched.dll");

        private static void Main(string[] args)
        {
            if (!File.Exists(InitialAssemblyPath))
            {
                throw new Exception("NestClient.dll cannot be found. Compile NestClient project first!");
            }

            var weaver = new NestSearchWeaver(
                InitialAssemblyPath,
                PatchedAssemblyPath,
                PatchedAssemblyName);
            {
                weaver.Execute();
                weaver.Write();
            }

            Console.WriteLine();
            Console.Write("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
