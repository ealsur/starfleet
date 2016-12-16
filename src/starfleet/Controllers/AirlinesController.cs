using starfleet.Models;
using starfleet.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace starfleet.Controllers
{
    public class AirlinesController : Controller
    {
        private readonly IDocumentDbService _ddbService;
        private readonly IRedisCache _redisCache;
        public AirlinesController(IDocumentDbService ddbService, IRedisCache redisCache)
        {
            _ddbService = ddbService;
            _redisCache = redisCache;
        }

        [HttpPost]
        public async Task<IActionResult> ByCountry([FromBody]string id)
        {
            var cacheKey = id.GetHashCode().ToString();
            var previousResult = _redisCache.Get<PagedResults<Airline>>(cacheKey);
            if(previousResult == null){
                previousResult = await _ddbService.GetAirlines(id,20);
                _redisCache.Put(cacheKey, previousResult, TimeSpan.FromHours(24));
            }
            return Json(previousResult);  
        }
    }
}
