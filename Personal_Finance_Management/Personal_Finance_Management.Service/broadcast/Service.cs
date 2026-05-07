using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
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


            var adminIdValue = _httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == "id")?.Value;

            var adminId = Guid.Parse(adminIdValue ?? throw new Exception("Admin ID claim is missing"));

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
        public async Task<Guid> GetAdminIdFromToken()
        {
            var adminIdValue = await Task.FromResult(_httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(x => x.Type == "id")?.Value);

            if (adminIdValue == null)
            {
                throw new Exception("Admin ID claim is missing");
            }

            return Guid.Parse(adminIdValue);
        }
    }
}