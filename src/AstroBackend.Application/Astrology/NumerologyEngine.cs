using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.Astrology
{
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

        public static readonly Dictionary<int, (string Title, string Text)> Meanings = new()
    {
        { 1, ("Lider", "Müstəqillik, təşəbbüskarlıq və yeni başlanğıclar. Öndə getmək, öz yolunu yaratmaq bacarığı.") },
        { 2, ("Diplomat", "Əməkdaşlıq, həssaslıq və tarazlıq. Münasibətlərdə körpü qurmaq, səbrlə dinləmək.") },
        { 3, ("Yaradıcı", "İfadə, ünsiyyət və optimizm. Sənət, söz və özünü göstərmək enerjisi.") },
        { 4, ("Qurucu", "Nizam, sabitlik və zəhmətkeşlik. Möhkəm təməl qurmaq, ardıcıl addımlar.") },
        { 5, ("Sərgərdan", "Azadlıq, dəyişkənlik və macəra. Yenilik axtarışı, çevik düşüncə.") },
        { 6, ("Qoruyucu", "Məsuliyyət, qayğı və ailə dəyərləri. Harmoniya yaratmaq, xidmət etmək.") },
        { 7, ("Axtarıcı", "Dərinlik, təhlil və mənəvi axtarış. Tənhalıqda güc tapmaq, həqiqəti araşdırmaq.") },
        { 8, ("Təşkilatçı", "Güc, maddi uğur və idarəetmə. Böyük layihələri həyata keçirmək bacarığı.") },
        { 9, ("Humanist", "Şəfqət, geniş baxış və tamamlanma. Başqalarına xidmət, universal sevgi.") },
        { 11, ("İntuitiv usta (Master)", "Yüksək intuisiya və ilham mənbəyi. Mənəvi rəhbərlik potensialı — daxili tarazlıq tələb edir.") },
        { 22, ("Böyük qurucu (Master)", "Böyük ideyaları reallığa çevirmək gücü. Vizyonu konkret və möhtəşəm nəticəyə çatdırmaq.") },
        { 33, ("Mənəvi müəllim (Master)", "Qeydsiz-şərtsiz qayğı və fədakarlıq. Başqalarını ruhən yüksəltmək missiyası.") }
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

        public static NumerologyResponse Compute(string fullName, string birthDateStr)
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

            (string Title, string Text) GetM(int num) => Meanings.TryGetValue(num, out var m) ? m : ("Enerji", "Xüsusi numeroloji vibrasiya.");

            var details = new List<NumerologyItemDto>
        {
            new(lifePath, "Həyat yolu (Life Path)", GetM(lifePath).Title, GetM(lifePath).Text),
            new(destiny, "Tale / İfadə ədədi (Destiny)", GetM(destiny).Title, GetM(destiny).Text),
            new(soulUrge, "Qəlb arzusu (Soul Urge)", GetM(soulUrge).Title, GetM(soulUrge).Text),
            new(personality, "Xarakter / Görünüş (Personality)", GetM(personality).Title, GetM(personality).Text),
            new(birthday, "Doğum günü ədədi", GetM(birthday).Title, GetM(birthday).Text)
        };

            return new NumerologyResponse(lifePath, destiny, soulUrge, personality, birthday, details);
        }
    }
}
