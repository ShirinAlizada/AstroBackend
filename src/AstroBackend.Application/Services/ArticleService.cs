using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IGenericRepository<Article> _articleRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ArticleService(IGenericRepository<Article> articleRepo, IUnitOfWork unitOfWork)
        {
            _articleRepo = articleRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<ArticleDto>> GetPublishedArticlesAsync(string? tag, string? search, string? sort, CancellationToken ct = default)
        {
            var query = _articleRepo.Query().Where(a => a.Published);

            if (!string.IsNullOrWhiteSpace(tag) && tag != "hamısı")
                query = query.Where(a => a.Tag.ToLower() == tag.ToLower());

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(s) || (a.Excerpt != null && a.Excerpt.ToLower().Contains(s)));
            }

            var list = sort == "populyar"
                ? query.OrderByDescending(a => a.Views).ToList()
                : query.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt).ToList();

            return list.Select(MapToDto).ToList();
        }

        public async Task<ArticleDto> GetArticleBySlugAsync(string slug, CancellationToken ct = default)
        {
            var article = await _articleRepo.FirstOrDefaultAsync(a => a.Slug.ToLower() == slug.ToLower() && a.Published, ct);
            if (article == null) throw new NotFoundException("Məqalə tapılmadı.");

            return MapToDto(article);
        }

        public async Task IncrementViewsAsync(string slug, CancellationToken ct = default)
        {
            var article = await _articleRepo.FirstOrDefaultAsync(a => a.Slug.ToLower() == slug.ToLower(), ct);
            if (article != null)
            {
                article.Views++;
                _articleRepo.Update(article);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<ArticleDto>> AdminGetAllArticlesAsync(CancellationToken ct = default)
        {
            var list = _articleRepo.Query().OrderByDescending(a => a.CreatedAt).ToList();
            return list.Select(MapToDto).ToList();
        }

        public async Task<ArticleDto> AdminCreateArticleAsync(Guid? authorId, CreateArticleRequest request, CancellationToken ct = default)
        {
            string slug = string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateSlug(request.Title)
                : GenerateSlug(request.Slug);

            var existing = await _articleRepo.FirstOrDefaultAsync(a => a.Slug == slug, ct);
            if (existing != null) slug += "-" + Guid.NewGuid().ToString("N")[..6];

            var article = new Article
            {
                AuthorId = authorId,
                Title = request.Title,
                Slug = slug,
                Excerpt = request.Excerpt,
                Body = request.Body,
                Tag = string.IsNullOrWhiteSpace(request.Tag) ? "ümumi" : request.Tag,
                CoverUrl = request.CoverUrl,
                Published = request.Published,
                PublishedAt = request.Published ? DateTime.UtcNow : null,
                Views = 0
            };

            await _articleRepo.AddAsync(article, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapToDto(article);
        }

        public async Task<ArticleDto> AdminUpdateArticleAsync(Guid id, UpdateArticleRequest request, CancellationToken ct = default)
        {
            var article = await _articleRepo.GetByIdAsync(id, ct);
            if (article == null) throw new NotFoundException("Məqalə tapılmadı.");

            article.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Slug))
                article.Slug = GenerateSlug(request.Slug);
            article.Excerpt = request.Excerpt;
            article.Body = request.Body;
            article.Tag = request.Tag;
            article.CoverUrl = request.CoverUrl;

            if (request.Published && !article.Published)
                article.PublishedAt = DateTime.UtcNow;

            article.Published = request.Published;
            article.UpdatedAt = DateTime.UtcNow;

            _articleRepo.Update(article);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapToDto(article);
        }

        public async Task AdminDeleteArticleAsync(Guid id, CancellationToken ct = default)
        {
            var article = await _articleRepo.GetByIdAsync(id, ct);
            if (article == null) throw new NotFoundException("Məqalə tapılmadı.");

            _articleRepo.Delete(article);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task AdminPublishArticleAsync(Guid id, bool publish, CancellationToken ct = default)
        {
            var article = await _articleRepo.GetByIdAsync(id, ct);
            if (article == null) throw new NotFoundException("Məqalə tapılmadı.");

            article.Published = publish;
            if (publish && !article.PublishedAt.HasValue)
                article.PublishedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;

            _articleRepo.Update(article);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static string GenerateSlug(string text)
        {
            var str = text.ToLower()
                .Replace("ə", "e").Replace("ı", "i").Replace("ö", "o")
                .Replace("ü", "u").Replace("ç", "c").Replace("ş", "s")
                .Replace("ğ", "g");

            var clean = new string(str.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
            while (clean.Contains("--")) clean = clean.Replace("--", "-");
            return clean.Trim('-');
        }

        private static ArticleDto MapToDto(Article a) => new(
            a.Id,
            a.AuthorId,
            a.Title,
            a.Slug,
            a.Excerpt,
            a.Body,
            a.Tag,
            a.CoverUrl,
            a.Published,
            a.PublishedAt,
            a.Views,
            a.CreatedAt
        );
    }

}
