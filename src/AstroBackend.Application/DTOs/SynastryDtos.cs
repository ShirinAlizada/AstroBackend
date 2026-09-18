namespace AstroBackend.Application.DTOs
{
    public record SynastryRequest(
    string SignA,
    string SignB,
    string? DateA = null,
    string? DateB = null
);

    public record SynastryResponse(
        int Overall,
        int Love,
        int Friendship,
        int Communication,
        List<string> Notes
    );
}
