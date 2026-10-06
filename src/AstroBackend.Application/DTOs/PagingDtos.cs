namespace AstroBackend.Application.DTOs
{
    /// <summary>
    /// Səhifələnmiş siyahı cavabı — Articles/Forum-topics/Shop-products kimi böyüyə bilən
    /// siyahı endpoint-ləri üçün ümumi "zərf". Page 1-dən başlayır.
    /// </summary>
    public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
