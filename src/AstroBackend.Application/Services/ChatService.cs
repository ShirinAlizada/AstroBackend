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
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public const string AstrologerSystemPrompt = @"Sən ""Virgo Astrology"" platformasının AI astroloq köməkçisisən.
Azərbaycan dilində, isti və aydın danışırsan.
Bürclər, doğum xəritəsi, tranzitlər, uyğunluq və ay fazaları haqqında izah verirsən.
Cavabların qısa (maksimum 200 söz), səmimi və praktik olsun; markdown başlıq və siyahılardan istifadə edə bilərsən.
Tibbi, hüquqi və maliyyə məsləhəti vermirsən, belə suallarda mütəxəssisə yönləndirirsən.
Astrologiyanın elmi sübut deyil, özünü dərk vasitəsi olduğunu lazım gələndə xatırladırsan.";

        public ChatService(
            IGenericRepository<ChatThread> threadRepo,
            IGenericRepository<ChatMessage> messageRepo,
            IGenericRepository<Profile> profileRepo,
            IAIService aiService,
            IUnitOfWork unitOfWork)
        {
            _threadRepo = threadRepo;
            _messageRepo = messageRepo;
            _profileRepo = profileRepo;
            _aiService = aiService;
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

            // 2. Fetch thread history for context
            var history = _messageRepo.Query()
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToList();

            var aiTurns = history.Select(m => new AiTurnDto(m.Role, m.Content)).ToList();
            aiTurns.Add(new AiTurnDto("user", request.Message));

            var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            string profileContext = profile != null
                ? $"\nİstifadəçi məlumatları: Ad: {profile.FullName}, Günəş bürcü: {profile.SunSign ?? "Məlum deyil"}, Ay bürcü: {profile.MoonSign ?? "Məlum deyil"}, Yüksələn: {profile.Ascendant ?? "Məlum deyil"}."
                : "";

            string fullSystemPrompt = AstrologerSystemPrompt + profileContext;

            // 3. Generate response via IAIService (Gemini with rule-based fallback)
            string replyText = await _aiService.GenerateTextAsync(fullSystemPrompt, aiTurns, ct);

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
    }


}
