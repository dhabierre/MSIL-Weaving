namespace NestClient.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        private const string ElasticHost = "http://127.0.0.1:9200";
        private const string ElasticIndex = "il-weaving-test";

        private static readonly string InitialAssemblyPath = Path.GetFullPath(@"..\..\..\NestClient\bin\Debug\NestClient.dll");
        private static readonly string PatchedAssemblyPath = Path.GetFullPath(@"..\..\..\NestClient\bin\Debug\NestClient.patched.dll");

        private static void Main(string[] args)
        {
            //InvokeInitialAssembly(); <- on n'a pas forcément de serveur Elasticsearch, alors on commente l'appel (#ES_BYPASS)
            InvokePatchedAssembly();

            Console.WriteLine();
            Console.Write("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static void InvokeInitialAssembly()
        {
            // Invocation des méthodes Search sur NestClient.dll (version initiale, pas de trace en output)

            Console.WriteLine($"Invoking {Path.GetFileName(InitialAssemblyPath)} (WITHOUT tracing)");
            Console.WriteLine();

            var initialAssembly = Assembly.LoadFrom(InitialAssemblyPath);

            var initialType = initialAssembly.GetTypes().First(t => t.FullName == "NestClient.NestSearchCaller");
            var initialInstance = Activator.CreateInstance(initialType, new[] { ElasticHost, ElasticIndex });

            var initialMethod_a_1 = initialType.GetMethod("ExecuteSearch_a_1");
            var initialMethod_a_2 = initialType.GetMethod("ExecuteSearch_a_2");
            var initialMethod_b_1 = initialType.GetMethod("ExecuteSearch_b_1");
            var initialMethod_b_2 = initialType.GetMethod("ExecuteSearch_b_2");
            var initialMethod_c_1 = initialType.GetMethod("ExecuteSearch_c_1");

            initialMethod_a_1.Invoke(initialInstance, null);
            initialMethod_a_2.Invoke(initialInstance, null);
            initialMethod_b_1.Invoke(initialInstance, null);
            initialMethod_b_2.Invoke(initialInstance, null);
            initialMethod_c_1.Invoke(initialInstance, null);
        }

        private static void InvokePatchedAssembly()
        {
            // Invocation des méthodes Search sur NestClient.patched.dll (version patchée, avec trace en output)

            Console.WriteLine($"Invoking {Path.GetFileName(PatchedAssemblyPath)} (WITH tracing)");
            Console.WriteLine();

            var patchedAssembly = Assembly.LoadFrom(PatchedAssemblyPath);

            var patchedType = patchedAssembly.GetTypes().First(t => t.FullName == "NestClient.NestSearchCaller");
            var patchedInstance = Activator.CreateInstance(patchedType, new[] { ElasticHost, ElasticIndex });

            var patchedMethod_a_1 = patchedType.GetMethod("ExecuteSearch_a_1");
            var patchedMethod_a_2 = patchedType.GetMethod("ExecuteSearch_a_2");
            var patchedMethod_b_1 = patchedType.GetMethod("ExecuteSearch_b_1");
            var patchedMethod_b_2 = patchedType.GetMethod("ExecuteSearch_b_2");
            var patchedMethod_c_1 = patchedType.GetMethod("ExecuteSearch_c_1");

            patchedMethod_a_1.Invoke(patchedInstance, null);
            patchedMethod_a_2.Invoke(patchedInstance, null);
            patchedMethod_b_1.Invoke(patchedInstance, null);
            patchedMethod_b_2.Invoke(patchedInstance, null);

            Console.WriteLine("--");
            patchedMethod_c_1.Invoke(patchedInstance, null); // 4 traces pour patchedMethod_c_1
            Console.WriteLine("--");
        }
    }
}
