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
            CancellationToken ct)
        {
            var list = await _articleService.GetPublishedArticlesAsync(tag, search, sort, ct);
            return Ok(list);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ArticleDto>> GetBySlug(string slug, CancellationToken ct)
        {
            var article = await _articleService.GetArticleBySlugAsync(slug, ct);
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
