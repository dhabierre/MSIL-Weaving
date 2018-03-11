namespace Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Pdb;
    using Mono.Cecil.Rocks;

    internal class NestSearchWeaver
    {
        private readonly string initialAssemblyPath;
        private readonly string patchedAssemblyPath;
        private readonly string patchedAssemblyName;

        private readonly ModuleDefinition moduleDefinition;

        public NestSearchWeaver(string initialAssemblyPath, string patchedAssemblyPath, string patchedAssemblyName)
        {
            this.initialAssemblyPath = initialAssemblyPath;
            this.patchedAssemblyPath = patchedAssemblyPath;
            this.patchedAssemblyName = patchedAssemblyName;

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(
                this.initialAssemblyPath,
                new ReaderParameters { ReadSymbols = true });

            this.moduleDefinition = assemblyDefinition.MainModule;
        }

        public void Execute()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Analyzing {initialAssemblyPath}...");
            Console.ResetColor();

            // ajout de la référence vers l'assembly NestClient.Extended qui contient la classe NestSearchWrapper & méthodes Search<...>() à utiliser (pour ajouter la trace) lors de l'injection IL
            this.moduleDefinition.Import(typeof(NestClient.Extended.NestSearchWrapper));

            foreach (var type in GetTypesToWeave())
            {
                foreach (var method in GetMethodsToWeave(type))
                {
                    this.WeaveMethod(method);
                }
            }
        }

        public void Write()
        {
            if (this.moduleDefinition.AssemblyReferences.Any(r => r.FullName == this.moduleDefinition.Assembly.FullName))
            {
                throw new Exception($"Something is going wrong: {this.moduleDefinition.Assembly.Name.Name} assembly has a ref on itself (-> bad BuildMethod() code?)...");
            }

            this.moduleDefinition.Assembly.Name.Name = this.patchedAssemblyName;

            this.moduleDefinition.Write(
                this.patchedAssemblyPath,
                new WriterParameters
                {
                    WriteSymbols = true,
                    SymbolWriterProvider = new PdbWriterProvider()
                });

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{patchedAssemblyPath} has been written!");
            Console.ResetColor();
        }

        private IEnumerable<TypeDefinition> GetTypesToWeave()
        {
            foreach (var type in this.moduleDefinition.GetTypes())
            {
                if (type.Name != "<Module>")
                {
                    Console.WriteLine($"Type: {type.Name}");

                    yield return type;
                }
            }
        }

        private IEnumerable<MethodDefinition> GetMethodsToWeave(TypeDefinition type)
        {
            foreach (var method in type.Methods)
            {
                Console.WriteLine($"Method: {method.Name}");

                if (FindNestSearchMethodIndices(method).Any())
                {
                    yield return method;
                }
            }
        }

        private void WeaveMethod(MethodDefinition method)
        {
            var indices = FindNestSearchMethodIndices(method);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Weaving {method.Name} -> {indices.Count()} IL instruction(s) found");
            Console.ResetColor();

            foreach (var index in indices)
            {
                var instructions = method.Body.Instructions;
                var instruction = instructions[index];

                var searchMethodRef = this.BuildMethod(instruction);

                instructions[index] = Instruction.Create(OpCodes.Call, searchMethodRef);
            }

            method.Body.SimplifyMacros();
            method.Body.OptimizeMacros();
        }

        private MethodReference BuildMethod(Instruction instruction)
        {
            // On va contruire dans un premier temps la MethodInfo qui correspond à la bonne méthode NestSearchWrapper.Search<...>(...) en fonction de l'instruction IL.
            // Puis on va contruire la MethodReference générique qui sera utilisée pour remplacer l'instruction IL cible.

            MethodInfo searchMethodInfo = null;

            var nestISearchRequestType = typeof(Nest.ISearchRequest);
            var nestIElasticClientType = typeof(Nest.IElasticClient);

            var nestSearchWrapperType = typeof(NestClient.Extended.NestSearchWrapper);

            var genericArguments = instruction.GetGenericArguments();
            var parameter = instruction.GetParameters().First(); // always 1, checked @ MethodsToWeave()

            if (parameter.ParameterType.FullName == nestISearchRequestType.FullName) // paramètre ISearchRequest -> on tape sur Search<T>(ISearchRequest) ou Search<T, TResult>(ISearchRequest) de la NestSearchWrapper (selon genericArguments.Count)
            {
                searchMethodInfo =
                    nestSearchWrapperType
                        .GetMethods()
                        .Where(m => m.Name == "Search")
                        .Select(x => new { M = x, A = x.GetGenericArguments(), P = x.GetParameters() })
                        .Where(x =>
                               x.A.Length == genericArguments.Count &&
                               x.P.Length == 2 &&
                               x.P[0].ParameterType == nestIElasticClientType &&
                               x.P[1].ParameterType == nestISearchRequestType)
                        .Select(x => x.M)
                        .Single();
            }
            else // paramètre Func<..., ...> -> on tape sur Search<T>(Func<..., ...>) ou Search<T, TResult>(Func<..., ...>) de la classe NestSearchWrapper (selon genericArguments.Count)
            {
                searchMethodInfo =
                    nestSearchWrapperType
                        .GetMethods()
                        .Where(m => m.Name == "Search")
                        .Select(x => new { M = x, A = x.GetGenericArguments(), P = x.GetParameters() })
                        .Where(x =>
                               x.A.Length == genericArguments.Count &&
                               x.P.Length == 2 &&
                               x.P[0].ParameterType == nestIElasticClientType &&
                               x.P[1].ParameterType.IsGenericInstance(typeof(Func<,>)) &&
                               x.P[1].ParameterType.GetGenericArguments().Skip(0).First().IsGenericInstance(typeof(Nest.SearchDescriptor<>)) &&
                               x.P[1].ParameterType.GetGenericArguments().Skip(1).First().IsGenericInstance(typeof(Nest.SearchDescriptor<>)))
                        .Select(x => x.M)
                        .Single();
            }

            var searchMethodRef =
                this.moduleDefinition
                    .Import(searchMethodInfo) // enregistrement de la méthode
                    .MakeGeneric(genericArguments.ToArray()); // typage la méthode par la suite, sur la MethodReference enregistrée

            return searchMethodRef;
        }

        private static IEnumerable<int> FindNestSearchMethodIndices(MethodDefinition method)
        {
            var indices = new List<int>();

            // Essaie de trouver les (éventuels) indexes des instructions Nest.IElasticClient::Search<...>(...) :

            // callvirt instance class [Nest]Nest.ISearchResponse`1<!!0> [Nest]Nest.IElasticClient::Search<class NestClient.Out>(class [Nest]Nest.ISearchRequest)
            // callvirt instance class [Nest]Nest.ISearchResponse`1<!!0> [Nest]Nest.IElasticClient::Search<class NestClient.Out>(class [mscorlib]System.Func`2<class [Nest]Nest.SearchDescriptor`1<!!0>, class [Nest]Nest.SearchDescriptor`1<!!0>>)
            // callvirt instance class [Nest]Nest.ISearchResponse`1<!!1> [Nest]Nest.IElasticClient::Search<class NestClient.In, class NestClient.Out>(class [Nest]Nest.ISearchRequest)
            // callvirt instance class [Nest]Nest.ISearchResponse`1<!!1> [Nest]Nest.IElasticClient::Search<class NestClient.In, class NestClient.Out>(class [mscorlib]System.Func`2<class [Nest]Nest.SearchDescriptor`1<!!0>, class [Nest]Nest.SearchDescriptor`1<!!0>>)

            // Signatures côté IElasticClient/ElasticClient :

            // ISearchResponse<T> Search<T> (Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector) where T : class;
            // ISearchResponse<T> Search<T>(ISearchRequest request) where T : class;
            // ISearchResponse<TResult> Search<T, TResult>(ISearchRequest request) where T : class where TResult : class;
            // ISearchResponse<TResult> Search<T, TResult>(Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector)

            var instructions = method.Body.Instructions;

            var nestIElasticClientType = typeof(Nest.IElasticClient);
            var nestElasticClientType = typeof(Nest.ElasticClient);

            foreach (var instruction in instructions.Where(i => i.OpCode == OpCodes.Callvirt && i.Operand is MethodReference)) // -> call : callvirt instance class
            {
                var operand = (MethodReference)instruction.Operand;

                if (operand.DeclaringType.Namespace != nestElasticClientType.Namespace) // -> namespace : [Nest]
                {
                    continue;
                }

                if (operand.DeclaringType.Name != nestIElasticClientType.Name && // -> type : [Nest]Nest.IElasticClient | [Nest]Nest.ElasticClient
                    operand.DeclaringType.Name != nestElasticClientType.Name)
                {
                    continue;
                }

                if (operand.Name != "Search") // -> méthode : [Nest]Nest.IElasticClient::Search()
                {
                    continue;
                }

                var genericArguments = instruction.GetGenericArguments();
                var parameters = instruction.GetParameters();

                if (genericArguments.Count == 0 || // -> méthode générique : [Nest]Nest.IElasticClient::Search<T>() | [Nest]Nest.IElasticClient::Search<T1, T2>()
                    genericArguments.Count > 2)
                {
                    continue;
                }

                if (parameters.Count != 1) // -> paramètre (1 seul) : [Nest]Nest.IElasticClient::Search<T>(ISearchRequest) | [Nest]Nest.IElasticClient::Search<T1, T2>(Func<...>)
                {
                    continue;
                }

                indices.Add(instructions.IndexOf(instruction));
            }

            return indices;
        }
    }
}
