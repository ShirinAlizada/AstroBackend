using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.DTOs
{
    public record ShopProductDto(
        Guid Id,
        string Category,
        string Slug,
        string Name,
        string? NameEn,
        string? NameRu,
        string Description,
        string? DescriptionEn,
        string? DescriptionRu,
        int PriceAzn,
        string? UnitLabel,
        string? ImageUrl,
        short SortOrder,
        bool IsActive,
        int Stock,
        double? AverageRating,
        int ReviewCount
    );

    public record CreateShopProductRequest(
        string Category,
        string? Slug,
        string Name,
        string? NameEn,
        string? NameRu,
        string Description,
        string? DescriptionEn,
        string? DescriptionRu,
        int PriceAzn,
        string? UnitLabel,
        string? ImageUrl,
        short SortOrder,
        bool IsActive,
        int Stock = 100
    );

    public record UpdateShopProductRequest(
        string Category,
        string Name,
        string? NameEn,
        string? NameRu,
        string Description,
        string? DescriptionEn,
        string? DescriptionRu,
        int PriceAzn,
        string? UnitLabel,
        string? ImageUrl,
        short SortOrder,
        bool IsActive,
        int Stock
    );

    public record ShopOrderItemDto(
        Guid Id,
        Guid? ProductId,
        string ProductName,
        int Quantity,
        int UnitPriceAzn
    );

    public record ShopOrderDto(
        Guid Id,
        Guid UserId,
        int SubtotalAzn,
        int DiscountPct,
        int TotalAzn,
        string FullName,
        string Phone,
        string Address,
        string? Note,
        string Status,
        DateTime CreatedAt,
        List<ShopOrderItemDto> Items
    );

    public record ShopCartItemRequest(
        Guid ProductId,
        string ProductName,
        int Quantity,
        int UnitPriceAzn
    );

    /// <summary>
    /// DiscountPct demo axınında frontend tərəfindən (sabit kod siyahısına qarşı)
    /// artıq doğrulanıb hesablanır — real ödəniş inteqrasiyası olmadığı üçün
    /// server burada təkrar doğrulama aparmır, sadəcə qəbul edib qeydə alır.
    /// </summary>
    public record PlaceShopOrderRequest(
        List<ShopCartItemRequest> Items,
        string FullName,
        string Phone,
        string Address,
        string? Note,
        int DiscountPct = 0
    );

    public record UpdateShopOrderStatusRequest(
        string Status
    );

    // --- Rəy/reytinq ---

    public record ShopProductReviewDto(
        Guid Id,
        Guid ProductId,
        Guid UserId,
        string? UserName,
        int Rating,
        string? Comment,
        DateTime CreatedAt
    );

    public record UpsertReviewRequest(int Rating, string? Comment);

    // --- Admin satış statistikası ---

    public record ShopTopProductDto(Guid ProductId, string ProductName, int UnitsSold, int RevenueAzn);

    public record ShopSalesStatsDto(
        int TotalRevenueAzn,
        int TotalOrders,
        int PendingOrders,
        List<ShopTopProductDto> TopProducts
    );


}
