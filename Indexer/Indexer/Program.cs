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

            // Set up a Blob Storage data source and indexer, and run the indexer to merge book data
            Console.WriteLine("Indexing and merging book data from blob storage...\n");
            await CreateAndRunBlobIndexerAsync(indexName, indexerClient);


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


        private static async Task CreateAndRunBlobIndexerAsync(string indexName, SearchIndexerClient indexerClient)
        {
            SearchIndexerDataSourceConnection blobDataSource = new SearchIndexerDataSourceConnection(
                name: configuration["BlobStorageAccountName"],
                type: SearchIndexerDataSourceType.AzureBlob,
                connectionString: configuration["BlobStorageConnectionString"],
                container: new SearchIndexerDataContainer("gapzap-pdf-docs"));

            // The blob data source does not need to be deleted if it already exists,
            // but the connection string might need to be updated if it has changed.
            await indexerClient.CreateOrUpdateDataSourceConnectionAsync(blobDataSource);

            Console.WriteLine("Creating Blob Storage indexer...\n");

            // Add a field mapping to match the Id field in the documents to 
            // the HotelId key field in the index
            List<FieldMapping> map = new List<FieldMapping> {
                new FieldMapping("Id")
                {
                    TargetFieldName =  "HotelId"
                }
            };

            IndexingParameters parameters = new IndexingParameters();
            parameters.Configuration.Add("parsingMode", "json");

            SearchIndexer blobIndexer = new SearchIndexer(
                name: "hotel-rooms-blob-indexer",
                dataSourceName: blobDataSource.Name,
                targetIndexName: indexName)
            {
                Parameters = parameters,
                Schedule = new IndexingSchedule(TimeSpan.FromDays(1))
            };

            blobIndexer.FieldMappings.Add(new FieldMapping("Id") { TargetFieldName = "HotelId" });

            // Reset the indexer if it already exists
            try
            {
                await indexerClient.GetIndexerAsync(blobIndexer.Name);
                //Rest the indexer if it exsits.
                await indexerClient.ResetIndexerAsync(blobIndexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { }

            await indexerClient.CreateOrUpdateIndexerAsync(blobIndexer);

            Console.WriteLine("Running Blob Storage indexer...\n");

            try
            {
                await indexerClient.RunIndexerAsync(blobIndexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 429)
            {
                Console.WriteLine("Failed to run indexer: {0}", ex.Message);
            }
        }
    }
}
