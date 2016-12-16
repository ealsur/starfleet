using System.Linq;
using System.Threading.Tasks;
using starfleet.Extensions;
using starfleet.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace starfleet.Services
{
    /// <summary>
    /// DocumentDB service
    /// </summary>
    public class DocumentDbService : IDocumentDbService
    {
        private readonly DocumentDbProvider _provider;
        public DocumentDbService(IConfiguration configuration)
        {
            _provider = new DocumentDbProvider(new DocumentDbSettings(configuration));
        }

        /// <summary>
        /// Builds a query for contact addreses
        /// </summary>
        /// <returns></returns>
        public async Task<PagedResults<starfleet.Models.Airline>> GetAirlines(string country,int size = 20, string continuationToken = "")
        {
            var feedOptions = new FeedOptions() { MaxItemCount = size };
            if (!string.IsNullOrEmpty(continuationToken))
            {
                feedOptions.RequestContinuation = continuationToken;
            }
            return  await _provider.CreateQuery<starfleet.Models.Airline>(feedOptions).Where(x => x.type == "airline" && x.country == country).ToPagedResults();
        }
    }
}
