using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.Astrology
{
    public static class PanchangEngine
    {
        public static readonly string[] TithiNamesAz =
        [
            "Pratipada", "Dvitiya", "Tritiya", "Chaturthi", "Panchami",
        "Shashthi", "Saptami", "Ashtami", "Navami", "Dashami",
        "Ekadashi", "Dvadashi", "Trayodashi", "Chaturdashi", "Purnima/Amavasya"
        ];

        public static readonly (string Name, string Quality)[] NakshatrasAz =
        [
            ("Aşvini", "Sürətli"), ("Bharani", "Kəskin"), ("Krittika", "Kəskin"),
        ("Rohini", "Sabit"), ("Mriqaşira", "Yumşaq"), ("Ardra", "Kəskin"),
        ("Punarvasu", "Hərəkətli"), ("Puşya", "Sürətli"), ("Aşleşa", "Kəskin"),
        ("Maqha", "Kəskin"), ("Purva Falquni", "Kəskin"), ("Uttara Falquni", "Sabit"),
        ("Hasta", "Sürətli"), ("Çitra", "Yumşaq"), ("Svati", "Hərəkətli"),
        ("Vişaxa", "Kəskin"), ("Anuradha", "Yumşaq"), ("Cyeştha", "Kəskin"),
        ("Mula", "Kəskin"), ("Purva Aşadha", "Kəskin"), ("Uttara Aşadha", "Sabit"),
        ("Şravana", "Hərəkətli"), ("Dhanişta", "Hərəkətli"), ("Şatabhişa", "Hərəkətli"),
        ("Purva Bhadrapada", "Kəskin"), ("Uttara Bhadrapada", "Sabit"), ("Revati", "Yumşaq")
        ];

        public static readonly string[] YogaNamesAz =
        [
            "Vişkambha", "Priti", "Ayuşman", "Saubhaqya", "Şobhana", "Atiqanda", "Sukarma", "Dhrti",
        "Şula", "Qanda", "Vrddhi", "Dhruva", "Vyaqhata", "Harşana", "Vajra", "Siddhi",
        "Vyatipata", "Variyan", "Parigha", "Şiva", "Siddha", "Sadhya", "Şubha", "Şukla",
        "Brahma", "Indra", "Vaidhrti"
        ];

        public static readonly string[] KaranaNamesAz =
        [
            "Bava", "Balava", "Kaulava", "Taitila", "Garija", "Vanija", "Vişti"
        ];

        public static readonly string[] DayColorsAz =
        [
            "Qırmızı", "Ağ", "Al-qırmızı", "Yaşıl", "Sarı", "Açıq mavi", "Tünd göy"
        ];

        private static readonly string[] TithiTypes = ["Nanda", "Bhadra", "Jaya", "Rikta", "Purna"];

        public static PanchangResponse Compute(DateTime date)
        {
            int dayOfYear = date.DayOfYear;
            int tithiIndex = ((dayOfYear * 3) % 30) + 1;
            string paksha = tithiIndex <= 15 ? "Şukla" : "Krişna";
            int tithiInHalf = ((tithiIndex - 1) % 15) + 1;
            string tithiName = TithiNamesAz[tithiInHalf - 1];
            string tithiType = TithiTypes[(tithiInHalf - 1) % 5];

            int nakIndex = (dayOfYear * 7) % 27;
            var nak = NakshatrasAz[nakIndex];

            int yogaIndex = (dayOfYear * 5) % 27;
            string yogaName = YogaNamesAz[yogaIndex];

            int karanaIndex = (dayOfYear * 2) % 7;
            string karanaName = KaranaNamesAz[karanaIndex];

            int weekday = (int)date.DayOfWeek;
            string dayColor = DayColorsAz[weekday];

            var guidance = new List<GuidanceItemDto>
        {
            new("Diş və Sağlamlıq",
                tithiType == "Rikta" ? "əlverişli" : "neytral",
                $"{tithiName} tithisi və {nak.Name} nakşatrasının ({nak.Quality}) enerjisi sağlamlıq və profilaktik addımlar üçün münasibdir."),

            new("Əmlak və Tikinti",
                nak.Quality == "Sabit" ? "əlverişli" : (nak.Quality == "Hərəkətli" ? "ehtiyatlı ol" : "neytral"),
                $"{nak.Name} nakşatrasının {nak.Quality.ToLower()} təbiəti uzunmüddətli əmlak və təməl qərarlarına təsir edir."),

            new("Sənəd və Rəsmi işlər",
                (tithiType == "Purna" || tithiType == "Jaya") ? "əlverişli" : "neytral",
                $"{tithiName} tithisi sənədləşmə, rəsmi müraciətlər və anlaşmalar üçün əlverişli tamamlanma təsirinə malikdir."),

            new("Ev işləri və Rahatlıq",
                nak.Quality == "Yumşaq" ? "əlverişli" : "neytral",
                $"{nak.Name} nakşatrasının mülayim təsiri ev nizamı və harmonik mühit üçün əlverişlidir.")
        };

            return new PanchangResponse(
                date.ToString("yyyy-MM-dd"),
                tithiIndex,
                tithiName,
                paksha,
                tithiType,
                nak.Name,
                nak.Quality,
                yogaName,
                karanaName,
                weekday,
                dayColor,
                guidance
            );
        }
    }
}
