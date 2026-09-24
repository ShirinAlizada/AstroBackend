
namespace AstroBackend.Application.DTOs
{
    public record ArticleDto(
    Guid Id,
    Guid? AuthorId,
    string Title,
    string Slug,
    string? Excerpt,
    string Body,
    string Tag,
    string? CoverUrl,
    bool Published,
    DateTime? PublishedAt,
    int Views,
    DateTime CreatedAt
);

    public record CreateArticleRequest(
        string Title,
        string? Slug,
        string? Excerpt,
        string Body,
        string Tag,
        string? CoverUrl,
        bool Published
    );

    public record UpdateArticleRequest(
        string Title,
        string? Slug,
        string? Excerpt,
        string Body,
        string Tag,
        string? CoverUrl,
        bool Published
    );

}
