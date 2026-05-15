using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.baseServices;

namespace Personal_Finance_Management.Service.broadcast
{
    public class Service : IService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Response.BroadcastsResponse> CreateBroadcast(Request.BroadcastsRequest request)
        {
            if (request == null)
            {
                throw new Exception("Invalid request");
            }
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new Exception("Title is required");

            if (string.IsNullOrWhiteSpace(request.Body))
                throw new Exception("Body is required");


            var adminId = ServiceClaimHelper.GetRequiredAdminId(_httpContextAccessor);

            var targetUsers = await _dbContext.Accounts.Where(x => x.Role.Code == "User" && x.Status == "Active")
                .Select(x => x.Id)
                .ToListAsync();
            var broadcast = new Repository.Entity.Broadcast
            {
                Id = Guid.NewGuid(),
                CreatedByAdminId = adminId,
                Title = request.Title,
                Body = request.Body,
                TargetAudience = request.TargetAudience,
                Status = request.ScheduledAt == null ? "Sent" : "Queued",
                ScheduledAt = request.ScheduledAt,
                SentAt = request.ScheduledAt == null ? DateTimeOffset.UtcNow : (DateTimeOffset?)null,
                TargetCount = targetUsers.Count,
                DeliveredCount = request.ScheduledAt == null ? targetUsers.Count : 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow

            };
            _dbContext.Broadcasts.Add(broadcast);
            if (request.ScheduledAt == null)
            {
                // hien: Tạo notifications cho user ngay lập tức
                var notifications = targetUsers.Select(userId => new Repository.Entity.Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BroadcastId = broadcast.Id,
                    Type = "Broadcast",
                    Title = broadcast.Title,
                    Body = broadcast.Body,
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow,
                }).ToList();
                _dbContext.Notifications.AddRange(notifications);
            }
            await _dbContext.SaveChangesAsync();
            return new Response.BroadcastsResponse
            {
                Id = broadcast.Id,
                Title = broadcast.Title,
                Body = broadcast.Body,
                TargetAudience = broadcast.TargetAudience,
                Status = broadcast.Status,
                ScheduledAt = broadcast.ScheduledAt,
                SentAt = broadcast.SentAt,
                TargetCount = broadcast.TargetCount,
                DeliveredCount = broadcast.DeliveredCount
            };
            // hien: targetAudience MVP dùng All.
            // Nếu scheduledAt = null, backend có thể đưa broadcast vào queue gửi ngay.
            // Khi dispatch đồng bộ, tạo notifications cho user trong cùng transaction với broadcast nếu có thể.
            // Nên ghi audit log với actionType = BroadcastSend, entityType = Broadcast.
        }

        public async Task<Page<Response.BroadcastsResponse>> GetBroadcasts(int pageIndex, int pageSize, string status = "Queued")
        {
            var totalItems = await _dbContext.Broadcasts.CountAsync();
            var adminId = await GetAdminIdFromToken();
            var broadcast = await _dbContext.Broadcasts.Where(x => x.CreatedByAdminId == adminId && (status == null || x.Status == status))
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new Response.BroadcastsResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Body = x.Body,
                    TargetAudience = x.TargetAudience,
                    Status = x.Status,
                    ScheduledAt = x.ScheduledAt,
                    SentAt = x.SentAt,
                    TargetCount = x.TargetCount,
                    DeliveredCount = x.DeliveredCount
                }).ToListAsync();

            var items = broadcast.ToList();
            return new Page<Response.BroadcastsResponse>
            {
                Items = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalItems,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };


        }
        public async Task<int> DispatchDueBroadcastsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var dispatchedCount = 0;

            var dueBroadcasts = await _dbContext.Broadcasts
                .Where(x => x.Status == "Queued"
                            && x.ScheduledAt != null
                            && x.ScheduledAt <= now)
                .OrderBy(x => x.ScheduledAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            if (dueBroadcasts.Count == 0)
            {
                return dispatchedCount;
            }

            var targetUsers = await _dbContext.Accounts
                .Where(x => x.Role.Code == "User" && x.Status == "Active")
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            foreach (var broadcast in dueBroadcasts)
            {
                var alreadyCreated = await _dbContext.Notifications
                    .AnyAsync(x => x.BroadcastId == broadcast.Id, cancellationToken);

                if (!alreadyCreated)
                {
                    var notifications = targetUsers.Select(userId => new Repository.Entity.Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        BroadcastId = broadcast.Id,
                        Type = "Broadcast",
                        Title = broadcast.Title,
                        Body = broadcast.Body,
                        IsRead = false,
                        CreatedAt = now,
                    }).ToList();

                    _dbContext.Notifications.AddRange(notifications);
                    broadcast.DeliveredCount = notifications.Count;
                }
                else
                {
                    broadcast.DeliveredCount = await _dbContext.Notifications
                        .CountAsync(x => x.BroadcastId == broadcast.Id, cancellationToken);
                }

                broadcast.Status = "Sent";
                broadcast.SentAt = now;
                broadcast.TargetCount = targetUsers.Count;
                broadcast.UpdatedAt = now;
                dispatchedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return dispatchedCount;
        }
        public async Task<Guid> GetAdminIdFromToken()
        {
            return await Task.FromResult(ServiceClaimHelper.GetRequiredAdminId(_httpContextAccessor));
        }
    }
}
