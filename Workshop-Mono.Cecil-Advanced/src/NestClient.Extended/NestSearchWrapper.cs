namespace NestClient.Extended
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Nest;

    public static class NestSearchWrapper
    {
        public static ISearchResponse<T> Search<T>(IElasticClient elasticClient, ISearchRequest request)
            where T : class
        {
            ISearchResponse<T> response = null;

            var stopwatch = Stopwatch.StartNew();

            // on n'a pas forcément de serveur Elasticsearch, alors on commente l'appel (#ES_BYPASS)
            //response = elasticClient.Search<T>(request);

            // on simule l'attente de l'appel au serveur Elasticsearch
            Thread.Sleep(new Random().Next(10, 50));

            Console.WriteLine($"[Search<T>(ISearchRequest)         ] Duration: {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }

        public static ISearchResponse<T> Search<T>(IElasticClient elasticClient, Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector)
            where T : class
        {
            ISearchResponse<T> response = null;

            var stopwatch = Stopwatch.StartNew();

            // on n'a pas forcément de serveur Elasticsearch, alors on commente l'appel (#ES_BYPASS)
            //response = elasticClient.Search(searchSelector);

            // on simule l'attente de l'appel au serveur Elasticsearch
            Thread.Sleep(new Random().Next(10, 50));

            Console.WriteLine($"[Search<T, TResult>(Func<...>)     ] Duration: {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }

        public static ISearchResponse<TResult> Search<T, TResult>(IElasticClient elasticClient, ISearchRequest request)
            where T : class
            where TResult : class
        {
            ISearchResponse<TResult> response = null;

            var stopwatch = Stopwatch.StartNew();

            // on n'a pas forcément de serveur Elasticsearch, alors on commente l'appel (#ES_BYPASS)
            //response = elasticClient.Search<T, TResult>(request);

            // on simule l'attente de l'appel au serveur Elasticsearch
            Thread.Sleep(new Random().Next(10, 50));

            Console.WriteLine($"[Search<T, TResult>(ISearchRequest)] Duration: {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }

        public static ISearchResponse<TResult> Search<T, TResult>(IElasticClient elasticClient, Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector)
            where T : class
            where TResult : class
        {
            ISearchResponse<TResult> response = null;

            var stopwatch = Stopwatch.StartNew();

            // on n'a pas forcément de serveur Elasticsearch, alors on commente l'appel (#ES_BYPASS)
            //response = elasticClient.Search<T, TResult>(searchSelector);

            // on simule l'attente de l'appel au serveur Elasticsearch
            Thread.Sleep(new Random().Next(10, 50));

            Console.WriteLine($"[Search<T, TResult>(Func<...>)     ] Duration: {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
    }
}
