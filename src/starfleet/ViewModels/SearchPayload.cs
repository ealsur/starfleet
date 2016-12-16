using System.Collections.Generic;
using Microsoft.Azure.Search.Models;

namespace starfleet.ViewModels
{
    public class SearchPayload
    {
        public int Page { get; set; }=1;
        public int PageSize { get; set; } = 50;
        
        public bool IncludeFacets { get; set; } = false;
        public string Text { get; set; }
        
        public Dictionary<string,string> Filters { get; set; } = new Dictionary<string,string>();
        
        public string OrderBy { get; set; } = "";
        
        public string QueryType { get; set; } = "simple";
        
        public SearchMode SearchMode { get; set; } = SearchMode.Any;
        
        public string ScoringProfile { get; set; }
        
        public string ScoringParameter { get; set; }
        
        public string Index { get; set; } = "news";
    }
}