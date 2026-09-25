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
            // 1. Ensure Database Created
            await context.Database.EnsureCreatedAsync();

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
            if (!await context.ForumTopics.AnyAsync())
            {
                var now = DateTime.UtcNow;

                var t1 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Aynur Məmmədova", Category = "ümumi", Title = "İlk dəfə buradayam, salam hamıya!", Body = "Salam! Bu yaxınlarda natal xəritəmi hesabladım və astrologiyaya maraq göstərməyə başladım. Bu forumda təcrübəli insanlar görürəm, ümid edirəm burda çox şey öyrənəcəm 🙂", CreatedAt = now.AddDays(-21) };
                var t2 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Elvin Qasımov", Category = "ümumi", Title = "Astrologiyaya necə başladınız?", Body = "Maraqlıdır, hamınız astrologiyaya necə maraq göstərməyə başlamısınız? Mənimki bir dostumun natal xəritəmi oxumasından sonra oldu, çox təəccübləndim doğruluğuna.", CreatedAt = now.AddDays(-19) };
                var t3 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Günay Səfərova", Category = "tranzitlər", Title = "Bu ay Merkuri retroqraddadır, kimin başına iş gəldi?", Body = "Mənim telefonum sındı, iş yerində sənəd itdi, bir də köhnə tanışım mesaj yazdı — klassik Merkuri retroqrad əlamətləri deyilmi? Sizdə necədir bu dövr?", CreatedAt = now.AddDays(-17) };
                var t4 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Tural Hüseynov", Category = "tranzitlər", Title = "Saturn Balıqlarda — kim hiss edir təsirini?", Body = "Saturn Balıqlar bürcünə keçəli hədsiz yorğunluq hiss edirəm, xüsusən yaradıcı işlərdə. Balıqlar/Qız yüksələni olanlar necə hiss edir özlərini?", CreatedAt = now.AddDays(-15) };
                var t5 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Nərmin Abbasova", Category = "tranzitlər", Title = "Yupiter keçidi karyeramda dəyişiklik gətirdi", Body = "Yupiter 10-cu evimə keçəndən sonra tamam gözlənilməz bir iş təklifi aldım. Sizdə də karyerada belə \"açılma\" hiss olub bu il?", CreatedAt = now.AddDays(-13) };
                var t6 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Kamran Əliyev", Category = "natal xəritə", Title = "Xəritəmdə 8-ci ev boşdur, bu normaldırmı?", Body = "Xəritəmi yoxlayanda gördüm ki 8-ci evdə heç bir planet yoxdur. Narahat olmalıyammı, yoxsa boş evlər tamam normaldır?", CreatedAt = now.AddDays(-12) };
                var t7 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Səbinə Rzayeva", Category = "natal xəritə", Title = "Yüksələnim Əkizlər amma özümü heç oxşatmıram", Body = "Yüksələn bürcüm Əkizlərdir amma özümü daha çox sakit, introvert insan kimi tanıyıram. Bu normal ola bilərmi, yoxsa doğum saatımda səhvlik var?", CreatedAt = now.AddDays(-10) };
                var t8 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Orxan Məmmədli", Category = "natal xəritə", Title = "Ay-Plüton kvadratı olan var? Necə idarə edirsiniz?", Body = "Natal xəritəmdə Ay-Plüton kvadratı var və çox intensiv emosiyalar yaşayıram bəzən. Bu aspekti olan varmı, necə balanslaşdırırsınız?", CreatedAt = now.AddDays(-9) };
                var t9 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Leyla Vəliyeva", Category = "natal xəritə", Title = "Xəritəmi necə oxumaq lazımdır, kömək edin", Body = "Xəritə bölməsindən hesabladım amma hər şey mənə çox mürəkkəb görünür. Haradan başlamaq lazımdır ki, öz xəritəmi başa düşüm?", CreatedAt = now.AddDays(-8) };
                var t10 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Aygün Nəbiyeva", Category = "cütlük xəritəsi", Title = "Partnyorumla Günəş-Ay kvadratımız var, çətindir", Body = "Uyğunluq bölməsində yoxladım, mənim Günəşimlə onun Ayı arasında kvadrat var. İlk vaxtlar çox cəlbedici idi amma indi tez-tez anlaşılmazlıq yaşayırıq. Belə təcrübəsi olan var?", CreatedAt = now.AddDays(-7) };
                var t11 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Rəşad Qurbanov", Category = "cütlük xəritəsi", Title = "Sinastriyada Venera-Mars trigonu nə deməkdir?", Body = "Sevgilimlə xəritələrimizi müqayisə etdim, Venera-Mars arasında trigon çıxdı. Bu praktikada özünü necə göstərir, kimin təcrübəsi var?", CreatedAt = now.AddDays(-6) };
                var t12 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Şəbnəm Hacıyeva", Category = "cütlük xəritəsi", Title = "Balıqlar və Oğlaq uyğunluğu təcrübəniz varmı?", Body = "Mən Balıqlar Günəşiyəm, sevgilim isə Oğlaq. Çox fərqli görünürük amma çox yaxşı tamamlayırıq bir-birimizi. Bu kombinasiyanı yaşayan var burada?", CreatedAt = now.AddDays(-5) };
                var t13 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Elnur Bağırov", Category = "sual-cavab", Title = "Doğum saatımı bilmirəm, nə etməliyəm?", Body = "Doğum şəhadətnaməmdə saat yazılmayıb, valideynlərim də dəqiq xatırlamır. Xəritəmi necə hesablaya bilərəm, yoxsa mümkün deyil?", CreatedAt = now.AddDays(-4) };
                var t14 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Nigar Cəfərova", Category = "sual-cavab", Title = "Sidereal və tropik zodiak fərqi nədir?", Body = "Bəzi saytlarda mənim bürcüm fərqli çıxır. Sonra öyrəndim ki sidereal və tropik sistem var. Bu saytda hansı sistem işlədilir, fərq nədir?", CreatedAt = now.AddDays(-3) };
                var t15 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Fərid Novruzov", Category = "sual-cavab", Title = "Şimal Node hansı evdədirsə nəyə işarədir?", Body = "Mənim Şimal Node-um 7-ci evdədir. Bu münasibətlərlə bağlı bir mesaj ola bilərmi? Fikirlərinizi bilmək istərdim.", CreatedAt = now.AddDays(-2) };
                var t16 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Aynur Məmmədova", Category = "ümumi", Title = "Bu forumda kim natal xəritəsini paylaşmaq istəyir?", Body = "Fikirləşdim bəlkə bir mövzu açaq, hərə öz Günəş-Ay-Yüksələn kombinasiyasını yazsın, maraqlı ola bilər müqayisə etmək 🙂 Mən başlayıram: Qoç-Əqrəb-Şir.", CreatedAt = now.AddDays(-2) };
                var t17 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Günay Səfərova", Category = "tranzitlər", Title = "Növbəti dolunay hansı bürcdə olacaq?", Body = "Bilən var bu ayın dolunayı hansı bürcdə baş verəcək? Günün bələdçisi bölməsinə baxdım amma dəqiq tarixi tapa bilmədim.", CreatedAt = now.AddDays(-1) };
                var t18 = new ForumTopic { Id = Guid.NewGuid(), AuthorName = "Kamran Əliyev", Category = "sual-cavab", Title = "Uyğunluq balı aşağıdır, deməli uyğun deyilikmi?", Body = "Partnyorumla uyğunluq balımız 45% çıxdı. Bu o deməkdirmi ki bizə uyğun deyilik, yoxsa bal tək amil deyil?", CreatedAt = now.AddHours(-12) };

                var topics = new List<ForumTopic> { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16, t17, t18 };
                await context.ForumTopics.AddRangeAsync(topics);

                var replies = new List<ForumReply>
            {
                new() { TopicId = t3.Id, AuthorName = "Tural Hüseynov", Body = "Mənim də noutbukum xarab oldu bu dövrdə 😅 Ən yaxşısı vacib sənədləri iki dəfə yoxlamaqdır bu müddətdə.", CreatedAt = now.AddDays(-16) },
                new() { TopicId = t3.Id, AuthorName = "Nərmin Abbasova", Body = "Köhnə iş yoldaşım mesaj yazdı mənə də, tam Merkuri retroqrad effekti. Amma yaxşı söhbətləşdik, deməli hər şey pis olmur bu dövrdə.", CreatedAt = now.AddDays(-15) },
                new() { TopicId = t3.Id, AuthorName = "Leyla Vəliyeva", Body = "Mən bu dövrdə yeni telefon almaqdan çəkindim, məsləhətlərə əsasən düzgün etmişəm deyəsən 🙂", CreatedAt = now.AddDays(-15) },
                new() { TopicId = t4.Id, AuthorName = "Aygün Nəbiyeva", Body = "Mən Qız yüksələniyəm, doğrudan da son aylarda özümü çox yorğun hiss edirəm. Yuxu rejiminə daha çox fikir verməyə başladım.", CreatedAt = now.AddDays(-14) },
                new() { TopicId = t4.Id, AuthorName = "Orxan Məmmədli", Body = "Bəlkə də bu, sadəcə daha çox istirahət etmək lazım olduğuna işarədir. Saturn həmişə yavaşlamağı öyrədir.", CreatedAt = now.AddDays(-14) },
                new() { TopicId = t6.Id, AuthorName = "Rəşad Qurbanov", Body = "Tamamilə normaldır, boş evlər sadəcə o sahədə daha az \"hadisə\" olduğunu göstərir, problem demək deyil.", CreatedAt = now.AddDays(-11) },
                new() { TopicId = t6.Id, AuthorName = "Nigar Cəfərova", Body = "Dəqiq, əksinə boş evlər bəzən daha sabit sahələr kimi də şərh olunur. Evin hökmdarına baxmaq daha vacibdir.", CreatedAt = now.AddDays(-11) },
                new() { TopicId = t7.Id, AuthorName = "Fərid Novruzov", Body = "Yüksələn həmişə açıq-aşkar görünmür, xüsusən Günəş və Ay bürcün güclü fərqli enerjiyə sahibdirsə. Doğum saatı bir neçə dəqiqə səhv olsa belə bürc dəyişə bilər, dəqiqliyi yoxlamaq faydalı olar.", CreatedAt = now.AddDays(-9) },
                new() { TopicId = t7.Id, AuthorName = "Şəbnəm Hacıyeva", Body = "Mənim də oxşar vəziyyətim var, amma yaxın dostlarım deyir ki əslində insanlarla tanış olanda Əkizlər enerjisi üzə çıxır, sadəcə tanımadığın insanlarla yox.", CreatedAt = now.AddDays(-9) },
                new() { TopicId = t8.Id, AuthorName = "Elnur Bağırov", Body = "Mənim də var bu aspekt. Terapiya və jurnal yazmaq çox köməkçi oldu emosiyaları tanımaqda.", CreatedAt = now.AddDays(-8) },
                new() { TopicId = t9.Id, AuthorName = "Aynur Məmmədova", Body = "Ən yaxşısı Günəş-Ay-Yüksələn üçlüyündən başlamaqdır, sonra planetlərin evlərinə baxarsan. Addım-addım gedəndə daha asan olur.", CreatedAt = now.AddDays(-7) },
                new() { TopicId = t9.Id, AuthorName = "Elvin Qasımov", Body = "Mən də elə başlamışdım, indi aspektlərə baxıram. Vaxt aparır amma maraqlı prosesdir.", CreatedAt = now.AddDays(-7) },
                new() { TopicId = t10.Id, AuthorName = "Şəbnəm Hacıyeva", Body = "Bizdə də oxşar aspekt var. Açıq ünsiyyət və bir-birinin emosional dilini öyrənmək çox kömək etdi.", CreatedAt = now.AddDays(-6) },
                new() { TopicId = t10.Id, AuthorName = "Tural Hüseynov", Body = "Çətin aspektlər həm də ən çox böyümə gətirən aspektlərdir, o baxımdan pis şey deyil.", CreatedAt = now.AddDays(-6) },
                new() { TopicId = t11.Id, AuthorName = "Aygün Nəbiyeva", Body = "Bizdə də var bu trigon, doğrudan da rahat və təbii bir cazibə yaradır, heç bir gərginlik hiss etmirik.", CreatedAt = now.AddDays(-5) },
                new() { TopicId = t13.Id, AuthorName = "Nigar Cəfərova", Body = "Təxmini saatla da xəritə hesablaya bilərsən, sadəcə ev sərhədləri və Yüksələn dəqiq olmaya bilər. Günəş və Ay bürcün adətən düzgün çıxır.", CreatedAt = now.AddDays(-3) },
                new() { TopicId = t13.Id, AuthorName = "Fərid Novruzov", Body = "Bəzi ölkələrdə doğum haqqında arayış xəstəxanadan alına bilir, saat da orda qeyd olunur. Yoxlamağa dəyər.", CreatedAt = now.AddDays(-3) },
                new() { TopicId = t14.Id, AuthorName = "Kamran Əliyev", Body = "Bu sayt tropik zodiakdan istifadə edir, Qərb astrologiyasında ən çox yayılan sistemdir. Sidereal sistem isə Vedik astrologiyada işlədilir, fərq ayanamsa düzəlişindən qaynaqlanır.", CreatedAt = now.AddDays(-2) },
                new() { TopicId = t15.Id, AuthorName = "Səbinə Rzayeva", Body = "7-ci evdə Şimal Node çox tez-tez münasibətlər vasitəsilə böyümə mənasında şərh olunur — bəlkə də tək başına deyil, başqaları ilə əlaqədə inkişaf etmək sənin yolundur.", CreatedAt = now.AddDays(-1) },
                new() { TopicId = t16.Id, AuthorName = "Orxan Məmmədli", Body = "Maraqlı fikirdir! Mənim də: Xərçəng-Balıq-Əqrəb. Çox su enerjisi 🌊", CreatedAt = now.AddDays(-1) },
                new() { TopicId = t16.Id, AuthorName = "Leyla Vəliyeva", Body = "Buğa-Oğlaq-Qız burda, tam torpaq insanıyam deyəsən 😄", CreatedAt = now.AddHours(-20) },
                new() { TopicId = t18.Id, AuthorName = "Rəşad Qurbanov", Body = "Bal tək amil deyil, sadəcə bəzi sahələrdə daha çox səy lazım olduğunu göstərir. Real münasibətlərdə ünsiyyət balı üstələyir.", CreatedAt = now.AddHours(-10) }
            };

                await context.ForumReplies.AddRangeAsync(replies);
                await context.SaveChangesAsync();
            }
        }
    }



}
