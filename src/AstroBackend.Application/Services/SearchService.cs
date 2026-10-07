using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroBackend.Application.Services;

/// <summary>
/// Frontend-in SiteNav.tsx-dəki qlobal axtarışı ilə eyni məntiqi güdür: məqalələr (Published,
/// başlıq+qısa təsvir+tam mətn, hər 3 dildə) ən çox 6, astroloqlar (ad+bio) ən çox 4, mağaza
/// məhsulları (IsActive, ad+təsvir, hər 3 dildə) ən çox 4, forum mövzuları (gizli olmayan,
/// başlıq+mətn, hər 3 dildə) ən çox 4 nəticə — cəmi ən çox 12-yə kəsilir, SiteNav-dakı eyni
/// qaydada (məqalə → astroloq → məhsul → forum).
/// </summary>
public class SearchService : ISearchService
{
    private readonly IGenericRepository<Article> _articleRepo;
    private readonly IGenericRepository<Astrologer> _astrologerRepo;
    private readonly IGenericRepository<ShopProduct> _productRepo;
    private readonly IGenericRepository<ForumTopic> _topicRepo;

    public SearchService(
        IGenericRepository<Article> articleRepo,
        IGenericRepository<Astrologer> astrologerRepo,
        IGenericRepository<ShopProduct> productRepo,
        IGenericRepository<ForumTopic> topicRepo)
    {
        _articleRepo = articleRepo;
        _astrologerRepo = astrologerRepo;
        _productRepo = productRepo;
        _topicRepo = topicRepo;
    }

    public async Task<IReadOnlyList<SearchHitDto>> SearchAsync(string query, string? lang = null, CancellationToken ct = default)
    {
        var term = (query ?? string.Empty).Trim();
        if (term.Length < 2)
            return Array.Empty<SearchHitDto>();

        var needle = term.ToLower();

        var articles = await _articleRepo.Query()
            .Where(a => a.Published && (
                a.Title.ToLower().Contains(needle) ||
                (a.TitleEn != null && a.TitleEn.ToLower().Contains(needle)) ||
                (a.TitleRu != null && a.TitleRu.ToLower().Contains(needle)) ||
                (a.Excerpt != null && a.Excerpt.ToLower().Contains(needle)) ||
                (a.ExcerptEn != null && a.ExcerptEn.ToLower().Contains(needle)) ||
                (a.ExcerptRu != null && a.ExcerptRu.ToLower().Contains(needle)) ||
                a.Body.ToLower().Contains(needle) ||
                (a.BodyEn != null && a.BodyEn.ToLower().Contains(needle)) ||
                (a.BodyRu != null && a.BodyRu.ToLower().Contains(needle))))
            .Take(6)
            .ToListAsync(ct);

        var astrologers = await _astrologerRepo.Query()
            .Where(x =>
                x.DisplayName.ToLower().Contains(needle) ||
                (x.Bio != null && x.Bio.ToLower().Contains(needle)))
            .Take(4)
            .ToListAsync(ct);

        var products = await _productRepo.Query()
            .Where(p => p.IsActive && (
                p.Name.ToLower().Contains(needle) ||
                (p.NameEn != null && p.NameEn.ToLower().Contains(needle)) ||
                (p.NameRu != null && p.NameRu.ToLower().Contains(needle)) ||
                p.Description.ToLower().Contains(needle) ||
                (p.DescriptionEn != null && p.DescriptionEn.ToLower().Contains(needle)) ||
                (p.DescriptionRu != null && p.DescriptionRu.ToLower().Contains(needle))))
            .Take(4)
            .ToListAsync(ct);

        var topics = await _topicRepo.Query()
            .Where(t => !t.IsHidden && (
                t.Title.ToLower().Contains(needle) ||
                (t.TitleEn != null && t.TitleEn.ToLower().Contains(needle)) ||
                (t.TitleRu != null && t.TitleRu.ToLower().Contains(needle)) ||
                t.Body.ToLower().Contains(needle) ||
                (t.BodyEn != null && t.BodyEn.ToLower().Contains(needle)) ||
                (t.BodyRu != null && t.BodyRu.ToLower().Contains(needle))))
            .Take(4)
            .ToListAsync(ct);

        var hits = new List<SearchHitDto>();

        foreach (var a in articles)
            hits.Add(new SearchHitDto("article", LocalizedArticleTitle(a, lang), $"/qezet/{a.Slug}"));

        // Astroloq profilləri ayrıca marşrutla açılmır (frontend-də də eyni) — ümumi siyahıya yönləndirir.
        foreach (var x in astrologers)
            hits.Add(new SearchHitDto("astrologer", x.DisplayName, "/astroloq"));

        // Mağaza məhsulları da ayrıca məhsul səhifəsinə malik deyil — "/tarot" ümumi mağaza səhifəsidir.
        foreach (var p in products)
            hits.Add(new SearchHitDto("product", LocalizedProductName(p, lang), "/tarot"));

        foreach (var t in topics)
            hits.Add(new SearchHitDto("forum", LocalizedTopicTitle(t, lang), $"/forum/{t.Id}"));

        return hits.Take(12).ToList();
    }

    private static string LocalizedArticleTitle(Article a, string? lang)
    {
        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(a.TitleEn) ? a.Title : a.TitleEn;
        if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(a.TitleRu) ? a.Title : a.TitleRu;
        return a.Title;
    }

    private static string LocalizedProductName(ShopProduct p, string? lang)
    {
        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(p.NameEn) ? p.Name : p.NameEn;
        if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(p.NameRu) ? p.Name : p.NameRu;
        return p.Name;
    }

    private static string LocalizedTopicTitle(ForumTopic t, string? lang)
    {
        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(t.TitleEn) ? t.Title : t.TitleEn;
        if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
            return string.IsNullOrWhiteSpace(t.TitleRu) ? t.Title : t.TitleRu;
        return t.Title;
    }
}
