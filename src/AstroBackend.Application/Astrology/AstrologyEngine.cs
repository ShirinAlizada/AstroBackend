using AstroBackend.Application.DTOs;

namespace AstroBackend.Application.Astrology
{
    public record AspectScore(int Overall, int Love, int Friendship, int Communication);

    public static class AstrologyEngine
    {
        public static readonly string[] SignsAz =
        [
            "Qoç", "Buğa", "Əkizlər", "Xərçəng", "Aslan", "Qız",
        "Tərəzi", "Əqrəb", "Oxatan", "Oğlaq", "Dolça", "Balıqlar"
        ];

        public static readonly Dictionary<string, string> Elements = new()
    {
        { "Qoç", "Od" }, { "Aslan", "Od" }, { "Oxatan", "Od" },
        { "Buğa", "Torpaq" }, { "Qız", "Torpaq" }, { "Oğlaq", "Torpaq" },
        { "Əkizlər", "Hava" }, { "Tərəzi", "Hava" }, { "Dolça", "Hava" },
        { "Xərçəng", "Su" }, { "Əqrəb", "Su" }, { "Balıqlar", "Su" }
    };

        public static string SunSignFromDate(DateTime date)
        {
            int m = date.Month;
            int d = date.Day;

            return m switch
            {
                1 => d <= 19 ? "Oğlaq" : "Dolça",
                2 => d <= 18 ? "Dolça" : "Balıqlar",
                3 => d <= 20 ? "Balıqlar" : "Qoç",
                4 => d <= 19 ? "Qoç" : "Buğa",
                5 => d <= 20 ? "Buğa" : "Əkizlər",
                6 => d <= 20 ? "Əkizlər" : "Xərçəng",
                7 => d <= 22 ? "Xərçəng" : "Aslan",
                8 => d <= 22 ? "Aslan" : "Qız",
                9 => d <= 22 ? "Qız" : "Tərəzi",
                10 => d <= 22 ? "Tərəzi" : "Əqrəb",
                11 => d <= 21 ? "Əqrəb" : "Oxatan",
                12 => d <= 21 ? "Oxatan" : "Oğlaq",
                _ => "Qoç"
            };
        }

        public static NatalChartResponse ComputeNatalChart(string dateStr, string timeStr, double latitude, double longitude)
        {
            if (!DateTime.TryParse(dateStr, out var birthDate))
            {
                birthDate = new DateTime(2000, 1, 1);
            }

            int hour = 12, minute = 0;
            if (!string.IsNullOrWhiteSpace(timeStr))
            {
                var parts = timeStr.Split(':');
                if (parts.Length >= 1 && int.TryParse(parts[0], out var h)) hour = h;
                if (parts.Length >= 2 && int.TryParse(parts[1], out var mi)) minute = mi;
            }

            string sunSign = SunSignFromDate(birthDate);
            int sunIndex = Array.IndexOf(SignsAz, sunSign);
            if (sunIndex < 0) sunIndex = 0;

            // Ascendant approximation based on birth time
            int ascIndex = (sunIndex + (hour / 2)) % 12;
            string ascendantSign = SignsAz[ascIndex];

            // Moon sign approximation
            int moonIndex = (sunIndex + (birthDate.Day % 12) + (hour / 4)) % 12;
            string moonSign = SignsAz[moonIndex];

            // Midheaven
            int mcIndex = (ascIndex + 9) % 12;
            string midheavenSign = SignsAz[mcIndex];

            // 12 Houses
            var houses = new List<HousePositionDto>();
            for (int i = 0; i < 12; i++)
            {
                int signIdx = (ascIndex + i) % 12;
                houses.Add(new HousePositionDto(i + 1, SignsAz[signIdx], (15 + (i * 2)) % 30));
            }

            // Planets
            var planets = new List<PlanetPositionDto>
        {
            new("Günəş", sunSign, (birthDate.Day * 2) % 30, (sunIndex - ascIndex + 12) % 12 + 1, false),
            new("Ay", moonSign, (birthDate.Day * 3 + hour) % 30, (moonIndex - ascIndex + 12) % 12 + 1, false),
            new("Merkuri", SignsAz[(sunIndex + 11) % 12], (birthDate.Day + 5) % 30, ((sunIndex + 11) - ascIndex + 12) % 12 + 1, (birthDate.Day % 3 == 0)),
            new("Venera", SignsAz[(sunIndex + 1) % 12], (birthDate.Day + 12) % 30, ((sunIndex + 1) - ascIndex + 12) % 12 + 1, false),
            new("Mars", SignsAz[(sunIndex + 4) % 12], (birthDate.Day + 18) % 30, ((sunIndex + 4) - ascIndex + 12) % 12 + 1, (birthDate.Day % 5 == 0)),
            new("Yupiter", SignsAz[(sunIndex + 7) % 12], (birthDate.Day + 22) % 30, ((sunIndex + 7) - ascIndex + 12) % 12 + 1, false),
            new("Saturn", SignsAz[(sunIndex + 10) % 12], (birthDate.Day + 8) % 30, ((sunIndex + 10) - ascIndex + 12) % 12 + 1, (birthDate.Day % 2 == 0)),
            new("Uran", SignsAz[(sunIndex + 2) % 12], 14, ((sunIndex + 2) - ascIndex + 12) % 12 + 1, false),
            new("Neptun", SignsAz[(sunIndex + 11) % 12], 26, ((sunIndex + 11) - ascIndex + 12) % 12 + 1, false),
            new("Pluton", SignsAz[(sunIndex + 9) % 12], 2, ((sunIndex + 9) - ascIndex + 12) % 12 + 1, false)
        };

            return new NatalChartResponse(
                planets,
                houses,
                sunSign,
                moonSign,
                ascendantSign,
                midheavenSign
            );
        }

        public static SynastryResponse ComputeSynastry(string signA, string signB)
        {
            int ia = Array.IndexOf(SignsAz, signA);
            int ib = Array.IndexOf(SignsAz, signB);
            if (ia < 0) ia = 0;
            if (ib < 0) ib = 0;

            int diff = Math.Min((ia - ib + 12) % 12, (ib - ia + 12) % 12);

            var scores = new Dictionary<int, AspectScore>
        {
            { 0, new AspectScore(88, 85, 90, 88) },
            { 1, new AspectScore(58, 55, 60, 60) },
            { 2, new AspectScore(82, 80, 85, 80) },
            { 3, new AspectScore(48, 50, 45, 48) },
            { 4, new AspectScore(94, 95, 92, 95) },
            { 5, new AspectScore(52, 50, 55, 50) },
            { 6, new AspectScore(72, 75, 68, 72) }
        };

            var score = scores.TryGetValue(diff, out var s) ? s : new AspectScore(65, 65, 65, 65);

            var notes = new List<string>();
            Elements.TryGetValue(signA, out var elemA);
            Elements.TryGetValue(signB, out var elemB);

            if (elemA != null && elemB != null)
            {
                if (elemA == elemB)
                {
                    notes.Add($"Hər iki bürc {elemA} ünsürünə aiddir — təbii harmoniya, oxşar həyat baxışı və yüksək daxili anlaşma.");
                }
                else if ((elemA == "Od" && elemB == "Hava") || (elemA == "Hava" && elemB == "Od"))
                {
                    notes.Add("Od və Hava ünsürlərinin qovuşması — bir-birini alovlandıran yüksək enerji, intellektual maraq və macəra.");
                }
                else if ((elemA == "Torpaq" && elemB == "Su") || (elemA == "Su" && elemB == "Torpaq"))
                {
                    notes.Add("Torpaq və Su ünsürlərinin vəhdəti — möhkəm təməl, dərin emosional etibar və qarşılıqlı dəstək.");
                }
                else
                {
                    notes.Add($"Fərqli ünsürlər ({elemA} və {elemB}) — bir-birinizi tamamlayaraq həyatınızda yeni dünyalar aça bilərsiniz.");
                }
            }

            if (score.Love >= 80)
                notes.Add("Venera və emosional cazibə yüksəkdir: romantik münasibətlər üçün olduqca əlverişli səma bağlantısı.");
            else
                notes.Add("Sevgi dili fərqlilik göstərə bilər: hisslərinizi açıq və səmimi ifadə etmək münasibəti möhkəmləndirər.");

            if (score.Communication >= 80)
                notes.Add("Merkuri intellektual uyğunluğu mükəmməldir: ortaq mövzular tapmaq və anlaşmaq çox asandır.");
            else
                notes.Add("Ünsiyyət zamanı səbir və təmkin önəmlidir: fərqli fikirlərə hörmətlə yanaşma tövsiyə olunur.");

            return new SynastryResponse(score.Overall, score.Love, score.Friendship, score.Communication, notes);
        }
    }

}
