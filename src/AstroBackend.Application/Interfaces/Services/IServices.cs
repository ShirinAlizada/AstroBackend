using AstroBackend.Application.DTOs;

namespace AstroBackend.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
}

public interface INatalChartService
{
    Task<NatalChartResponse> GetMyChartAsync(Guid userId, CancellationToken ct = default);
    Task<NatalChartResponse> CalculateChartAsync(CalculateChartRequest request);
    Task SaveMyChartAsync(Guid userId, SaveNatalChartRequest request, CancellationToken ct = default);
}

public interface ISynastryService
{
    SynastryResponse CalculateCompatibility(SynastryRequest request);
}

public interface IHoroscopeService
{
    Task<IReadOnlyList<HoroscopeDto>> GetHoroscopesAsync(string? sign, string? period, CancellationToken ct = default);
    Task<HoroscopeDto?> GetCurrentHoroscopeAsync(string sign, string period, CancellationToken ct = default);
    Task<HoroscopeDto> CreateHoroscopeAsync(CreateHoroscopeRequest request, CancellationToken ct = default);
    Task<HoroscopeDto> UpdateHoroscopeAsync(Guid id, UpdateHoroscopeRequest request, CancellationToken ct = default);
    Task DeleteHoroscopeAsync(Guid id, CancellationToken ct = default);
}

public interface IAstrologerService
{
    Task<IReadOnlyList<AstrologerDto>> GetVerifiedAstrologersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AstrologerDto>> GetAllAstrologersAsync(CancellationToken ct = default);
    Task<AstrologerDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AstrologerDto> CreateAstrologerAsync(Guid? userId, CreateAstrologerRequest request, CancellationToken ct = default);
    Task<AstrologerDto> UpdateAstrologerAsync(Guid id, UpdateAstrologerRequest request, CancellationToken ct = default);
    Task VerifyAstrologerAsync(Guid id, bool verified, CancellationToken ct = default);
    Task DeleteAstrologerAsync(Guid id, CancellationToken ct = default);
}

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetAstrologerBookingsAsync(Guid astrologerUserId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct = default);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid currentUserId, bool isAdmin, string status, CancellationToken ct = default);
}

public interface IJournalService
{
    Task<IReadOnlyList<JournalEntryDto>> GetMyEntriesAsync(Guid userId, CancellationToken ct = default);
    Task<JournalEntryDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<JournalEntryDto> CreateEntryAsync(Guid userId, CreateJournalEntryRequest request, CancellationToken ct = default);
    Task<JournalEntryDto> UpdateEntryAsync(Guid id, Guid userId, UpdateJournalEntryRequest request, CancellationToken ct = default);
    Task DeleteEntryAsync(Guid id, Guid userId, CancellationToken ct = default);
}

public interface IForumService
{
    Task<IReadOnlyList<ForumTopicDto>> GetTopicsAsync(string? category, CancellationToken ct = default);
    Task<ForumTopicDto> GetTopicByIdAsync(Guid id, CancellationToken ct = default);
    Task<ForumTopicDto> CreateTopicAsync(Guid userId, string authorName, CreateTopicRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ForumReplyDto>> GetRepliesAsync(Guid topicId, CancellationToken ct = default);
    Task<ForumReplyDto> CreateReplyAsync(Guid topicId, Guid userId, string authorName, CreateReplyRequest request, CancellationToken ct = default);
    Task SetTopicHiddenAsync(Guid topicId, bool isHidden, CancellationToken ct = default);
    Task SetReplyHiddenAsync(Guid replyId, bool isHidden, CancellationToken ct = default);
    Task DeleteTopicAsync(Guid topicId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task DeleteReplyAsync(Guid replyId, Guid userId, bool isAdmin, CancellationToken ct = default);
}

public interface IArticleService
{
    Task<IReadOnlyList<ArticleDto>> GetPublishedArticlesAsync(string? tag, string? search, string? sort, CancellationToken ct = default);
    Task<ArticleDto> GetArticleBySlugAsync(string slug, CancellationToken ct = default);
    Task IncrementViewsAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<ArticleDto>> AdminGetAllArticlesAsync(CancellationToken ct = default);
    Task<ArticleDto> AdminCreateArticleAsync(Guid? authorId, CreateArticleRequest request, CancellationToken ct = default);
    Task<ArticleDto> AdminUpdateArticleAsync(Guid id, UpdateArticleRequest request, CancellationToken ct = default);
    Task AdminDeleteArticleAsync(Guid id, CancellationToken ct = default);
    Task AdminPublishArticleAsync(Guid id, bool publish, CancellationToken ct = default);
    Task<AiArticleResultDto> GenerateArticleWithAiAsync(GenerateArticleAiRequest request, CancellationToken ct = default);
}

public interface IChatService
{
    Task<IReadOnlyList<ChatThreadDto>> GetThreadsAsync(Guid userId, CancellationToken ct = default);
    Task<ChatThreadDto> CreateThreadAsync(Guid userId, CreateThreadRequest request, CancellationToken ct = default);
    Task DeleteThreadAsync(Guid threadId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid threadId, Guid userId, CancellationToken ct = default);
    Task<ChatMessageDto> SendMessageAsync(Guid threadId, Guid userId, SendMessageRequest request, CancellationToken ct = default);
}

public interface INumerologyService
{
    NumerologyResponse Calculate(NumerologyRequest request);
}

public interface IPanchangService
{
    PanchangResponse GetPanchang(PanchangRequest request);
}

public interface IAdminService
{
    Task<IReadOnlyList<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task UpdateUserRoleAsync(Guid targetUserId, string role, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default);
    Task ToggleUserActiveStatusAsync(Guid userId, CancellationToken ct = default);
    Task<AdminUserDto> CreateUserAsync(RegisterRequest request, string role, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default);
    Task DeleteUserAsync(Guid targetUserId, Guid currentUserId, bool isSuperAdmin, CancellationToken ct = default);
}

public interface IAIService
{
    Task<string> GenerateTextAsync(string systemPrompt, List<AiTurnDto> messages, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamTextAsync(string systemPrompt, List<AiTurnDto> messages, CancellationToken ct = default);
}