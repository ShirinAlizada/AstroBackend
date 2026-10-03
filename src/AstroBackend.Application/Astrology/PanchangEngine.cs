using AstroBackend.Application.DTOs;

namespace AstroBackend.Application.Astrology
{
    /// <summary>
    /// Günün Bələdçisi (Daily Guide) hesablama mühərriki.
    ///
    /// 2026-10 tarixli yenidənqurmadan sonra bu mühərrik Vedik Panchang sistemini (Tithi,
    /// Nakşatra, Yoga, Karana, Tara-bala) tamamilə buraxıb. Frontend-dəki
    /// <c>src/lib/daily-guide.ts</c> ilə paralel olaraq indi xalis Qərb (tropik) astrologiyasına
    /// əsaslanır: bugünkü Ay bürcü/ünsürü, klassik gün hakimi planet və kateqoriyaları idarə
    /// edən planet cütləri arasındaki aspektlər (Sevgi=Venera+Ay, Karyera=Günəş+Saturn,
    /// Maliyyə=Yupiter+Venera, Sağlamlıq=Mars+Ay).
    ///
    /// QEYD: sinif, fayl, DTO və API marşrut adları ("Panchang") qəsdən DƏYİŞDİRİLMƏYİB. Bu
    /// backend-də `dotnet build` ilə yoxlama mümkün olmadığı üçün (bax: tapşırığın qeydləri),
    /// mövcud DI qeydiyyatını (IPanchangService → PanchangService) və API marşrutunu
    /// (/api/panchang/*) riskə atmamaq üçün yalnız hesablama məntiqi və cavab formatı (bax:
    /// PanchangDtos.cs) yenilənib — PanchangService.cs, PanchangController.cs, IServices.cs və
    /// hər iki ServiceRegistration.cs faylı dəyişməyib.
    /// </summary>
    public static class PanchangEngine
    {
        /// <summary>Gün rəngi — frontend-dəki eyni funksiya ilə eynidir, dəyişməyib: 0=Bazar...6=Şənbə.</summary>
        public static readonly string[] DayColorsAz =
        [
            "Qırmızı", "Ağ", "Al-qırmızı", "Yaşıl", "Sarı", "Açıq mavi", "Tünd göy"
        ];

        /// <summary>Klassik gün hakimi planetləri: 0=Bazar...6=Şənbə (DateTime.DayOfWeek ilə eyni indeksləmə).</summary>
        public static readonly string[] DayRulersAz =
        [
            "Günəş", "Ay", "Mars", "Merkuri", "Yupiter", "Venera", "Saturn"
        ];

        private const double BakuLatitude = 40.4093;
        private const double BakuLongitude = 49.8671;

        /// <summary>Kateqoriya → onu idarə edən 2 planet (frontend-dəki CATEGORIES ilə eynidir).</summary>
        private static readonly (string CategoryAz, string PlanetA, string PlanetB)[] Categories =
        [
            ("Sevgi", "Venera", "Ay"),
            ("Karyera", "Günəş", "Saturn"),
            ("Maliyyə", "Yupiter", "Venera"),
            ("Sağlamlıq", "Mars", "Ay"),
        ];

        /// <summary>Harmonik aspektlər — əlverişli sayılır (frontend-dəki FAVORABLE_ASPECT_KEYS ilə eynidir).</summary>
        private static readonly HashSet<string> FavorableAspects = new()
        {
            "Konyunksiya", "Sekstil", "Trigon"
        };

        /// <summary>Gərgin əsas aspektlər.</summary>
        private static readonly HashSet<string> TenseAspects = new()
        {
            "Kvadrat", "Oppozisiya"
        };

        public static PanchangResponse Compute(DateTime date)
        {
            // Bu backend-də real efemerid (canlı səma) hesablayıcısı yoxdur — AstrologyEngine yalnız
            // natal xəritə hesablayır. "Bugünkü səma"nı təxmin etmək üçün həmin gün, günorta saat
            // 12:00, Bakı koordinatları ilə bir "natal" xəritə hesablanır. Bu, frontend-dəki real
            // astronomik `computeCurrentSky()` funksiyasının sadələşdirilmiş ekvivalentidir və
            // AstrologyEngine.AspectNameAz-ın (bürc fərqinə əsaslanan, hər zaman nəticə verən)
            // təbiətinə uyğundur.
            var sky = AstrologyEngine.ComputeNatalChart(date.ToString("yyyy-MM-dd"), "12:00", BakuLatitude, BakuLongitude);

            string SignOf(string planet) => planet switch
            {
                "Günəş" => sky.Sun,
                "Ay" => sky.Moon,
                _ => sky.Planets.FirstOrDefault(p => p.Name == planet)?.Sign ?? sky.Sun
            };

            string moonSign = sky.Moon;
            AstrologyEngine.Elements.TryGetValue(moonSign, out var moonElement);
            moonElement ??= "—";

            int weekday = (int)date.DayOfWeek;
            string dayColor = DayColorsAz[weekday];
            string dayRuler = DayRulersAz[weekday];

            var aspects = new List<AspectHitDto>();
            var guidance = new List<GuidanceItemDto>();

            foreach (var (categoryAz, planetA, planetB) in Categories)
            {
                string signA = SignOf(planetA);
                string signB = SignOf(planetB);
                string aspect = AstrologyEngine.AspectNameAz(signA, signB);

                aspects.Add(new AspectHitDto(planetA, planetB, signA, signB, aspect));

                string verdict = FavorableAspects.Contains(aspect)
                    ? "əlverişli"
                    : TenseAspects.Contains(aspect)
                        ? "ehtiyatlı ol"
                        : "neytral";

                string aspectLower = aspect.ToLowerInvariant();
                string categoryLower = categoryAz.ToLowerInvariant();

                string reason = verdict switch
                {
                    "əlverişli" =>
                        $"{planetA} ({signA}) və {planetB} ({signB}) arasındakı {aspectLower} bu gün {categoryLower} sahəsində axını asanlaşdırır.",
                    "ehtiyatlı ol" =>
                        $"{planetA} ({signA}) və {planetB} ({signB}) arasındakı {aspectLower} {categoryLower} mövzularında bu gün bir az ehtiyatlı olmağı tələb edir.",
                    _ =>
                        $"{planetA} ({signA}) və {planetB} ({signB}) arasındakı {aspectLower} {categoryLower} sahəsinə neytral təsir göstərir — bugünkü gün hakimi {dayRuler} enerjisinə etibar edə bilərsiniz.",
                };

                guidance.Add(new GuidanceItemDto(categoryAz, verdict, reason));
            }

            return new PanchangResponse(
                date.ToString("yyyy-MM-dd"),
                moonSign,
                moonElement,
                dayRuler,
                weekday,
                dayColor,
                aspects,
                guidance
            );
        }
    }
}
