﻿using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using starfleet.Models;
using System.Linq;
using System.Collections.Concurrent;

namespace starfleet.Services
{
	/// <summary>
	/// Azure Search service
	/// </summary>
	public class SearchService: ISearchService
	{
		private SearchServiceClient client;
		//Maintaining a Dictionary of Index Clients is better performant
		private ConcurrentDictionary<string, ISearchIndexClient> indexClients;
		public SearchService(string accountName,string queryKey)
		{
			client = new SearchServiceClient(accountName, new SearchCredentials(queryKey));
			indexClients = new ConcurrentDictionary<string, ISearchIndexClient>();
		}

		/// <summary>
		/// Obtains a new IndexClient and avoids Socket Exhaustion by reusing previous clients.
		/// </summary>
		/// <param name="indexName"></param>
		/// <returns></returns>
		private ISearchIndexClient GetClient(string indexName)
		{
			return indexClients.GetOrAdd(indexName, client.Indexes.GetClient(indexName));
		}

		/// <summary>
		/// Obtains suggestions from an index
		/// </summary>
		/// <param name="indexName"></param>
		/// <param name="suggesterName"></param>
		/// <param name="term"></param>
		/// <param name="filters"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public DocumentSuggestResult Suggest(string indexName, string suggesterName, string term, bool useFuzzySearch = true,string filters="", int amount = 10)
		{
			return GetClient(indexName).Documents.Suggest(term, suggesterName, new SuggestParameters() { Top = amount, UseFuzzyMatching = useFuzzySearch, Filter = filters });
		}

		public DocumentSearchResult Search(string indexName, SearchPayload payload){
				var indexClient = GetClient(indexName);
				var sp = new SearchParameters();
				sp.Top = payload.PageSize;
				sp.Skip = (payload.Page - 1) * payload.PageSize;
				if(payload.Filters!=null){
					sp.Filter = string.Join(" and ", payload.Filters.Select(x=> $"{x.Key} eq '{x.Value}'").ToArray());
				}
				sp.OrderBy = payload.OrderBy.Split(',');
				sp.QueryType = "full".Equals(payload.QueryType)?QueryType.Full:QueryType.Simple;
				sp.SearchMode = payload.SearchMode;
				
				if(!string.IsNullOrEmpty(payload.ScoringProfile)){
					sp.ScoringProfile = payload.ScoringProfile;
				} 
				sp.IncludeTotalResultCount = true;
				if(payload.IncludeFacets){
					var appliedFilters = (payload.Filters??new Dictionary<string,string>()).Select(x=>x.Key);
					//Asking for facets for applied filters is not really needed
					//The amount is actually the result count
					sp.Facets = payload.Facets.Except(appliedFilters).ToList();
				}
				return indexClient.Documents.Search(payload.Text, sp);
			}

	}
}
