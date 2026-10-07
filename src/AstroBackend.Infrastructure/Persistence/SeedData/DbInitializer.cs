using AstroBackend.Application.Interfaces.Security;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AstroBackend.Infrastructure.Persistence.SeedData
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            // 1. Apply pending EF Core migrations (əvvəllər EnsureCreatedAsync() istifadə olunurdu,
            // bu da __EFMigrationsHistory-ni doldurmur və repodakı mövcud migration-larla
            // (eyni vaxtda `dotnet ef database update` ilə) toqquşurdu).
            await context.Database.MigrateAsync();

            // 2. Seed Super Admin User
            var superAdminEmail = "shirinma@code.edu.az";
            var superAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = new User
                {
                    Email = superAdminEmail,
                    FullName = "Super Admin (Şirin)",
                    PasswordHash = passwordHasher.HashPassword("SuperAdmin123!"),
                    Role = AppRole.SuperAdmin,
                    IsActive = true
                };
                await context.Users.AddAsync(superAdmin);
                await context.Profiles.AddAsync(new Profile
                {
                    UserId = superAdmin.Id,
                    FullName = "Super Admin (Şirin)",
                    Bio = "Virgo Astrology platformasının Baş Super Administratoru."
                });
                await context.SaveChangesAsync();
            }

            // 3. Seed Admin User
            var adminEmail = "shirin.elizade127@gmail.com";
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    Email = adminEmail,
                    FullName = "Admin (Şirin Əlizadə)",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    Role = AppRole.Admin,
                    IsActive = true
                };
                await context.Users.AddAsync(admin);
                await context.Profiles.AddAsync(new Profile
                {
                    UserId = admin.Id,
                    FullName = "Admin (Şirin Əlizadə)",
                    Bio = "Platforma Administratoru."
                });
                await context.SaveChangesAsync();
            }

            // 4. Seed Astrologers
            if (!await context.Astrologers.AnyAsync())
            {
                var astrologers = new List<Astrologer>
            {
                new()
                {
                    DisplayName = "Leyla Kərimova",
                    Title = "Natal xəritə mütəxəssisi",
                    Bio = "15 illik təcrübə ilə natal xəritə və həyat yolu təhlili.",
                    SpecialtiesJson = JsonSerializer.Serialize(new[] { "Natal xəritə", "Karyera" }),
                    LanguagesJson = JsonSerializer.Serialize(new[] { "Azərbaycan", "Türk" }),
                    PriceAzn = 80,
                    Rating = 4.9m,
                    Verified = true
                },
                new()
                {
                    DisplayName = "Rəşad Hüseynov",
                    Title = "Sinastriya astroloqu",
                    Bio = "Münasibət uyğunluğu və sinastriya təhlilləri.",
                    SpecialtiesJson = JsonSerializer.Serialize(new[] { "Sinastriya", "Sevgi" }),
                    LanguagesJson = JsonSerializer.Serialize(new[] { "Azərbaycan", "İngilis" }),
                    PriceAzn = 65,
                    Rating = 4.8m,
                    Verified = true
                },
                new()
                {
                    DisplayName = "Nigar Əliyeva",
                    Title = "Tranzit və proqnoz",
                    Bio = "Tranzitlər əsasında illik proqnozlar hazırlayır.",
                    SpecialtiesJson = JsonSerializer.Serialize(new[] { "Tranzit", "Proqnoz" }),
                    LanguagesJson = JsonSerializer.Serialize(new[] { "Azərbaycan", "Rus" }),
                    PriceAzn = 70,
                    Rating = 4.7m,
                    Verified = true
                },
                 new()
                {
                    DisplayName = "Ləman Əliyeva",
                    Title = "Reiki Ustadi",
                    Bio = "Reiki qəbulu və tədbirləri ilə məşğul olur.",
                    SpecialtiesJson = JsonSerializer.Serialize(new[] { "Reiki", "Tədbirlər" }),
                    LanguagesJson = JsonSerializer.Serialize(new[] { "Azərbaycan", "İngilis", "Rus", "Türk" }),
                    PriceAzn = 200,
                    Rating = 5.0m,
                    Verified = true
                }
            };

                await context.Astrologers.AddRangeAsync(astrologers);
                await context.SaveChangesAsync();
            }

            // 5. Seed Horoscopes for 12 signs
            if (!await context.Horoscopes.AnyAsync())
            {
                string[] signs = ["Qoç", "Buğa", "Əkizlər", "Xərçəng", "Aslan", "Qız", "Tərəzi", "Əqrəb", "Oxatan", "Oğlaq", "Dolça", "Balıqlar"];
                var horoscopes = new List<Horoscope>();
                var today = DateTime.UtcNow.Date;

                foreach (var sign in signs)
                {
                    horoscopes.Add(new Horoscope
                    {
                        Sign = sign,
                        Period = HoroscopePeriod.Daily,
                        PeriodStart = today,
                        Content = $"Bu gün {sign} bürcü üçün səmavi axın olduqca güclüdür. Daxili intuisiyanıza güvənin, qərarlarınızı səhər saatlarında verin və axşam saatlarını istirahətə ayırın.",
                        Love = 80,
                        Career = 75,
                        Finance = 70
                    });

                    horoscopes.Add(new Horoscope
                    {
                        Sign = sign,
                        Period = HoroscopePeriod.Weekly,
                        PeriodStart = today,
                        Content = $"Bu həftə {sign} bürcü üçün yeni fürsətlər və şəxsi inkişaf vəd edir. Əlaqələrdə səmimiyyət və iş həyatında təşəbbüskarlıq sizə uğur qazandıracaq.",
                        Love = 85,
                        Career = 80,
                        Finance = 78
                    });

                    horoscopes.Add(new Horoscope
                    {
                        Sign = sign,
                        Period = HoroscopePeriod.Monthly,
                        PeriodStart = today,
                        Content = $"Bu ay {sign} bürcü nümayəndələri üçün mühüm maliyyə və karyera qərarları ayı olacaq. Planetlərin ahəngdar düzülüşü məqsədlərinizə çatmağa kömək edir.",
                        Love = 88,
                        Career = 85,
                        Finance = 82
                    });
                }

                await context.Horoscopes.AddRangeAsync(horoscopes);
                await context.SaveChangesAsync();
            }

            // 6. Seed Articles (14 rich articles from frontend migration)
            if (!await context.Articles.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var articles = new List<Article>
            {
                new()
                {
                    Title = "Günəş bürcün əslində nə deməkdir?",
                    Slug = "gunes-burcu-ne-demekdir",
                    Excerpt = "Ulduz falının ən çox bilinən hissəsi, əslində xəritənin yalnız bir təbəqəsidir.",
                    Body = "Kimsə səndən bürcünü soruşanda, əksər hallarda Günəş bürcündən danışılır. Bu, doğulduğun anda Günəşin hansı zodiak işarəsində olduğunu göstərir və şəxsiyyətinin nüvəsini, iradəni və həyatda nəyi ifadə etməyə çalışdığını əks etdirir.\n\nLakin Günəş bürcü tək başına tam mənzərəni vermir. Ay sənin daxili dünyanı, Yüksələn isə başqalarının səni necə gördüyünü göstərir. Yalnız Günəşə baxaraq özünü tanımaq, kitabın yalnız birinci fəslini oxumağa bənzəyir.\n\nBuna baxmayaraq, Günəş bürcü güclü bir başlanğıc nöqtəsidir. O, sənin əsas enerjini, liderlik tərzini və nəyin səni canlandırdığını göstərir — və natal xəritənin qalan hissəsini anlamaq üçün ən yaxşı yol məhz oradan başlamaqdır.",
                    Tag = "Günəş",
                    Published = true,
                    PublishedAt = now.AddDays(-58),
                    Views = 812
                },
                new()
                {
                    Title = "Ayın fazaları və emosional dövrələr",
                    Slug = "ayin-fazalari-emosional-dovrler",
                    Excerpt = "Hər ay fazası fərqli bir daxili ritmə uyğun gəlir.",
                    Body = "Ay təxminən 29.5 gündə Yer ətrafında tam dövr edir və bu müddətdə səkkiz əsas fazadan keçir. Hər faza fərqli bir emosional və enerji ritminə uyğun gəlir.\n\nYeni ay niyyət qoymaq, toxum əkmək üçün ən əlverişli andır. Artan ay mərhələsində hərəkətə keçmək, planları həyata keçirmək asanlaşır. Dolunay isə kulminasiya, aydınlıq və bəzən emosional intensivlik gətirir.\n\nAzalan ay isə buraxmaq, sadələşdirmək və dincəlmək üçün dəvətdir. Öz emosional ritmini ay fazaları ilə izləmək, daxili dəyişkənliyi anlamaq üçün sadə, lakin güclü bir vasitədir.",
                    Tag = "Ay",
                    Published = true,
                    PublishedAt = now.AddDays(-51),
                    Views = 634
                },
                new()
                {
                    Title = "Venera retroqradında sevgi həyatına nə olur?",
                    Slug = "venera-retroqrad-sevgi",
                    Excerpt = "Venera geri hərəkət edəndə keçmiş münasibətlər yenidən üzə çıxa bilər.",
                    Body = "Venera təxminən 18 ayda bir dəfə, 6 həftəyə yaxın müddətdə retroqrad görünür. Bu dövrdə sevgi, münasibətlər, pul və dəyərlər mövzuları xüsusi diqqət tələb edir.\n\nKöhnə tanışlar, keçmiş partnyorlar və ya bitməmiş emosional məsələlər bu dövrdə yenidən gündəmə gələ bilər. Astroloqlar bu müddətdə yeni münasibətə başlamağı və ya vacib maliyyə qərarları verməyi tövsiyə etmirlər — çünki sonradan fikir dəyişə bilərsən.\n\nƏvəzində Venera retroqradı öz dəyərlərini, özünə sevgini və münasibətlərində nəyə həqiqətən ehtiyacın olduğunu yenidən nəzərdən keçirmək üçün əla fürsətdir.",
                    Tag = "Tranzit",
                    Published = true,
                    PublishedAt = now.AddDays(-45),
                    Views = 1204
                },
                new()
                {
                    Title = "Yüksələn bürc: ilk təəssüratının sirri",
                    Slug = "yukselen-burc-ilk-teessurat",
                    Excerpt = "Yüksələn bürc başqalarının səni ilk anda necə gördüyünü göstərir.",
                    Body = "Yüksələn bürc (Asendent) doğum anında şərq üfüqündə qalxan zodiak işarəsidir. O, dəqiq doğum saatı olmadan hesablana bilməz — buna görə də natal xəritədə ən çox səhv edilən nöqtələrdən biridir.\n\nYüksələn bürc sənin xarici görünüşünü, ilk təəssüratını və dünyaya yanaşma tərzini formalaşdırır. Məsələn, Aslan yüksələni olan insan hətta sakit Balıqlar Günəşi ilə belə özündən əmin görünə bilər.\n\nBu bürc həm də natal xəritənin 1-ci evinin başlanğıcını təyin edir və bütün ev sistemini formalaşdırır — buna görə dəqiq doğum saatı bilmək, xəritəni düzgün oxumaq üçün vacibdir.",
                    Tag = "Xəritə",
                    Published = true,
                    PublishedAt = now.AddDays(-40),
                    Views = 947
                },
                new()
                {
                    Title = "12 bürcün element və keyfiyyət qruplaşması",
                    Slug = "burclerin-element-keyfiyyeti",
                    Excerpt = "Hər bürc bir elementə və bir keyfiyyətə aiddir — bu, xarakterin açarıdır.",
                    Body = "12 zodiak bürcü dörd elementə bölünür: Od (Qoç, Aslan, Oxatan), Torpaq (Buğa, Qız, Oğlaq), Hava (Əkizlər, Tərəzi, Dolça) və Su (Xərçəng, Əqrəb, Balıqlar). Element sənin əsas enerji tipini göstərir — Od hərəkətli, Torpaq praktik, Hava intellektual, Su isə emosionaldır.\n\nEyni zamanda hər bürc üç keyfiyyətdən birinə aiddir: Sabit-başlanğıc (Qoç, Xərçəng, Tərəzi, Oğlaq), Sabit (Buğa, Aslan, Əqrəb, Dolça) və Dəyişkən (Əkizlər, Qız, Oxatan, Balıqlar).\n\nBu iki təsnifatı birləşdirəndə hər bürcün unikal xarakteri aydınlaşır — məsələn Aslan həm Od, həm də Sabitdir, buna görə də ehtiraslı, lakin sabit liderlik enerjisi daşıyır.",
                    Tag = "Bürclər",
                    Published = true,
                    PublishedAt = now.AddDays(-34),
                    Views = 1560
                },
                new()
                {
                    Title = "Sinastriya nədir və necə oxunur?",
                    Slug = "sinastriya-nedir",
                    Excerpt = "İki natal xəritənin üst-üstə qoyulması münasibətin dinamikasını göstərir.",
                    Body = "Sinastriya iki insanın natal xəritələrini müqayisə edərək aralarındakı astroloji dinamikanı öyrənən üsuldur. Hər iki xəritədəki planetlərin bir-birinə formalaşdırdığı bucaqlar (aspektlər) münasibətin güclü və çətin tərəflərini göstərir.\n\nMəsələn, bir tərəfin Venerasının digərinin Marsı ilə harmonik aspekti cazibə və ehtirası artıra bilər, Ay-Ay kvadratı isə emosional anlaşılmazlıqlara işarə edə bilər.\n\nSinastriya təkcə \"uyğunuq, ya yox\" sualına cavab vermir — o, münasibətdə hansı sahələrin işlənməli olduğunu göstərən bir xəritədir. Heç bir kombinasiya mükəmməl deyil, hər biri öz dərsi ilə gəlir.",
                    Tag = "Sinastriya",
                    Published = true,
                    PublishedAt = now.AddDays(-29),
                    Views = 2103
                },
                new()
                {
                    Title = "Saturn qayıdışı: 29 yaş böhranının astrologiyası",
                    Slug = "saturn-qayidisi-29-yas",
                    Excerpt = "Saturn təqribən 29 ildən bir doğum mövqeyinə qayıdır və həyatı yenidən qurur.",
                    Body = "Saturn Günəş ətrafında tam dövrünü təxminən 29.5 ildə tamamlayır. Bu o deməkdir ki, hər insan 27-30 yaşları arasında \"Saturn qayıdışı\" adlanan dövrü yaşayır — Saturn doğum anındakı mövqeyinə geri qayıdır.\n\nBu dövr çox zaman böyük yaşam dəyişiklikləri ilə müşayiət olunur: karyera dəyişikliyi, münasibətlərin ciddiləşməsi və ya bitməsi, məsuliyyətin artması. Saturn struktur və nizam planetidir — bu qayıdış səni \"böyüməyə\" məcbur edir.\n\nÇətin görünsə də, Saturn qayıdışı əslində özünü daha möhkəm təməllər üzərində qurmaq üçün bir dəvətdir. İkinci qayıdış 58-60 yaşlarında baş verir və oxşar, lakin daha müdrik bir mərhələ gətirir.",
                    Tag = "Tranzit",
                    Published = true,
                    PublishedAt = now.AddDays(-23),
                    Views = 1789
                },
                new()
                {
                    Title = "Natal Ay bürcün emosional ehtiyaclarını necə göstərir?",
                    Slug = "natal-ay-burcun-emosional-ehtiyaclar",
                    Excerpt = "Ay bürcün, təhlükəsizlik hissi üçün nəyə ehtiyacın olduğunu açıqlayır.",
                    Body = "Natal xəritədə Ay, daxili dünyanı, instinktiv reaksiyaları və emosional ehtiyacları göstərir. Günəş kim olmaq istədiyimizi, Ay isə özümüzü təhlükəsiz hiss etmək üçün nəyə ehtiyacımız olduğunu göstərir.\n\nMəsələn, Xərçəng Ayı olan insan üçün ev və ailə təhlükəsizlik mənbəyidir, Dolça Ayı olan üçün isə azadlıq və intellektual əlaqə vacibdir. Ay bürcünü tanımaq, öz emosional tetiklərini və rahatlama üsullarını anlamağa kömək edir.\n\nMünasibətlərdə partnyorunun Ay bürcünü bilmək də faydalıdır — bu, onun stress anında nəyə ehtiyac duyduğunu anlamağın açarı ola bilər.",
                    Tag = "Ay",
                    Published = true,
                    PublishedAt = now.AddDays(-18),
                    Views = 876
                },
                new()
                {
                    Title = "Şimal Node və həyat missiyası",
                    Slug = "shimal-node-heyat-missiyasi",
                    Excerpt = "Node oxu, keçmiş vərdişlərdən böyümə istiqamətinə doğru yolu göstərir.",
                    Body = "Ay Nodeları — Şimal və Cənub Node — real planetlər deyil, Ayın orbitinin ekliptika ilə kəsişdiyi riyazi nöqtələrdir. Astrologiyada onlar həyat yolunu və ruhani böyüməni simvolizə edir.\n\nCənub Node rahat, tanış olan, \"artıq bilinən\" keyfiyyətləri göstərir — bəzən keçmiş vərdişlər kimi başa düşülür. Şimal Node isə bizi çağıran, çətin gələn, amma böyümə gətirən istiqamətdir.\n\nMəsələn, Əkizlər Cənub Node — Oxatan Şimal Node olan insan üçün detallardan çıxıb daha geniş mənaya, fəlsəfəyə doğru hərəkət etmək inkişaf yolu ola bilər. Node oxu tez-tez \"bu həyatda nə öyrənməliyəm\" sualına cavab axtaranlar üçün maraqlıdır.",
                    Tag = "Xəritə",
                    Published = true,
                    PublishedAt = now.AddDays(-14),
                    Views = 592
                },
                new()
                {
                    Title = "Astrokartoqrafiya: harada yaşamaq sənə uyğundur?",
                    Slug = "astrokartografiya-harada-yasamaq",
                    Excerpt = "Natal xəritən dünya xəritəsi üzərinə köçürüləndə fərqli yerlərin təsiri üzə çıxır.",
                    Body = "Astrokartoqrafiya natal xəritədəki planet xətlərini dünya xəritəsi üzərinə köçürən bir texnikadır. Fikir sadədir: planetlərin təsiri coğrafi məkandan asılı olaraq dəyişir.\n\nMəsələn, Veneranın xəttinin keçdiyi şəhərdə yaşamaq və ya səyahət etmək sevgi, gözəllik və harmoniya ilə bağlı təcrübələri gücləndirə bilər, Marsın xətti isə enerji və rəqabəti artıra bilər, bəzən də gərginlik gətirə bilər.\n\nBu üsul köçmək, iş üçün şəhər seçmək və ya sadəcə müəyyən yerlərdə niyə fərqli hiss etdiyini anlamaq istəyənlər üçün maraqlı bir alətdir — minlərlə insanın təcrübəsi ilə formalaşıb.",
                    Tag = "Praktika",
                    Published = true,
                    PublishedAt = now.AddDays(-11),
                    Views = 445
                },
                new()
                {
                    Title = "Merkurinin bürcü düşüncə tərzini necə formalaşdırır?",
                    Slug = "merkurinin-burcu-dushunce-tarzi",
                    Excerpt = "Merkuri necə düşündüyünü və ünsiyyət qurduğunu göstərir.",
                    Body = "Merkuri Günəşdən heç vaxt çox uzaqlaşmadığı üçün onun bürcü adətən Günəş bürcünə yaxın (eyni və ya qonşu bürclərdə) olur. Merkuri düşüncə tərzini, öyrənmə üslubunu və ünsiyyət tərzini idarə edir.\n\nOd bürclərində Merkuri sürətli və birbaşa düşünür, Torpaq bürclərində praktik və detallı, Hava bürclərində analitik və sosial, Su bürclərində isə intuitiv və emosional əsaslı düşünür.\n\nÖz Merkuri bürcünü bilmək, həm öz düşüncə tərzini qəbul etmək, həm də başqaları ilə daha effektiv ünsiyyət qurmaq üçün faydalıdır.",
                    Tag = "Ünsiyyət",
                    Published = true,
                    PublishedAt = now.AddDays(-8),
                    Views = 321
                },
                new()
                {
                    Title = "Marsın bürcü: enerjini haraya yönəldirsən?",
                    Slug = "marsin-burcu-enerji",
                    Excerpt = "Mars hərəkətə keçmə tərzini və nəyin səni motivasiya etdiyini göstərir.",
                    Body = "Mars istək, hərəkət və mübarizə planetidir. Onun bürcü sənin necə hərəkətə keçdiyini, nəyin səni qıcıqlandırdığını və enerjini necə ifadə etdiyini göstərir.\n\nQoç Marsı — təbii evində — birbaşa və impulsivdir, Oğlaq Marsı isə strateji və səbirlidir. Tərəzi Marsı münaqişədən çəkinə bilər, Əqrəb Marsı isə dərin və intensiv enerji daşıyır.\n\nMarsın çətin aspektləri enerjini ifadə etməkdə maneələr yarada bilər, harmonik aspektlər isə hərəkətə keçməyi asanlaşdırır. Öz Mars bürcünü tanımaq, motivasiya itirdiyin anlarda özünü daha yaxşı başa düşməyə kömək edir.",
                    Tag = "Enerji",
                    Published = true,
                    PublishedAt = now.AddDays(-5),
                    Views = 210
                },
                new()
                {
                    Title = "Cütlərin uyğunluğunda Veneranın rolu",
                    Slug = "cutlerin-uygunlugunda-venera",
                    Excerpt = "Venera cazibəni və münasibətdə nəyə dəyər verdiyini göstərir.",
                    Body = "Venera sevgi, gözəllik və dəyərlər planetidir. Sinastriyada iki insanın Venera mövqeləri arasındakı aspektlər, cazibənin təbiətini və münasibətdə nəyin vacib sayıldığını göstərir.\n\nVenera-Venera harmonik aspektləri adətən ortaq zövq və dəyərlərə işarə edir — rahat, təbii bir uyğunluq hissi yaradır. Venera-Mars aspektləri isə fiziki cazibəni və ehtirası gücləndirir.\n\nAmma çətin aspektlər də faydasız deyil — onlar münasibətə dərinlik və böyümə üçün fürsət gətirə bilər, sadəcə daha çox şüurlu səy tələb edir.",
                    Tag = "Sinastriya",
                    Published = true,
                    PublishedAt = now.AddDays(-3),
                    Views = 156
                },
                new()
                {
                    Title = "Astrologiya və meditasiya: gündəlik praktika üçün 5 addım",
                    Slug = "astrologiya-meditasiya-5-addim",
                    Excerpt = "Astroloji məlumatı gündəlik daxili işə çevirən sadə praktika.",
                    Body = "Astrologiya yalnız proqnozlaşdırma aləti deyil — o, həm də özünütanıma və daxili işə dəvətdir. Aşağıdakı sadə praktika astroloji məlumatı gündəlik həyata inteqrasiya etməyə kömək edə bilər:\n\n1. Səhər bugünkü Ay bürcünü yoxla və özündən soruş: bu enerji ilə necə hərəkət edə bilərəm?\n2. Öz Günəş, Ay və Yüksələn bürclərini xatırla və onların hər birinin bu gün necə özünü göstərdiyini müşahidə et.\n3. Beş dəqiqə sakit otur və nəfəsinə fokuslan.\n4. Cari tranzitlərdən biri haqqında düşün və həyatında necə əks olunduğunu qeyd et.\n5. Gün sonunda jurnal yazaraq müşahidələrini qeyd et — zamanla bu, öz daxili ritmini daha dərindən tanımana kömək edəcək.",
                    Tag = "Meditasiya",
                    Published = true,
                    PublishedAt = now.AddDays(-1),
                    Views = 89
                }
            };

                await context.Articles.AddRangeAsync(articles);
                await context.SaveChangesAsync();
            }

            // 7. Seed Forum Topics and Replies (18 topics & 22 replies)
            // DİQQƏT: ForumTopic.UserId və ForumReply.UserId DB-də REQUIRED (non-nullable) FK-dır
            // (AppDbContext: HasForeignKey(...).OnDelete(Restrict), IsRequired default-u true).
            // Əvvəlki versiyada bu sahə heç təyin olunmurdu (default Guid.Empty qalırdı), nəticədə
            // SaveChangesAsync() FK constraint pozuntusu ilə throw edirdi və bu try/catch-siz xəta
            // Program.cs-dəki ÜST SƏVİYYƏ catch-ə qədər yayılıb bütün SeedAsync-i ORTADA DAYANDIRIRDI —
            // səbəbindən bundan SONRAKI 8-ci bölmə (SubscriptionPlans) HEÇ VAXT işləmirdi, baxmayaraq
            // ki astroloqlar/horoskoplar/məqalələr (bu bölmədən ƏVVƏL olduğu üçün) normal seed olunurdu.
            // Fiks: bu demo forum yazılarını artıq mövcud olan superAdmin-in Id-sinə bağlayırıq.
            if (!await context.ForumTopics.AnyAsync())
            {
                var now = DateTime.UtcNow;

                var t1 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Aynur Məmmədova", Category = "ümumi", Title = "İlk dəfə buradayam, salam hamıya!", Body = "Salam! Bu yaxınlarda natal xəritəmi hesabladım və astrologiyaya maraq göstərməyə başladım. Bu forumda təcrübəli insanlar görürəm, ümid edirəm burda çox şey öyrənəcəm 🙂", CreatedAt = now.AddDays(-21) };
                var t2 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Elvin Qasımov", Category = "ümumi", Title = "Astrologiyaya necə başladınız?", Body = "Maraqlıdır, hamınız astrologiyaya necə maraq göstərməyə başlamısınız? Mənimki bir dostumun natal xəritəmi oxumasından sonra oldu, çox təəccübləndim doğruluğuna.", CreatedAt = now.AddDays(-19) };
                var t3 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Günay Səfərova", Category = "tranzitlər", Title = "Bu ay Merkuri retroqraddadır, kimin başına iş gəldi?", Body = "Mənim telefonum sındı, iş yerində sənəd itdi, bir də köhnə tanışım mesaj yazdı — klassik Merkuri retroqrad əlamətləri deyilmi? Sizdə necədir bu dövr?", CreatedAt = now.AddDays(-17) };
                var t4 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Tural Hüseynov", Category = "tranzitlər", Title = "Saturn Balıqlarda — kim hiss edir təsirini?", Body = "Saturn Balıqlar bürcünə keçəli hədsiz yorğunluq hiss edirəm, xüsusən yaradıcı işlərdə. Balıqlar/Qız yüksələni olanlar necə hiss edir özlərini?", CreatedAt = now.AddDays(-15) };
                var t5 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Nərmin Abbasova", Category = "tranzitlər", Title = "Yupiter keçidi karyeramda dəyişiklik gətirdi", Body = "Yupiter 10-cu evimə keçəndən sonra tamam gözlənilməz bir iş təklifi aldım. Sizdə də karyerada belə \"açılma\" hiss olub bu il?", CreatedAt = now.AddDays(-13) };
                var t6 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Kamran Əliyev", Category = "natal xəritə", Title = "Xəritəmdə 8-ci ev boşdur, bu normaldırmı?", Body = "Xəritəmi yoxlayanda gördüm ki 8-ci evdə heç bir planet yoxdur. Narahat olmalıyammı, yoxsa boş evlər tamam normaldır?", CreatedAt = now.AddDays(-12) };
                var t7 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Səbinə Rzayeva", Category = "natal xəritə", Title = "Yüksələnim Əkizlər amma özümü heç oxşatmıram", Body = "Yüksələn bürcüm Əkizlərdir amma özümü daha çox sakit, introvert insan kimi tanıyıram. Bu normal ola bilərmi, yoxsa doğum saatımda səhvlik var?", CreatedAt = now.AddDays(-10) };
                var t8 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Orxan Məmmədli", Category = "natal xəritə", Title = "Ay-Plüton kvadratı olan var? Necə idarə edirsiniz?", Body = "Natal xəritəmdə Ay-Plüton kvadratı var və çox intensiv emosiyalar yaşayıram bəzən. Bu aspekti olan varmı, necə balanslaşdırırsınız?", CreatedAt = now.AddDays(-9) };
                var t9 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Leyla Vəliyeva", Category = "natal xəritə", Title = "Xəritəmi necə oxumaq lazımdır, kömək edin", Body = "Xəritə bölməsindən hesabladım amma hər şey mənə çox mürəkkəb görünür. Haradan başlamaq lazımdır ki, öz xəritəmi başa düşüm?", CreatedAt = now.AddDays(-8) };
                var t10 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Aygün Nəbiyeva", Category = "cütlük xəritəsi", Title = "Partnyorumla Günəş-Ay kvadratımız var, çətindir", Body = "Uyğunluq bölməsində yoxladım, mənim Günəşimlə onun Ayı arasında kvadrat var. İlk vaxtlar çox cəlbedici idi amma indi tez-tez anlaşılmazlıq yaşayırıq. Belə təcrübəsi olan var?", CreatedAt = now.AddDays(-7) };
                var t11 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Rəşad Qurbanov", Category = "cütlük xəritəsi", Title = "Sinastriyada Venera-Mars trigonu nə deməkdir?", Body = "Sevgilimlə xəritələrimizi müqayisə etdim, Venera-Mars arasında trigon çıxdı. Bu praktikada özünü necə göstərir, kimin təcrübəsi var?", CreatedAt = now.AddDays(-6) };
                var t12 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Şəbnəm Hacıyeva", Category = "cütlük xəritəsi", Title = "Balıqlar və Oğlaq uyğunluğu təcrübəniz varmı?", Body = "Mən Balıqlar Günəşiyəm, sevgilim isə Oğlaq. Çox fərqli görünürük amma çox yaxşı tamamlayırıq bir-birimizi. Bu kombinasiyanı yaşayan var burada?", CreatedAt = now.AddDays(-5) };
                var t13 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Elnur Bağırov", Category = "sual-cavab", Title = "Doğum saatımı bilmirəm, nə etməliyəm?", Body = "Doğum şəhadətnaməmdə saat yazılmayıb, valideynlərim də dəqiq xatırlamır. Xəritəmi necə hesablaya bilərəm, yoxsa mümkün deyil?", CreatedAt = now.AddDays(-4) };
                var t14 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Nigar Cəfərova", Category = "sual-cavab", Title = "Sidereal və tropik zodiak fərqi nədir?", Body = "Bəzi saytlarda mənim bürcüm fərqli çıxır. Sonra öyrəndim ki sidereal və tropik sistem var. Bu saytda hansı sistem işlədilir, fərq nədir?", CreatedAt = now.AddDays(-3) };
                var t15 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Fərid Novruzov", Category = "sual-cavab", Title = "Şimal Node hansı evdədirsə nəyə işarədir?", Body = "Mənim Şimal Node-um 7-ci evdədir. Bu münasibətlərlə bağlı bir mesaj ola bilərmi? Fikirlərinizi bilmək istərdim.", CreatedAt = now.AddDays(-2) };
                var t16 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Aynur Məmmədova", Category = "ümumi", Title = "Bu forumda kim natal xəritəsini paylaşmaq istəyir?", Body = "Fikirləşdim bəlkə bir mövzu açaq, hərə öz Günəş-Ay-Yüksələn kombinasiyasını yazsın, maraqlı ola bilər müqayisə etmək 🙂 Mən başlayıram: Qoç-Əqrəb-Şir.", CreatedAt = now.AddDays(-2) };
                var t17 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Günay Səfərova", Category = "tranzitlər", Title = "Növbəti dolunay hansı bürcdə olacaq?", Body = "Bilən var bu ayın dolunayı hansı bürcdə baş verəcək? Günün bələdçisi bölməsinə baxdım amma dəqiq tarixi tapa bilmədim.", CreatedAt = now.AddDays(-1) };
                var t18 = new ForumTopic { Id = Guid.NewGuid(), UserId = superAdmin.Id, AuthorName = "Kamran Əliyev", Category = "sual-cavab", Title = "Uyğunluq balı aşağıdır, deməli uyğun deyilikmi?", Body = "Partnyorumla uyğunluq balımız 45% çıxdı. Bu o deməkdirmi ki bizə uyğun deyilik, yoxsa bal tək amil deyil?", CreatedAt = now.AddHours(-12) };

                var topics = new List<ForumTopic> { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16, t17, t18 };
                await context.ForumTopics.AddRangeAsync(topics);

                var replies = new List<ForumReply>
            {
                new() { TopicId = t3.Id, UserId = superAdmin.Id, AuthorName = "Tural Hüseynov", Body = "Mənim də noutbukum xarab oldu bu dövrdə 😅 Ən yaxşısı vacib sənədləri iki dəfə yoxlamaqdır bu müddətdə.", CreatedAt = now.AddDays(-16) },
                new() { TopicId = t3.Id, UserId = superAdmin.Id, AuthorName = "Nərmin Abbasova", Body = "Köhnə iş yoldaşım mesaj yazdı mənə də, tam Merkuri retroqrad effekti. Amma yaxşı söhbətləşdik, deməli hər şey pis olmur bu dövrdə.", CreatedAt = now.AddDays(-15) },
                new() { TopicId = t3.Id, UserId = superAdmin.Id, AuthorName = "Leyla Vəliyeva", Body = "Mən bu dövrdə yeni telefon almaqdan çəkindim, məsləhətlərə əsasən düzgün etmişəm deyəsən 🙂", CreatedAt = now.AddDays(-15) },
                new() { TopicId = t4.Id, UserId = superAdmin.Id, AuthorName = "Aygün Nəbiyeva", Body = "Mən Qız yüksələniyəm, doğrudan da son aylarda özümü çox yorğun hiss edirəm. Yuxu rejiminə daha çox fikir verməyə başladım.", CreatedAt = now.AddDays(-14) },
                new() { TopicId = t4.Id, UserId = superAdmin.Id, AuthorName = "Orxan Məmmədli", Body = "Bəlkə də bu, sadəcə daha çox istirahət etmək lazım olduğuna işarədir. Saturn həmişə yavaşlamağı öyrədir.", CreatedAt = now.AddDays(-14) },
                new() { TopicId = t6.Id, UserId = superAdmin.Id, AuthorName = "Rəşad Qurbanov", Body = "Tamamilə normaldır, boş evlər sadəcə o sahədə daha az \"hadisə\" olduğunu göstərir, problem demək deyil.", CreatedAt = now.AddDays(-11) },
                new() { TopicId = t6.Id, UserId = superAdmin.Id, AuthorName = "Nigar Cəfərova", Body = "Dəqiq, əksinə boş evlər bəzən daha sabit sahələr kimi də şərh olunur. Evin hökmdarına baxmaq daha vacibdir.", CreatedAt = now.AddDays(-11) },
                new() { TopicId = t7.Id, UserId = superAdmin.Id, AuthorName = "Fərid Novruzov", Body = "Yüksələn həmişə açıq-aşkar görünmür, xüsusən Günəş və Ay bürcün güclü fərqli enerjiyə sahibdirsə. Doğum saatı bir neçə dəqiqə səhv olsa belə bürc dəyişə bilər, dəqiqliyi yoxlamaq faydalı olar.", CreatedAt = now.AddDays(-9) },
                new() { TopicId = t7.Id, UserId = superAdmin.Id, AuthorName = "Şəbnəm Hacıyeva", Body = "Mənim də oxşar vəziyyətim var, amma yaxın dostlarım deyir ki əslində insanlarla tanış olanda Əkizlər enerjisi üzə çıxır, sadəcə tanımadığın insanlarla yox.", CreatedAt = now.AddDays(-9) },
                new() { TopicId = t8.Id, UserId = superAdmin.Id, AuthorName = "Elnur Bağırov", Body = "Mənim də var bu aspekt. Terapiya və jurnal yazmaq çox köməkçi oldu emosiyaları tanımaqda.", CreatedAt = now.AddDays(-8) },
                new() { TopicId = t9.Id, UserId = superAdmin.Id, AuthorName = "Aynur Məmmədova", Body = "Ən yaxşısı Günəş-Ay-Yüksələn üçlüyündən başlamaqdır, sonra planetlərin evlərinə baxarsan. Addım-addım gedəndə daha asan olur.", CreatedAt = now.AddDays(-7) },
                new() { TopicId = t9.Id, UserId = superAdmin.Id, AuthorName = "Elvin Qasımov", Body = "Mən də elə başlamışdım, indi aspektlərə baxıram. Vaxt aparır amma maraqlı prosesdir.", CreatedAt = now.AddDays(-7) },
                new() { TopicId = t10.Id, UserId = superAdmin.Id, AuthorName = "Şəbnəm Hacıyeva", Body = "Bizdə də oxşar aspekt var. Açıq ünsiyyət və bir-birinin emosional dilini öyrənmək çox kömək etdi.", CreatedAt = now.AddDays(-6) },
                new() { TopicId = t10.Id, UserId = superAdmin.Id, AuthorName = "Tural Hüseynov", Body = "Çətin aspektlər həm də ən çox böyümə gətirən aspektlərdir, o baxımdan pis şey deyil.", CreatedAt = now.AddDays(-6) },
                new() { TopicId = t11.Id, UserId = superAdmin.Id, AuthorName = "Aygün Nəbiyeva", Body = "Bizdə də var bu trigon, doğrudan da rahat və təbii bir cazibə yaradır, heç bir gərginlik hiss etmirik.", CreatedAt = now.AddDays(-5) },
                new() { TopicId = t13.Id, UserId = superAdmin.Id, AuthorName = "Nigar Cəfərova", Body = "Təxmini saatla da xəritə hesablaya bilərsən, sadəcə ev sərhədləri və Yüksələn dəqiq olmaya bilər. Günəş və Ay bürcün adətən düzgün çıxır.", CreatedAt = now.AddDays(-3) },
                new() { TopicId = t13.Id, UserId = superAdmin.Id, AuthorName = "Fərid Novruzov", Body = "Bəzi ölkələrdə doğum haqqında arayış xəstəxanadan alına bilir, saat da orda qeyd olunur. Yoxlamağa dəyər.", CreatedAt = now.AddDays(-3) },
                new() { TopicId = t14.Id, UserId = superAdmin.Id, AuthorName = "Kamran Əliyev", Body = "Bu sayt tropik zodiakdan istifadə edir, Qərb astrologiyasında ən çox yayılan sistemdir. Sidereal sistem isə Vedik astrologiyada işlədilir, fərq ayanamsa düzəlişindən qaynaqlanır.", CreatedAt = now.AddDays(-2) },
                new() { TopicId = t15.Id, UserId = superAdmin.Id, AuthorName = "Səbinə Rzayeva", Body = "7-ci evdə Şimal Node çox tez-tez münasibətlər vasitəsilə böyümə mənasında şərh olunur — bəlkə də tək başına deyil, başqaları ilə əlaqədə inkişaf etmək sənin yolundur.", CreatedAt = now.AddDays(-1) },
                new() { TopicId = t16.Id, UserId = superAdmin.Id, AuthorName = "Orxan Məmmədli", Body = "Maraqlı fikirdir! Mənim də: Xərçəng-Balıq-Əqrəb. Çox su enerjisi 🌊", CreatedAt = now.AddDays(-1) },
                new() { TopicId = t16.Id, UserId = superAdmin.Id, AuthorName = "Leyla Vəliyeva", Body = "Buğa-Oğlaq-Qız burda, tam torpaq insanıyam deyəsən 😄", CreatedAt = now.AddHours(-20) },
                new() { TopicId = t18.Id, UserId = superAdmin.Id, AuthorName = "Rəşad Qurbanov", Body = "Bal tək amil deyil, sadəcə bəzi sahələrdə daha çox səy lazım olduğunu göstərir. Real münasibətlərdə ünsiyyət balı üstələyir.", CreatedAt = now.AddHours(-10) }
            };

                await context.ForumReplies.AddRangeAsync(replies);
                await context.SaveChangesAsync();
            }
            // 8. Seed Subscription Plans (Standart/Premium — "Pulsuz" DB-də sətir kimi mövcud deyil)
            if (!await context.SubscriptionPlans.AnyAsync())
            {
                var plans = new List<SubscriptionPlan>
            {
                new()
                {
                    Key = "standart",
                    Name = "Standart",
                    Tagline = "Əsas astroloji vasitələrə tam giriş",
                    PriceAzn = 9,
                    Features = new List<string>
                    {
                        "Tam natal xəritə təkəri (planet, ev və aspekt təfərrüatları)",
                        "Uyğunluq (sinastriya) — planet-planet detallı təhlil",
                        "Gündəlik, həftəlik və aylıq horoskop",
                        "Numerologiya hesablamaları",
                        "Günün bələdçisi",
                        "AI Astroloq söhbəti — gündə 15 mesaj",
                        "Jurnal — limitsiz qeyd",
                        "Astroloqlarla rezervasiya"
                    },
                    AiMessagesPerDay = 15,
                    SynastryFullDetail = true,
                    BookingDiscountPct = 0,
                    SortOrder = 1,
                    IsActive = true
                },
                new()
                {
                    Key = "premium",
                    Name = "Premium",
                    Tagline = "Ən dərin təhlillər və limitsiz AI dəstəyi",
                    PriceAzn = 19,
                    Features = new List<string>
                    {
                        "Standart paketin bütün imkanları",
                        "AI Astroloq söhbəti — limitsiz mesaj",
                        "Astroloq rezervasiyalarında 15% endirim",
                        "Yeni məqalələrə prioritet giriş",
                        "Prioritet dəstək"
                    },
                    AiMessagesPerDay = null,
                    SynastryFullDetail = true,
                    BookingDiscountPct = 15,
                    SortOrder = 2,
                    IsActive = true
                }
            };

                await context.SubscriptionPlans.AddRangeAsync(plans);
                await context.SaveChangesAsync();
            }

            // 9. Seed Shop Products (Tarot/Kristal/Şam/Kitab — 8 məhsul, hamısının öz şəkli
            // wwwroot/uploads/shop/ qovluğunda saxlanılır; bu şəkillər repo ilə birlikdə
            // gəlmədiyi üçün (fərdi seçilmiş şəkillərdir) DB sıfırlandıqda ImageUrl sahələri
            // qalır, lakin faktiki fayllar yenidən əl ilə həmin qovluğa köçürülməlidir əgər
            // wwwroot da silinibsə. Qiymətlər və təsvirlər canlı admin API ilə əvvəlcə
            // yaradılmış sətirlərlə eynidir — DB sıfırlanarsa eyni vəziyyət bərpa olunsun deyə.
            if (!await context.ShopProducts.AnyAsync())
            {
                var products = new List<ShopProduct>
            {
                new()
                {
                    Category = ShopCategory.Tarot,
                    Slug = "klassik-tarot-kartlari",
                    Name = "Klassik Tarot Kartları (Rider-Waite)",
                    NameEn = "Classic Tarot Deck (Rider-Waite)",
                    Description = "78 kartdan ibarət klassik Rider-Waite tarot dəsti, Azərbaycan dilində izahat kitabçası ilə birlikdə. Başlayanlar üçün ideal seçimdir.",
                    PriceAzn = 35,
                    UnitLabel = "dəst",
                    ImageUrl = "/uploads/shop/tarot-rider-waite.jpg",
                    SortOrder = 1,
                    IsActive = true,
                    Stock = 50
                },
                new()
                {
                    Category = ShopCategory.Tarot,
                    Slug = "qara-ay-tarot-desti",
                    Name = "Qara Ay Tarot Dəsti",
                    NameEn = "Dark Moon Tarot Deck",
                    Description = "Qaranlıq və mistik illüstrasiyalarla hazırlanmış 78 kartlıq premium tarot dəsti. Dərin sezgi işi üçün nəzərdə tutulub.",
                    PriceAzn = 42,
                    UnitLabel = "dəst",
                    ImageUrl = "/uploads/shop/tarot-qara-ay.jpg",
                    SortOrder = 2,
                    IsActive = true,
                    Stock = 30
                },
                new()
                {
                    Category = ShopCategory.Kristal,
                    Slug = "ametist-kristal-das",
                    Name = "Ametist Kristal Daş",
                    NameEn = "Amethyst Crystal Stone",
                    Description = "Təbii ametist daşı — sakitlik, intuisiya və mənəvi qorunma üçün istifadə olunur. Hər daş fərdi formaya malikdir.",
                    PriceAzn = 18,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/kristal-ametist.jpg",
                    SortOrder = 1,
                    IsActive = true,
                    Stock = 80
                },
                new()
                {
                    Category = ShopCategory.Kristal,
                    Slug = "roza-kuars-urek-dasi",
                    Name = "Roza Kuars Ürək Daşı",
                    NameEn = "Rose Quartz Heart Stone",
                    Description = "Ürək formasında yonulmuş roza kuars — sevgi enerjisini, şəfqəti və emosional balansı gücləndirir.",
                    PriceAzn = 22,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/kristal-roza-kuars.jpg",
                    SortOrder = 2,
                    IsActive = true,
                    Stock = 60
                },
                new()
                {
                    Category = ShopCategory.Sham,
                    Slug = "lavanda-aromaterapiya-sami",
                    Name = "Lavanda Aromaterapiya Şamı",
                    NameEn = "Lavender Aromatherapy Candle",
                    Description = "Təbii soya mumundan hazırlanmış, lavanda ətirli rahatlaşdırıcı şam. Meditasiya və ritual seansları üçün idealdır. Yanma müddəti ~30 saat.",
                    PriceAzn = 15,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/sham-lavanda.jpg",
                    SortOrder = 1,
                    IsActive = true,
                    Stock = 100
                },
                new()
                {
                    Category = ShopCategory.Sham,
                    Slug = "ay-enerjili-ritual-sami",
                    Name = "Ay Enerjili Ritual Şamı",
                    NameEn = "Moon-Charged Ritual Candle",
                    Description = "Dolunay enerjisi ilə \"şarj edilmiş\" ritual şamı — niyyət qoyma və manifestasiya təcrübələri üçün hazırlanıb.",
                    PriceAzn = 20,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/sham-ay-enerjili.jpg",
                    SortOrder = 2,
                    IsActive = true,
                    Stock = 70
                },
                new()
                {
                    Category = ShopCategory.Kitab,
                    Slug = "astrologiyaya-giris-burcler-evler",
                    Name = "Astrologiyaya Giriş: Bürclər və Evlər",
                    NameEn = "Introduction to Astrology: Signs and Houses",
                    Description = "12 bürc, 12 ev və əsas planetlərin mənalarını sadə dildə izah edən başlanğıc səviyyəsi astrologiya kitabı.",
                    PriceAzn = 12,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/kitab-astrologiyaya-giris.jpg",
                    SortOrder = 1,
                    IsActive = true,
                    Stock = 40
                },
                new()
                {
                    Category = ShopCategory.Kitab,
                    Slug = "natal-xerite-oxuma-beledcisi",
                    Name = "Natal Xəritə Oxuma Bələdçisi",
                    NameEn = "Natal Chart Reading Guide",
                    Description = "Öz natal xəritəni addım-addım oxumağı öyrədən praktik bələdçi — planet mövqeləri, aspektlər və evlərin şərhi daxildir.",
                    PriceAzn = 16,
                    UnitLabel = "ədəd",
                    ImageUrl = "/uploads/shop/kitab-natal-xerite.jpg",
                    SortOrder = 2,
                    IsActive = true,
                    Stock = 40
                }
            };

                await context.ShopProducts.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // 9b. Backfill: 36 horoskop sətrinin (12 bürc × gündəlik/həftəlik/aylıq) AZ mətnini
            // real, fərdiləşdirilmiş mətnlə əvəz edir və EN/RU tərcümələrini əlavə edir (frontend
            // migrasiyası 20261007080000_horoscope_translations_fix.sql-dən portlanıb). Yuxarıdaki
            // 5-ci bölmədəki generic placeholder mətni ($"Bu gün {sign} bürcü üçün...") yalnız DB
            // BOŞ olanda yazılırdı (AnyAsync() gate) — artıq mövcud DB-lərdə bu placeholder qalıb.
            // Bu blok HƏR DƏFƏ işə salınır (gate-dən asılı olmadan) və sign+period üzrə tapdığı ən son
            // sətri real mətnlə yeniləyir — idempotentdir, təkrar işə salınsa problem yaratmaz.
            {
                var allHoroscopes = await context.Horoscopes.ToListAsync();
                var horoscopeTranslations = new List<(string Sign, HoroscopePeriod Period, string Az, string En, string Ru)>
                {
                    ("Qoç", HoroscopePeriod.Daily, "Bu gün enerjin zirvədədir — başladığın işi sona çatdırmaq üçün ideal məqamdasan. Qərarlarını tez ver, amma ətrafındakılarla məsləhətləşməyi unutma.", "Your energy is at its peak today — this is the ideal moment to finish what you started. Make your decisions quickly, but don't forget to consult the people around you.", "Сегодня твоя энергия на пике — идеальный момент, чтобы завершить начатое. Принимай решения быстро, но не забывай советоваться с окружающими."),
                    ("Qoç", HoroscopePeriod.Weekly, "Bu həftə liderlik instinktin önə çıxır, komanda içində təşəbbüsü sən götürəcəksən. Maliyyə mövzusunda diqqətli ol, tələsik xərcdən çəkin.", "Your leadership instinct takes the lead this week — you'll be the one taking initiative within the team. Be careful with finances, and avoid impulsive spending.", "На этой неделе на первый план выходит твой лидерский инстинкт — именно ты возьмёшь на себя инициативу в команде. Будь внимателен в финансах, избегай поспешных трат."),
                    ("Qoç", HoroscopePeriod.Monthly, "Bu ay yeni başlanğıclar üçün əlverişlidir — iş yerində və ya şəxsi layihələrdə addım atmağa cəsarət et. Ayın ikinci yarısında münasibətlərə daha çox vaxt ayırmaq sənə yaxşı gələcək.", "This month favors new beginnings — have the courage to take action at work or on personal projects. In the second half of the month, giving more time to your relationships will do you good.", "Этот месяц благоприятен для новых начинаний — не бойся сделать шаг вперёд на работе или в личных проектах. Во второй половине месяца тебе будет полезно уделить больше времени отношениям."),
                    ("Buğa", HoroscopePeriod.Daily, "Sabitlik və rahatlıq bu gün sənin üçün prioritetdir — gündəlik rutinini pozmamaq ən doğru seçimdir. Maliyyə qərarlarında tələsmə, axşam saatları düşünməyə həsr olunsun.", "Stability and comfort are your priority today — sticking to your daily routine is the right choice. Don't rush financial decisions; set aside the evening hours for reflection.", "Стабильность и комфорт сегодня в приоритете — лучший выбор — не нарушать привычный распорядок дня. Не торопись с финансовыми решениями, вечерние часы посвяти размышлениям."),
                    ("Buğa", HoroscopePeriod.Weekly, "Bu həftə səbrin mükafatlanacaq — uzun müddətdir gözlədiyin bir xəbər gələ bilər. Material məsələlərdə ağıllı planlaşdırma indi öz bəhrəsini verəcək.", "Your patience will be rewarded this week — news you've been waiting a long time for may arrive. Smart planning in material matters will start paying off now.", "На этой неделе твоё терпение будет вознаграждено — может прийти новость, которую ты давно ждал. Разумное планирование в материальных делах начнёт приносить плоды."),
                    ("Buğa", HoroscopePeriod.Monthly, "Bu ay əsaslı təməllər qurmaq vaxtıdır — iş və ev məsələlərində möhkəm addımlar at. Sevgi həyatında isə daha açıq olmaq münasibətlərini dərinləşdirəcək.", "This month is the time to build solid foundations — take firm steps in work and home matters. In your love life, being more open will deepen your relationships.", "Этот месяц — время строить прочный фундамент: делай уверенные шаги в работе и домашних делах. А в любви большая открытость сделает твои отношения глубже."),
                    ("Əkizlər", HoroscopePeriod.Daily, "Zehnin bu gün adətən olduğundan da çevikdir — yeni fikirlər, söhbətlər və təkliflər səni həyəcanlandıracaq. Amma bir mövzuya fokuslanmaqda çətinlik çəkə bilərsən.", "Your mind is even sharper than usual today — new ideas, conversations, and offers will excite you. But you may find it hard to focus on just one topic.", "Сегодня твой ум особенно живой — новые идеи, разговоры и предложения будут тебя радовать. Но тебе может быть трудно сосредоточиться на одной теме."),
                    ("Əkizlər", HoroscopePeriod.Weekly, "Bu həftə ünsiyyət qabiliyyətin ön plana çıxır — danışıqlar, müsahibələr və ya yeni tanışlıqlar uğurlu keçəcək. Qərar verməzdən əvvəl bir az gözlə.", "Your communication skills take center stage this week — negotiations, interviews, or new acquaintances will go well. Wait a little before making a decision.", "На этой неделе на первый план выходят твои коммуникативные способности — переговоры, собеседования или новые знакомства пройдут удачно. Перед принятием решения немного подожди."),
                    ("Əkizlər", HoroscopePeriod.Monthly, "Bu ay öyrənmə və inkişaf ayıdır — yeni bacarıq və ya sahəyə maraq göstərəcəksən. Maliyyədə dəyişkənlik ola bilər, ona görə ehtiyat büdcəsi saxla.", "This month is about learning and growth — you'll take an interest in a new skill or field. Finances may be unpredictable, so keep a reserve budget.", "Этот месяц — месяц обучения и развития: ты заинтересуешься новым навыком или сферой. В финансах возможна нестабильность, поэтому держи резервный бюджет."),
                    ("Xərçəng", HoroscopePeriod.Daily, "Hisslərin bu gün adətən olduğundan daha güclüdür — ailə və yaxınlarınla vaxt keçirmək sənə rahatlıq gətirəcək. İş yerində emosional qərar verməkdən çəkin.", "Your feelings run stronger than usual today — spending time with family and loved ones will bring you comfort. Avoid making emotional decisions at work.", "Сегодня твои чувства сильнее обычного — время, проведённое с семьёй и близкими, принесёт тебе утешение. На работе избегай эмоциональных решений."),
                    ("Xərçəng", HoroscopePeriod.Weekly, "Bu həftə ev və ailə mövzuları diqqət mərkəzindədir — köhnə bir münasibəti düzəltmək üçün münasib zamandır. Maliyyə planında ehtiyatlı addımlar at.", "Home and family matters take center stage this week — it's a good time to mend an old relationship. Take cautious steps in your financial planning.", "На этой неделе в центре внимания темы дома и семьи — подходящее время, чтобы наладить старые отношения. В финансовом планировании действуй осторожно."),
                    ("Xərçəng", HoroscopePeriod.Monthly, "Bu ay emosional dərinlik və intuisiya sənə yol göstərəcək — qərarlarını ürəyinin səsinə görə ver. Karyerada sakit, amma davamlı irəliləyiş gözlənilir.", "This month, emotional depth and intuition will guide you — make your decisions by listening to your heart. Expect quiet but steady progress in your career.", "В этом месяце эмоциональная глубина и интуиция будут твоим проводником — принимай решения, слушая своё сердце. В карьере ожидается тихий, но устойчивый прогресс."),
                    ("Aslan", HoroscopePeriod.Daily, "Bu gün diqqət mərkəzində olacaqsan — xarizman və özünəinamın ətrafındakıları təsirləndirəcək. Səxavətli ol, amma büdcəni də nəzarətdə saxla.", "You'll be the center of attention today — your charisma and self-confidence will make an impression on those around you. Be generous, but keep your budget under control.", "Сегодня ты будешь в центре внимания — твоя харизма и уверенность в себе произведут впечатление на окружающих. Будь щедрым, но держи бюджет под контролем."),
                    ("Aslan", HoroscopePeriod.Weekly, "Bu həftə yaradıcı layihələr və ya təqdimatlar üçün ulduzlar səninlədir — özünü göstərmək fürsətini qaçırma. Cütlük münasibətlərində tərifə ehtiyac olduğunu unutma.", "The stars are with you this week for creative projects or presentations — don't miss the chance to shine. In your romantic relationship, remember that praise matters.", "На этой неделе звёзды на твоей стороне в творческих проектах и презентациях — не упусти шанс проявить себя. В отношениях не забывай, что похвала важна."),
                    ("Aslan", HoroscopePeriod.Monthly, "Bu ay liderlik və tanınma ayıdır — zəhmətin nəhayət qiymətləndiriləcək. Sevgidə qürurunu bir az yumşaltmaq münasibətləri gücləndirəcək.", "This month is about leadership and recognition — your hard work will finally be appreciated. In love, softening your pride a little will strengthen your relationships.", "Этот месяц — месяц лидерства и признания: твой труд наконец оценят по заслугам. В любви немного смягчить гордость поможет укрепить отношения."),
                    ("Qız", HoroscopePeriod.Daily, "Təfərrüatlara diqqətin bu gün sənə böyük üstünlük verir — işdəki kiçik bir səhvi vaxtında tuta bilərsən. Özünü tənqid etməkdə ölçünü aşma.", "Your attention to detail gives you a real edge today — you may catch a small mistake at work just in time. Don't overdo it with self-criticism.", "Сегодня твоё внимание к деталям даёт тебе большое преимущество — ты можешь вовремя заметить небольшую ошибку на работе. Не переусердствуй с самокритикой."),
                    ("Qız", HoroscopePeriod.Weekly, "Bu həftə təşkilatçılıq bacarığın sayəsində yığılmış işləri nizama salacaqsan. Sağlamlığına da diqqət ayırmağı unutma — fasilə vermək zəiflik deyil.", "This week, your organizational skills will help you bring order to a backlog of tasks. Don't forget to look after your health too — taking a break isn't a weakness.", "На этой неделе благодаря своим организаторским способностям ты наведёшь порядок в накопившихся делах. Не забывай и о здоровье — отдыхать — не слабость."),
                    ("Qız", HoroscopePeriod.Monthly, "Bu ay praktiklik və planlaşdırma sənə uğur gətirəcək — uzunmüddətli hədəflər üçün konkret addımlar at. Münasibətlərdə mükəmməllik gözləməkdən bir az əl çək.", "This month, practicality and planning will bring you success — take concrete steps toward your long-term goals. Ease up a little on expecting perfection in relationships.", "Этот месяц принесёт тебе успех благодаря практичности и планированию — сделай конкретные шаги к долгосрочным целям. В отношениях немного отступи от ожидания совершенства."),
                    ("Tərəzi", HoroscopePeriod.Daily, "Tarazlıq axtarışın bu gün münasibətlərdə özünü göstərir — mübahisəli bir məsələdə ədalətli vasitəçi ola bilərsən. Qərarsızlıq vaxt itkisinə səbəb olmasın.", "Your search for balance shows up in your relationships today — you may end up being the fair mediator in a dispute. Don't let indecision waste your time.", "Сегодня твой поиск равновесия проявляется в отношениях — ты можешь стать справедливым посредником в спорном вопросе. Не позволяй нерешительности отнимать время."),
                    ("Tərəzi", HoroscopePeriod.Weekly, "Bu həftə tərəfdaşlıq və əməkdaşlıq mövzuları önə çıxır — iş və ya şəxsi həyatda kiminləsə razılaşma əldə edəcəksən. Estetik zövqün yaradıcı işlərdə köməyinə çatacaq.", "Partnership and collaboration take the spotlight this week — you'll reach an agreement with someone, at work or in your personal life. Your aesthetic taste will help you in creative work.", "На этой неделе на первый план выходят темы партнёрства и сотрудничества — ты достигнешь согласия с кем-то на работе или в личной жизни. Твой эстетический вкус поможет в творческих делах."),
                    ("Tərəzi", HoroscopePeriod.Monthly, "Bu ay münasibətlər ayıdır — həm romantik, həm də peşəkar əlaqələrdə harmoniya qurmaq üçün əlverişli vaxtdır. Maliyyədə tərəfdaşlıq əsaslı qərarlar faydalı olacaq.", "This month is about relationships — a favorable time to build harmony in both romantic and professional connections. Partnership-based financial decisions will work in your favor.", "Этот месяц — месяц отношений: благоприятное время для построения гармонии и в романтических, и в профессиональных связях. В финансах решения, основанные на партнёрстве, окажутся полезными."),
                    ("Əqrəb", HoroscopePeriod.Daily, "Bu gün səthin altındakı həqiqəti görmək bacarığın güclənir — gizli qalan bir məsələ üzə çıxa bilər. Kimə etibar etdiyinə diqqət et.", "Your ability to see beneath the surface is heightened today — something that's been hidden may come to light. Pay attention to who you trust.", "Сегодня усиливается твоя способность видеть правду под поверхностью — может всплыть скрытый до этого вопрос. Будь внимателен к тому, кому доверяешь."),
                    ("Əqrəb", HoroscopePeriod.Weekly, "Bu həftə dərin dəyişikliklər üçün enerji toplayırsan — köhnə bir vərdişi və ya münasibəti arxada qoymaq vaxtı ola bilər. İş məsələlərində strategiyanı gizli saxla.", "This week you're gathering energy for deep change — it may be time to leave an old habit or relationship behind. Keep your strategy at work to yourself.", "На этой неделе ты накапливаешь энергию для глубоких перемен — возможно, пришло время оставить в прошлом старую привычку или отношения. В рабочих вопросах держи свою стратегию в секрете."),
                    ("Əqrəb", HoroscopePeriod.Monthly, "Bu ay transformasiya ayıdır — maliyyə və ya karyerada köklü bir dəyişiklik baş verə bilər. Sevgidə ehtirasın güclüdür, amma qısqanclığa yer vermə.", "This month is about transformation — a fundamental change may happen in your finances or career. In love, your passion is strong, but don't give room to jealousy.", "Этот месяц — месяц трансформации: в финансах или карьере может произойти коренная перемена. В любви твоя страсть сильна, но не давай места ревности."),
                    ("Oxatan", HoroscopePeriod.Daily, "Macəra hissi bu gün səni adi gündəlikdən uzaqlaşdırmaq istəyir — qısa bir səyahət və ya yeni təcrübə əhval-ruhiyyəni qaldıracaq. Vədlərini yerinə yetirməyi unutma.", "Your sense of adventure wants to pull you away from the usual routine today — a short trip or a new experience will lift your mood. Don't forget to keep your promises.", "Сегодня чувство авантюризма хочет увести тебя от обычной рутины — короткая поездка или новый опыт поднимут настроение. Не забывай выполнять свои обещания."),
                    ("Oxatan", HoroscopePeriod.Weekly, "Bu həftə üfüqlərini genişləndirən fürsətlər qarşına çıxa bilər — təhsil, səyahət və ya xaricdən təklif mövzusunda xəbər gözlə. Maliyyədə həddən artıq nikbin olma.", "Opportunities that expand your horizons may come your way this week — expect news about education, travel, or an offer from abroad. Don't be overly optimistic about finances.", "На этой неделе тебе могут встретиться возможности, расширяющие горизонты — ожидай новостей об образовании, путешествии или предложении из-за границы. В финансах не будь чрезмерно оптимистичен."),
                    ("Oxatan", HoroscopePeriod.Monthly, "Bu ay genişlənmə və azadlıq ayıdır — yeni bir sahəyə addım atmaq üçün cəsarətin var. Münasibətlərdə sərbəstliyini qorumaqla bağlılıq arasında tarazlıq tap.", "This month is about expansion and freedom — you have the courage to step into a new field. In relationships, find the balance between keeping your independence and staying committed.", "Этот месяц — месяц расширения и свободы: у тебя есть смелость сделать шаг в новую сферу. В отношениях найди баланс между сохранением свободы и привязанностью."),
                    ("Oğlaq", HoroscopePeriod.Daily, "Məsuliyyət hissi bu gün səni irəli aparır — planlaşdırdığın işi intizamla başa çatdıracaqsan. Özünə bir az mərhəmət göstərməyi unutma.", "A sense of responsibility carries you forward today — you'll finish the task you planned with discipline. Don't forget to show yourself a little compassion.", "Сегодня чувство ответственности ведёт тебя вперёд — ты с дисциплиной завершишь запланированное дело. Не забывай проявлять немного сострадания к себе."),
                    ("Oğlaq", HoroscopePeriod.Weekly, "Bu həftə zəhmətin nəticə verir — rəhbərlik tərəfindən diqqət çəkə bilərsən. Ailə ilə vaxt keçirməyə də yer aç, iş hər şey deyil.", "Your hard work pays off this week — you may catch the attention of your superiors. Make room for time with family too — work isn't everything.", "На этой неделе твой труд приносит результат — ты можешь привлечь внимание руководства. Найди время и для семьи — работа — не всё."),
                    ("Oğlaq", HoroscopePeriod.Monthly, "Bu ay karyera və nüfuz ayıdır — uzun müddətdir qurduğun strategiya bəhrəsini verməyə başlayır. Maliyyədə qənaətcil yanaşman sənə sabitlik gətirəcək.", "This month is about career and standing — the strategy you've been building for a long time starts to bear fruit. Your frugal approach to finances will bring you stability.", "Этот месяц — месяц карьеры и влияния: стратегия, которую ты выстраивал долгое время, начинает приносить плоды. Бережливый подход к финансам принесёт тебе стабильность."),
                    ("Dolça", HoroscopePeriod.Daily, "Orijinal fikirlərin bu gün diqqət çəkəcək — adi yoldan fərqli bir həll yolu təklif etmə vaxtıdır. Dostların dəstəyinə ehtiyac duysan, çəkinmədən müraciət et.", "Your original ideas will draw attention today — it's a good time to propose a solution that breaks from the usual path. If you need your friends' support, don't hesitate to ask.", "Сегодня твои оригинальные идеи привлекут внимание — время предложить решение, отличное от привычного пути. Если тебе нужна поддержка друзей, не стесняйся обратиться."),
                    ("Dolça", HoroscopePeriod.Weekly, "Bu həftə sosial çevrən genişlənir — yeni tanışlıqlar və ya icma layihələri sənə ilham verəcək. Maliyyə məsələlərində qeyri-adi, amma işə yarayan bir yol tapacaqsan.", "Your social circle expands this week — new acquaintances or community projects will inspire you. In financial matters, you'll find an unusual but workable solution.", "На этой неделе твой круг общения расширяется — новые знакомства или общественные проекты вдохновят тебя. В финансовых вопросах ты найдёшь необычный, но рабочий путь."),
                    ("Dolça", HoroscopePeriod.Monthly, "Bu ay yenilik və müstəqillik ayıdır — ənənəvi qaydalardan kənara çıxıb öz yolunu getmək istəyin güclənir. Münasibətlərdə məsafəyə ehtiyac duysan, bunu açıq izah et.", "This month is about innovation and independence — your urge to step outside traditional rules and follow your own path grows stronger. If you need distance in a relationship, explain it openly.", "Этот месяц — месяц новаторства и независимости: усиливается желание выйти за рамки традиционных правил и идти своим путём. Если в отношениях нужна дистанция, объясни это открыто."),
                    ("Balıqlar", HoroscopePeriod.Daily, "Həssaslığın bu gün sənə başqalarının demədiyini hiss etmək imkanı verir — bu bacarığı yaradıcı işində istifadə et. Reallıqdan qaçmaqdansa, üzləş.", "Your sensitivity today lets you sense what others leave unsaid — put that gift to use in your creative work. Rather than escaping reality, face it.", "Сегодня твоя чувствительность позволяет улавливать то, что другие не говорят вслух — используй эту способность в творческой работе. Вместо того чтобы убегать от реальности, посмотри ей в лицо."),
                    ("Balıqlar", HoroscopePeriod.Weekly, "Bu həftə xəyal gücün və intuisiyan güclüdür — sənət, musiqi və ya yazıya vaxt ayırsan, gözəl nəticələr alacaqsan. Pul məsələlərində real rəqəmlərə sadiq qal.", "Your imagination and intuition are strong this week — if you give time to art, music, or writing, you'll get beautiful results. In money matters, stick to the real numbers.", "На этой неделе твоё воображение и интуиция сильны — если уделишь время искусству, музыке или писательству, получишь прекрасные результаты. В денежных вопросах держись реальных цифр."),
                    ("Balıqlar", HoroscopePeriod.Monthly, "Bu ay ruhani və yaradıcı inkişaf ayıdır — daxili səsini dinləmək sənə doğru istiqaməti göstərəcək. Sevgidə dərin bir bağlılıq qurmaq üçün əlverişli zamandır.", "This month is about spiritual and creative growth — listening to your inner voice will show you the right direction. It's a favorable time to build a deep bond in love.", "Этот месяц — месяц духовного и творческого роста: умение слушать свой внутренний голос покажет тебе верное направление. Это благоприятное время для построения глубокой связи в любви."),
                };

                foreach (var (sign, period, az, en, ru) in horoscopeTranslations)
                {
                    var match = allHoroscopes
                        .Where(h => h.Sign == sign && h.Period == period)
                        .OrderByDescending(h => h.PeriodStart)
                        .FirstOrDefault();
                    if (match == null) continue;

                    match.Content = az;
                    match.ContentEn = en;
                    match.ContentRu = ru;
                    context.Horoscopes.Update(match);
                }

                await context.SaveChangesAsync();
            }

            // 9c. Backfill: mağaza məhsullarının UnitLabel-i ("dəst"/"ədəd" — generic ölçü sözü,
            // frontend-dəki fərqli slug-lar üzrə fərdi spesifikasiyalardan fərqli olaraq) üçün
            // EN/RU tərcüməsi. Yuxarıdaki 9-cu bölmədəki "if (!AnyAsync())" gate-dən asılı olmadan
            // hər başlanğıcda işə salınır, idempotentdir.
            {
                var allProducts = await context.ShopProducts.ToListAsync();
                foreach (var product in allProducts)
                {
                    if (product.UnitLabel == "dəst")
                    {
                        product.UnitLabelEn = "set";
                        product.UnitLabelRu = "набор";
                        context.ShopProducts.Update(product);
                    }
                    else if (product.UnitLabel == "ədəd")
                    {
                        product.UnitLabelEn = "piece";
                        product.UnitLabelRu = "шт.";
                        context.ShopProducts.Update(product);
                    }
                }

                await context.SaveChangesAsync();
            }

            // 10. Seed-expansion: 12 yeni çoxdilli (AZ+EN+RU) məqalə — frontend
            // migrasiyasından (20261007100000_articles_and_forum_i18n_expansion.sql) portlanıb.
            // Additivdir: bu slug-lar yuxarıdaki 14 məqalənin slug-ları ilə TOQQUŞMUR, ona görə
            // yuxarıdaki "if (!await context.Articles.AnyAsync())" gate-indən ASILI OLMAYARAQ
            // (o artıq true-dur, çünki 14 məqalə var) hər başlanğıcda slug üzrə yoxlanılıb əlavə olunur.
            {
                var now10 = DateTime.UtcNow;
                var newArticles = new List<Article>
                {
                    new()
                    {
                        Title = "Kiron: yaralı şəfaçı arxetipi",
                        TitleEn = "Chiron: the wounded healer archetype",
                        TitleRu = "Хирон: архетип раненого целителя",
                        Slug = "kiron-yarali-shefaci",
                        Excerpt = "Kiron ən dərin yarandığımız yer, həm də başqalarına şəfa vermə qabiliyyətimizin mənbəyidir.",
                        ExcerptEn = "Chiron marks our earliest, deepest wound — and often the root of our ability to heal others.",
                        ExcerptRu = "Хирон отмечает нашу самую раннюю и глубокую рану — и часто становится источником способности исцелять других.",
                        Body = "Kiron Saturn ilə Uran arasında dövr edən kiçik bir göy cismidir və astrologiyada 'yaralı şəfaçı' adlanır. Onun xəritədəki mövqeyi, ən erkən və ən dərin yaranan həssaslığımızı göstərir — adətən uşaqlıqda formalaşan, həyat boyu yenidən üzə çıxan bir mövzu.\n\nKironun bürcü və evi, bu yaranın harada daha çox hiss olunduğunu göstərir. Məsələn, 7-ci evdə Kiron münasibətlərdə etibar məsələləri ilə bağlı ola bilər, 10-cu evdə isə karyerada görünməmə və ya qiymətləndirilməmə hissi ilə.\n\nParadoks burasındadır ki, məhz bu həssas nöqtə zamanla ən böyük güc mənbəyinə çevrilə bilər. Öz yarasını tanıyan insan, çox vaxt başqalarına şəfa vermək qabiliyyəti ilə seçilir — buna görə də Kiron tez-tez həkimlərin, terapevtlərin və müəllimlərin xəritəsində önəmli mövqedə olur.",
                        BodyEn = "Chiron is a small body orbiting between Saturn and Uranus, known in astrology as the 'wounded healer.' Its placement in the chart points to our earliest and deepest sensitivity — usually something shaped in childhood that keeps resurfacing throughout life.\n\nChiron's sign and house show where that wound tends to be felt most. In the 7th house, for instance, it can show up as trust issues in relationships; in the 10th house, as a feeling of being overlooked or unappreciated in one's career.\n\nThe paradox is that this tender point can become, over time, a real source of strength. Someone who has come to know their own wound often develops a real capacity to help others heal — which is why Chiron so often sits prominently in the charts of doctors, therapists, and teachers.",
                        BodyRu = "Хирон — небольшое небесное тело, движущееся по орбите между Сатурном и Ураном, в астрологии известное как 'раненый целитель'. Его положение в карте указывает на самую раннюю и глубокую чувствительную точку — обычно сформированную в детстве и всплывающую снова на протяжении жизни.\n\nЗнак и дом Хирона показывают, где эта рана ощущается сильнее всего. Например, в 7-м доме это может проявляться как проблемы с доверием в отношениях, а в 10-м — как ощущение незамеченности или недооценённости в карьере.\n\nПарадокс в том, что именно эта чувствительная точка со временем может стать источником настоящей силы. Человек, узнавший свою рану, часто обретает способность помогать исцеляться другим — поэтому Хирон нередко занимает заметное место в картах врачей, терапевтов и учителей.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-0),
                        Views = 70
                    },
                    new()
                    {
                        Title = "Zenit (MC): karyera və ictimai imic xəritəsi",
                        TitleEn = "The Midheaven (MC): your career and public image",
                        TitleRu = "Зенит (MC): карьера и публичный образ",
                        Slug = "mc-zenit-peshe-nufuz",
                        Excerpt = "Zenit nöqtəsi dünyaya hansı sifətlə tanındığını göstərir.",
                        ExcerptEn = "The Midheaven shows the face you're known by in the world.",
                        ExcerptRu = "Зенит показывает, каким лицом тебя узнаёт мир.",
                        Body = "Natal xəritənin ən yuxarı nöqtəsi Zenit (Midheaven, MC) adlanır və doğum anında səma xəritəsinin ən yüksək hissəsini göstərir. Bu nöqtə karyera istiqamətini, ictimai nüfuzu və insanların səni ilk növbədə necə tanıdığını əks etdirir.\n\nZenitin bürcü, hansı sahədə öz izini qoymaq istədiyini göstərə bilər. Oğlaq Zenit struktur və nüfuz axtarır, Dolça Zenit isə yenilik və icma işləri ilə tanınmaq istəyir. Zenitə yaxın planetlər də ictimai imicə güclü təsir göstərir.\n\nZenit təkcə 'hansı iş' sualına cavab vermir — o, daha geniş mənada, dünyaya hansı töhfəni vermək istədiyini göstərir. Buna görə də karyera seçimində çətinlik çəkənlər üçün Zenit araştırmaya başlamaq üçün yaxşı nöqtədir.",
                        BodyEn = "The highest point of the birth chart is called the Midheaven (MC), marking the top of the sky map at the moment of birth. It reflects career direction, public standing, and how people tend to recognize you first.\n\nThe Midheaven's sign can point to the area where you're drawn to make your mark. A Capricorn MC tends to seek structure and authority, while an Aquarius MC often wants to be known for innovation and community work. Planets near the Midheaven also shape public image strongly.\n\nThe Midheaven doesn't just answer 'what job' — in a broader sense, it shows what contribution you want to make to the world. That makes it a good starting point for anyone struggling with career direction.",
                        BodyRu = "Самая высокая точка натальной карты называется Зенитом (Midheaven, MC) и отмечает верхнюю часть карты неба в момент рождения. Она отражает направление карьеры, общественное положение и то, каким человека узнают первым делом.\n\nЗнак Зенита может подсказать, в какой сфере человек стремится оставить свой след. Зенит в Козероге тянется к структуре и авторитету, а Зенит в Водолее — к новаторству и работе на благо сообщества. Планеты рядом с Зенитом также сильно влияют на публичный образ.\n\nЗенит не отвечает только на вопрос 'какая профессия' — в более широком смысле он показывает, какой вклад человек хочет внести в мир. Поэтому тем, кто испытывает трудности с выбором карьеры, стоит начать именно с изучения своего Зенита.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-1),
                        Views = 110
                    },
                    new()
                    {
                        Title = "Yupiter tranziti: genişlənmə zamanı",
                        TitleEn = "Jupiter transits: a season of expansion",
                        TitleRu = "Транзит Юпитера: сезон расширения",
                        Slug = "yupiter-tranziti-genislenme",
                        Excerpt = "Yupiter keçdiyi hər evdə böyümə və imkan qapıları açır.",
                        ExcerptEn = "Wherever Jupiter is moving through, doors to growth and opportunity tend to open.",
                        ExcerptRu = "Там, где движется Юпитер, обычно открываются двери к росту и возможностям.",
                        Body = "Yupiter bürcdən bürcə təxminən 12 ildə bir dövr edir və hər bürcdə bir il qədər qalır. Bu, onun xəritənin hər evini təxminən bir il ərzində 'ziyarət etdiyi' deməkdir — və həmin dövrdə o sahədə genişlənmə, inkişaf və yeni imkanlar güclənir.\n\nYupiter 2-ci evdən keçərkən maliyyə imkanları, 9-cu evdən keçərkən təhsil və səyahət, 11-ci evdən keçərkən isə sosial şəbəkə və icma bağları güclənə bilər. Təbii ki, Yupiter həmişə asanlıqla gəlməyən, bəzən həddindən artıqlıqla müşayiət olunan böyümə də gətirə bilər.\n\nYupiter tranziti zamanı ən yaxşı strategiya, açılan qapılardan məqsədyönlü istifadə etməkdir — çünki bu geniş enerji pəncərəsi həmişə açıq qalmır, təxminən bir ildən sonra Yupiter növbəti bürcə keçir və fokus dəyişir.",
                        BodyEn = "Jupiter takes about 12 years to orbit the Sun, spending roughly a year in each sign. That means it 'visits' each house of the chart for about a year at a time — and during that stretch, growth, opportunity, and expansion in that area of life tend to pick up.\n\nJupiter moving through the 2nd house can bring financial opportunity, through the 9th house can bring education and travel, and through the 11th house can strengthen social networks and community ties. Of course, Jupiter's growth doesn't always arrive easily — it can also come with overextension or excess.\n\nThe best strategy during a Jupiter transit is to make deliberate use of the doors that open, since this expansive window doesn't stay open forever — after roughly a year, Jupiter moves into the next sign and the focus shifts.",
                        BodyRu = "Юпитер совершает полный оборот примерно за 12 лет, проводя около года в каждом знаке. Это значит, что он 'посещает' каждый дом карты примерно на год — и в это время рост, возможности и расширение в соответствующей сфере жизни усиливаются.\n\nЮпитер во 2-м доме может принести финансовые возможности, в 9-м — образование и путешествия, а в 11-м — укрепление социальных связей и сообщества. Конечно, рост Юпитера не всегда приходит легко — он может сопровождаться и избыточностью, перегибом.\n\nЛучшая стратегия во время транзита Юпитера — осознанно использовать открывающиеся двери, ведь это окно расширения не остаётся открытым навсегда: примерно через год Юпитер переходит в следующий знак, и фокус смещается.",
                        Tag = "Tranzit",
                        Published = true,
                        PublishedAt = now10.AddDays(-2),
                        Views = 150
                    },
                    new()
                    {
                        Title = "Lilit (Qara Ay): kölgədəki güc",
                        TitleEn = "Lilith (the Dark/Black Moon): power in the shadow",
                        TitleRu = "Лилит (Чёрная Луна): сила в тени",
                        Slug = "lilit-qara-ay-kolge-guc",
                        Excerpt = "Lilit bizim bəyənilməyən, lakin əsl gücümüzün gizləndiyi tərəfimizi göstərir.",
                        ExcerptEn = "Lilith points to the part of us we were taught to hide — and where our real power often hides with it.",
                        ExcerptRu = "Лилит указывает на ту часть нас, которую нас учили прятать — и где часто скрывается наша настоящая сила.",
                        Body = "Lilit, real göy cismi deyil — Ayın orbitindəki ən uzaq nöqtədir və astrologiyada 'Qara Ay' adlanır. O, cəmiyyət tərəfindən qəbul edilməsi çətin olan, bəzən 'yaraşmaz' sayılan, lakin çox güclü instinktləri və istəkləri simvolizə edir.\n\nLilitin bürcü, haradan 'utanmaq' öyrədildiyimizi göstərə bilər — məsələn Əkizlər Lilit birbaşa, kəskin danışmaqdan çəkinməyi, Buğa Lilit isə öz dəyərini açıq şəkildə tələb etməkdən qorxmağı göstərə bilər.\n\nLilitlə işləmək, bu basdırılmış enerjini tanımaq və qəbul etməkdir. Kölgədə saxlanılan bu güc, üzə çıxanda çox vaxt ən həqiqi, ən sərbəst tərəfimizə çevrilir — buna görə də Lilit 'qaranlıq' deyil, daha çox 'görünməyən güc' kimi başa düşülməlidir.",
                        BodyEn = "Lilith isn't a physical body — it's the furthest point in the Moon's orbit, known in astrology as the 'Black Moon.' It symbolizes instincts and desires that are hard for society to accept, sometimes labeled 'inappropriate,' yet often genuinely powerful.\n\nLilith's sign can show where we were taught to feel ashamed. A Gemini Lilith, for example, might point to being discouraged from speaking bluntly, while a Taurus Lilith might point to fear of openly claiming one's own worth.\n\nWorking with Lilith means recognizing and accepting this suppressed energy. Once it's brought into the light, this shadow-held power often becomes the most authentic, most liberated part of us — which is why Lilith is better understood not as 'darkness,' but as a kind of hidden strength.",
                        BodyRu = "Лилит — это не физическое тело, а самая дальняя точка орбиты Луны, известная в астрологии как 'Чёрная Луна'. Она символизирует инстинкты и желания, которые общество принимает с трудом, иногда считая их 'неподобающими', хотя на деле они бывают по-настоящему сильными.\n\nЗнак Лилит может показать, где нас учили стыдиться. Например, Лилит в Близнецах может указывать на то, что человека отучали говорить прямо и резко, а Лилит в Тельце — на страх открыто заявлять о своей ценности.\n\nРабота с Лилит — это узнавание и принятие этой подавленной энергии. Выведенная на свет, эта хранимая в тени сила часто становится самой подлинной, самой свободной частью нас — поэтому Лилит правильнее понимать не как 'темноту', а как своего рода скрытую силу.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-3),
                        Views = 190
                    },
                    new()
                    {
                        Title = "8-ci ev: yaxınlıq, dəyişim və paylaşılan resurslar",
                        TitleEn = "The 8th house: intimacy, transformation, and shared resources",
                        TitleRu = "8-й дом: близость, трансформация и общие ресурсы",
                        Slug = "8-ci-ev-yaxinlik-deyishim",
                        Excerpt = "8-ci ev səthin altındakı, dərin və çevrilmə gətirən mövzuları idarə edir.",
                        ExcerptEn = "The 8th house governs the deep, below-the-surface themes that bring real transformation.",
                        ExcerptRu = "8-й дом управляет глубинными, скрытыми темами, которые приносят настоящую трансформацию.",
                        Body = "8-ci ev natal xəritənin ən dərin və tez-tez ən az başa düşülən sahələrindən biridir. O, yalnız ölüm və gizli sirlərlə bağlı deyil — gündəlik həyatda daha çox dərin emosional yaxınlıq, cinsəllik, paylaşılan maliyyə resursları (irs, kredit, ortaq əmlak) və şəxsi transformasiya ilə əlaqəlidir.\n\n8-ci evdə planetlər olan insanlar üçün münasibətlər adətən səthi qala bilmir — onlar dərin etibar, zəiflik və bəzən intensiv emosional təcrübələr axtarır. Bu ev həm də 'buraxma' mövzusunu idarə edir: köhnə vərdişlərdən, münasibətlərdən və ya kimliklərdən əl çəkmək.\n\n8-ci evin boş olması, bu mövzuların həyatda daha az önə çıxdığı demək deyil — sadəcə bu sahədə daha az 'planet enerjisi' cəmləşdiyini göstərir. Evin hökmdar planetinə baxmaq, bu sahəni daha dərindən anlamaq üçün faydalıdır.",
                        BodyEn = "The 8th house is one of the deepest and most often misunderstood areas of the birth chart. It isn't only about death and hidden secrets — in everyday life, it's more connected to deep emotional intimacy, sexuality, shared financial resources (inheritance, debt, joint property), and personal transformation.\n\nFor people with planets in the 8th house, relationships rarely stay surface-level — they tend to seek deep trust, vulnerability, and sometimes intense emotional experiences. This house also governs letting go: releasing old habits, relationships, or identities.\n\nAn empty 8th house doesn't mean these themes play a smaller role in life — it simply means less planetary energy is concentrated there. Looking at the house's ruling planet is a useful way to understand this area more deeply.",
                        BodyRu = "8-й дом — одна из самых глубоких и часто наименее понятых областей натальной карты. Он не только про смерть и тайны — в повседневной жизни он больше связан с глубокой эмоциональной близостью, сексуальностью, общими финансовыми ресурсами (наследство, долги, совместное имущество) и личной трансформацией.\n\nУ людей с планетами в 8-м доме отношения редко остаются поверхностными — они склонны искать глубокое доверие, уязвимость и иногда довольно интенсивные эмоциональные переживания. Этот дом также управляет темой отпускания: старых привычек, отношений или идентичностей.\n\nПустой 8-й дом не означает, что эти темы играют меньшую роль в жизни — это просто показывает, что в этой сфере сосредоточено меньше планетарной энергии. Чтобы глубже понять эту область, полезно посмотреть на управляющую домом планету.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-4),
                        Views = 230
                    },
                    new()
                    {
                        Title = "Stellium nədir? Bir bürcdə toplaşan planetlər",
                        TitleEn = "What is a stellium? When planets cluster together",
                        TitleRu = "Что такое стеллиум? Когда планеты скапливаются вместе",
                        Slug = "stellium-nedir-planet-toplusu",
                        Excerpt = "Üç və ya daha çox planetin bir yerdə cəmləşməsi, xəritədə güclü bir fokus nöqtəsi yaradır.",
                        ExcerptEn = "Three or more planets gathered in one place creates a powerful point of focus in the chart.",
                        ExcerptRu = "Три или больше планет, собранных в одном месте, создают мощную точку фокуса в карте.",
                        Body = "Stellium, natal xəritədə eyni bürcdə (və ya eyni evdə) üç və ya daha çox planetin toplaşması deməkdir. Bu konsentrasiya, o bürcün (və ya evin) mövzularını xəritədə digərlərindən daha da güclü edir.\n\nMəsələn, Əqrəbdə stellium olan insan üçün intensivlik, dərinlik və nəzarət mövzuları həyatın demək olar hər sahəsində önə çıxa bilər. 10-cu evdə stellium olan insan isə karyera və ictimai nüfuzu demək olar ki, hər şeydən üstün tutur.\n\nStellium gücün cəmləşdiyi yer olduğu üçün, həm böyük potensial, həm də müəyyən bir birtərəflilik riski daşıyır. Xəritənin digər hissələrinə — xüsusən əks tərəfdəki boş sahələrə — diqqət yetirmək, balansı tapmaq üçün faydalıdır.",
                        BodyEn = "A stellium is when three or more planets cluster in the same sign (or the same house) of the natal chart. That concentration makes the themes of that sign — or house — stand out far more strongly than the rest of the chart.\n\nSomeone with a stellium in Scorpio, for instance, might find intensity, depth, and control showing up in nearly every area of life. Someone with a stellium in the 10th house, meanwhile, tends to put career and public standing above almost everything else.\n\nBecause a stellium is where power concentrates, it carries both real potential and a certain risk of one-sidedness. Paying attention to the rest of the chart — especially the emptier areas on the opposite side — is useful for finding balance.",
                        BodyRu = "Стеллиум — это скопление трёх или более планет в одном знаке (или одном доме) натальной карты. Такая концентрация делает темы этого знака или дома значительно сильнее выраженными, чем остальная часть карты.\n\nНапример, у человека со стеллиумом в Скорпионе интенсивность, глубина и контроль могут проявляться почти во всех сферах жизни. А у человека со стеллиумом в 10-м доме карьера и общественное положение почти всегда выходят на первый план.\n\nПоскольку стеллиум — это место концентрации силы, он несёт в себе как большой потенциал, так и определённый риск однобокости. Чтобы найти баланс, полезно обращать внимание на остальную часть карты — особенно на более пустые области с противоположной стороны.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-5),
                        Views = 270
                    },
                    new()
                    {
                        Title = "Retroqrad mövsümü: təkcə Merkuri deyil",
                        TitleEn = "Retrograde season: it's not just Mercury",
                        TitleRu = "Сезон ретроградов: не только Меркурий",
                        Slug = "retroqrad-movsumu-butun-planetler",
                        Excerpt = "Hər planetin öz retroqrad dövrü və öz dərsi var.",
                        ExcerptEn = "Every planet has its own retrograde cycle — and its own lesson to offer.",
                        ExcerptRu = "У каждой планеты свой ретроградный цикл — и свой урок.",
                        Body = "Merkuri retroqradı ən tanınmış olsa da, demək olar ki, bütün planetlər (Günəş və Ay istisna olmaqla) müəyyən dövrlərlə retroqrad görünür. Hər birinin öz müddəti və öz tematikası var.\n\nVenera retroqradı (təxminən 18 ayda bir, 6 həftə) sevgi və dəyərləri yenidən nəzərdən keçirməyə çağırır. Mars retroqradı (təxminən 2 ildə bir, 2 ay) enerji və hərəkətlə bağlı məsələləri yavaşladır. Yupiter və Saturn kimi uzaq planetlərin retroqradı isə hər il bir neçə ay davam edir və daha uzunmüddətli, strukturla bağlı dərslər gətirir.\n\nBütün retroqrad dövrlərinin ortaq cəhəti budur: irəli hərəkət deyil, geriyə baxış vaxtıdır. Hansı planet retroqraddadırsa, onun idarə etdiyi sahədə yenidən qiymətləndirmə, düzəliş və tamamlama üçün fürsət yaranır.",
                        BodyEn = "Mercury retrograde gets the most attention, but almost every planet (aside from the Sun and Moon) appears to move backward at certain points. Each one has its own duration and its own theme.\n\nVenus retrograde (roughly every 18 months, lasting about 6 weeks) calls for reassessing love and values. Mars retrograde (roughly every 2 years, lasting about 2 months) slows down matters related to energy and action. The retrogrades of farther planets like Jupiter and Saturn last several months each year and bring longer, more structural lessons.\n\nWhat all retrograde periods share is this: it's a time for looking back, not pushing forward. Whichever planet is retrograde, the area it governs opens up for reassessment, correction, and completion.",
                        BodyRu = "Ретроградный Меркурий привлекает больше всего внимания, но практически каждая планета (кроме Солнца и Луны) в определённые периоды кажется движущейся назад. У каждой — своя продолжительность и своя тема.\n\nРетроградная Венера (примерно раз в 18 месяцев, длится около 6 недель) призывает пересмотреть любовь и ценности. Ретроградный Марс (примерно раз в 2 года, длится около 2 месяцев) замедляет темы энергии и действия. Ретроградность более далёких планет, таких как Юпитер и Сатурн, длится несколько месяцев каждый год и приносит более долгосрочные, структурные уроки.\n\nОбщее для всех ретроградных периодов — это время не для движения вперёд, а для взгляда назад. Какая бы планета ни была ретроградной, в управляемой ею сфере открывается возможность для переоценки, исправления и завершения.",
                        Tag = "Tranzit",
                        Published = true,
                        PublishedAt = now10.AddDays(-6),
                        Views = 310
                    },
                    new()
                    {
                        Title = "Xəritədə element çatışmazlığı nə deməkdir?",
                        TitleEn = "What does an elemental imbalance in your chart mean?",
                        TitleRu = "Что значит дисбаланс элементов в карте?",
                        Slug = "element-chatishmazligi-xaritede",
                        Excerpt = "Dörd elementdən birinin az olması, çox olması qədər vacib məlumat verir.",
                        ExcerptEn = "Having too little of one element says just as much as having too much.",
                        ExcerptRu = "Нехватка одного элемента говорит не меньше, чем его избыток.",
                        Body = "Natal xəritədəki 10 əsas planet dörd element arasında (Od, Torpaq, Hava, Su) bölüşdürülür. Bəzən bir xəritədə bir element çox üstündür, bəzənsə demək olar ki, heç yoxdur — bu, 'element çatışmazlığı' adlanır.\n\nSu çatışmazlığı olan insan emosiyaları tanımaqda, onları ifadə etməkdə çətinlik çəkə bilər, lakin bu heç nə hiss etməmək demək deyil — sadəcə bu dili təbii öyrənməyib. Torpaq çatışmazlığı isə praktik məsələlərdə (pul, gündəlik rutina) çətinlik yarada bilər, baxmayaraq ki şəxs başqa sahələrdə çox bacarıqlı ola bilər.\n\nÇatışmazlıq bir çatışmazlıq kimi görünsə də, əslində bu, şüurlu inkişaf üçün bir istiqamətdir. Çatışan elementin keyfiyyətlərini bilərəkdən həyata gətirmək — məsələn Su çatışmazlığı olan üçün bilərəkdən emosional əlaqələrə vaxt ayırmaq — balansı bərpa etməyə kömək edir.",
                        BodyEn = "The 10 main planets in a natal chart are distributed across four elements — Fire, Earth, Air, and Water. Sometimes one element dominates a chart, and sometimes it's almost entirely missing — this is called an elemental imbalance.\n\nSomeone with little Water might struggle to recognize or express emotion, though that doesn't mean they feel nothing — they simply never learned that language naturally. A lack of Earth can create difficulty with practical matters like money or daily routine, even if the person is quite skilled in other areas.\n\nWhile it looks like a deficiency, it's really a direction for conscious growth. Deliberately bringing in the missing element's qualities — for example, someone low on Water making a point of setting aside time for emotional connection — helps restore the balance.",
                        BodyRu = "10 основных планет натальной карты распределены между четырьмя элементами — Огонь, Земля, Воздух и Вода. Иногда один элемент сильно преобладает в карте, а иногда его почти совсем нет — это называется дисбалансом элементов.\n\nЧеловеку с малым количеством Воды может быть трудно распознавать и выражать эмоции, но это не значит, что он ничего не чувствует — он просто не научился этому языку естественным образом. Недостаток Земли может создавать сложности с практическими вопросами — деньгами, повседневной рутиной, — даже если человек весьма способен в других сферах.\n\nХотя это выглядит как нехватка, на самом деле это направление для осознанного роста. Намеренное развитие качеств недостающего элемента — например, человек с малой Водой, который сознательно уделяет время эмоциональной близости, — помогает восстановить баланс.",
                        Tag = "Bürclər",
                        Published = true,
                        PublishedAt = now10.AddDays(-7),
                        Views = 350
                    },
                    new()
                    {
                        Title = "Fortuna nöqtəsi: xəritədəki gizli şans",
                        TitleEn = "The Part of Fortune: a hidden sweet spot in the chart",
                        TitleRu = "Точка Фортуны: скрытое удачное место в карте",
                        Slug = "fortuna-noqtesi-gizli-shans",
                        Excerpt = "Fortuna nöqtəsi, təbii axının və asanlığın harada tapıla biləcəyini göstərir.",
                        ExcerptEn = "The Part of Fortune points to where natural flow and ease are easiest to find.",
                        ExcerptRu = "Точка Фортуны показывает, где легче найти естественный поток и лёгкость.",
                        Body = "Fortuna nöqtəsi (Part of Fortune) real planet deyil, Günəş, Ay və Yüksələnin mövqelərindən riyazi yolla hesablanan bir nöqtədir. Ənənəvi astrologiyada o, 'şans' və ya təbii uğurun harada daha asan tapılacağını göstərən simvolik nöqtə sayılır.\n\nFortuna nöqtəsinin olduğu ev, insanın az səylə, daha təbii şəkildə müsbət nəticələr əldə etdiyi sahəni göstərə bilər. Məsələn, 5-ci evdə Fortuna yaradıcılıq və özünüifadədə, 10-cu evdə isə karyerada təbii bir 'axın' hissi gətirə bilər.\n\nFortuna nöqtəsi 'hər şey asan olacaq' demək deyil — daha doğrusu, o, harada daha az müqavimətlə irəli getmək mümkün olduğunu göstərən bir işarədir. Çətin dövrlərdə bu sahəyə qayıtmaq, bəzən itirilən tarazlığı tapmağa kömək edir.",
                        BodyEn = "The Part of Fortune isn't a real planet — it's a mathematically calculated point derived from the positions of the Sun, Moon, and Ascendant. In traditional astrology, it's treated as a symbolic marker of where luck, or natural success, tends to be easier to find.\n\nThe house that holds the Part of Fortune can point to an area where a person achieves positive outcomes with less effort, more naturally. In the 5th house, for example, it might bring a sense of flow in creativity and self-expression; in the 10th house, in career.\n\nThe Part of Fortune doesn't mean 'everything will be easy' — it's more a signpost for where less resistance tends to be found along the way. Returning to this area during harder periods can sometimes help restore a sense of balance that's been lost.",
                        BodyRu = "Точка Фортуны — не настоящая планета, а математически рассчитанная точка, выведенная из положений Солнца, Луны и Асцендента. В традиционной астрологии она считается символическим указателем того, где удачу или естественный успех найти легче.\n\nДом, в котором находится Точка Фортуны, может показывать сферу, где человек достигает положительных результатов с меньшими усилиями, более естественно. В 5-м доме, например, это может давать ощущение потока в творчестве и самовыражении, а в 10-м — в карьере.\n\nТочка Фортуны не означает, что 'всё будет легко' — это скорее указатель того, где по пути встречается меньше сопротивления. Возвращение к этой сфере в трудные периоды иногда помогает восстановить утраченное равновесие.",
                        Tag = "Xəritə",
                        Published = true,
                        PublishedAt = now10.AddDays(-8),
                        Views = 390
                    },
                    new()
                    {
                        Title = "Kompozit xəritə və sinastriya: fərq nədir?",
                        TitleEn = "Composite chart vs. synastry: what's the difference?",
                        TitleRu = "Композитная карта и синастрия: в чём разница?",
                        Slug = "kompozit-xerite-sinastriya-ferqi",
                        Excerpt = "Sinastriya iki fərdi müqayisə edir, kompozit isə münasibəti öz başına bir 'varlıq' kimi göstərir.",
                        ExcerptEn = "Synastry compares two individuals; a composite chart shows the relationship itself as its own entity.",
                        ExcerptRu = "Синастрия сравнивает двух людей, а композитная карта показывает сами отношения как отдельную сущность.",
                        Body = "Sinastriya və kompozit xəritə, cütlərin uyğunluğunu araştırmaq üçün istifadə olunan iki fərqli, lakin tamamlayıcı üsuldur. Sinastriya iki natal xəritəni üst-üstə qoyaraq, hər bir tərəfin planetlərinin digərinə necə təsir etdiyini göstərir.\n\nKompozit xəritə isə fərqli bir yanaşmadır: iki xəritənin orta nöqtələri hesablanaraq, münasibətin özünə aid, üçüncü, müstəqil bir xəritə yaradılır. Bu xəritə, 'mən' və 'sən' deyil, məhz 'biz' — münasibətin özünün — xarakterini, məqsədini və çətinliklərini göstərir.\n\nSinastriya 'biz bir-birimizə necə təsir edirik' sualına, kompozit isə 'bu münasibət öz başına nədir, hara gedir' sualına cavab axtarır. Hər iki üsulu birlikdə istifadə etmək, münasibətin daha dolğun mənzərəsini verir.",
                        BodyEn = "Synastry and the composite chart are two different, complementary techniques used to study compatibility between two people. Synastry overlays two natal charts to show how each person's planets affect the other.\n\nA composite chart takes a different approach: the midpoints between the two charts are calculated to create a third, independent chart that belongs to the relationship itself. This chart shows the character, purpose, and challenges of 'we' — the relationship itself — rather than 'me' or 'you.'\n\nSynastry asks 'how do we affect each other,' while the composite chart asks 'what is this relationship on its own, and where is it heading.' Using both techniques together gives a fuller picture of the relationship.",
                        BodyRu = "Синастрия и композитная карта — два разных, но взаимодополняющих метода изучения совместимости между двумя людьми. Синастрия накладывает две натальные карты друг на друга, показывая, как планеты каждого влияют на другого.\n\nКомпозитная карта использует другой подход: вычисляются средние точки между двумя картами, и создаётся третья, самостоятельная карта, принадлежащая самим отношениям. Эта карта показывает характер, цель и трудности именно 'нас' — самих отношений, — а не 'меня' или 'тебя'.\n\nСинастрия отвечает на вопрос 'как мы влияем друг на друга', а композитная карта — на вопрос 'что представляют собой эти отношения сами по себе и куда они движутся'. Использование обоих методов вместе даёт более полную картину отношений.",
                        Tag = "Sinastriya",
                        Published = true,
                        PublishedAt = now10.AddDays(-9),
                        Views = 430
                    },
                    new()
                    {
                        Title = "Saturn evlərdə: intizam hansı sahəyə gəlir?",
                        TitleEn = "Saturn by house: where discipline is being asked for",
                        TitleRu = "Сатурн по домам: где требуется дисциплина",
                        Slug = "saturn-evlerde-intizam",
                        Excerpt = "Saturnun tranzit etdiyi ev, məhz o sahədə struktur qurmağı tələb edir.",
                        ExcerptEn = "Whichever house Saturn is transiting calls for building real structure in that area.",
                        ExcerptRu = "В каком бы доме ни проходил транзит Сатурна, он требует выстроить там настоящую структуру.",
                        Body = "Saturn bir bürcdə təxminən 2.5 il qalır və xəritənin hər evini öz növbəsi ilə ziyarət edir. Hansı evdən keçirsə, o sahədə məsuliyyət, struktur və çox zaman çətin, lakin faydalı dərslər gətirir.\n\nSaturn 7-ci evdən keçərkən münasibətlərdə ciddiləşmə və öhdəlik mövzuları, 6-cı evdən keçərkən iş rejimi və sağlamlıqla bağlı intizam, 4-cü evdən keçərkən isə ailə və ev məsələlərində məsuliyyət önə çıxa bilər.\n\nSaturn tranziti çox vaxt məhdudiyyət kimi hiss olunur, amma əslində o, uzunmüddətli nəticə üçün möhkəm təməl qurmağa çağırır. Bu dövrdə qısamüddətli asanlıq axtarmaq əvəzinə, səbirlə struktur qurmaq, gələcəkdə daha sabit nəticələr verir.",
                        BodyEn = "Saturn spends about 2.5 years in each sign, visiting each house of the chart in turn. Wherever it's transiting, it tends to bring responsibility, structure, and often difficult but ultimately useful lessons.\n\nSaturn moving through the 7th house can bring themes of commitment and seriousness in relationships; through the 6th house, discipline around work routine and health; through the 4th house, responsibility around family and home matters.\n\nA Saturn transit often feels restrictive, but it's really an invitation to build a solid foundation for the long run. Choosing patient structure-building over short-term ease during this period tends to pay off with more stable results later.",
                        BodyRu = "Сатурн проводит около 2,5 лет в каждом знаке, по очереди проходя через каждый дом карты. Где бы он ни находился транзитом, он, как правило, приносит ответственность, структуру и часто трудные, но в итоге полезные уроки.\n\nСатурн в 7-м доме может приносить темы серьёзности и обязательств в отношениях, в 6-м — дисциплину в рабочем режиме и здоровье, в 4-м — ответственность в семейных и домашних делах.\n\nТранзит Сатурна часто ощущается как ограничение, но на деле это приглашение выстроить прочный фундамент на долгий срок. Выбор терпеливого строительства структуры вместо поиска краткосрочной лёгкости в этот период обычно окупается более стабильными результатами в будущем.",
                        Tag = "Tranzit",
                        Published = true,
                        PublishedAt = now10.AddDays(-10),
                        Views = 470
                    },
                    new()
                    {
                        Title = "Dolça erası nədir? Presessiya haqqında sadə izah",
                        TitleEn = "What is the Age of Aquarius? A simple look at precession",
                        TitleRu = "Что такое эра Водолея? Простое объяснение прецессии",
                        Slug = "dolca-esri-presessiya",
                        Excerpt = "'Dolça erası' ifadəsi minlərlə illik astronomik bir dövrəyə işarə edir.",
                        ExcerptEn = "'The Age of Aquarius' points to a slow astronomical cycle spanning thousands of years.",
                        ExcerptRu = "'Эра Водолея' указывает на медленный астрономический цикл, растянутый на тысячелетия.",
                        Body = "Yerin fırlanma oxu çox yavaş şəkildə, təxminən 26 000 illik dövrlə öz ətrafında hərəkət edir — bu hadisə 'presessiya' adlanır. Bu yavaş hərəkət nəticəsində, tropik bahar bərabərliyi nöqtəsi minlərlə il ərzində zodiak bürcləri arasında 'sürüşür'.\n\nHər 'era' təxminən 2150 il davam edir və bahar bərabərliyi nöqtəsinin hansı bürcdə yerləşdiyinə əsaslanır. Son bir neçə min il 'Balıqlar erası' sayılır, və astroloqlar arasında hazırda 'Dolça erasına' keçid dövründə olduğumuza dair fikirlər mövcuddur — baxmayaraq ki dəqiq tarix mövzusunda yekdil razılıq yoxdur.\n\nVacib qeyd: bu, gündəlik istifadə etdiyimiz fərdi Günəş bürcündən tamam fərqli bir mövzudur — fərdi bürc hesablamaları bu minilliklik dövrədən təsirlənmir. 'Era' anlayışı daha geniş, kollektiv-mədəni dəyişiklikləri simvolik şəkildə izah etmək üçün istifadə olunur.",
                        BodyEn = "Earth's rotational axis moves very slowly in a roughly 26,000-year cycle — a phenomenon called precession. As a result of this slow drift, the tropical spring equinox point gradually 'slides' through the zodiac signs over thousands of years.\n\nEach 'age' lasts roughly 2,150 years and is defined by which sign the spring equinox point falls in. The last couple thousand years are considered the 'Age of Pisces,' and there's ongoing discussion among astrologers about whether we're now moving into the 'Age of Aquarius' — though there's no universal agreement on the exact date.\n\nAn important note: this is an entirely different topic from the individual Sun sign used in everyday astrology — personal sign calculations aren't affected by this millennia-long cycle. The idea of an 'age' is used, instead, as a symbolic way to talk about broader, collective cultural shifts.",
                        BodyRu = "Ось вращения Земли очень медленно смещается по циклу продолжительностью около 26 000 лет — это явление называется прецессией. В результате этого медленного смещения точка весеннего равноденствия в тропическом зодиаке постепенно 'скользит' по знакам зодиака на протяжении тысячелетий.\n\nКаждая 'эра' длится около 2150 лет и определяется тем, в каком знаке находится точка весеннего равноденствия. Последние пару тысяч лет считаются 'эрой Рыб', и среди астрологов продолжается дискуссия о том, вступаем ли мы сейчас в 'эру Водолея' — хотя единого согласия по точной дате нет.\n\nВажное замечание: это совершенно другая тема, не связанная с индивидуальным знаком Солнца, которым мы пользуемся в повседневной астрологии — расчёт личного знака не зависит от этого тысячелетнего цикла. Понятие 'эры' используется скорее как символический способ говорить о более широких, коллективно-культурных переменах.",
                        Tag = "Fəlsəfə",
                        Published = true,
                        PublishedAt = now10.AddDays(-11),
                        Views = 510
                    },
                };

                var existingArticleSlugs = (await context.Articles.Select(a => a.Slug).ToListAsync()).ToHashSet();
                var articlesToAdd = newArticles.Where(a => !existingArticleSlugs.Contains(a.Slug)).ToList();
                if (articlesToAdd.Count > 0)
                {
                    await context.Articles.AddRangeAsync(articlesToAdd);
                    await context.SaveChangesAsync();
                }
            }

            // 11. Seed-expansion: 6 yeni forum mövzusu (3 ingilis, 3 rus mənşəli — "sual-cavab"
            // kateqoriyası) + 8 cavab, frontend migrasiyasından portlanıb. Supabase-də user_id
            // NULL idi; backend-də FK (ForumTopic.UserId/ForumReply.UserId) REQUIRED olduğu üçün
            // superAdmin.Id-yə bağlanır (bax bölmə 7-dəki eyni fiks). Additiv və idempotent: sabit
            // Guid-lər üzrə yoxlanılıb yalnız yoxdursa əlavə olunur.
            {
                var now11 = DateTime.UtcNow;
                var existingTopicIds = (await context.ForumTopics.Select(t => t.Id).ToListAsync()).ToHashSet();

                var newTopics = new List<ForumTopic>
                {
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111101"),
                        UserId = superAdmin.Id,
                        AuthorName = "Sarah Mitchell",
                        Category = "sual-cavab",
                        Title = "What's the actual difference between my Sun sign and Rising sign?",
                        TitleEn = "What's the actual difference between my Sun sign and Rising sign?",
                        TitleRu = null,
                        Body = "I keep reading that my Sun sign and my Rising sign 'aren't the same thing,' but I'm struggling to actually feel the difference in real life. Can someone explain it in plain terms, maybe with an example?",
                        BodyEn = "I keep reading that my Sun sign and my Rising sign 'aren't the same thing,' but I'm struggling to actually feel the difference in real life. Can someone explain it in plain terms, maybe with an example?",
                        BodyRu = null,
                        CreatedAt = now11.AddDays(-6)
                    },
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111102"),
                        UserId = superAdmin.Id,
                        AuthorName = "Emily Novak",
                        Category = "sual-cavab",
                        Title = "Is it normal to feel nothing during a big transit everyone's talking about?",
                        TitleEn = "Is it normal to feel nothing during a big transit everyone's talking about?",
                        TitleRu = null,
                        Body = "There's apparently a big transit happening right now that a lot of people online are saying is intense, but I genuinely don't feel any different. Does that mean something's wrong, or is it normal for a transit to just... not land for some people?",
                        BodyEn = "There's apparently a big transit happening right now that a lot of people online are saying is intense, but I genuinely don't feel any different. Does that mean something's wrong, or is it normal for a transit to just... not land for some people?",
                        BodyRu = null,
                        CreatedAt = now11.AddDays(-5)
                    },
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111103"),
                        UserId = superAdmin.Id,
                        AuthorName = "Oliver Bennett",
                        Category = "sual-cavab",
                        Title = "How do you deal with having your Moon and Sun in conflicting elements?",
                        TitleEn = "How do you deal with having your Moon and Sun in conflicting elements?",
                        TitleRu = null,
                        Body = "My Sun is in a fire sign and my Moon is in a water sign, and sometimes it genuinely feels like two different people arguing inside my head — one wants to charge ahead, the other wants to retreat and feel things out. Anyone else deal with this combination?",
                        BodyEn = "My Sun is in a fire sign and my Moon is in a water sign, and sometimes it genuinely feels like two different people arguing inside my head — one wants to charge ahead, the other wants to retreat and feel things out. Anyone else deal with this combination?",
                        BodyRu = null,
                        CreatedAt = now11.AddDays(-4)
                    },
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111104"),
                        UserId = superAdmin.Id,
                        AuthorName = "Анна Соколова",
                        Category = "sual-cavab",
                        Title = "Что на самом деле значит, если в карте вообще нет воды?",
                        TitleEn = null,
                        TitleRu = "Что на самом деле значит, если в карте вообще нет воды?",
                        Body = "Посчитала свою натальную карту и обнаружила, что у меня нет ни одной планеты в водных знаках. Подруга сказала, что это значит, что я 'холодная' в плане эмоций, но мне это не кажется правдой про себя. Это действительно так работает?",
                        BodyEn = null,
                        BodyRu = "Посчитала свою натальную карту и обнаружила, что у меня нет ни одной планеты в водных знаках. Подруга сказала, что это значит, что я 'холодная' в плане эмоций, но мне это не кажется правдой про себя. Это действительно так работает?",
                        CreatedAt = now11.AddDays(-3)
                    },
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111105"),
                        UserId = superAdmin.Id,
                        AuthorName = "Мария Петрова",
                        Category = "sual-cavab",
                        Title = "Как понять, где у меня восьмой дом, если я не разбираюсь в датах рождения?",
                        TitleEn = null,
                        TitleRu = "Как понять, где у меня восьмой дом, если я не разбираюсь в датах рождения?",
                        Body = "Хочу разобраться в своей карте, но постоянно путаюсь, когда дело доходит до домов. Кто-то может просто по-человечески объяснить, как вообще найти, в каком доме что находится, без сложной терминологии?",
                        BodyEn = null,
                        BodyRu = "Хочу разобраться в своей карте, но постоянно путаюсь, когда дело доходит до домов. Кто-то может просто по-человечески объяснить, как вообще найти, в каком доме что находится, без сложной терминологии?",
                        CreatedAt = now11.AddDays(-2)
                    },
                    new()
                    {
                        Id = Guid.Parse("11111111-1111-4111-8111-111111111106"),
                        UserId = superAdmin.Id,
                        AuthorName = "Виктор Соколов",
                        Category = "sual-cavab",
                        Title = "У кого-то было такое: транзит обещал одно, а случилось совсем другое?",
                        TitleEn = null,
                        TitleRu = "У кого-то было такое: транзит обещал одно, а случилось совсем другое?",
                        Body = "Читал прогноз про сильный транзит в этом месяце, который должен был принести 'прорыв в карьере'. В итоге ничего такого не произошло, зато случилось кое-что совсем в личной жизни. Это нормально, что транзиты иногда 'ошибаются' по теме?",
                        BodyEn = null,
                        BodyRu = "Читал прогноз про сильный транзит в этом месяце, который должен был принести 'прорыв в карьере'. В итоге ничего такого не произошло, зато случилось кое-что совсем в личной жизни. Это нормально, что транзиты иногда 'ошибаются' по теме?",
                        CreatedAt = now11.AddDays(-1)
                    },
                };

                var topicsToAdd = newTopics.Where(t => !existingTopicIds.Contains(t.Id)).ToList();
                if (topicsToAdd.Count > 0)
                {
                    await context.ForumTopics.AddRangeAsync(topicsToAdd);
                    await context.SaveChangesAsync();
                }

                var existingReplyIds = (await context.ForumReplies.Select(r => r.Id).ToListAsync()).ToHashSet();

                var newReplies = new List<ForumReply>
                {
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222201"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111101"),
                        UserId = superAdmin.Id,
                        AuthorName = "James Cooper",
                        Body = "Think of it this way: your Sun sign is who you are underneath everything, your core drive. Your Rising sign is more like the outfit you walk into a room wearing — the first impression people get before they know you well. I'm a Cancer Sun with a Leo Rising, so people assume I'm bold and outgoing at first, but underneath I'm actually pretty sensitive and private.",
                        BodyEn = "Think of it this way: your Sun sign is who you are underneath everything, your core drive. Your Rising sign is more like the outfit you walk into a room wearing — the first impression people get before they know you well. I'm a Cancer Sun with a Leo Rising, so people assume I'm bold and outgoing at first, but underneath I'm actually pretty sensitive and private.",
                        BodyRu = null,
                        CreatedAt = now11.AddHours(-118)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222202"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111102"),
                        UserId = superAdmin.Id,
                        AuthorName = "James Cooper",
                        Body = "Totally normal. A transit only really activates something if it's hitting a sensitive point in your own chart — a planet, an angle, something specific. If it's not touching anything of yours directly, it can pass by pretty quietly.",
                        BodyEn = "Totally normal. A transit only really activates something if it's hitting a sensitive point in your own chart — a planet, an angle, something specific. If it's not touching anything of yours directly, it can pass by pretty quietly.",
                        BodyRu = null,
                        CreatedAt = now11.AddHours(-93)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222203"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111102"),
                        UserId = superAdmin.Id,
                        AuthorName = "Sarah Mitchell",
                        Body = "Agreed — I'd rather feel nothing than feel everything everyone online is describing, honestly. Not every placement reacts the same way.",
                        BodyEn = "Agreed — I'd rather feel nothing than feel everything everyone online is describing, honestly. Not every placement reacts the same way.",
                        BodyRu = null,
                        CreatedAt = now11.AddHours(-91)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222204"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111103"),
                        UserId = superAdmin.Id,
                        AuthorName = "Emily Novak",
                        Body = "Yes, and honestly it took me years to stop seeing it as a conflict and start seeing it as a conversation. The fire part gets to act, the water part gets to feel it through afterward. It doesn't have to be either/or all the time.",
                        BodyEn = "Yes, and honestly it took me years to stop seeing it as a conflict and start seeing it as a conversation. The fire part gets to act, the water part gets to feel it through afterward. It doesn't have to be either/or all the time.",
                        BodyRu = null,
                        CreatedAt = now11.AddHours(-68)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222205"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111104"),
                        UserId = superAdmin.Id,
                        AuthorName = "Дмитрий Волков",
                        Body = "Нет, это не значит 'холодная' — это скорее значит, что эмоциональный язык не является для вас естественным, выученным с детства способом реагировать. Вы вполне можете глубоко чувствовать, просто выражаете и обрабатываете это иначе, часто через действие или мысль, а не через прямое эмоциональное выражение.",
                        BodyEn = null,
                        BodyRu = "Нет, это не значит 'холодная' — это скорее значит, что эмоциональный язык не является для вас естественным, выученным с детства способом реагировать. Вы вполне можете глубоко чувствовать, просто выражаете и обрабатываете это иначе, часто через действие или мысль, а не через прямое эмоциональное выражение.",
                        CreatedAt = now11.AddHours(-45)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222206"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111105"),
                        UserId = superAdmin.Id,
                        AuthorName = "Дмитрий Волков",
                        Body = "Самый простой способ — посмотреть на расчёт своей карты (на этом сайте это делается автоматически по дате, времени и месту рождения) и найти раздел с 12 секторами, расположенными по кругу. Каждый сектор пронумерован от 1 до 12, начиная от левой горизонтальной линии и идя против часовой стрелки. Там, где показаны планеты внутри этих секторов — это и есть дома. Точное время рождения важно, потому что от него зависят границы секторов.",
                        BodyEn = null,
                        BodyRu = "Самый простой способ — посмотреть на расчёт своей карты (на этом сайте это делается автоматически по дате, времени и месту рождения) и найти раздел с 12 секторами, расположенными по кругу. Каждый сектор пронумерован от 1 до 12, начиная от левой горизонтальной линии и идя против часовой стрелки. Там, где показаны планеты внутри этих секторов — это и есть дома. Точное время рождения важно, потому что от него зависят границы секторов.",
                        CreatedAt = now11.AddHours(-22)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222207"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111106"),
                        UserId = superAdmin.Id,
                        AuthorName = "Мария Петрова",
                        Body = "У меня было похожее. Думаю, дело в том, что общие прогнозы описывают тему очень широко, а в вашей личной карте этот транзит может активировать совсем другой дом или планету, чем у большинства читателей.",
                        BodyEn = null,
                        BodyRu = "У меня было похожее. Думаю, дело в том, что общие прогнозы описывают тему очень широко, а в вашей личной карте этот транзит может активировать совсем другой дом или планету, чем у большинства читателей.",
                        CreatedAt = now11.AddHours(-10)
                    },
                    new()
                    {
                        Id = Guid.Parse("22222222-2222-4222-8222-222222222208"),
                        TopicId = Guid.Parse("11111111-1111-4111-8111-111111111106"),
                        UserId = superAdmin.Id,
                        AuthorName = "Анна Соколова",
                        Body = "Плюс один транзит редко действует изолированно — в это же время могли происходить и другие, менее заметные движения, которые в итоге и определили, где именно всё проявится.",
                        BodyEn = null,
                        BodyRu = "Плюс один транзит редко действует изолированно — в это же время могли происходить и другие, менее заметные движения, которые в итоге и определили, где именно всё проявится.",
                        CreatedAt = now11.AddHours(-6)
                    },
                };

                var repliesToAdd = newReplies.Where(r => !existingReplyIds.Contains(r.Id)).ToList();
                if (repliesToAdd.Count > 0)
                {
                    await context.ForumReplies.AddRangeAsync(repliesToAdd);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
