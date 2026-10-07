using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    /// <summary>
    /// GET api/Search?q=...&amp;lang=az|en|ru — saytın bütün açıq məzmunu üzrə qlobal axtarış
    /// (bax: ISearchService/SearchService). Autentifikasiya tələb olunmur.
    /// </summary>
    public class SearchController : BaseApiController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SearchHitDto>>> Search(
            [FromQuery] string q,
            [FromQuery] string? lang,
            CancellationToken ct)
        {
            var results = await _searchService.SearchAsync(q, lang, ct);
            return Ok(results);
        }
    }
}
