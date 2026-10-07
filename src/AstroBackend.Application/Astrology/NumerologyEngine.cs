using AstroBackend.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstroBackend.Application.Astrology
{
    /// <summary>
    /// Bir əsas ədədin (1-9, 11, 22, 33) üç dildə (az/en/ru) başlıq, mətn və açar sözləri,
    /// həmçinin təbii uyğunluq göstərdiyi digər əsas ədədlər — frontend-dəki `NumberMeaning`
    /// interfeysi (src/lib/numerology.ts, `NUMBER_MEANINGS`) ilə eynidir.
    /// </summary>
    public sealed record NumberMeaningEntry(
        string TitleAz, string TitleEn, string TitleRu,
        string TextAz, string TextEn, string TextRu,
        string[] KeywordsAz, string[] KeywordsEn, string[] KeywordsRu,
        int[] Compatible
    );

    public static class NumerologyEngine
    {
        private static readonly Dictionary<char, int> LetterValues = new()
    {
        { 'a', 1 }, { 'j', 1 }, { 's', 1 }, { 'ş', 1 },
        { 'b', 2 }, { 'k', 2 }, { 't', 2 },
        { 'c', 3 }, { 'l', 3 }, { 'u', 3 }, { 'ü', 3 }, { 'ç', 3 }, { 'ı', 3 },
        { 'd', 4 }, { 'm', 4 }, { 'v', 4 },
        { 'e', 5 }, { 'n', 5 }, { 'w', 5 }, { 'ə', 5 },
        { 'f', 6 }, { 'o', 6 }, { 'x', 6 }, { 'ö', 6 },
        { 'g', 7 }, { 'p', 7 }, { 'y', 7 }, { 'ğ', 7 },
        { 'h', 8 }, { 'q', 8 }, { 'z', 8 },
        { 'i', 9 }, { 'r', 9 }
    };

        private static readonly HashSet<char> Vowels = ['a', 'e', 'ə', 'i', 'ı', 'o', 'ö', 'u', 'ü'];
        private static readonly HashSet<int> MasterNumbers = [11, 22, 33];

        /// <summary>
        /// Frontend-dəki `NUMBER_MEANINGS` (src/lib/numerology.ts) ilə sözbəsöz üst-üstə düşür —
        /// mətn, açar sözlər və uyğunluq siyahıları dəyişdirilmədən köçürülüb (az mətndəki
        /// "Quruculu" kimi kiçik qeyri-müntəzəmliklər də frontend ilə tam parallellik üçün saxlanıb).
        /// </summary>
        public static readonly Dictionary<int, NumberMeaningEntry> Meanings = new()
    {
        {
            1, new NumberMeaningEntry(
                "Lider", "Leader", "Лидер",
                "Müstəqillik, təşəbbüskarlıq və yeni başlanğıclar. Öndə getmək, öz yolunu yaratmaq bacarığı.",
                "Independence, initiative and new beginnings. The drive to go first and carve your own path.",
                "Независимость, инициативность и новые начинания. Стремление идти первым и прокладывать свой путь.",
                new[] { "Liderlik", "Müstəqillik", "Təşəbbüs" },
                new[] { "Leadership", "Independence", "Initiative" },
                new[] { "Лидерство", "Независимость", "Инициатива" },
                new[] { 3, 5 })
        },
        {
            2, new NumberMeaningEntry(
                "Diplomat", "Diplomat", "Дипломат",
                "Əməkdaşlıq, həssaslıq və tarazlıq. Münasibətlərdə körpü qurmaq, səbrlə dinləmək.",
                "Cooperation, sensitivity and balance. A gift for building bridges and listening with patience.",
                "Сотрудничество, чуткость и баланс. Дар строить мосты и терпеливо слушать.",
                new[] { "Əməkdaşlıq", "Həssaslıq", "Tarazlıq" },
                new[] { "Cooperation", "Sensitivity", "Balance" },
                new[] { "Сотрудничество", "Чуткость", "Баланс" },
                new[] { 6, 9 })
        },
        {
            3, new NumberMeaningEntry(
                "Yaradıcı", "Creative", "Творец",
                "İfadə, ünsiyyət və optimizm. Sənət, söz və özünü göstərmək enerjisi.",
                "Expression, communication and optimism. The energy of art, words and showing yourself to the world.",
                "Самовыражение, общение и оптимизм. Энергия искусства, слова и умения проявить себя.",
                new[] { "İfadə", "Ünsiyyət", "Optimizm" },
                new[] { "Expression", "Communication", "Optimism" },
                new[] { "Самовыражение", "Общение", "Оптимизм" },
                new[] { 1, 5 })
        },
        {
            4, new NumberMeaningEntry(
                "Quruculu", "Builder", "Строитель",
                "Nizam, sabitlik və zəhmətkeşlik. Möhkəm təməl qurmaq, ardıcıl addımlar.",
                "Order, stability and hard work. Laying solid foundations and moving forward step by step.",
                "Порядок, стабильность и трудолюбие. Умение закладывать прочный фундамент и идти к цели шаг за шагом.",
                new[] { "Nizam", "Sabitlik", "Zəhmət" },
                new[] { "Order", "Stability", "Discipline" },
                new[] { "Порядок", "Стабильность", "Труд" },
                new[] { 7, 8 })
        },
        {
            5, new NumberMeaningEntry(
                "Sərgərdan", "Wanderer", "Странник",
                "Azadlıq, dəyişkənlik və macəra. Yenilik axtarışı, çevik düşüncə.",
                "Freedom, change and adventure. A restless search for novelty and a flexible mind.",
                "Свобода, перемены и приключения. Неутомимый поиск нового и гибкость мышления.",
                new[] { "Azadlıq", "Dəyişkənlik", "Macəra" },
                new[] { "Freedom", "Change", "Adventure" },
                new[] { "Свобода", "Перемены", "Приключения" },
                new[] { 1, 3 })
        },
        {
            6, new NumberMeaningEntry(
                "Qoruyucu", "Guardian", "Хранитель",
                "Məsuliyyət, qayğı və ailə dəyərləri. Harmoniya yaratmaq, xidmət etmək.",
                "Responsibility, care and family values. Creating harmony and being of service to others.",
                "Ответственность, забота и семейные ценности. Умение создавать гармонию и служить другим.",
                new[] { "Məsuliyyət", "Qayğı", "Ailə" },
                new[] { "Responsibility", "Care", "Family" },
                new[] { "Ответственность", "Забота", "Семья" },
                new[] { 2, 9 })
        },
        {
            7, new NumberMeaningEntry(
                "Axtarıcı", "Seeker", "Искатель",
                "Dərinlik, təhlil və mənəvi axtarış. Tənhalıqda güc tapmaq, həqiqəti araşdırmaq.",
                "Depth, analysis and a spiritual search. Finding strength in solitude and uncovering the truth.",
                "Глубина, анализ и духовный поиск. Умение находить силу в одиночестве и доходить до истины.",
                new[] { "Dərinlik", "Təhlil", "Mənəviyyat" },
                new[] { "Depth", "Analysis", "Spirituality" },
                new[] { "Глубина", "Анализ", "Духовность" },
                new[] { 4, 8 })
        },
        {
            8, new NumberMeaningEntry(
                "Təşkilatçı", "Organizer", "Организатор",
                "Güc, maddi uğur və idarəetmə. Böyük layihələri həyata keçirmək bacarığı.",
                "Power, material success and management. The ability to carry out big projects and lead.",
                "Сила, материальный успех и управление. Способность реализовывать крупные проекты и вести за собой.",
                new[] { "Güc", "Maddi uğur", "İdarəetmə" },
                new[] { "Power", "Material success", "Management" },
                new[] { "Сила", "Материальный успех", "Управление" },
                new[] { 4, 7 })
        },
        {
            9, new NumberMeaningEntry(
                "Humanist", "Humanitarian", "Гуманист",
                "Şəfqət, geniş baxış və tamamlanma. Başqalarına xidmət, universal sevgi.",
                "Compassion, a broad perspective and completion. Serving others with a universal kind of love.",
                "Сострадание, широкий взгляд и завершение. Служение другим во имя всеобщей любви.",
                new[] { "Şəfqət", "Geniş baxış", "Tamamlanma" },
                new[] { "Compassion", "Broad perspective", "Completion" },
                new[] { "Сострадание", "Широкий взгляд", "Завершение" },
                new[] { 2, 6 })
        },
        {
            11, new NumberMeaningEntry(
                "İntuitiv usta (Master)", "Intuitive Master", "Интуитивный мастер",
                "Yüksək intuisiya və ilham mənbəyi. Mənəvi rəhbərlik potensialı — tarazlıq tələb edir.",
                "High intuition and a source of inspiration. Potential for spiritual guidance — balance is required.",
                "Высокая интуиция и источник вдохновения. Потенциал духовного наставничества — требует баланса.",
                new[] { "İntuisiya", "İlham", "Mənəvi rəhbərlik" },
                new[] { "Intuition", "Inspiration", "Spiritual guidance" },
                new[] { "Интуиция", "Вдохновение", "Духовное наставничество" },
                new[] { 2, 6 })
        },
        {
            22, new NumberMeaningEntry(
                "Böyük qurucu (Master)", "Master Builder", "Великий строитель",
                "Böyük ideyaları reallığa çevirmək gücü. Vizyonu konkret nəticəyə çatdırmaq.",
                "The power to turn big ideas into reality. Carrying a vision all the way to a concrete result.",
                "Сила превращать большие идеи в реальность. Умение довести видение до конкретного результата.",
                new[] { "Vizyon", "Reallaşdırma", "Böyük ideyalar" },
                new[] { "Vision", "Realization", "Big ideas" },
                new[] { "Видение", "Реализация", "Большие идеи" },
                new[] { 4, 8 })
        },
        {
            33, new NumberMeaningEntry(
                "Mənəvi müəllim (Master)", "Master Teacher", "Духовный учитель",
                "Qeydsiz-şərtsiz qayğı və fədakarlıq. Başqalarını ruhən yüksəltmək missiyası.",
                "Unconditional care and devotion. A mission to spiritually uplift others.",
                "Безусловная забота и самоотдача. Миссия духовно возвышать других.",
                new[] { "Fədakarlıq", "Qayğı", "Ruhən yüksəltmə" },
                new[] { "Devotion", "Care", "Spiritual upliftment" },
                new[] { "Самоотдача", "Забота", "Духовное возвышение" },
                new[] { 6, 9 })
        },
    };

        /// <summary>
        /// Şəxsi il ədədinin (1-9) qısa mövzusu — frontend-dəki `PERSONAL_YEAR_THEMES`
        /// (src/lib/numerology.ts) ilə eynidir.
        /// </summary>
        private static readonly Dictionary<int, (string Az, string En, string Ru)> PersonalYearThemes = new()
    {
        { 1, ("Yeni başlanğıclar ili — toxum əkmə vaxtıdır.", "A year of new beginnings — time to plant the seeds.", "Год новых начинаний — время сажать семена.") },
        { 2, ("Səbr və əməkdaşlıq ili — münasibətlər önə çıxır.", "A year of patience and partnership — relationships take the lead.", "Год терпения и сотрудничества — на первом плане отношения.") },
        { 3, ("Yaradıcılıq və ünsiyyət ili — özünü ifadə et.", "A year of creativity and self-expression — let yourself be seen.", "Год творчества и общения — время проявить себя.") },
        { 4, ("Zəhmət və quruculuq ili — möhkəm təməl qur.", "A year of discipline and building — lay a solid foundation.", "Год труда и строительства — закладывай прочный фундамент.") },
        { 5, ("Dəyişiklik və azadlıq ili — gözlənilməzliyə açıq ol.", "A year of change and freedom — stay open to the unexpected.", "Год перемен и свободы — будь открыт неожиданностям.") },
        { 6, ("Məsuliyyət və ailə ili — ev və münasibətlər önəmlidir.", "A year of responsibility and family — home and relationships matter most.", "Год ответственности и семьи — дом и отношения выходят на первый план.") },
        { 7, ("Dərinləşmə və düşüncə ili — içinə çək, araşdır.", "A year of reflection and inner work — turn inward and explore.", "Год размышлений и внутренней работы — обратись внутрь себя.") },
        { 8, ("Güc və nailiyyət ili — zəhmətin bəhrəsini yığırsan.", "A year of power and achievement — you reap what you've sown.", "Год силы и достижений — пора собирать плоды труда.") },
        { 9, ("Tamamlanma ili — buraxmaq və yekunlaşdırmaq vaxtıdır.", "A year of completion — time to let go and close a chapter.", "Год завершения — время отпускать и подводить итоги.") },
    };

        public static int ReduceNumber(int n, bool keepMaster = true)
        {
            int value = n;
            while (value > 9 && !(keepMaster && MasterNumbers.Contains(value)))
            {
                int sum = 0;
                while (value > 0)
                {
                    sum += value % 10;
                    value /= 10;
                }
                value = sum;
            }
            return value;
        }

        /// <summary>
        /// Yetkinlik (Maturity) ədədi — Həyat yolu və Tale ədədlərinin cəmindən alınır.
        /// Frontend-dəki `maturityNumber` (src/lib/numerology.ts) ilə eynidir.
        /// </summary>
        public static int MaturityNumber(int lifePath, int destiny) => ReduceNumber(lifePath + destiny);

        /// <summary>
        /// Şəxsi il ədədi — doğum günü + doğum ayı + hədəf ilin rəqəmləri. Frontend-dəki
        /// `personalYearNumber` (src/lib/numerology.ts) ilə eynidir: `keepMaster` yoxdur
        /// (klassik konvensiyaya görə şəxsi il həmişə 1-9 arasına endirilir), `forYear`
        /// göndərilməzsə cari il istifadə olunur.
        /// </summary>
        public static int PersonalYearNumber(string birthDateStr, int? forYear = null)
        {
            int day = 1, month = 1;
            if (DateTime.TryParse(birthDateStr, out var bdt))
            {
                day = bdt.Day;
                month = bdt.Month;
            }

            int year = forYear ?? DateTime.UtcNow.Year;
            string digitsStr = $"{day}{month}{year}";
            int sum = digitsStr.Where(char.IsDigit).Sum(c => c - '0');
            return ReduceNumber(sum, keepMaster: false);
        }

        /// <summary>Verilmiş əsas ədədin (1-9, 11, 22, 33) seçilmiş dildəki başlıq/mətn/açar sözlərini qaytarır.</summary>
        public static (string Title, string Text, List<string> Keywords) LocalizedMeaning(int number, string? lang)
        {
            if (!Meanings.TryGetValue(number, out var m))
                return ("Enerji", "Xüsusi numeroloji vibrasiya.", new List<string>());

            if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
                return (m.TitleEn, m.TextEn, m.KeywordsEn.ToList());
            if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase))
                return (m.TitleRu, m.TextRu, m.KeywordsRu.ToList());
            return (m.TitleAz, m.TextAz, m.KeywordsAz.ToList());
        }

        /// <summary>Şəxsi il ədədinin seçilmiş dildəki qısa mövzu mətni.</summary>
        public static string LocalizedPersonalYearTheme(int number, string? lang)
        {
            if (!PersonalYearThemes.TryGetValue(number, out var theme)) return "";
            if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase)) return theme.En;
            if (string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase)) return theme.Ru;
            return theme.Az;
        }

        public static NumerologyResponse Compute(string fullName, string birthDateStr, string? lang = null, int? forYear = null)
        {
            var cleanDigits = new string(birthDateStr.Where(char.IsDigit).ToArray());
            int birthSum = cleanDigits.Sum(c => c - '0');
            int lifePath = ReduceNumber(birthSum);

            int bDay = 1;
            if (DateTime.TryParse(birthDateStr, out var bdt))
                bDay = bdt.Day;
            int birthday = ReduceNumber(bDay);

            var cleanName = fullName.ToLower().Where(c => LetterValues.ContainsKey(c)).ToArray();
            int destinySum = cleanName.Sum(c => LetterValues[c]);
            int destiny = ReduceNumber(destinySum);

            int soulSum = cleanName.Where(Vowels.Contains).Sum(c => LetterValues[c]);
            int soulUrge = ReduceNumber(soulSum);

            int personalitySum = cleanName.Where(c => !Vowels.Contains(c)).Sum(c => LetterValues[c]);
            int personality = ReduceNumber(personalitySum);

            int maturity = MaturityNumber(lifePath, destiny);
            int personalYear = PersonalYearNumber(birthDateStr, forYear);
            string personalYearTheme = LocalizedPersonalYearTheme(personalYear, lang);

            NumerologyItemDto BuildItem(int number, string name)
            {
                var (title, text, keywords) = LocalizedMeaning(number, lang);
                var compatible = Meanings.TryGetValue(number, out var m) ? m.Compatible.ToList() : new List<int>();
                return new NumerologyItemDto(number, name, title, text, keywords, compatible);
            }

            var details = new List<NumerologyItemDto>
        {
            BuildItem(lifePath, "Həyat yolu (Life Path)"),
            BuildItem(destiny, "Tale / İfadə ədədi (Destiny)"),
            BuildItem(soulUrge, "Qəlb arzusu (Soul Urge)"),
            BuildItem(personality, "Xarakter / Görünüş (Personality)"),
            BuildItem(birthday, "Doğum günü ədədi"),
        };

            return new NumerologyResponse(lifePath, destiny, soulUrge, personality, birthday, maturity, personalYear, personalYearTheme, details);
        }
    }

}
