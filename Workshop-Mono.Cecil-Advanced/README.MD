# MSIL Weaving / Mono.Cecil / Exemple avancé

## Définition

Vous pouvez consulter la page [MSIL Injection](https://www.postsharp.net/aop.net/msil-injection) de PostSharp pour avoir une définition claire du concept.

Il existe plusieurs outils et frameworks permettant de transformer le code IL ou d'avoir une approche [AOP](https://en.wikipedia.org/wiki/Aspect-oriented_programming). \
Certains outils agissent post-compile, d'autres au runtime.

Quelques outils intéressants
- [PostShap](https://www.postsharp.net/) (outil)
- [Fody](https://github.com/Fody/Fody) (framework)
- ...


## Workshops

1. [Mono.Cecil - Exemple simple]()
2. [Mono.Cecil - Exemple avancé]()
3. [Fody]()

Dans ce papier, nous allons utiliser [Mono.Cecil](www.mono-project.com/Cecil/) pour injecter du code IL. \
Mono.Cecil est LA référence en la matière ! La plupart des outils et frameworks se basent sur ce dernier.

## Workshop

Dans cet exemple, nous souhaitons tracer le temps d'exécution des méthodes de recherches `Search` fournies par la librairie [Nest](https://www.elastic.co/guide/en/elasticsearch/client/net-api/master/introduction.html) (API de requétage pour Elasticsearch) sans modifier cette dernière.

La librairie Nest expose 4 méthodes :
```
ISearchResponse<T> Search<T> (Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector)
ISearchResponse<T> Search<T>(ISearchRequest request)
ISearchResponse<TResult> Search<T, TResult>(ISearchRequest request)
ISearchResponse<TResult> Search<T, TResult>(Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector)
```

Il convient d'analyser le code appelant faisant appel aux différentes méthodes Search et d'injecter le traitement voulu dans le code IL via
Mono.Cecil.

### Contenu des sources

- **NestClient** (projet Library) \
  Contient des méthodes faisant appel aux différentes méthodes Search. \
  Il s'agit de l'assembly qui va être compilée puis transformée par injection IL.

- **NestClient.Extended** (projet Library) \
  Contient la classe NestSearchWrapper avec les 4 méthodes Search disposants de la trace d'exécution (ici, la trace se fera dans la console). \
  Ce projet sera linké sur l'assembly transformée pour que les appels Search soient reroutés sur ces 4 méthodes.
  ```
  public static ISearchResponse<T> Search<T>(IElasticClient elasticClient, ISearchRequest request) where T : class
  {
      var stopwatch = Stopwatch.StartNew();
      var response = elasticClient.Search<T>(request);
      Console.WriteLine($"[Search<T>(ISearchRequest) ] Duration: {stopwatch.ElapsedMilliseconds}ms");
      return response;
  }

  public static ISearchResponse<T> Search<T>(IElasticClient elasticClient, Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector) where T : class
  {
      var stopwatch = Stopwatch.StartNew();
      var response = elasticClient.Search(searchSelector);
      Console.WriteLine($"[Search<T, TResult>(Func<...>) ] Duration: {stopwatch.ElapsedMilliseconds}ms");
      return response;
  }

  [... 2 signatures supplémentaires sur <T, TResult>...]
  ```

- **NestHooking** (projet Console) \
  Va servir de weaver pour la transformation (patching).

- **NestClient.Tests** (projet Console) \
  Va servir pour tester l'invocation (dynamiquement) de la librairie NestClient.dll de base & NestClient.patched.dll issue du traitement NestHooking.

### Mise en pratique

Effectuez les opération suivantes :
1. Compiler toute la solution (F5 ne suffit pas)
2. Exécuter Weaver pour effectuer la transformation (résultat dans NestClient\bin\Debug\NestClient.patch.dll)
3. Exécuter NestClient.Tests pour voir le résultat

#### Output de NestClient.Tests

```
Invoking NestClient.dll (WITHOUT tracing)

Invoking NestClient.patched.dll (WITH tracing)

[Search<T>(ISearchRequest)         ] Duration: 20ms
[Search<T, TResult>(Func<...>)     ] Duration: 18ms
[Search<T, TResult>(ISearchRequest)] Duration: 3ms
[Search<T, TResult>(Func<...>)     ] Duration: 18ms
--
[Search<T>(ISearchRequest)         ] Duration: 3ms
[Search<T, TResult>(Func<...>)     ] Duration: 0ms
[Search<T, TResult>(ISearchRequest)] Duration: 3ms
[Search<T, TResult>(Func<...>)     ] Duration: 0ms
--
Press ENTER to exit...
```

### Weaver

#### Invocation

`Weaver\Program.cs`

```
namespace NestHooking
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

```

#### Source

`NestHooking\Weavers\NestSearchWeaver.cs`

1. Charge la librairie `NestClient.dll` en mémoire via Mono.Cecil.
2. Ajoute à la librairie une référence vers la librairie `NestClient.Extended.dll` qui contient les 4 appels Search + tracing.
3. Parcourt les types de l'assembly. Pour chaque type, détermine les méthodes faisant appel à l'une des méthodes Search de Nest.
4. Pour chacune des méthodes concernées, transforme le code IL des appels à Search via les implémentations définies dans `NestClient.Extended.NestSearchWrapper.cs`.

NestSearchWeaver
```
TODO ?
```

#### IL

IL avant injection/transformation :
```
.method public hidebysig
   instance class [Nest]Nest.ISearchResponse`1<class NestClient.Out>
ExecuteSearch_a_1 () cil managed
{
   .maxstack 3
   .locals init (
      [0] class [Nest]Nest.SearchRequest searchRequest,
      [1] class [Nest]Nest.ISearchResponse`1<class NestClient.Out>
   )
   IL_0000: nop
   IL_0001: newobj instance void [Nest]Nest.SearchRequest::.ctor()
   IL_0006: dup
   IL_0007: ldc.i4.0
   IL_0008: newobj instance void valuetype
[mscorlib]System.Nullable`1<int32>::.ctor(!0)
   IL_000d: callvirt instance void [Nest]Nest.SearchRequest::set_From(valuetype
[mscorlib]System.Nullable`1<int32>)
   IL_0012: nop
   IL_0013: dup
   IL_0014: ldc.i4.5
   IL_0015: newobj instance void valuetype
[mscorlib]System.Nullable`1<int32>::.ctor(!0)
   IL_001a: callvirt instance void [Nest]Nest.SearchRequest::set_Size(valuetype
[mscorlib]System.Nullable`1<int32>)
   IL_001f: nop
   IL_0020: stloc.0
   IL_0021: ldarg.0
   IL_0022: ldfld class [Nest]Nest.IElasticClient
NestClient.NestSearchCaller::elasticClient
   IL_0027: ldloc.0
   // on va transformer cette instruction via NestHooking en faisant appel à la
   // bonne méthode NestClient.Extended.NestSearchWrapper.Search<...>(...)
   IL_0028: callvirt instance class [Nest]Nest.ISearchResponse`1<!!0>
[Nest]Nest.IElasticClient::Search<class NestClient.Out>(class
[Nest]Nest.ISearchRequest)
   IL_002d: stloc.1
   IL_002e: br.s IL_0030
   IL_0030: ldloc.1
   IL_0031: ret
}
```

IL après injection/transformation :

```
.method public hidebysig
   instance class [Nest]Nest.ISearchResponse`1<class NestClient.Out>
ExecuteSearch_a_1 () cil managed
{
   .maxstack 3
   .locals init (
      [0] class [Nest]Nest.SearchRequest searchRequest,
      [1] class [Nest]Nest.ISearchResponse`1<class NestClient.Out>
   )
   IL_0000: nop
   IL_0001: newobj instance void [Nest]Nest.SearchRequest::.ctor()
   IL_0006: dup
   IL_0007: ldc.i4.0
   IL_0008: newobj instance void valuetype
[mscorlib]System.Nullable`1<int32>::.ctor(!0)
   IL_000d: callvirt instance void [Nest]Nest.SearchRequest::set_From(valuetype
[mscorlib]System.Nullable`1<int32>)
   IL_0012: nop
   IL_0013: dup
   IL_0014: ldc.i4.5
   IL_0015: newobj instance void valuetype
[mscorlib]System.Nullable`1<int32>::.ctor(!0)
   IL_001a: callvirt instance void [Nest]Nest.SearchRequest::set_Size(valuetype
[mscorlib]System.Nullable`1<int32>)
   IL_001f: nop
   IL_0020: stloc.0
   IL_0021: ldarg.0
   IL_0022: ldfld class [Nest]Nest.IElasticClient
NestClient.NestSearchCaller::elasticClient
   IL_0027: ldloc.0
   IL_0028: call class [Nest]Nest.ISearchResponse`1<!!0>
[NestClient.Extended]NestClient.Extended.NestSearchWrapper::Search<class
   NestClient.Out>(class [Nest]Nest.IElasticClient, class [Nest]Nest.ISearchRequest) // TADA !!
   IL_002d: stloc.1
   IL_002e: br.s IL_0030
   IL_0030: ldloc.1
   IL_0031: ret
}
```