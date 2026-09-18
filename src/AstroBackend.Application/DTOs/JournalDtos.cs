namespace AstroBackend.Application.DTOs
{
    public record JournalEntryDto(
    Guid Id,
    DateTime EntryDate,
    int Mood,
    string? Title,
    string Content,
    string? TransitNote,
    DateTime CreatedAt
);

    public record CreateJournalEntryRequest(
        DateTime EntryDate,
        int Mood,
        string? Title,
        string Content,
        string? TransitNote
    );

    public record UpdateJournalEntryRequest(
        DateTime EntryDate,
        int Mood,
        string? Title,
        string Content,
        string? TransitNote
    );

}
