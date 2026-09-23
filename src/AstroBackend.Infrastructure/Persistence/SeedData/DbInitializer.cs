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

            // 2. Seed Super Admin User (from migrations)
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
                    Bio = "Virgo Astrology / Destiny Reads platformasının Baş Super Administratoru."
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
                    DisplayName = "Ləman Əliyeva",
                    Title = "Sinastriya astroloqu",
                    Bio = "Usui Reiki Seansları.",
                    SpecialtiesJson = JsonSerializer.Serialize(new[] { "Sinastriya", "Sevgi" }),
                    LanguagesJson = JsonSerializer.Serialize(new[] { "Azərbaycan", "İngilis" }),
                    PriceAzn = 175,
                    Rating = 5.0m,
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

            // 6. Seed Articles
            if (!await context.Articles.AnyAsync())
            {
                var articles = new List<Article>
            {
                new()
                {
                    Title = "Mercury retrogradında necə qərar vermək olar?",
                    Slug = "mercury-retrogradinda-nece-qerar-vermek-olar",
                    Tag = "Təlimat",
                    Excerpt = "Görünüşdə geriyə hərəkət edən planetlər bizi yavaşlatmağa çağırır, dayandırmaq isə qorxutmur.",
                    Body = "Merkuri retroqrad dövründə rabitə, səyahət və texnologiya sahələrində kiçik ləngimələr müşahidə oluna bilər. Lakin bu dövr köhnə layihələri tamamlamaq, yarımçıq qalmış münasibətləri aydınlaşdırmaq və daxili aləmimizə diqqət yetirmək üçün əvəzolunmaz bir mərhələdir.",
                    Published = true,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    Views = 42
                },
                new()
                {
                    Title = "Tam ayın bürclərə təsiri",
                    Slug = "tam-ayin-burclere-tesiri",
                    Tag = "Ay",
                    Excerpt = "Dolunay dövründə duyğular güclənir, bu isə özünü dərk etmək üçün əla fürsətdir.",
                    Body = "Dolunay göy üzündə parlaqlığın ən yüksək həddə çatdığı andır. Bu zaman şüuraltı hisslər səthə çıxır, həyatımızdakı qeyri-müəyyənliklər işıqlanır. Həmin dövrdə meditasiya etmək və emosional gərginliyi yaradıcılığa çevirmək tövsiyə olunur.",
                    Published = true,
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    Views = 68
                },
                new()
                {
                    Title = "Doğum xəritəsindəki 12 ev nə deməkdir?",
                    Slug = "dogum-xeritesindeki-12-ev-ne-demekdir",
                    Tag = "Xəritə",
                    Excerpt = "Hər ev həyatınızın bir sahəsini göstərir — maddi, ruhani, əlaqələr və karyera.",
                    Body = "Astroloji xəritədəki 12 ev insanın şəxsiyyətindən tutmuş (1-ci ev), maliyyəsinə (2-ci ev), münasibətlərinə (7-ci ev) və karyerasına (10-cu ev) qədər bütün həyat sahələrini əhatə edir. Hər evin hansı bürcdə yerləşməsi həmin sahədəki potensialınızı müəyyən edir.",
                    Published = true,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    Views = 115
                }
            };

                await context.Articles.AddRangeAsync(articles);
                await context.SaveChangesAsync();
            }
        }
    }


}
