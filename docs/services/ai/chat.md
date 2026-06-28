# AI — Chat

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/ai/chat` |
| Auth | Bearer User |
| Status thành công | `200 OK` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Cung cấp dịch vụ trò chuyện tư vấn tài chính cá nhân bằng AI (sử dụng Gemini) cho người dùng hiện tại, có cơ chế tự động chuyển sang câu trả lời quy tắc (fallback rule-based) nếu kết nối AI bị lỗi hoặc tắt.

## Request

```json
{
  "message": "string",
  "recentMessages": [
    {
      "sender": "string",
      "content": "string"
    }
  ]
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| message | string | ✅ | Nội dung câu hỏi mới nhất từ người dùng |
| recentMessages | mảng object | ❌ | Lịch sử các câu hỏi và trả lời trước đó |

## Response

```json
{
  "answer": "string",
  "suggestions": ["string"],
  "source": "string"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| answer | string | Câu trả lời từ AI hoặc câu trả lời tự động (fallback) |
| suggestions | mảng string | Các câu hỏi gợi ý tiếp theo dành cho người dùng |
| source | string | Nguồn gốc câu trả lời (`AI` hoặc `RuleBased`) |

## Luồng xử lý

1. `AIChatController.Chat` nhận yêu cầu và gọi đến phương thức `AI.IService.ChatBot`.
2. Dịch vụ phân tích lấy ngữ cảnh tài chính của người dùng hiện tại (nếu cần thiết).
3. Tải các cấu hình hoạt động của AI (`AiSetting`) từ DB.
4. Gửi yêu cầu kèm theo lịch sử trò chuyện sang nhà cung cấp AI (Google Gemini).
5. Nếu cuộc gọi thành công, trả về câu trả lời của AI. Nếu thất bại hoặc tính năng bị tắt, kích hoạt hệ thống trả lời tự động dựa trên quy tắc (rule-based).

## Quy tắc nghiệp vụ

- **Ownership**: Chỉ xử lý và trả lời dựa theo ngữ cảnh tài chính của chính người dùng hiện tại.
- **Validation**: Đảm bảo tin nhắn gửi lên không vượt quá giới hạn ký tự và không trống.
- **Side effects**: Không làm thay đổi trực tiếp số dư hay thông tin tài chính, có thể lưu nhật ký chat nếu cần thiết.
- **External side effects**: Gọi HTTP Request sang dịch vụ Google Gemini AI bên ngoài.
- **Security**: Không được phép trả về khóa bảo mật (`ApiKeyEncrypted`), các prompt hướng dẫn hệ thống nhạy cảm (system prompt) cho người dùng.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực không hợp lệ |
| 422 | VALIDATION_FAILED | Tin nhắn trống hoặc sai định dạng |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/AIChatController.cs` |
| Service | `Personal_Finance_Management.Service/ai/Service.cs` |
| DTO | `Personal_Finance_Management.Service/ai/Request.cs`, `Response.cs` |
| Entity | `AiSetting` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-AI-001 | Sự cố kết nối vùng (regional failure) của nhà cung cấp AI cần được ghi nhận kỹ trên môi trường Render | Có thể tự động kích hoạt fallback nhiều hơn bình thường |

## Checklist

- [ ] Khi dịch vụ AI bên ngoài gặp sự cố, hệ thống vẫn phản hồi câu trả lời dựa trên quy tắc (rule-based) an toàn.
- [ ] Các thông tin mật như API key hoặc prompt gốc không bị lộ ra ngoài.
- [ ] Dữ liệu tài chính dùng làm ngữ cảnh chỉ thuộc về người dùng đang trò chuyện.