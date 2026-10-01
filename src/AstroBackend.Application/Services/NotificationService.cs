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
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IGenericRepository<Notification> notificationRepo, IUnitOfWork unitOfWork)
        {
            _notificationRepo = notificationRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(Guid userId, CancellationToken ct = default)
        {
            var notifications = await _notificationRepo.FindAsync(n => n.UserId == userId, ct);
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }

        public async Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId, ct);
            if (notification == null)
                throw new NotFoundException("Bildiriş", notificationId);
            if (notification.UserId != userId)
                throw new ForbiddenException("Bu bildirişə giriş icazəniz yoxdur.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
                _notificationRepo.Update(notification);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        {
            var unread = await _notificationRepo.FindAsync(n => n.UserId == userId && !n.IsRead, ct);
            if (unread.Count == 0)
                return;

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
                _notificationRepo.Update(notification);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        /// <summary>Digər servislər (ForumService, ShopService) tərəfindən daxili olaraq çağırılır — hadisə əsaslı bildiriş yaradır.</summary>
        public async Task CreateAsync(Guid userId, string type, string title, string? body, string? link, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Link = link,
                IsRead = false
            };

            await _notificationRepo.AddAsync(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static NotificationDto MapToDto(Notification n) => new(
            n.Id,
            n.Type,
            n.Title,
            n.Body,
            n.Link,
            n.IsRead,
            n.CreatedAt
        );
    }

}
