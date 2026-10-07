using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;


[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly IAstrologerService _astrologerService;
    private readonly IBookingService _bookingService;
    private readonly IHoroscopeService _horoscopeService;
    private readonly IForumService _forumService;
    private readonly IArticleService _articleService;
    private readonly IShopService _shopService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IContactService _contactService;
    private readonly ICurrentUserService _currentUserService;

    public AdminController(
        IAdminService adminService,
        IAstrologerService astrologerService,
        IBookingService bookingService,
        IHoroscopeService horoscopeService,
        IForumService forumService,
        IArticleService articleService,
        IShopService shopService,
        ISubscriptionService subscriptionService,
        IContactService contactService,
        ICurrentUserService currentUserService)
    {
        _adminService = adminService;
        _astrologerService = astrologerService;
        _bookingService = bookingService;
        _horoscopeService = horoscopeService;
        _forumService = forumService;
        _articleService = articleService;
        _shopService = shopService;
        _subscriptionService = subscriptionService;
        _contactService = contactService;
        _currentUserService = currentUserService;
    }

    private bool IsSuperAdmin => _currentUserService.Role?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ?? false;
    private Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedException("Giriş edilməyib.");

    // --- USERS MANAGEMENT ---
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAllUsers(CancellationToken ct)
    {
        var users = await _adminService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserDto>> CreateUser([FromBody] RegisterRequest request, [FromQuery] string role = "user", CancellationToken ct = default)
    {
        var user = await _adminService.CreateUserAsync(request, role, CurrentUserId, IsSuperAdmin, ct);
        return Ok(user);
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await _adminService.DeleteUserAsync(id, CurrentUserId, IsSuperAdmin, ct);
        return Ok(new { message = "İstifadəçi silindi." });
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromQuery] string role, CancellationToken ct)
    {
        await _adminService.UpdateUserRoleAsync(id, role, CurrentUserId, IsSuperAdmin, ct);
        return Ok(new { message = "İstifadəçi rolu yeniləndi." });
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
    {
        await _adminService.ToggleUserActiveStatusAsync(id, ct);
        return Ok(new { message = "İstifadəçi statusu dəyişdirildi." });
    }

    // --- ARTICLES (QƏZET) MANAGEMENT ---
    [HttpGet("articles")]
    public async Task<ActionResult<IReadOnlyList<ArticleDto>>> GetAllArticles(CancellationToken ct)
    {
        var list = await _articleService.AdminGetAllArticlesAsync(ct);
        return Ok(list);
    }

    [HttpPost("articles")]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleRequest request, CancellationToken ct)
    {
        var created = await _articleService.AdminCreateArticleAsync(CurrentUserId, request, ct);
        return Ok(created);
    }

    [HttpPost("articles/generate-ai")]
    public async Task<ActionResult<AiArticleResultDto>> GenerateAiArticle([FromBody] GenerateArticleAiRequest request, CancellationToken ct)
    {
        var result = await _articleService.GenerateArticleWithAiAsync(request, ct);
        return Ok(result);
    }

    [HttpPut("articles/{id:guid}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(Guid id, [FromBody] UpdateArticleRequest request, CancellationToken ct)
    {
        var updated = await _articleService.AdminUpdateArticleAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("articles/{id:guid}")]
    public async Task<IActionResult> DeleteArticle(Guid id, CancellationToken ct)
    {
        await _articleService.AdminDeleteArticleAsync(id, ct);
        return Ok(new { message = "Məqalə silindi." });
    }

    [HttpPatch("articles/{id:guid}/publish")]
    public async Task<IActionResult> PublishArticle(Guid id, [FromQuery] bool publish, CancellationToken ct)
    {
        await _articleService.AdminPublishArticleAsync(id, publish, ct);
        return Ok(new { message = $"Məqalə statusu: {(publish ? "Dərc olundu" : "Qaralama")}." });
    }

    // --- ASTROLOGERS MANAGEMENT ---
    [HttpGet("astrologers")]
    public async Task<ActionResult<IReadOnlyList<AstrologerDto>>> GetAllAstrologers(CancellationToken ct)
    {
        var list = await _astrologerService.GetAllAstrologersAsync(ct);
        return Ok(list);
    }

    [HttpPost("astrologers")]
    public async Task<ActionResult<AstrologerDto>> CreateAstrologer([FromBody] CreateAstrologerRequest request, CancellationToken ct)
    {
        var created = await _astrologerService.CreateAstrologerAsync(null, request, ct);
        return Ok(created);
    }

    [HttpPut("astrologers/{id:guid}")]
    public async Task<ActionResult<AstrologerDto>> UpdateAstrologer(Guid id, [FromBody] UpdateAstrologerRequest request, CancellationToken ct)
    {
        var updated = await _astrologerService.UpdateAstrologerAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpPatch("astrologers/{id:guid}/verify")]
    public async Task<IActionResult> VerifyAstrologer(Guid id, [FromQuery] bool verified, CancellationToken ct)
    {
        await _astrologerService.VerifyAstrologerAsync(id, verified, ct);
        return Ok(new { message = $"Astroloq təsdiqlənməsi: {verified}." });
    }

    [HttpDelete("astrologers/{id:guid}")]
    public async Task<IActionResult> DeleteAstrologer(Guid id, CancellationToken ct)
    {
        await _astrologerService.DeleteAstrologerAsync(id, ct);
        return Ok(new { message = "Astroloq silindi." });
    }

    // --- BOOKINGS MANAGEMENT ---
    [HttpGet("bookings")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAllBookings(CancellationToken ct)
    {
        var list = await _bookingService.GetAllBookingsAsync(ct);
        return Ok(list);
    }

    // --- HOROSCOPES MANAGEMENT ---
    [HttpPost("horoscopes")]
    public async Task<ActionResult<HoroscopeDto>> CreateHoroscope([FromBody] CreateHoroscopeRequest request, CancellationToken ct)
    {
        var created = await _horoscopeService.CreateHoroscopeAsync(request, ct);
        return Ok(created);
    }

    [HttpPut("horoscopes/{id:guid}")]
    public async Task<ActionResult<HoroscopeDto>> UpdateHoroscope(Guid id, [FromBody] UpdateHoroscopeRequest request, CancellationToken ct)
    {
        var updated = await _horoscopeService.UpdateHoroscopeAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("horoscopes/{id:guid}")]
    public async Task<IActionResult> DeleteHoroscope(Guid id, CancellationToken ct)
    {
        await _horoscopeService.DeleteHoroscopeAsync(id, ct);
        return Ok(new { message = "Horoskop silindi." });
    }

    // --- FORUM MODERATION ---
    // Admin tərəfindən (real istifadəçi hesabı olmadan) mövzu/rəy yaratmaq — məs. çoxdilli
    // demo məzmunu seed etmək üçün. FK tələbinə görə mövzu/rəy cari admin istifadəçisinin
    // Id-sinə bağlanır, lakin göstərilən müəllif adı sərbəst mətn kimi gəlir (bax: ForumService).
    [HttpPost("forum/topics")]
    public async Task<ActionResult<ForumTopicDto>> AdminCreateTopic([FromBody] AdminCreateTopicRequest request, CancellationToken ct)
    {
        var created = await _forumService.CreateTopicAsync(
            CurrentUserId,
            request.AuthorName,
            new CreateTopicRequest(request.Category, request.Title, request.Body, request.TitleEn, request.TitleRu, request.BodyEn, request.BodyRu),
            ct);
        return Ok(created);
    }

    [HttpPost("forum/topics/{topicId:guid}/replies")]
    public async Task<ActionResult<ForumReplyDto>> AdminCreateReply(Guid topicId, [FromBody] AdminCreateReplyRequest request, CancellationToken ct)
    {
        var created = await _forumService.CreateReplyAsync(
            topicId,
            CurrentUserId,
            request.AuthorName,
            new CreateReplyRequest(request.Body, request.BodyEn, request.BodyRu),
            ct);
        return Ok(created);
    }

    [HttpPatch("forum/topics/{id:guid}/hide")]
    public async Task<IActionResult> SetTopicHidden(Guid id, [FromQuery] bool isHidden, CancellationToken ct)
    {
        await _forumService.SetTopicHiddenAsync(id, isHidden, ct);
        return Ok(new { message = $"Mövzunun gizliliyi dəyişdirildi: {isHidden}." });
    }

    [HttpPatch("forum/replies/{id:guid}/hide")]
    public async Task<IActionResult> SetReplyHidden(Guid id, [FromQuery] bool isHidden, CancellationToken ct)
    {
        await _forumService.SetReplyHiddenAsync(id, isHidden, ct);
        return Ok(new { message = $"Rəyin gizliliyi dəyişdirildi: {isHidden}." });
    }

    // --- SHOP MANAGEMENT ---
    [HttpGet("shop/products")]
    public async Task<ActionResult<IReadOnlyList<ShopProductDto>>> GetAllShopProducts(CancellationToken ct)
    {
        var list = await _shopService.GetAllProductsAsync(ct);
        return Ok(list);
    }

    [HttpPost("shop/products")]
    public async Task<ActionResult<ShopProductDto>> CreateShopProduct([FromBody] CreateShopProductRequest request, CancellationToken ct)
    {
        var created = await _shopService.CreateProductAsync(request, ct);
        return Ok(created);
    }

    [HttpPut("shop/products/{id:guid}")]
    public async Task<ActionResult<ShopProductDto>> UpdateShopProduct(Guid id, [FromBody] UpdateShopProductRequest request, CancellationToken ct)
    {
        var updated = await _shopService.UpdateProductAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("shop/products/{id:guid}")]
    public async Task<IActionResult> DeleteShopProduct(Guid id, CancellationToken ct)
    {
        await _shopService.DeleteProductAsync(id, ct);
        return Ok(new { message = "Məhsul silindi." });
    }

    [HttpGet("shop/orders")]
    public async Task<ActionResult<IReadOnlyList<ShopOrderDto>>> GetAllShopOrders(CancellationToken ct)
    {
        var list = await _shopService.GetAllOrdersAsync(ct);
        return Ok(list);
    }

    [HttpPatch("shop/orders/{id:guid}/status")]
    public async Task<ActionResult<ShopOrderDto>> UpdateShopOrderStatus(Guid id, [FromBody] UpdateShopOrderStatusRequest request, CancellationToken ct)
    {
        var updated = await _shopService.UpdateOrderStatusAsync(id, request.Status, ct);
        return Ok(updated);
    }

    [HttpGet("shop/stats")]
    public async Task<ActionResult<ShopSalesStatsDto>> GetShopStats(CancellationToken ct)
    {
        var stats = await _shopService.GetSalesStatsAsync(ct);
        return Ok(stats);
    }

    // --- SUBSCRIPTION PLANS MANAGEMENT ---
    [HttpGet("subscriptions/plans")]
    public async Task<ActionResult<IReadOnlyList<AdminSubscriptionPlanDto>>> GetAllSubscriptionPlans(CancellationToken ct)
    {
        var list = await _subscriptionService.AdminGetAllPlansAsync(ct);
        return Ok(list);
    }

    [HttpPost("subscriptions/plans")]
    public async Task<ActionResult<AdminSubscriptionPlanDto>> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request, CancellationToken ct)
    {
        var created = await _subscriptionService.AdminCreatePlanAsync(request, ct);
        return Ok(created);
    }

    [HttpPut("subscriptions/plans/{id:guid}")]
    public async Task<ActionResult<AdminSubscriptionPlanDto>> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanRequest request, CancellationToken ct)
    {
        var updated = await _subscriptionService.AdminUpdatePlanAsync(id, request, ct);
        return Ok(updated);
    }

    [HttpDelete("subscriptions/plans/{id:guid}")]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id, CancellationToken ct)
    {
        await _subscriptionService.AdminDeletePlanAsync(id, ct);
        return Ok(new { message = "Abunəlik paketi silindi." });
    }

    // --- CONTACT MESSAGES MANAGEMENT ---
    [HttpGet("contact-messages")]
    public async Task<ActionResult<IReadOnlyList<ContactMessageDto>>> GetAllContactMessages(CancellationToken ct)
    {
        var list = await _contactService.AdminGetAllAsync(ct);
        return Ok(list);
    }

    [HttpPatch("contact-messages/{id:guid}/read")]
    public async Task<IActionResult> MarkContactMessageRead(Guid id, [FromQuery] bool isRead = true, CancellationToken ct = default)
    {
        await _contactService.AdminMarkReadAsync(id, isRead, ct);
        return Ok(new { message = "Mesaj statusu yeniləndi." });
    }
}
