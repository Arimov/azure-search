using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Indexer
{
    class Program
    {
        private static SearchIndexClient _indexClient;
        private static SearchClient _searchClient;
        static IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        static IConfigurationRoot configuration = builder.Build();

        static async Task Main()
        {
            string searchServiceUri = configuration["SearchServiceUri"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];            

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(adminApiKey));
            SearchIndexerClient indexerClient = new SearchIndexerClient(new Uri(searchServiceUri), new AzureKeyCredential(adminApiKey));

            string indexName = "book";

            // Delete the search index, if exist
            Console.WriteLine($"Deleting index {indexName} if exist...\n");
            await DeleteIndexIfExistsAsync(indexName, indexClient);

            // Create sesrach index
            Console.WriteLine("Creating index...\n");
            await CreateIndexAsync(indexName, indexClient);

            // Set up a SQL data source and indexer, and run the indexer to import book metadata from SQL DB
            Console.WriteLine("Indexing SQL DB meta data...\n");
            await CreateAndRunSQLIndexerAsync(indexName, indexerClient);


            Console.WriteLine("Complete.  Press any key to end application...\n");
            Console.ReadKey();
        }


        private static async Task DeleteIndexIfExistsAsync(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                await indexClient.GetIndexAsync(indexName);
                await indexClient.DeleteIndexAsync(indexName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified index not exist, 404 will be thrown.
            }
        }

        private static async Task CreateIndexAsync(string indexName, SearchIndexClient indexClient)
        {
            // Create a new search index structure that matches the properties of the Book class.
            FieldBuilder bulder = new FieldBuilder();
            var definition = new SearchIndex(indexName, bulder.Build(typeof(Book)));

            await indexClient.CreateIndexAsync(definition);
        }

        private static async Task CreateAndRunSQLIndexerAsync(string indexName, SearchIndexerClient indexerClient)
        {

            SearchIndexerDataSourceConnection sqlDataSource = new SearchIndexerDataSourceConnection(
                name: configuration["SQLDatabaseName"],
                type: SearchIndexerDataSourceType.AzureSql,
                connectionString: configuration["SQLConnectSctring"],
                container: new SearchIndexerDataContainer("books"));

            // The data source does not need to be deleted if it already exists,
            // but the connection string might need to be updated if it has changed.            
            await indexerClient.CreateOrUpdateDataSourceConnectionAsync(sqlDataSource);

            Console.WriteLine("Creating SQL indexer...\n");

            SearchIndexer sqlIndexer = new SearchIndexer(
                name: "books-indexer",
                dataSourceName: sqlDataSource.Name,
                targetIndexName: indexName)
            {
                //here you can set the desired schedule for indexing repetitions
                Schedule = new IndexingSchedule(TimeSpan.FromDays(1))  
            };

            // Indexers keep metadata about how much they have already indexed.
            // If we already ran this sample, the indexer will remember that it already
            // indexed the sample data and not run again.
            // To avoid this, reset the indexer if it exists.   
            try
            {
                await indexerClient.GetIndexerAsync(sqlIndexer.Name);
                //Rest the indexer if it exsits.
                await indexerClient.ResetIndexerAsync(sqlIndexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified indexer not exist, 404 will be thrown.
            }

            await indexerClient.CreateOrUpdateIndexerAsync(sqlIndexer);

            Console.WriteLine("Running SQL indexer...\n");

            try
            {
                await indexerClient.RunIndexerAsync(sqlIndexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 429)
            {
                Console.WriteLine("Failed to run sql indexer: {0}", ex.Message);
            }
        }
    }
}
