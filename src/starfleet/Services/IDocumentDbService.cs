using System.Threading.Tasks;
using starfleet.Models;

namespace starfleet.Services
{
    /// <summary>
    /// DocumentDB service
    /// </summary>
    public interface IDocumentDbService
    {
        Task<PagedResults<starfleet.Models.Airline>> GetAirlines(string country,int size = 20, string continuationToken = "");
    }
}