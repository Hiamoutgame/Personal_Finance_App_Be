using System.Net.Http.Json;
using System.Text.Json;
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



    public async Task<Response.AnswerResponse> ChatBot(Request.ChatBoxRequest request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
        }

        var message = NormalizeRequiredText(request.Message, "message", "Message is required.");
        ValidateRecentMessages(request.RecentMessages);

        var userId = GetCurrentUserId();
        var setting = await _dbContext.AiSettings
            .AsNoTracking()
            .OrderByDescending(ai => ai.UpdatedAt)
            .FirstOrDefaultAsync();
        var effectiveSetting = setting ?? new AiSetting
        {
            Id = Guid.Empty,
            ModelName = _configuration["GoogleAI:DefaultModel"]!,
            SystemPrompt = _configuration["GoogleAI:SystemPrompt"] ?? "",
            Temperature = _configuration.GetValue("GoogleAI:Temperature", 0.7m),
            MaxTokens = _configuration.GetValue("GoogleAI:MaxTokens", 1000),
            IsEnabled = _configuration.GetValue("GoogleAI:IsEnabled", true),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var apiKey = (_configuration["GoogleAI:ApiKey"])?.Trim();
        if (!effectiveSetting.IsEnabled || string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildRuleBasedFallback();
        }

        try
        {
            var prompt = await BuildChatPrompt(userId, message, request.RecentMessages, effectiveSetting);
            var answer = await CallGoogleAi(
                effectiveSetting,
                apiKey,
                prompt,
                _configuration.GetValue("GoogleAI:TimeoutSeconds", 30));

            if (string.IsNullOrWhiteSpace(answer))
            {
                return BuildRuleBasedFallback();
            }

            return new Response.AnswerResponse
            {
                Answer = answer.Trim(),
                Suggestions =
                [
                    "Xem lại các giao dịch chi tiêu gần đây để tìm nguyên nhân.",
                    "Kiểm tra các hạn mức đang gần ngưỡng cảnh báo.",
                    "Điều chỉnh nhu cầu chi tiêu nếu cần."
                ],
                Source = "AI"
            };
        }
        catch
        {
            return BuildRuleBasedFallback();
        }
    }

    public async Task<Response.AdminAiSettingsResponse> GetAdminAiSettings()
    {
        var setting = await _dbContext.AiSettings
            .AsNoTracking()
            .OrderByDescending(ai => ai.UpdatedAt)
            .FirstOrDefaultAsync();
        var apiKey = (_configuration["GoogleAI:ApiKey"])?.Trim();

        if (setting is null)
        {
            return new Response.AdminAiSettingsResponse
            {
                ModelName = _configuration["GoogleAI:DefaultModel"]!,
                SystemPrompt = _configuration["GoogleAI:SystemPrompt"] ?? "",
                Temperature = _configuration.GetValue("GoogleAI:Temperature", 0.7m),
                MaxTokens = _configuration.GetValue("GoogleAI:MaxTokens", 1000),
                IsEnabled = _configuration.GetValue("GoogleAI:IsEnabled", true),
                ApiKeyMasked = MaskApiKey(apiKey)
            };
        }

        return new Response.AdminAiSettingsResponse
        {
            ModelName = setting.ModelName,
            SystemPrompt = setting.SystemPrompt,
            Temperature = setting.Temperature,
            MaxTokens = setting.MaxTokens,
            IsEnabled = setting.IsEnabled,
            ApiKeyMasked = MaskApiKey(apiKey)
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
                ModelName = _configuration["GoogleAI:DefaultModel"]!,
                SystemPrompt = _configuration["GoogleAI:SystemPrompt"] ?? "",
                Temperature = _configuration.GetValue("GoogleAI:Temperature", 0.7m),
                MaxTokens = _configuration.GetValue("GoogleAI:MaxTokens", 1000),
                IsEnabled = _configuration.GetValue("GoogleAI:IsEnabled", true)
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
    private async Task<string> BuildChatPrompt(
        Guid userId,
        string message,
        List<Request.RecentMessage>? recentMessages,
        AiSetting setting)
    {
        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var accounts = await _dbContext.FinancialAccounts
            .AsNoTracking()
            .Where(account => account.UserId == userId && account.IsActive)
            .Select(account => new
            {
                account.Name,
                account.AccountType,
                account.CurrentBalance,
                account.Currency,
                account.IsDefault
            })
            .ToListAsync();

        var jars = await _dbContext.Jars
            .AsNoTracking()
            .Where(jar => jar.UserId == userId && jar.Status == "Active")
            .Select(jar => new
            {
                jar.Name,
                jar.Balance,
                jar.Currency
            })
            .ToListAsync();

        var monthTransactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId
                && !transaction.IsDeleted
                && transaction.TransactionDate >= monthStart
                && transaction.TransactionDate <= now)
            .Select(transaction => new
            {
                transaction.Type,
                transaction.TransactionsAmount,
                transaction.Note,
                transaction.TransactionDate,
                CategoryName = transaction.Category == null ? null : transaction.Category.Name
            })
            .ToListAsync();

        var incomeThisMonth = monthTransactions
            .Where(transaction => transaction.Type == "Income")
            .Sum(transaction => Math.Abs(transaction.TransactionsAmount));

        var expenseThisMonth = monthTransactions
            .Where(transaction => transaction.Type == "Expense")
            .Sum(transaction => Math.Abs(transaction.TransactionsAmount));

        var topCategories = monthTransactions
            .Where(transaction => transaction.Type == "Expense")
            .GroupBy(transaction => transaction.CategoryName ?? "Uncategorized")
            .Select(group => new
            {
                Category = group.Key,
                Amount = group.Sum(item => Math.Abs(item.TransactionsAmount))
            })
            .OrderByDescending(item => item.Amount)
            .Take(5)
            .ToList();

        var recentTransactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && !transaction.IsDeleted)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Take(8)
            .Select(transaction => new
            {
                transaction.Type,
                transaction.TransactionsAmount,
                transaction.Note,
                transaction.TransactionDate,
                CategoryName = transaction.Category == null ? null : transaction.Category.Name
            })
            .ToListAsync();

        var safeRecentMessages = (recentMessages ?? [])
            .TakeLast(6)
            .Select(item => new
            {
                Sender = item.Sender.Trim(),
                Content = Truncate(item.Content.Trim(), 500)
            })
            .ToList();

        var context = new
        {
            Currency = "VND",
            UserQuestion = message,
            Accounts = accounts,
            Jars = jars,
            ThisMonth = new
            {
                Income = incomeThisMonth,
                Expense = expenseThisMonth,
                Net = incomeThisMonth - expenseThisMonth,
                TopSpendingCategories = topCategories
            },
            RecentTransactions = recentTransactions,
            RecentMessages = safeRecentMessages
        };

        var systemPrompt = string.IsNullOrWhiteSpace(setting.SystemPrompt)
            ? "You are FinJar, a personal finance assistant. Answer in Vietnamese. Be concise, practical, and avoid exposing secrets or raw internal data."
            : setting.SystemPrompt.Trim();

        return systemPrompt
               + Environment.NewLine
               + "Use this JSON financial context. Return only a helpful answer, not JSON."
               + Environment.NewLine
               + JsonSerializer.Serialize(context);
    }

    private static async Task<string?> CallGoogleAi(
        AiSetting setting,
        string apiKey,
        string prompt,
        int timeoutSeconds)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };

        var modelName = setting.ModelName.Trim();

        var requestUri =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelName)}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = setting.Temperature,
                maxOutputTokens = setting.MaxTokens
            }
        };

        using var response = await httpClient.PostAsJsonAsync(requestUri, payload);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(responseStream);

        if (!json.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.ValueKind != JsonValueKind.Array
            || candidates.GetArrayLength() == 0)
        {
            return null;
        }

        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var parts)
            || parts.ValueKind != JsonValueKind.Array
            || parts.GetArrayLength() == 0)
        {
            return null;
        }

        return parts[0].TryGetProperty("text", out var text)
            ? text.GetString()
            : null;
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

    private Guid GetCurrentUserId()
    {
        var userIdValue = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "id")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim is missing");
        }

        return userId;
    }

    private static void ValidateRecentMessages(List<Request.RecentMessage>? recentMessages)
    {
        if (recentMessages is null)
        {
            return;
        }

        foreach (var recentMessage in recentMessages)
        {
            var sender = NormalizeRequiredText(
                recentMessage.Sender,
                "sender",
                "Recent message sender is required.");

            if (sender != "User" && sender != "AI")
            {
                throw AppValidationException.BadRequest(
                    "Recent message sender must be User or AI.",
                    "sender",
                    "AI_CHAT_INVALID");
            }

            NormalizeRequiredText(
                recentMessage.Content,
                "content",
                "Recent message content is required.");
        }
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

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
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
