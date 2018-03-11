namespace NestClient
{
    using System;
    using Nest;

    public class NestSearchCallerWanted
    {
        private readonly IElasticClient elasticClient;

        public NestSearchCallerWanted(string host, string defaultIndex)
        {
            this.elasticClient = new ElasticClient(new ConnectionSettings(new Uri(host), defaultIndex));
        }
        
        public ISearchResponse<Out> ExecuteSearch_a_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            return Extended.NestSearchWrapper.Search<Out>(this.elasticClient, searchRequest);
        }

        public ISearchResponse<Out> ExecuteSearch_a_2()
        {
            return Extended.NestSearchWrapper.Search<Out>(this.elasticClient, x => x.MatchAll());
        }

        public ISearchResponse<Out> ExecuteSearch_b_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            return Extended.NestSearchWrapper.Search<In, Out>(this.elasticClient, searchRequest);
        }

        public ISearchResponse<Out> ExecuteSearch_b_2()
        {
            return Extended.NestSearchWrapper.Search<In, Out>(this.elasticClient, x => x.MatchAll());
        }

        public ISearchResponse<Out> ExecuteSearch_c_1()
        {
            var searchRequest = new SearchRequest { From = 0, Size = 5 };

            Extended.NestSearchWrapper.Search<Out>(this.elasticClient, searchRequest);
            Extended.NestSearchWrapper.Search<Out>(this.elasticClient, x => x.MatchAll());
            Extended.NestSearchWrapper.Search<In, Out>(this.elasticClient, searchRequest);
            Extended.NestSearchWrapper.Search<In, Out>(this.elasticClient, x => x.MatchAll());

            return null;
        }
    }
}
