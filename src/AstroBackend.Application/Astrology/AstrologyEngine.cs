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

        public static readonly Dictionary<string, string> SignSymbols = new()
    {
        { "Qoç", "♈" }, { "Buğa", "♉" }, { "Əkizlər", "♊" },
        { "Xərçəng", "♋" }, { "Aslan", "♌" }, { "Qız", "♍" },
        { "Tərəzi", "♎" }, { "Əqrəb", "♏" }, { "Oxatan", "♐" },
        { "Oğlaq", "♑" }, { "Dolça", "♒" }, { "Balıqlar", "♓" }
    };

        public static readonly Dictionary<string, string> BodySymbols = new()
    {
        { "Günəş", "☉" }, { "Ay", "☾" }, { "Merkuri", "☿" },
        { "Venera", "♀" }, { "Mars", "♂" }, { "Yupiter", "♃" },
        { "Saturn", "♄" }, { "Uran", "♅" }, { "Neptun", "♆" },
        { "Pluton", "♇" }, { "Xiron", "⚷" }
    };

        public static readonly Dictionary<string, string> Elements = new()
    {
        { "Qoç", "Od" }, { "Aslan", "Od" }, { "Oxatan", "Od" },
        { "Buğa", "Torpaq" }, { "Qız", "Torpaq" }, { "Oğlaq", "Torpaq" },
        { "Əkizlər", "Hava" }, { "Tərəzi", "Hava" }, { "Dolça", "Hava" },
        { "Xərçəng", "Su" }, { "Əqrəb", "Su" }, { "Balıqlar", "Su" }
    };

        public static (int Degree, int Minute) SplitDegree(double rawWithinSign)
        {
            int totalMinutes = (int)Math.Round(rawWithinSign * 60);
            int degree = totalMinutes / 60;
            int minute = totalMinutes % 60;
            if (degree >= 30)
            {
                degree = 29;
                minute = 59;
            }
            return (degree, minute);
        }

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

            // Ascendant approximation based on birth time & coordinates
            int ascIndex = (sunIndex + (hour / 2)) % 12;
            string ascendantSign = SignsAz[ascIndex];
            var (ascDeg, ascMin) = SplitDegree((hour * 2.5 + minute * 0.1 + (birthDate.Day % 5)) % 30);

            // Moon sign approximation
            int moonIndex = (sunIndex + (birthDate.Day % 12) + (hour / 4)) % 12;
            string moonSign = SignsAz[moonIndex];

            // Midheaven
            int mcIndex = (ascIndex + 9) % 12;
            string midheavenSign = SignsAz[mcIndex];
            var (mcDeg, mcMin) = SplitDegree((hour * 2.1 + (birthDate.Day % 7)) % 30);

            // 12 Houses with degrees and minutes
            var houses = new List<HousePositionDto>();
            for (int i = 0; i < 12; i++)
            {
                int signIdx = (ascIndex + i) % 12;
                var (hDeg, hMin) = SplitDegree((12 + (i * 2.3) + (birthDate.Day % 4)) % 30);
                houses.Add(new HousePositionDto(i + 1, SignsAz[signIdx], hDeg, hMin));
            }

            // 10 Celestial Bodies
            var (sunDeg, sunMin) = SplitDegree((birthDate.Day * 1.8 + hour * 0.1) % 30);
            var (moonDeg, moonMin) = SplitDegree((birthDate.Day * 2.6 + hour * 0.5) % 30);
            var (merDeg, merMin) = SplitDegree((birthDate.Day + 5.4 + hour * 0.2) % 30);
            var (venDeg, venMin) = SplitDegree((birthDate.Day + 12.3) % 30);
            var (marDeg, marMin) = SplitDegree((birthDate.Day + 18.7) % 30);
            var (jupDeg, jupMin) = SplitDegree((birthDate.Day + 22.1) % 30);
            var (satDeg, satMin) = SplitDegree((birthDate.Day + 8.9) % 30);

            var planets = new List<PlanetPositionDto>
        {
            new("Günəş", sunSign, sunDeg, sunMin, (sunIndex - ascIndex + 12) % 12 + 1, false),
            new("Ay", moonSign, moonDeg, moonMin, (moonIndex - ascIndex + 12) % 12 + 1, false),
            new("Merkuri", SignsAz[(sunIndex + 11) % 12], merDeg, merMin, ((sunIndex + 11) - ascIndex + 12) % 12 + 1, (birthDate.Day % 3 == 0)),
            new("Venera", SignsAz[(sunIndex + 1) % 12], venDeg, venMin, ((sunIndex + 1) - ascIndex + 12) % 12 + 1, false),
            new("Mars", SignsAz[(sunIndex + 4) % 12], marDeg, marMin, ((sunIndex + 4) - ascIndex + 12) % 12 + 1, (birthDate.Day % 5 == 0)),
            new("Yupiter", SignsAz[(sunIndex + 7) % 12], jupDeg, jupMin, ((sunIndex + 7) - ascIndex + 12) % 12 + 1, false),
            new("Saturn", SignsAz[(sunIndex + 10) % 12], satDeg, satMin, ((sunIndex + 10) - ascIndex + 12) % 12 + 1, (birthDate.Day % 2 == 0)),
            new("Uran", SignsAz[(sunIndex + 2) % 12], 14, 28, ((sunIndex + 2) - ascIndex + 12) % 12 + 1, false),
            new("Neptun", SignsAz[(sunIndex + 11) % 12], 26, 45, ((sunIndex + 11) - ascIndex + 12) % 12 + 1, false),
            new("Pluton", SignsAz[(sunIndex + 9) % 12], 2, 14, ((sunIndex + 9) - ascIndex + 12) % 12 + 1, false)
        };

            return new NatalChartResponse(
                planets,
                houses,
                sunSign,
                moonSign,
                new AnglePointDto(ascendantSign, ascDeg, ascMin),
                new AnglePointDto(midheavenSign, mcDeg, mcMin)
            );
        }

        public static string AspectNameAz(string signA, string signB)
        {
            int ia = Array.IndexOf(SignsAz, signA);
            int ib = Array.IndexOf(SignsAz, signB);
            if (ia < 0 || ib < 0) return "—";
            int diff = Math.Min((ia - ib + 12) % 12, (ib - ia + 12) % 12);
            return diff switch
            {
                0 => "Konyunksiya",
                1 => "Yarımsekstil",
                2 => "Sekstil",
                3 => "Kvadrat",
                4 => "Trigon",
                5 => "Kvinkuns",
                6 => "Oppozisiya",
                _ => "—"
            };
        }

        public static int PairScore(string signA, string signB)
        {
            int ia = Array.IndexOf(SignsAz, signA);
            int ib = Array.IndexOf(SignsAz, signB);
            if (ia < 0 || ib < 0) return 50;
            int diff = Math.Min((ia - ib + 12) % 12, (ib - ia + 12) % 12);
            return diff switch
            {
                0 => 85,
                1 => 55,
                2 => 80,
                3 => 45,
                4 => 95,
                5 => 50,
                6 => 70,
                _ => 60
            };
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

            // Planet Pair Details for 5 key planets
            string[] keyPlanets = ["Günəş", "Ay", "Venera", "Mars", "Merkuri"];
            var details = new List<PlanetPairDetailDto>();

            foreach (var planet in keyPlanets)
            {
                string sA = signA;
                string sB = signB;
                if (planet == "Ay") { sA = SignsAz[(ia + 4) % 12]; sB = SignsAz[(ib + 4) % 12]; }
                if (planet == "Venera") { sA = SignsAz[(ia + 1) % 12]; sB = SignsAz[(ib + 1) % 12]; }
                if (planet == "Mars") { sA = SignsAz[(ia + 2) % 12]; sB = SignsAz[(ib + 2) % 12]; }
                if (planet == "Merkuri") { sA = SignsAz[(ia + 11) % 12]; sB = SignsAz[(ib + 11) % 12]; }

                Elements.TryGetValue(sA, out var elA);
                Elements.TryGetValue(sB, out var elB);

                details.Add(new PlanetPairDetailDto(
                    planet,
                    BodySymbols.TryGetValue(planet, out var sym) ? sym : "•",
                    sA,
                    sB,
                    elA ?? "—",
                    elB ?? "—",
                    AspectNameAz(sA, sB),
                    PairScore(sA, sB)
                ));
            }

            return new SynastryResponse(score.Overall, score.Love, score.Friendship, score.Communication, notes, details);
        }

        /// <summary>
        /// Tam natal xəritəyə əsaslanan uyğunluq (sinastriya) hesablaması. <see cref="ComputeSynastry"/>-dən
        /// fərqli olaraq təxmini "bürc fərqi" düsturu ilə kifayətlənmir — hər iki şəxsin faktiki hesablanmış
        /// Günəş, Ay, Venera, Mars, Merkuri və Ascendant mövqelərini birbaşa müqayisə edir. Çəkilər və qeyd
        /// mətnləri frontend-dəki `computeSynastry`/`synastryDetails` (src/lib/astrology.ts) ilə eynidir.
        /// </summary>
        public static SynastryResponse ComputeSynastryFromCharts(NatalChartResponse a, NatalChartResponse b)
        {
            static string GetSign(NatalChartResponse chart, string name) =>
                chart.Planets.FirstOrDefault(p => p.Name == name)?.Sign ?? "—";

            string sunA = GetSign(a, "Günəş"), sunB = GetSign(b, "Günəş");
            string moonA = GetSign(a, "Ay"), moonB = GetSign(b, "Ay");
            string venusA = GetSign(a, "Venera"), venusB = GetSign(b, "Venera");
            string marsA = GetSign(a, "Mars"), marsB = GetSign(b, "Mars");
            string merA = GetSign(a, "Merkuri"), merB = GetSign(b, "Merkuri");

            int sun = PairScore(sunA, sunB);
            int moon = PairScore(moonA, moonB);
            int venus = PairScore(venusA, venusB);
            int mars = PairScore(marsA, marsB);
            int mercury = PairScore(merA, merB);
            int asc = PairScore(a.Ascendant.Sign, b.Ascendant.Sign);

            int love = (int)Math.Round(venus * 0.45 + moon * 0.35 + mars * 0.2);
            int friendship = (int)Math.Round(sun * 0.4 + asc * 0.3 + mercury * 0.3);
            int communication = (int)Math.Round(mercury * 0.6 + sun * 0.2 + asc * 0.2);
            int overall = (int)Math.Round((love + friendship + communication) / 3.0);

            var notes = new List<string>();
            Elements.TryGetValue(sunA, out var ea);
            Elements.TryGetValue(sunB, out var eb);
            if (ea != null && eb != null)
            {
                notes.Add(ea == eb
                    ? $"Hər iki Günəş {ea} elementindədir — təbii anlaşma və oxşar ritm."
                    : $"Günəş elementləri fərqlidir ({ea} və {eb}) — bir-birinizi tamamlaya bilərsiniz.");
            }
            notes.Add(moon >= 75
                ? "Ay bağlantınız güclüdür: emosional təhlükəsizlik hissi yüksəkdir."
                : "Ay bağlantısı gərginlik yarada bilər: hisslərinizi açıq danışın.");
            notes.Add(venus >= 75
                ? "Venera harmoniyası romantikanı və estetik zövqləri birləşdirir."
                : "Venera fərqi sevgi dilinizin fərqli olduğunu göstərir.");
            notes.Add(mercury >= 70
                ? "Merkuri uyğunluğu ünsiyyəti asanlaşdırır."
                : "Merkuri gərginliyi anlaşılmazlıq riski yaradır — səbirli olun.");

            var signPairs = new Dictionary<string, (string A, string B)>
            {
                ["Günəş"] = (sunA, sunB),
                ["Ay"] = (moonA, moonB),
                ["Venera"] = (venusA, venusB),
                ["Mars"] = (marsA, marsB),
                ["Merkuri"] = (merA, merB),
            };

            var details = new List<PlanetPairDetailDto>();
            foreach (var planet in new[] { "Günəş", "Ay", "Venera", "Mars", "Merkuri" })
            {
                var (sA, sB) = signPairs[planet];
                Elements.TryGetValue(sA, out var elA);
                Elements.TryGetValue(sB, out var elB);
                details.Add(new PlanetPairDetailDto(
                    planet,
                    BodySymbols.TryGetValue(planet, out var sym) ? sym : "•",
                    sA,
                    sB,
                    elA ?? "—",
                    elB ?? "—",
                    AspectNameAz(sA, sB),
                    PairScore(sA, sB)
                ));
            }

            return new SynastryResponse(overall, love, friendship, communication, notes, details);
        }
    }


}
