using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Service.Base;

namespace Personal_Finance_Management.Service.notification;

public class Service : IService
{
    private readonly AppDbContext _appDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
    {
        _appDbContext = appDbContext;
        _httpContextAccessor = httpContextAccessor;
    }
    private Guid GetCurrentUserId()
    {
        return ServiceClaimHelper.GetRequiredUserId(_httpContextAccessor);
    }
    
    
    public async Task<Response.GetNotificationsResponse> GetNotifications(string? type, string? status, int pageSize, int pageIndex)
    {
        var userIdGuid = GetCurrentUserId();

        
        var query = _appDbContext.Notifications.Where(n => n.UserId == userIdGuid);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(n => n.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.ToLower() == "read") 
                query = query.Where(n => n.IsRead == true);
            else if (status.ToLower() == "unread") 
                query = query.Where(n => n.IsRead == false);
        }
        
        int totalItems = await query.CountAsync();

        var pagedData = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new Response.NotificationResponse()
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                MetadataJson = n.MetadataJson,
                OccurredAt = n.CreatedAt
            })
            .ToListAsync();
        
        int unreadCount = await _appDbContext.Notifications.CountAsync(n => n.UserId == userIdGuid && n.IsRead == false);
        
        return new Response.GetNotificationsResponse
        {
            Items = pagedData,
            TotalItems = totalItems,
            PageSize = pageSize,
            PageIndex = pageIndex,
            UnreadCount = unreadCount
        };
    }

    public async Task<Response.UpdateStatusResponse> UpdateStatus(Request.UpdateStatusRequest request)
    {
        var userIdGuid = GetCurrentUserId();

        var query = _appDbContext.Notifications.Where(n => n.UserId == userIdGuid);
        
        if (request.MarkAll == false)
        {
            if (request.Ids == null)
                throw new Exception(" chọn MarkAll = true");
                
            query = query.Where(n => request.Ids.Contains(n.Id));
        }

        var notificationsToUpdate = await query.ToListAsync();
        int updatedCount = 0;
        var now = DateTimeOffset.UtcNow;
        
        foreach (var notification in notificationsToUpdate)
        {
            if (notification.IsRead != request.IsRead)
            {
                notification.IsRead = request.IsRead;
                
                if (request.IsRead == true) 
                {
                    notification.ReadAt = now; 
                }
                else 
                {
                    notification.ReadAt = null; 
                }
                
                updatedCount = updatedCount + 1;
            }
        }

        await _appDbContext.SaveChangesAsync();
        
        int unreadCount = await _appDbContext.Notifications.CountAsync(n => n.UserId == userIdGuid && n.IsRead == false);

        return new Response.UpdateStatusResponse
        {
            UpdatedCount = updatedCount,
            UnreadCount = unreadCount
        };
    }

    
}
