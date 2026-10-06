using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AstroBackend.Application.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IGenericRepository<Article> _articleRepo;
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public const string ArticleSystemPrompt = @"Sən ""Virgo Astrology"" platformasının Məqalələr bölməsi üçün redaktorsan.
            Verilən mövzuda Azərbaycan dilində məqalə yazırsan.
            Cavabı tam olaraq bu formatda ver, başqa heç nə yazma:
            BAŞLIQ: <cəlbedici başlıq>
            XÜLASƏ: <bir cümləlik anons>
            MƏTN:
            <4-6 abzaslıq məqalə mətni>";

        public ArticleService(
            IGenericRepository<Article> articleRepo,
            IAIService aiService,
            IUnitOfWork unitOfWork)
        {
            _articleRepo = articleRepo;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ArticleDto>> GetPublishedArticlesAsync(string? tag, string? search, string? sort, string? lang = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var query = _articleRepo.Query().Where(a => a.Published);

            if (!string.IsNullOrWhiteSpace(tag) && tag != "hamısı")
                query = query.Where(a => a.Tag.ToLower() == tag.ToLower());

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(s) || (a.Excerpt != null && a.Excerpt.ToLower().Contains(s)));
            }

            query = sort == "populyar"
                ? query.OrderByDescending(a => a.Views)
                : query.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);

            (page, pageSize) = NormalizePaging(page, pageSize);
            var totalCount = await query.CountAsync(ct);
            var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<ArticleDto>(list.Select(a => MapToDto(a, lang)).ToList(), page, pageSize, totalCount);
        }

        /// <summary>page ən azı 1, pageSize 1-100 aralığına sıxılır — xarici sorğu parametrlərinin sərbəst dəyərlər göndərməsinin qarşısını alır.</summary>
        private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;
            return (page, pageSize);
        }

        public async Task<ArticleDto> GetArticleBySlugAsync(string slug, string? lang = null, CancellationToken ct = default)
        {
            var article = await _articleRepo.FirstOrDefaultAsync(a => a.Slug.ToLower() == slug.ToLower() && a.Published, ct);
            if (article == null) throw new NotFoundException("Məqalə tapılmadı.");

            return MapToDto(article, lang);
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
            var list = await _articleRepo.Query().OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
            return list.Select(a => MapToDto(a)).ToList();
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

        public async Task<AiArticleResultDto> GenerateArticleWithAiAsync(GenerateArticleAiRequest request, CancellationToken ct = default)
        {
            var userTurn = new List<AiTurnDto>
        {
            new("user", $"Mövzu: {request.Topic}. Kateqoriya/Teq: {request.Tag ?? "ümumi"}.")
        };

            string aiOutput = await _aiService.GenerateTextAsync(ArticleSystemPrompt, userTurn, ct);

            string title = request.Topic;
            string excerpt = "Astroloji bələdçi və daxili kəşf.";
            string body = aiOutput;

            // Parse formatted response
            var lines = aiOutput.Split('\n');
            string currentSection = "";
            var bodyLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("BAŞLIQ:", StringComparison.OrdinalIgnoreCase))
                {
                    title = trimmed["BAŞLIQ:".Length..].Trim();
                }
                else if (trimmed.StartsWith("XÜLASƏ:", StringComparison.OrdinalIgnoreCase))
                {
                    excerpt = trimmed["XÜLASƏ:".Length..].Trim();
                }
                else if (trimmed.StartsWith("MƏTN:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "body";
                }
                else if (currentSection == "body")
                {
                    bodyLines.Add(line);
                }
                else if (!trimmed.StartsWith("BAŞLIQ:", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("XÜLASƏ:", StringComparison.OrdinalIgnoreCase))
                {
                    bodyLines.Add(line);
                }
            }

            if (bodyLines.Count > 0)
            {
                body = string.Join("\n", bodyLines).Trim();
            }

            return new AiArticleResultDto(title, excerpt, body, request.Tag ?? "ümumi");
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

        /// <summary>lang == "en"/"ru" olduqda uyğun tərcümə sütunları, boş/null olduqda Azərbaycan mətni istifadə olunur.</summary>
        private static ArticleDto MapToDto(Article a, string? lang = null)
        {
            string title = a.Title;
            string? excerpt = a.Excerpt;
            string body = a.Body;

            if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            {
                title = string.IsNullOrWhiteSpace(a.TitleEn) ? a.Title : a.TitleEn;
                excerpt = string.IsNullOrWhiteSpace(a.ExcerptEn) ? a.Excerpt : a.ExcerptEn;
                body = string.IsNullOrWhiteSpace(a.BodyEn) ? a.Body : a.BodyEn;
            }
            else if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            {
                title = string.IsNullOrWhiteSpace(a.TitleRu) ? a.Title : a.TitleRu;
                excerpt = string.IsNullOrWhiteSpace(a.ExcerptRu) ? a.Excerpt : a.ExcerptRu;
                body = string.IsNullOrWhiteSpace(a.BodyRu) ? a.Body : a.BodyRu;
            }

            return new(
                a.Id,
                a.AuthorId,
                title,
                a.Slug,
                excerpt,
                body,
                a.Tag,
                a.CoverUrl,
                a.Published,
                a.PublishedAt,
                a.Views,
                a.CreatedAt
            );
        }
    }


}
