using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    public class ArticlesController : BaseApiController
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetArticles(
            [FromQuery] string? tag,
            [FromQuery] string? search,
            [FromQuery] string? sort,
            [FromQuery] string? lang,
            CancellationToken ct)
        {
            var list = await _articleService.GetPublishedArticlesAsync(tag, search, sort, lang, ct);
            return Ok(list);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ArticleDto>> GetBySlug(string slug, [FromQuery] string? lang, CancellationToken ct)
        {
            var article = await _articleService.GetArticleBySlugAsync(slug, lang, ct);
            return Ok(article);
        }

        [HttpPost("{slug}/view")]
        public async Task<IActionResult> IncrementViews(string slug, CancellationToken ct)
        {
            await _articleService.IncrementViewsAsync(slug, ct);
            return Ok(new { message = "Baxış sayı artırıldı." });
        }
    }

}
