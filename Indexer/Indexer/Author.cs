using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System;
using System.Text.Json.Serialization;

namespace Indexer
{
    public partial class Author
    {
        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnMicrosoft)]
        public string AuthorName { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string FirstName { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string LastName { get; set; }

        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public DateTime birth { get; set; }

        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string[] Tags { get; set; }
    }
}
