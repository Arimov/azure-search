# Indexer - from two sources

This console application forms an indexer - a function that collects data 
from two sources into a single index file for the service **Azure Cognitive Search**

Data source types used in this application:
- blob container containing pdf files (books)
- SQL Server on VM in Azure (book metadata: author, count pages, ...)

### Setup
In settings file `appsettings.json` need set up you data:

```
{
  "SearchServiceUri": "<YOUR-SEARCH-SERVICE-URI>",
  "SearchServiceQueryApiKey": "<YOUR-SEARCH-SERVICE-API-KEY>"
}
```

This example uses the database `gapzap`. 
Here is the code for creating database tables: `..\db\create.sql`