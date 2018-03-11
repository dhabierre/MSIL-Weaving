namespace NestClient
{
    using System;
    using Nest;

    public class NestSearchCaller
    {
        private readonly IElasticClient elasticClient;

        public NestSearchCaller(string host, string defaultIndex)
        {
            this.elasticClient = new ElasticClient(new ConnectionSettings(new Uri(host), defaultIndex));
        }

        public ISearchResponse<Out> ExecuteSearch_a_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            return this.elasticClient.Search<Out>(searchRequest); // To change with Mono.Cecil
        }

        public ISearchResponse<Out> ExecuteSearch_a_2()
        {
            return this.elasticClient.Search<Out>(x => x.MatchAll()); // To change with Mono.Cecil
        }

        public ISearchResponse<Out> ExecuteSearch_b_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            return this.elasticClient.Search<In, Out>(searchRequest); // To change with Mono.Cecil
        }

        public ISearchResponse<Out> ExecuteSearch_b_2()
        {
            return this.elasticClient.Search<In, Out>(x => x.MatchAll()); // To change with Mono.Cecil
        }

        public ISearchResponse<Out> ExecuteSearch_c_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            this.elasticClient.Search<Out>(searchRequest); // To change with Mono.Cecil
            this.elasticClient.Search<Out>(x => x.MatchAll()); // To change with Mono.Cecil
            this.elasticClient.Search<In, Out>(searchRequest); // To change with Mono.Cecil
            this.elasticClient.Search<In, Out>(x => x.MatchAll()); // To change with Mono.Cecil

            return null;
        }
    }

    public class In { }

    public class Out { }
}
