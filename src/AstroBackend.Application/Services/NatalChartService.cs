using AstroBackend.Application.Astrology;
using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;   
using System.Text.Json;

namespace AstroBackend.Application.Services
{
    public class NatalChartService : INatalChartService
    {
        private readonly IGenericRepository<NatalChart> _chartRepo;
        private readonly IGenericRepository<Profile> _profileRepo;
        private readonly IUnitOfWork _unitOfWork;

        public NatalChartService(IGenericRepository<NatalChart> chartRepo, IGenericRepository<Profile> profileRepo, IUnitOfWork unitOfWork)
        {
            _chartRepo = chartRepo;
            _profileRepo = profileRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<NatalChartResponse> GetMyChartAsync(Guid userId, CancellationToken ct = default)
        {
            var chart = await _chartRepo.FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (chart != null && !string.IsNullOrWhiteSpace(chart.ChartJson) && chart.ChartJson != "{}")
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<NatalChartResponse>(chart.ChartJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (parsed != null) return parsed;
                }
                catch { }
            }

            // Calculate dynamically from user profile if not saved
            var profile = await _profileRepo.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            if (profile == null || string.IsNullOrWhiteSpace(profile.BirthDate))
                throw new NotFoundException("Natal xəritəni görmək üçün öncə profilinizdə doğum məlumatlarınızı daxil edin.");

            var calculated = AstrologyEngine.ComputeNatalChart(
                profile.BirthDate,
                profile.BirthTime ?? "12:00",
                profile.BirthLat ?? 40.4093,
                profile.BirthLon ?? 49.8671
            );

            return calculated;
        }

        public Task<NatalChartResponse> CalculateChartAsync(CalculateChartRequest request)
        {
            var chart = AstrologyEngine.ComputeNatalChart(request.Date, request.Time, request.Latitude, request.Longitude);
            return Task.FromResult(chart);
        }

        public async Task SaveMyChartAsync(Guid userId, SaveNatalChartRequest request, CancellationToken ct = default)
        {
            var chart = await _chartRepo.FirstOrDefaultAsync(c => c.UserId == userId, ct);
            if (chart == null)
            {
                chart = new NatalChart { UserId = userId, ChartJson = request.ChartJson };
                await _chartRepo.AddAsync(chart, ct);
            }
            else
            {
                chart.ChartJson = request.ChartJson;
                chart.UpdatedAt = DateTime.UtcNow;
                _chartRepo.Update(chart);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

}
