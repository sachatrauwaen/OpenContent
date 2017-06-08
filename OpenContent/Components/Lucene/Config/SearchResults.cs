namespace Satrabel.OpenContent.Components.Lucene.Config
{
    public class SearchResults
    {
        public SearchResults()
        {
            ids = new string[0];
        }
        public int TotalResults { get; set; }
        public string[] ids { get; set; }
        public ResultDefinition ResultDefinition { get; set; }
    }

    public class ResultDefinition
    {
        public string Filter { get; set; }
        public string Query { get; set; }
        public string Sort { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}