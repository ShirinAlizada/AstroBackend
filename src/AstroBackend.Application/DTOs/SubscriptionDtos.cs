using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.DTOs
{
    public record SubscriptionPlanDto(
    string Key,
    string Name,
    string? Tagline,
    int PriceAzn,
    string BillingPeriod,
    List<string> Features,
    int? AiMessagesPerDay,
    bool SynastryFullDetail,
    int BookingDiscountPct
);

    public record UserSubscriptionDto(
        string PlanKey,
        string Status,
        DateTime StartedAt,
        DateTime CurrentPeriodEnd,
        string BillingPeriod
    );

    /// <summary>
    /// Demo/mock alış tələbi — kart məlumatları server tərəfə ötürülmür (frontend
    /// checkout formu yalnız UI validasiyası üçündür, real provayder inteqrasiyası yoxdur).
    /// </summary>
    public record PurchasePlanRequest(string PlanKey, string BillingPeriod = "monthly");

    public record PaymentTransactionDto(
        Guid Id,
        string PlanKey,
        int AmountAzn,
        string Provider,
        string Status,
        string? Note,
        string BillingPeriod,
        DateTime CreatedAt
    );

    /// <summary>
    /// Admin idarəetməsi üçün xam (bütün dillər daxil) paket şəkli — ShopProductDto-dakı
    /// NameEn/NameRu naxışının analoqu. Tətbiq/müştəri tərəfinə göndərilən SubscriptionPlanDto
    /// isə yalnız bir dildə (lang parametrinə görə həll olunmuş) göstərir.
    /// </summary>
    public record AdminSubscriptionPlanDto(
        Guid Id,
        string Key,
        string Name,
        string? Tagline,
        string? TaglineEn,
        string? TaglineRu,
        int PriceAzn,
        string BillingPeriod,
        List<string> Features,
        List<string>? FeaturesEn,
        List<string>? FeaturesRu,
        int? AiMessagesPerDay,
        bool SynastryFullDetail,
        int BookingDiscountPct,
        short SortOrder,
        bool IsActive
    );

    public record CreateSubscriptionPlanRequest(
        string Key,
        string Name,
        string? Tagline,
        string? TaglineEn,
        string? TaglineRu,
        int PriceAzn,
        string BillingPeriod,
        List<string> Features,
        List<string>? FeaturesEn,
        List<string>? FeaturesRu,
        int? AiMessagesPerDay,
        bool SynastryFullDetail,
        int BookingDiscountPct,
        short SortOrder,
        bool IsActive
    );

    public record UpdateSubscriptionPlanRequest(
        string Name,
        string? Tagline,
        string? TaglineEn,
        string? TaglineRu,
        int PriceAzn,
        string BillingPeriod,
        List<string> Features,
        List<string>? FeaturesEn,
        List<string>? FeaturesRu,
        int? AiMessagesPerDay,
        bool SynastryFullDetail,
        int BookingDiscountPct,
        short SortOrder,
        bool IsActive
    );

}
