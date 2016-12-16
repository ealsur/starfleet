using starfleet.Models;
using starfleet.Services;
using Microsoft.AspNetCore.Mvc;


namespace starfleet.Controllers
{
    public class SearchController : Controller
    {
        private ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }
        [HttpPost]
        public IActionResult Search([FromBody]SearchPayload payload)
        {
            return Json(_searchService.Search("airports",payload));
        }
        
        [HttpGet]
        public IActionResult Suggest(string term, bool fuzzy = false)
        {
            var response = _searchService.Suggest("airports", "iata", term, fuzzy); 
            return Json(response);  
        }
    }
}
