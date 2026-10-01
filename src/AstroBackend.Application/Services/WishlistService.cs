using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Repositories;
using AstroBackend.Application.Interfaces.Services;
using AstroBackend.Domain.Entities;
using AstroBackend.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroBackend.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IGenericRepository<WishlistItem> _wishlistRepo;
        private readonly IGenericRepository<ShopProduct> _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IGenericRepository<WishlistItem> wishlistRepo, IGenericRepository<ShopProduct> productRepo, IUnitOfWork unitOfWork)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<WishlistItemDto>> GetMyWishlistAsync(Guid userId, CancellationToken ct = default)
        {
            var items = await _wishlistRepo.FindAsync(w => w.UserId == userId, ct);
            return items
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WishlistItemDto(w.ProductId, w.CreatedAt))
                .ToList();
        }

        public async Task AddAsync(Guid userId, Guid productId, CancellationToken ct = default)
        {
            var product = await _productRepo.GetByIdAsync(productId, ct);
            if (product == null)
                throw new NotFoundException("Məhsul", productId);

            var existing = await _wishlistRepo.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
            if (existing != null)
                return;

            await _wishlistRepo.AddAsync(new WishlistItem { UserId = userId, ProductId = productId }, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task RemoveAsync(Guid userId, Guid productId, CancellationToken ct = default)
        {
            var existing = await _wishlistRepo.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);
            if (existing == null)
                return;

            _wishlistRepo.Delete(existing);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

}
