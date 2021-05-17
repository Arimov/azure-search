using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System;
using System.Text.Json.Serialization;

namespace Indexer
{
    public partial class Book
    {
        [SimpleField(IsFilterable = true, IsKey = true)]
        public string BookId { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string Title { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
        public string Content { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string Theme { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public double? Rating { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public Author Author { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string MetadataStorageName { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public string MetadataStorageContentType { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public Int64 MetadataStorageSize { get; set; }


        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string[] Tags { get; set; }

    }
}
