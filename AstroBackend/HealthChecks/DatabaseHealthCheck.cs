using AstroBackend.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AstroBackend.HealthChecks
{
    /// <summary>
    /// SQL Server əlaqəsinin canlı olub-olmadığını yoxlayan sadə health-check. Ayrıca NuGet
    /// paketi (Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore) tələb etmir —
    /// sadəcə AppDbContext.Database.CanConnectAsync() ilə yoxlanılır. Monitorinq alətləri
    /// (uptime-robot, k8s liveness/readiness probe və s.) /health endpoint-ini çağıra bilər.
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public DatabaseHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync(ct);
                return canConnect
                    ? HealthCheckResult.Healthy("Verilənlər bazası əlaqəsi aktivdir.")
                    : HealthCheckResult.Unhealthy("Verilənlər bazasına qoşulmaq mümkün olmadı.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Verilənlər bazası xətası.", ex);
            }
        }
    }
}
