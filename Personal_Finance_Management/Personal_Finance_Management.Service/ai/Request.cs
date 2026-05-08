namespace Personal_Finance_Management.Service.AI;
//     {
    //     "message": "Tháng này tui có đang tiêu quá tay không?",
    //     "recentMessages": [
        //     {
        //         "sender": "User",
        //         "content": "Tháng này tui chi ổn không?"
        //     },
        //     {
        //         "sender": "AI",
        //         "content": "Bạn đang chi ăn uống hơi cao so với mức chi tháng này."
        //     }
//     ]
// }
public class Request
{
    public class ChatBoxRequest
    {
        public required string Message { get; set; }
        public List<RecentMessage>? RecentMessages { get; set; }
    }
    public class RecentMessage
    {
        public required string Sender { get; set; }
        public required string Content { get; set; }
    }

    public class UpdateAiSettingsRequest
    {
        public string? ModelName { get; set; }
        public string? SystemPrompt { get; set; }
        public decimal? Temperature { get; set; }
        public int? MaxTokens { get; set; }
        public bool? IsEnabled { get; set; }
    }
}
