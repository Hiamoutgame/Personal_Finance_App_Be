using Microsoft.AspNetCore.Http;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Base;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }
    bool TryGetUserId(out Guid userId);
}

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            if (!TryGetUserId(out var id))
            {
                throw AppValidationException.Unauthorized(
                    "User id claim is missing or invalid.",
                    ErrorCodes.Unauthorized);
            }
            return id;
        }
    }

    public bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var raw = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == AppClaimTypes.Id)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
