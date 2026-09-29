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
        private readonly IUnitOfWork _unitOfWork;

        public ShopService(
            IGenericRepository<ShopProduct> productRepo,
            IGenericRepository<ShopOrder> orderRepo,
            IGenericRepository<ShopOrderItem> itemRepo,
            IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _itemRepo = itemRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<ShopProductDto>> GetActiveProductsAsync(CancellationToken ct = default)
        {
            var list = await _productRepo.FindAsync(p => p.IsActive, ct);
            return list.OrderBy(p => p.Category).ThenBy(p => p.SortOrder).Select(MapProduct).ToList();
        }

        public async Task<IReadOnlyList<ShopProductDto>> GetAllProductsAsync(CancellationToken ct = default)
        {
            var list = await _productRepo.GetAllAsync(ct);
            return list.OrderBy(p => p.Category).ThenBy(p => p.SortOrder).Select(MapProduct).ToList();
        }

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
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive
            };

            await _productRepo.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapProduct(product);
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
            product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            product.SortOrder = request.SortOrder;
            product.IsActive = request.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapProduct(product);
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
        /// statusu ilə, səbətdəki bütün sətirlərlə birlikdə qeydə alınır.
        /// </summary>
        public async Task<ShopOrderDto> PlaceOrderAsync(Guid userId, PlaceShopOrderRequest request, CancellationToken ct = default)
        {
            if (request.Items.Count == 0)
                throw new BadRequestException("Səbət boşdur.");
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Address))
                throw new BadRequestException("Ad, telefon və ünvan tələb olunur.");
            if (request.Items.Any(i => i.Quantity <= 0))
                throw new BadRequestException("Say düzgün deyil.");

            var total = request.Items.Sum(i => i.UnitPriceAzn * i.Quantity);

            var order = new ShopOrder
            {
                UserId = userId,
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
                UnitPriceAzn = i.UnitPriceAzn
            }).ToList();

            foreach (var item in items)
                await _itemRepo.AddAsync(item, ct);

            await _unitOfWork.SaveChangesAsync(ct);

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

        public async Task<ShopOrderDto> UpdateOrderStatusAsync(Guid orderId, string status, CancellationToken ct = default)
        {
            var order = await _orderRepo.GetByIdAsync(orderId, ct);
            if (order == null)
                throw new NotFoundException("Sifariş tapılmadı.");

            if (!Enum.TryParse<ShopOrderStatus>(status, true, out var newStatus))
                throw new BadRequestException("Status yanlışdır.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            var items = await _itemRepo.FindAsync(i => i.OrderId == order.Id, ct);
            return MapOrder(order, items);
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

        private static ShopOrderDto MapOrder(ShopOrder o, IReadOnlyList<ShopOrderItem> items) => new(
            o.Id,
            o.UserId,
            o.TotalAzn,
            o.FullName,
            o.Phone,
            o.Address,
            o.Note,
            o.Status.ToString().ToLower(),
            o.CreatedAt,
            items.Select(i => new ShopOrderItemDto(i.Id, i.ProductId, i.ProductName, i.Quantity, i.UnitPriceAzn)).ToList()
        );

        private static ShopProductDto MapProduct(ShopProduct p) => new(
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
            p.ImageUrl,
            p.SortOrder,
            p.IsActive
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
