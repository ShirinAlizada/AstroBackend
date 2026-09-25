using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IGenericRepository<ChatThread> _threadRepo;
        private readonly IGenericRepository<ChatMessage> _messageRepo;
        private readonly IGenericRepository<Profile> _profileRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(
            IGenericRepository<ChatThread> threadRepo,
            IGenericRepository<ChatMessage> messageRepo,
            IGenericRepository<Profile> profileRepo,
            IUnitOfWork unitOfWork)
        {
            _threadRepo = threadRepo;
            _messageRepo = messageRepo;
            _profileRepo = profileRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<ChatThreadDto>> GetThreadsAsync(Guid userId, CancellationToken ct = default)
        {
            var list = _threadRepo.Query()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.UpdatedAt)
                .ToList();

            return list.Select(t => new ChatThreadDto(t.Id, t.UserId, t.Title, t.CreatedAt, t.UpdatedAt)).ToList();
        }

        public async Task<ChatThreadDto> CreateThreadAsync(Guid userId, CreateThreadRequest request, CancellationToken ct = default)
        {
            var thread = new ChatThread
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? "Yeni söhbət" : request.Title
            };

            await _threadRepo.AddAsync(thread, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return new ChatThreadDto(thread.Id, thread.UserId, thread.Title, thread.CreatedAt, thread.UpdatedAt);
        }

        public async Task DeleteThreadAsync(Guid threadId, Guid userId, CancellationToken ct = default)
        {
            var thread = await _threadRepo.GetByIdAsync(threadId, ct);
            if (thread == null || thread.UserId != userId) throw new NotFoundException("Söhbət tapılmadı.");

            _threadRepo.Delete(thread);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<ChatMessageDto>> GetMessagesAsync(Guid threadId, Guid userId, CancellationToken ct = default)
        {
            var thread = await _threadRepo.GetByIdAsync(threadId, ct);
            if (thread == null || thread.UserId != userId) throw new NotFoundException("Söhbət tapılmadı.");

            var list = _messageRepo.Query()
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            return list.Select(m => new ChatMessageDto(m.Id, m.ThreadId, m.UserId, m.Role, m.Content, m.CreatedAt)).ToList();
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid threadId, Guid userId, SendMessageRequest request, CancellationToken ct = default)
        {
            var thread = await _threadRepo.GetByIdAsync(threadId, ct);
            if (thread == null || thread.UserId != userId) throw new NotFoundException("Söhbət tapılmadı.");

            // 1. Save user message
            var userMsg = new ChatMessage
            {
                ThreadId = threadId,
                UserId = userId,
                Role = "user",
                Content = request.Message
            };
            await _messageRepo.AddAsync(userMsg, ct);

            // 2. Generate Astrological AI Assistant response
            var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            string sunSign = profile?.SunSign ?? "Bürcünüz";

            string replyText = GenerateAstrologicalReply(request.Message, sunSign, profile?.FullName);

            var assistantMsg = new ChatMessage
            {
                ThreadId = threadId,
                UserId = userId,
                Role = "assistant",
                Content = replyText
            };
            await _messageRepo.AddAsync(assistantMsg, ct);

            // Update thread title if first message
            if (thread.Title == "Yeni söhbət")
            {
                thread.Title = request.Message.Length > 30 ? request.Message[..30] + "…" : request.Message;
            }
            thread.UpdatedAt = DateTime.UtcNow;
            _threadRepo.Update(thread);

            await _unitOfWork.SaveChangesAsync(ct);

            return new ChatMessageDto(assistantMsg.Id, assistantMsg.ThreadId, assistantMsg.UserId, assistantMsg.Role, assistantMsg.Content, assistantMsg.CreatedAt);
        }

        private static string GenerateAstrologicalReply(string userQuestion, string sunSign, string? name)
        {
            var q = userQuestion.ToLower();
            string intro = string.IsNullOrWhiteSpace(name) ? $"Səlam səmavi axtarışçı ({sunSign})" : $"Salam, {name} ({sunSign} bürcü)";

            if (q.Contains("retroqrad") || q.Contains("retro"))
            {
                return $"{intro}! Planetlərin retroqrad hərəkəti həyatı dayandırmaq deyil, daxilə baxmaq və keçmiş planları yenidən nəzərdən keçirmək üçün bir fürsətdir. Bu dövrdə tələsik qərarlar əvəzinə yarımçıq qalmış işləri yekunlaşdırmaq sizə böyük xeyir gətirəcəkdir.";
            }
            if (q.Contains("uyğunluq") || q.Contains("sevgi") || q.Contains("münasibət"))
            {
                return $"{intro}, münasibətlərdə ən vacib amil təkcə Günəş bürcü deyil, həm də Venera və Ay yerləşmələridir. Əgər bir-birinizin emosional ehtiyaclarına diqqət yetirsəniz və hisslərinizi açıq ifadə etsəniz, səmavi ahəng münasibətinizdə çiçəklənəcəkdir.";
            }
            if (q.Contains("karyera") || q.Contains("iş") || q.Contains("pul") || q.Contains("maliyyə"))
            {
                return $"{intro}, Saturn və Yupiterin hazırkı tranzitləri zəhmətkeşlik və nizam tələb edir. Bu ərəfədə başladığınız strateji addımlar uzunmüddətli maddi sabitlik və möhkəm təməl vəd edir.";
            }

            return $"{intro}. Səma xəritəniz göstərir ki, hazırkı dövrdə intuisiyanıza güvənmək və daxili harmoniyanı qorumaq ən doğru yoldur. Ulduzlar sizə bələdçilik edir, lakin seçimlər hər zaman sizin iradənizdən asılıdır. Əlavə olaraq natal xəritəniz üzrə hansı sahəni dərindən araşdırmaq istərdiniz?";
        }
    }

}
