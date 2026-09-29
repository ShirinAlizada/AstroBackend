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
    bool IsActive
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
        bool IsActive
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
        bool IsActive
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

    public record PlaceShopOrderRequest(
        List<ShopCartItemRequest> Items,
        string FullName,
        string Phone,
        string Address,
        string? Note
    );

    public record UpdateShopOrderStatusRequest(
        string Status
    );

}
