namespace Personal_Finance_Management.Service.AI;

// {
//     "answer": "Tháng này bạn đang chi nhanh hơn bình thường ở nhóm ăn uống và mua sắm. Nếu tiếp tục tốc độ này, bạn có thể vượt ngân sách trước cuối tháng.",
//     "suggestions": [
//     "Giảm chi ăn ngoài trong 7 ngày tới.",
//     "Kiểm tra lại các giao dịch mua sắm gần đây trước khi tạo khoản chi mới.",
//     "Theo dõi các hạn mức đang gần ngưỡng cảnh báo."
//         ],
//     "source": "AI"
// }
public class Response
{
    public class AnswerResponse
    {
        public string Answer { get; set; }
        public List<string>? Suggestions { get; set; }
        public string Source { get; set; }
    }

    public class AdminAiSettingsResponse
    {
        public string ModelName { get; set; } = null!;
        public string SystemPrompt { get; set; } = null!;
        public decimal Temperature { get; set; }
        public int MaxTokens { get; set; }
        public bool IsEnabled { get; set; }
        public string? ApiKeyMasked { get; set; }
    }

    public class UpdateAiSettingsResponse
    {
        public string ModelName { get; set; } = null!;
        public bool IsEnabled { get; set; }
    }
}
