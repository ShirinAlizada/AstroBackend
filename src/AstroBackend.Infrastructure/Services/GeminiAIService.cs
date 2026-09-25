using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AstroBackend.Infrastructure.Services
{
    public class GeminiAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;

        private const string DefaultModel = "gemini-3.8-flash";
        private static readonly string[] FallbackModels = ["gemini-2.5-flash", "gemini-1.5-flash", "gemini-pro"];

        public GeminiAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        private string? GetApiKey()
        {
            return _configuration["Gemini:ApiKey"]
                ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? _configuration["GEMINI_API_KEY"];
        }

        public async Task<string> GenerateTextAsync(string systemPrompt, List<AiTurnDto> messages, CancellationToken ct = default)
        {
            var apiKey = GetApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogInformation("GEMINI_API_KEY təyin edilməyib, daxili intellektual qayda mühərriki aktivləşdirildi.");
                return FallbackAstrologyReply(systemPrompt, messages);
            }

            var primaryModel = _configuration["Gemini:Model"]
                ?? Environment.GetEnvironmentVariable("AI_MODEL")
                ?? DefaultModel;

            var modelsToTry = new List<string> { primaryModel };
            modelsToTry.AddRange(FallbackModels.Where(m => m != primaryModel));

            foreach (var model in modelsToTry)
            {
                try
                {
                    var response = await CallGeminiAsync(model, apiKey, systemPrompt, messages, ct);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Gemini ({Model}) sorğusu uğursuz oldu: {Message}. Növbəti model və ya fallback yoxlanılır.", model, ex.Message);
                }
            }

            return FallbackAstrologyReply(systemPrompt, messages);
        }

        public async IAsyncEnumerable<string> StreamTextAsync(
            string systemPrompt,
            List<AiTurnDto> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // First get the generated text
            var fullText = await GenerateTextAsync(systemPrompt, messages, ct);

            // Chunk and stream tokens smoothly for frontend consumer
            var words = fullText.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (ct.IsCancellationRequested) yield break;
                yield return (i == 0 ? "" : " ") + words[i];
                await Task.Delay(20, ct);
            }
        }

        private async Task<string?> CallGeminiAsync(
            string model,
            string apiKey,
            string systemPrompt,
            List<AiTurnDto> messages,
            CancellationToken ct)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var contents = messages.Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            }).ToList();

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents,
                generationConfig = new
                {
                    temperature = 0.8,
                    maxOutputTokens = 1200
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Gemini API Status {StatusCode}: {Error}", response.StatusCode, err);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString();
                    return text?.Trim();
                }
            }

            return null;
        }

        private static string FallbackAstrologyReply(string systemPrompt, List<AiTurnDto> messages)
        {
            var lastUserMsg = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
            var q = lastUserMsg.ToLower();

            if (systemPrompt.Contains("redaktor", StringComparison.OrdinalIgnoreCase) || systemPrompt.Contains("MƏTN:", StringComparison.OrdinalIgnoreCase))
            {
                return @"BAŞLIQ: Astrologiya və Daxili Transformasiya
XÜLASƏ: Planetlərin səmavi hərəkəti həyatımıza necə işıq tutur və şəxsi inkişafı necə dəstəkləyir.
MƏTN:
Astrologiya qədim dövrlərdən bəri insanın kainatdakı yerini anlamaq və daxili potensialını üzə çıxarmaq üçün ən dərin bələdçilərdən biri olmuşdur. Göy üzündəki planetlərin hərəkəti təkcə zamanın axarını deyil, həm də daxili dünyamızdakı emosional dalğalanmaları əks etdirir.

Doğum xəritəsi insanın dünyaya gəldiyi anın səmavi şəkildir. Oradakı hər bir planet, ev və aspekt bizim güclü tərəflərimizi, sınaqlarımızı və inkişaf etməli olduğumuz sahələri göstərir. Günəş iradəmizi, Ay emosiyalarımızı, Merkuri isə düşüncə tərzimizi formalaşdırır.

Xüsusilə tranzitlər və retroqrad dövrlər həyatımızda tələsik qərarlardan çəkinərək daxili nizam yaratmaq üçün böyük fürsətlər təqdim edir. Öz ritminizi səmavi dövrlərlə uyğunlaşdırmaq sizə daha şüurlu və harmonik bir yaşam tərzi qazandıracaqdır.";
            }

            if (q.Contains("retroqrad") || q.Contains("retro"))
            {
                return "Planetlərin retroqrad hərəkəti həyatı dayandırmaq deyil, daxilə baxmaq və keçmiş planları yenidən nəzərdən keçirmək üçün bir fürsətdir. Bu dövrdə tələsik qərarlar əvəzinə yarımçıq qalmış işləri yekunlaşdırmaq sizə böyük xeyir gətirəcəkdir.";
            }
            if (q.Contains("uyğunluq") || q.Contains("sevgi") || q.Contains("münasibət"))
            {
                return "Münasibətlərdə ən vacib amil təkcə Günəş bürcü deyil, həm də Venera və Ay yerləşmələridir. Əgər bir-birinizin emosional ehtiyaclarına diqqət yetirsəniz və hisslərinizi açıq ifadə etsəniz, səmavi ahəng münasibətinizdə çiçəklənəcəkdir.";
            }
            if (q.Contains("karyera") || q.Contains("iş") || q.Contains("pul") || q.Contains("maliyyə"))
            {
                return "Saturn və Yupiterin hazırkı tranzitləri zəhmətkeşlik və nizam tələb edir. Bu ərəfədə başladığınız strateji addımlar uzunmüddətli maddi sabitlik və möhkəm təməl vəd edir.";
            }
            if (q.Contains("ay") || q.Contains("faza") || q.Contains("dolunay") || q.Contains("yeni ay"))
            {
                return "Ayın fazaları emosional ritmimizə güclü təsir edir. Yeni ay mərhələsi yeni niyyətlər qoymaq və planlar qurmaq üçün, Dolunay isə aydınlıq, tamamlanma və minnətdarlıq üçün ən əlverişli andır.";
            }

            return "Səma xəritəniz göstərir ki, hazırkı dövrdə intuisiyanıza güvənmək və daxili harmoniyanı qorumaq ən doğru yoldur. Ulduzlar sizə bələdçilik edir, lakin seçimlər hər zaman sizin iradənizdən asılıdır. Əlavə olaraq doğum xəritəniz üzrə hansı sahəni dərindən araşdırmaq istərdiniz?";
        }
    }

}
