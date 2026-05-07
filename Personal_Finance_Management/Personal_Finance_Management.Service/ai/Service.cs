using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Validations;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Personal_Finance_Management.Service.AI;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(
        AppDbContext dbContext,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.AdminAiSettingsResponse> GetAdminAiSettings()
    {
        var setting = await _dbContext.AiSettings
            .AsNoTracking()
            .OrderByDescending(ai => ai.UpdatedAt)
            .FirstOrDefaultAsync();

        if (setting is null)
        {
            return new Response.AdminAiSettingsResponse
            {
                ModelName = GetDefaultModelName(),
                SystemPrompt = "",
                Temperature = 0.7m,
                MaxTokens = 1000,
                IsEnabled = false,
                ApiKeyMasked = MaskApiKey(_configuration["GoogleAI:ApiKey"])
            };
        }

        return new Response.AdminAiSettingsResponse
        {
            ModelName = setting.ModelName,
            SystemPrompt = setting.SystemPrompt,
            Temperature = setting.Temperature,
            MaxTokens = setting.MaxTokens,
            IsEnabled = setting.IsEnabled,
            ApiKeyMasked = MaskApiKey(_configuration["GoogleAI:ApiKey"])
        };
    }

    public async Task<Response.UpdateAiSettingsResponse> UpdateAdminAiSettings(Request.UpdateAiSettingsRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        var adminId = GetCurrentAdminId();
        var now = DateTimeOffset.UtcNow;

        var setting = await _dbContext.AiSettings
            .OrderByDescending(ai => ai.UpdatedAt)
            .FirstOrDefaultAsync();

        if (setting is null)
        {
            setting = new AiSetting
            {
                Id = Guid.NewGuid(),
                ModelName = GetDefaultModelName(),
                SystemPrompt = "",
                Temperature = 0.7m,
                MaxTokens = 1000,
                IsEnabled = false
            };
            _dbContext.AiSettings.Add(setting);
        }

        if (request.ModelName is not null)
        {
            setting.ModelName = NormalizeRequiredText(
                request.ModelName,
                "modelName",
                "Model name is required.");
        }

        if (request.SystemPrompt is not null)
        {
            setting.SystemPrompt = NormalizeRequiredText(
                request.SystemPrompt,
                "systemPrompt",
                "System prompt is required.");
        }

        if (request.Temperature.HasValue)
        {
            if (request.Temperature.Value < 0 || request.Temperature.Value > 2)
            {
                throw AppValidationException.BadRequest(
                    "Temperature must be between 0 and 2.",
                    "temperature",
                    "AI_SETTING_INVALID");
            }

            setting.Temperature = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            if (request.MaxTokens.Value <= 0)
            {
                throw AppValidationException.BadRequest(
                    "Max tokens must be greater than 0.",
                    "maxTokens",
                    "AI_SETTING_INVALID");
            }

            setting.MaxTokens = request.MaxTokens.Value;
        }

        if (request.IsEnabled.HasValue)
        {
            setting.IsEnabled = request.IsEnabled.Value;
        }

        setting.UpdatedAt = now;
        setting.UpdatedByAdminId = adminId;

        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorAccountId = adminId,
            ActionType = "AiSettingChange",
            EntityType = "AiSetting",
            EntityId = setting.Id,
            Description = $"Updated AI settings: {setting.ModelName}",
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        return new Response.UpdateAiSettingsResponse
        {
            ModelName = setting.ModelName,
            IsEnabled = setting.IsEnabled
        };
    }

    private static Response.AnswerResponse BuildRuleBasedFallback()
    {
        return new Response.AnswerResponse
        {
            Answer =
                "Dựa trên dữ liệu hiện tại, bạn nên kiểm tra lại các khoản chi gần đây và các hạn mức đang hoạt động.",
            Suggestions =
            [
                "Kiểm tra top danh mục chi tiêu trong tháng hiện tại.",
                "Đặt hoặc điều chỉnh hạn mức cho nhóm chi tiêu thường vượt ngưỡng."
            ],
            Source = "RuleBased"
        };
    }

    private string GetDefaultModelName()
    {
        return _configuration["GoogleAI:DefaultModel"]?.Trim() ?? "gemini-2.0-flash";
    }

    private Guid GetCurrentAdminId()
    {
        var adminIdValue = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "id")?.Value;

        if (!Guid.TryParse(adminIdValue, out var adminId))
        {
            throw new UnauthorizedAccessException("Admin ID claim is missing");
        }

        return adminId;
    }

    private static string NormalizeRequiredText(string value, string field, string message)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw AppValidationException.BadRequest(message, field, "REQUIRED");
        }

        return normalizedValue;
    }

    private static string? MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var trimmedKey = apiKey.Trim();
        if (trimmedKey.Length <= 8)
        {
            return "****";
        }

        return $"{trimmedKey[..4]}...{trimmedKey[^4..]}";
    }
    
    
}