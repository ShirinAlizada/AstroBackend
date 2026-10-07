using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Enums;
using AstroBackend.Domain.Exceptions;

namespace AstroBackend.Application.Services
{
    public class ShopService : IShopService
    {
        private readonly IGenericRepository<ShopProduct> _productRepo;
        private readonly IGenericRepository<ShopOrder> _orderRepo;
        private readonly IGenericRepository<ShopOrderItem> _itemRepo;
        private readonly IGenericRepository<ShopProductReview> _reviewRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly INotificationService _notificationService;
        private readonly IPushSubscriptionService _pushSubscriptionService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public ShopService(
            IGenericRepository<ShopProduct> productRepo,
            IGenericRepository<ShopOrder> orderRepo,
            IGenericRepository<ShopOrderItem> itemRepo,
            IGenericRepository<ShopProductReview> reviewRepo,
            IGenericRepository<User> userRepo,
            INotificationService notificationService,
            IPushSubscriptionService pushSubscriptionService,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _itemRepo = itemRepo;
            _reviewRepo = reviewRepo;
            _userRepo = userRepo;
            _notificationService = notificationService;
            _pushSubscriptionService = pushSubscriptionService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ShopProductDto>> GetActiveProductsAsync(string? search, string? sort, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var list = term == null
                ? await _productRepo.FindAsync(p => p.IsActive, ct)
                : await _productRepo.FindAsync(p => p.IsActive && p.Name.Contains(term), ct);

            var ordered = ApplySort(list, sort);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var totalCount = ordered.Count;
            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var dtos = await MapProductsAsync(paged, ct);

            return new PagedResult<ShopProductDto>(dtos.ToList(), page, pageSize, totalCount);
        }

        public async Task<IReadOnlyList<ShopProductDto>> GetAllProductsAsync(CancellationToken ct = default)
        {
            var list = await _productRepo.GetAllAsync(ct);
            var ordered = list.OrderBy(p => p.Category).ThenBy(p => p.SortOrder).ToList();
            return await MapProductsAsync(ordered, ct);
        }

        private static List<ShopProduct> ApplySort(IReadOnlyList<ShopProduct> list, string? sort) => sort?.ToLowerInvariant() switch
        {
            "price_asc" => list.OrderBy(p => p.PriceAzn).ToList(),
            "price_desc" => list.OrderByDescending(p => p.PriceAzn).ToList(),
            "name" => list.OrderBy(p => p.Name).ToList(),
            "newest" => list.OrderByDescending(p => p.CreatedAt).ToList(),
            _ => list.OrderBy(p => p.Category).ThenBy(p => p.SortOrder).ToList(),
        };

        public async Task<ShopProductDto> CreateProductAsync(CreateShopProductRequest request, CancellationToken ct = default)
        {
            if (!Enum.TryParse<ShopCategory>(request.Category, true, out var category))
                throw new BadRequestException("Kateqoriya yanlışdır.");

            var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name) : request.Slug.Trim();

            var product = new ShopProduct
            {
                Category = category,
                Slug = slug,
                Name = request.Name.Trim(),
                NameEn = string.IsNullOrWhiteSpace(request.NameEn) ? null : request.NameEn.Trim(),
                NameRu = string.IsNullOrWhiteSpace(request.NameRu) ? null : request.NameRu.Trim(),
                Description = request.Description.Trim(),
                DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
                DescriptionRu = string.IsNullOrWhiteSpace(request.DescriptionRu) ? null : request.DescriptionRu.Trim(),
                PriceAzn = request.PriceAzn,
                UnitLabel = string.IsNullOrWhiteSpace(request.UnitLabel) ? null : request.UnitLabel.Trim(),
                UnitLabelEn = string.IsNullOrWhiteSpace(request.UnitLabelEn) ? null : request.UnitLabelEn.Trim(),
                UnitLabelRu = string.IsNullOrWhiteSpace(request.UnitLabelRu) ? null : request.UnitLabelRu.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                Stock = request.Stock
            };

            await _productRepo.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapProduct(product, null, 0);
        }

        public async Task<ShopProductDto> UpdateProductAsync(Guid id, UpdateShopProductRequest request, CancellationToken ct = default)
        {
            var product = await _productRepo.GetByIdAsync(id, ct);
            if (product == null)
                throw new NotFoundException("Məhsul tapılmadı.");

            if (!Enum.TryParse<ShopCategory>(request.Category, true, out var category))
                throw new BadRequestException("Kateqoriya yanlışdır.");

            product.Category = category;
            product.Name = request.Name.Trim();
            product.NameEn = string.IsNullOrWhiteSpace(request.NameEn) ? null : request.NameEn.Trim();
            product.NameRu = string.IsNullOrWhiteSpace(request.NameRu) ? null : request.NameRu.Trim();
            product.Description = request.Description.Trim();
            product.DescriptionEn = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim();
            product.DescriptionRu = string.IsNullOrWhiteSpace(request.DescriptionRu) ? null : request.DescriptionRu.Trim();
            product.PriceAzn = request.PriceAzn;
            product.UnitLabel = string.IsNullOrWhiteSpace(request.UnitLabel) ? null : request.UnitLabel.Trim();
            product.UnitLabelEn = string.IsNullOrWhiteSpace(request.UnitLabelEn) ? null : request.UnitLabelEn.Trim();
            product.UnitLabelRu = string.IsNullOrWhiteSpace(request.UnitLabelRu) ? null : request.UnitLabelRu.Trim();
            product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            product.SortOrder = request.SortOrder;
            product.IsActive = request.IsActive;
            product.Stock = request.Stock;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);

            var (avg, count) = await GetRatingAsync(product.Id, ct);
            return MapProduct(product, avg, count);
        }

        public async Task DeleteProductAsync(Guid id, CancellationToken ct = default)
        {
            var product = await _productRepo.GetByIdAsync(id, ct);
            if (product == null)
                throw new NotFoundException("Məhsul tapılmadı.");

            _productRepo.Delete(product);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Demo/mock sifariş: real ödəniş əməliyyatı yoxdur — sifariş dərhal "Yeni"
        /// statusu ilə, səbətdəki bütün sətirlərlə birlikdə qeydə alınır. Stok
        /// yoxlanılır və uğurlu sifarişdə azaldılır; endirim faizi (DiscountPct)
        /// frontend-də artıq doğrulanıb ötürülür (bax: PlaceShopOrderRequest qeydi).
        /// </summary>
        public async Task<ShopOrderDto> PlaceOrderAsync(Guid userId, PlaceShopOrderRequest request, CancellationToken ct = default)
        {
            if (request.Items.Count == 0)
                throw new BadRequestException("Səbət boşdur.");
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Address))
                throw new BadRequestException("Ad, telefon və ünvan tələb olunur.");
            if (request.Items.Any(i => i.Quantity <= 0))
                throw new BadRequestException("Say düzgün deyil.");

            var discountPct = Math.Clamp(request.DiscountPct, 0, 100);

            var productIds = request.Items.Where(i => i.ProductId != Guid.Empty).Select(i => i.ProductId).Distinct().ToList();
            var products = productIds.Count == 0
                ? new List<ShopProduct>()
                : (await _productRepo.FindAsync(p => productIds.Contains(p.Id), ct)).ToList();
            var productsById = products.ToDictionary(p => p.Id);

            // Stok yoxlaması məhsul üzrə AQREQASİYA olunmuş miqdara görə aparılır: səbətdə eyni
            // ProductId üçün bir neçə sətir olsa (hər sətir ayrıca stok limitindən keçsə belə),
            // ÜMUMİ tələb olunan say stoku keçməməlidir — əks halda overselling mümkün olurdu.
            var requestedQtyByProduct = request.Items
                .Where(i => i.ProductId != Guid.Empty)
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            foreach (var (productId, requestedQty) in requestedQtyByProduct)
            {
                if (productsById.TryGetValue(productId, out var product) && product.Stock < requestedQty)
                    throw new BadRequestException($"\"{product.Name}\" məhsulundan stokda yalnız {product.Stock} ədəd qalıb.");
            }

            // Qiymət server tərəfindən kataloqdan təsdiqlənir — client-dən gələn UnitPriceAzn
            // etibar edilə bilən deyil (asanlıqla saxtalaşdırıla bilər). Yalnız kataloqda uyğun
            // məhsul tapılmayanda (silinmiş/legacy ProductId) client qiyməti ehtiyat kimi qalır.
            int ResolveUnitPrice(ShopCartItemRequest item) =>
                item.ProductId != Guid.Empty && productsById.TryGetValue(item.ProductId, out var p)
                    ? p.PriceAzn
                    : item.UnitPriceAzn;

            var subtotal = request.Items.Sum(i => ResolveUnitPrice(i) * i.Quantity);
            var total = subtotal - (subtotal * discountPct / 100);

            var order = new ShopOrder
            {
                UserId = userId,
                SubtotalAzn = subtotal,
                DiscountPct = discountPct,
                TotalAzn = total,
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Address = request.Address.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                Status = ShopOrderStatus.Yeni
            };

            await _orderRepo.AddAsync(order, ct);

            var items = request.Items.Select(i => new ShopOrderItem
            {
                OrderId = order.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPriceAzn = ResolveUnitPrice(i)
            }).ToList();

            foreach (var item in items)
                await _itemRepo.AddAsync(item, ct);

            foreach (var (productId, requestedQty) in requestedQtyByProduct)
            {
                if (productsById.TryGetValue(productId, out var product))
                {
                    product.Stock -= requestedQty;
                    product.UpdatedAt = DateTime.UtcNow;
                    _productRepo.Update(product);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);

            // Sifariş təsdiq e-poçtu — best-effort, uğursuz olsa sifarişin özünə mane olmur.
            try
            {
                var buyer = await _userRepo.GetByIdAsync(userId, ct);
                if (buyer != null)
                {
                    await _emailService.SendAsync(
                        buyer.Email,
                        "Sifarişiniz qəbul edildi — Virgo Astrology",
                        $"<p>Salam {System.Net.WebUtility.HtmlEncode(order.FullName)},</p>" +
                        $"<p>Sifarişiniz qəbul edildi. Ümumi məbləğ: <strong>{order.TotalAzn} AZN</strong>.</p>" +
                        "<p>Status dəyişdikdə sizə bildiriş göndəriləcək.</p>" +
                        "<p>Hörmətlə,<br/>Virgo Astrology komandası</p>",
                        ct);
                }
            }
            catch
            {
                // Email göndərilməsə belə sifariş artıq qeydə alınıb.
            }

            return MapOrder(order, items);
        }

        public async Task<IReadOnlyList<ShopOrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken ct = default)
        {
            var orders = (await _orderRepo.FindAsync(o => o.UserId == userId, ct))
                .OrderByDescending(o => o.CreatedAt).ToList();
            return await MapOrdersAsync(orders, ct);
        }

        public async Task<IReadOnlyList<ShopOrderDto>> GetAllOrdersAsync(CancellationToken ct = default)
        {
            var orders = (await _orderRepo.GetAllAsync(ct))
                .OrderByDescending(o => o.CreatedAt).ToList();
            return await MapOrdersAsync(orders, ct);
        }

        /// <summary>Status dəyişəndə sifarişi verən istifadəçiyə bildiriş yaradılır (bax: INotificationService).</summary>
        public async Task<ShopOrderDto> UpdateOrderStatusAsync(Guid orderId, string status, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
                throw new NotFoundException("Sifariş tapılmadı.");

            if (!Enum.TryParse<ShopOrderStatus>(status, true, out var newStatus))
                throw new BadRequestException("Status yanlışdır.");

            var statusChanged = order.Status != newStatus;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            if (statusChanged)
            {
                var statusLabel = newStatus switch
                {
                    ShopOrderStatus.Yeni => "Yeni",
                    ShopOrderStatus.Tesdiqlenib => "Təsdiqləndi",
                    ShopOrderStatus.Gonderilib => "Göndərildi",
                    ShopOrderStatus.LegvEdilib => "Ləğv edildi",
                    _ => newStatus.ToString()
                };
                await _notificationService.CreateAsync(
                    order.UserId,
                    "order_status",
                    "Sifariş statusu yeniləndi",
                    $"Sifarişiniz \"{statusLabel}\" statusuna keçdi.",
                    "/sifarislerim",
                    ct);

                await _pushSubscriptionService.NotifyUserAsync(
                    order.UserId,
                    "Sifariş statusu yeniləndi",
                    $"Sifarişiniz \"{statusLabel}\" statusuna keçdi.",
                    "/sifarislerim",
                    ct);

                try
                {
                    var buyer = await _userRepo.GetByIdAsync(order.UserId, ct);
                    if (buyer != null)
                    {
                        await _emailService.SendAsync(
                            buyer.Email,
                            "Sifariş statusu yeniləndi — Virgo Astrology",
                            $"<p>Salam {System.Net.WebUtility.HtmlEncode(order.FullName)},</p>" +
                            $"<p>Sifarişiniz \"{statusLabel}\" statusuna keçdi.</p>" +
                            "<p>Hörmətlə,<br/>Virgo Astrology komandası</p>",
                            ct);
                    }
                }
                catch
                {
                    // Best-effort — status dəyişikliyi artıq qeydə alınıb.
                }
            }

            var items = await _itemRepo.FindAsync(i => i.OrderId == order.Id, ct);
            return MapOrder(order, items);
        }

        // --- Rəy/reytinq ---

        public async Task<IReadOnlyList<ShopProductReviewDto>> GetReviewsAsync(Guid productId, CancellationToken ct = default)
        {
            var reviews = (await _reviewRepo.FindAsync(r => r.ProductId == productId, ct))
                .OrderByDescending(r => r.CreatedAt).ToList();
            if (reviews.Count == 0)
                return Array.Empty<ShopProductReviewDto>();

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var users = (await _userRepo.FindAsync(u => userIds.Contains(u.Id), ct)).ToDictionary(u => u.Id);

            return reviews.Select(r =>
            {
                users.TryGetValue(r.UserId, out var user);
                return new ShopProductReviewDto(r.Id, r.ProductId, r.UserId, user?.FullName, r.Rating, r.Comment, r.CreatedAt);
            }).ToList();
        }

        /// <summary>Hər istifadəçi bir məhsula bir rəy yaza bilər — təkrar göndərsə mövcud rəy yenilənir.</summary>
        public async Task<ShopProductReviewDto> UpsertReviewAsync(Guid productId, Guid userId, UpsertReviewRequest request, CancellationToken ct = default)
        {
            if (request.Rating is < 1 or > 5)
                throw new BadRequestException("Reytinq 1 ilə 5 arasında olmalıdır.");

            var product = await _productRepo.GetByIdAsync(productId, ct);
            if (product == null)
                throw new NotFoundException("Məhsul tapılmadı.");

            var review = await _reviewRepo.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, ct);
            if (review == null)
            {
                review = new ShopProductReview { ProductId = productId, UserId = userId, Rating = request.Rating, Comment = request.Comment };
                await _reviewRepo.AddAsync(review, ct);
            }
            else
            {
                review.Rating = request.Rating;
                review.Comment = request.Comment;
                review.UpdatedAt = DateTime.UtcNow;
                _reviewRepo.Update(review);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var user = await _userRepo.GetByIdAsync(userId, ct);
            return new ShopProductReviewDto(review.Id, review.ProductId, review.UserId, user?.FullName, review.Rating, review.Comment, review.CreatedAt);
        }

        private async Task<(double? avg, int count)> GetRatingAsync(Guid productId, CancellationToken ct)
        {
            var reviews = await _reviewRepo.FindAsync(r => r.ProductId == productId, ct);
            if (reviews.Count == 0)
                return (null, 0);
            return (reviews.Average(r => (double)r.Rating), reviews.Count);
        }

        // --- Admin satış statistikası ---

        /// <summary>Mövcud ShopOrder/ShopOrderItem üzərindən hesablanan sadə satış statistikası. Abunəlik/ödəniş modulu hələ olmadığı üçün yalnız mağaza gəliri əhatə olunur.</summary>
        public async Task<ShopSalesStatsDto> GetSalesStatsAsync(CancellationToken ct = default)
        {
            var orders = await _orderRepo.GetAllAsync(ct);
            var totalRevenue = orders.Where(o => o.Status != ShopOrderStatus.LegvEdilib).Sum(o => o.TotalAzn);
            var pendingOrders = orders.Count(o => o.Status == ShopOrderStatus.Yeni);

            var allItems = await _itemRepo.GetAllAsync(ct);
            var relevantOrderIds = orders.Where(o => o.Status != ShopOrderStatus.LegvEdilib).Select(o => o.Id).ToHashSet();

            var topProducts = allItems
                .Where(i => relevantOrderIds.Contains(i.OrderId))
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new ShopTopProductDto(
                    g.Key.ProductId ?? Guid.Empty,
                    g.Key.ProductName,
                    g.Sum(i => i.Quantity),
                    g.Sum(i => i.Quantity * i.UnitPriceAzn)))
                .OrderByDescending(p => p.RevenueAzn)
                .Take(5)
                .ToList();

            return new ShopSalesStatsDto(totalRevenue, orders.Count, pendingOrders, topProducts);
        }

        private async Task<IReadOnlyList<ShopOrderDto>> MapOrdersAsync(List<ShopOrder> orders, CancellationToken ct)
        {
            var orderIds = orders.Select(o => o.Id).ToList();
            var allItems = await _itemRepo.FindAsync(i => orderIds.Contains(i.OrderId), ct);
            var itemsByOrder = allItems.GroupBy(i => i.OrderId).ToDictionary(g => g.Key, g => g.ToList());

            return orders.Select(o =>
            {
                itemsByOrder.TryGetValue(o.Id, out var items);
                return MapOrder(o, items ?? new List<ShopOrderItem>());
            }).ToList();
        }

        private async Task<IReadOnlyList<ShopProductDto>> MapProductsAsync(IReadOnlyList<ShopProduct> products, CancellationToken ct)
        {
            if (products.Count == 0)
                return Array.Empty<ShopProductDto>();

            var productIds = products.Select(p => p.Id).ToList();
            var reviews = await _reviewRepo.FindAsync(r => productIds.Contains(r.ProductId), ct);
            var byProduct = reviews.GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => (avg: g.Average(r => (double)r.Rating), count: g.Count()));

            return products.Select(p =>
            {
                byProduct.TryGetValue(p.Id, out var stats);
                return MapProduct(p, byProduct.ContainsKey(p.Id) ? stats.avg : null, byProduct.ContainsKey(p.Id) ? stats.count : 0);
            }).ToList();
        }

        private static ShopOrderDto MapOrder(ShopOrder o, IReadOnlyList<ShopOrderItem> items) => new(
            o.Id,
            o.UserId,
            o.SubtotalAzn,
            o.DiscountPct,
            o.TotalAzn,
            o.FullName,
            o.Phone,
            o.Address,
            o.Note,
            o.Status.ToString().ToLower(),
            o.CreatedAt,
            items.Select(i => new ShopOrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPriceAzn)).ToList()
        );

        private static ShopProductDto MapProduct(ShopProduct p, double? avgRating, int reviewCount) => new(
            p.Id,
            p.Category.ToString().ToLower(),
            p.Slug,
            p.Name,
            p.NameEn,
            p.NameRu,
            p.Description,
            p.DescriptionEn,
            p.DescriptionRu,
            p.PriceAzn,
            p.UnitLabel,
            p.UnitLabelEn,
            p.UnitLabelRu,
            p.ImageUrl,
            p.SortOrder,
            p.IsActive,
            p.Stock,
            avgRating,
            reviewCount
        );

        private static string Slugify(string text)
        {
            var map = new Dictionary<char, char> { ['ə'] = 'e', ['ı'] = 'i', ['ö'] = 'o', ['ü'] = 'u', ['ç'] = 'c', ['ş'] = 's', ['ğ'] = 'g' };
            var lowered = text.Trim().ToLowerInvariant();
            var mapped = new string(lowered.Select(c => map.TryGetValue(c, out var r) ? r : c).ToArray());
            var slug = System.Text.RegularExpressions.Regex.Replace(mapped, "[^a-z0-9]+", "-").Trim('-');
            return slug.Length > 60 ? slug[..60] : slug;
        }
    }

}
