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
            // 1. Ensure Database Created / Migrations applied
            await context.Database.EnsureCreatedAsync();

            // 2. Seed Admin User if not exists
            var adminEmail = "shirinma@code.edu.az";
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    Email = adminEmail,
                    FullName = "Sistem Admini",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    Role = AppRole.Admin,
                    IsActive = true
                };
                await context.Users.AddAsync(admin);
                await context.Profiles.AddAsync(new Profile
                {
                    UserId = admin.Id,
                    FullName = "Sistem Admini",
                    Bio = "Ruh Astrolojiya platformasının baş administratoru."
                });
                await context.SaveChangesAsync();
            }

            // 3. Seed Astrologers if empty
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

            // 4. Seed Horoscopes for 12 signs if empty
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
        }
    }

}
