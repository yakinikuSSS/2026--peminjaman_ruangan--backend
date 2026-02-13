using PeminjamanRuangan.DTOs.Dashboard;

namespace PeminjamanRuangan.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
