using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services;

/// <summary>
/// "Əlaqə" formu mesajlarının backend qatı — frontend-dəki (Supabase) contact_messages
/// funksionallığının bu backend-dəki analoqu. [EnableRateLimiting("contact")] IP-səviyyəli
/// qoruma verir; bura əlavə olaraq eyni e-poçt üzrə tətbiq-səviyyəli məhdudiyyət qoyulur
/// (son 1 saatda 3-dən çox mesaj icazəli deyil).
/// </summary>
public class ContactService : IContactService
{
    private readonly IGenericRepository<ContactMessage> _contactRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public ContactService(
        IGenericRepository<ContactMessage> contactRepo,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _contactRepo = contactRepo;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<ContactMessageDto> SubmitAsync(CreateContactMessageRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
            throw new BadRequestException("Ad, e-poçt və mesaj mütləq daxil edilməlidir.");

        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentCount = (await _contactRepo.FindAsync(
            m => m.Email.ToLower() == request.Email.Trim().ToLower() && m.CreatedAt >= oneHourAgo, ct)).Count;

        if (recentCount >= 3)
            throw new BadRequestException("Çox tez-tez mesaj göndərirsiniz. Zəhmət olmasa 1 saat sonra yenidən cəhd edin.");

        var message = new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Message = request.Message.Trim(),
            IsRead = false
        };

        await _contactRepo.AddAsync(message, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Təsdiq e-poçtu — best-effort, uğursuz olsa mesajın qeydə alınmasına mane olmur.
        try
        {
            await _emailService.SendAsync(
                message.Email,
                "Mesajınız alındı — Virgo Astrology",
                $"<p>Salam {System.Net.WebUtility.HtmlEncode(message.Name)},</p>" +
                "<p>Mesajınız bizə uğurla çatdı. Ən qısa zamanda sizinlə əlaqə saxlayacağıq.</p>" +
                "<p>Hörmətlə,<br/>Virgo Astrology komandası</p>",
                ct);
        }
        catch
        {
            // Email göndərilməsə belə, mesaj qeydə alınıb — bu kritik xəta deyil.
        }

        return MapToDto(message);
    }

    public async Task<IReadOnlyList<ContactMessageDto>> AdminGetAllAsync(CancellationToken ct = default)
    {
        var list = await _contactRepo.GetAllAsync(ct);
        return list.OrderByDescending(m => m.CreatedAt).Select(MapToDto).ToList();
    }

    public async Task AdminMarkReadAsync(Guid id, bool isRead, CancellationToken ct = default)
    {
        var message = await _contactRepo.GetByIdAsync(id, ct);
        if (message == null)
            throw new NotFoundException("Mesaj tapılmadı.");

        message.IsRead = isRead;
        message.UpdatedAt = DateTime.UtcNow;
        _contactRepo.Update(message);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static ContactMessageDto MapToDto(ContactMessage m) => new(
        m.Id,
        m.Name,
        m.Email,
        m.Message,
        m.IsRead,
        m.CreatedAt
    );
}
