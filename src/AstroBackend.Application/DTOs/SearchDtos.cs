namespace AstroBackend.Application.DTOs
{
    /// <summary>
    /// Qlobal axtarış nəticəsinin bir sətri. Kind: "article" | "astrologer" | "product" | "forum".
    /// Url artıq hazır (frontend-in TanStack Router marşrutları ilə uyğun — bax SearchService),
    /// client sadəcə naviqasiya üçün istifadə edə bilər.
    /// </summary>
    public record SearchHitDto(
        string Kind,
        string Title,
        string Url
    );
}
