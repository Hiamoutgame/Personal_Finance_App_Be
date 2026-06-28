# Financial Accounts — SePay Callback

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `GET /api/v1/financial-accounts/sepay/callback` |
| Auth | Public (Anonymous) |
| Status thành công | `302 Found`, `200 OK` hoặc `400 Bad Request` tùy luồng |
| Status hiện tại (code) | Chuyển hướng (redirect) nếu có `redirectUrl`, ngược lại `200 OK`/`400 Bad Request` |

## Mục đích

Nhận lệnh gọi lại (callback) từ SePay sau khi người dùng đồng ý cấp quyền liên kết. Trao đổi mã `code` để lấy token và tự động tạo hoặc cập nhật tài khoản liên kết (linked financial account).

## Request

*Truyền qua tham số query (Query Params)*

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| code | string hay null | ❌ | Mã xác thực trả về từ SePay |
| state | string | ✅ | Trạng thái bảo mật để chống CSRF |
| error | string hay null | ❌ | Mã lỗi nếu người dùng từ chối hoặc gặp sự cố |

## Response

```json
{
  "success": true,
  "message": "string",
  "financialAccountId": "guid | null",
  "redirectUrl": "string | null"
}
```

*(Nếu không có URL chuyển hướng (redirectUrl), API sẽ trả về JSON bên trên)*

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| success | boolean | Trạng thái xử lý thành công hay thất bại |
| message | string | Lời nhắn chi tiết |
| financialAccountId | guid hay null | ID của nguồn tiền được tạo hoặc liên kết |
| redirectUrl | string hay null | Đường dẫn chuyển hướng trả về FE |

## Luồng xử lý

1. SePay chuyển hướng lại (redirect) về callback kèm theo `state`.
2. Dịch vụ tìm phiên bản kết nối ngân hàng (`BankConnectionSession`) thông qua `state`.
3. Nếu có mã `code`, tiến hành đổi lấy token thông qua `SepayClient`.
4. Gọi API lấy thông tin tài khoản ngân hàng từ SePay, sau đó thêm mới hoặc cập nhật thông tin vào bảng `financial_accounts` với chế độ `LinkedApi`.
5. Đóng phiên bản kết nối và thực hiện chuyển hướng trình duyệt về FE nếu URL được định nghĩa trước.

## Quy tắc nghiệp vụ

- **Ownership**: Quyền sở hữu được xác định thông qua trạng thái phiên (`session state`), không dựa trên mã JWT.
- **Validation**: Đảm bảo trạng thái `state` hợp lệ và chưa hết hạn; xử lý các mã lỗi nếu provider trả về.
- **Side effects**: Cập nhật trạng thái session, tạo hoặc cập nhật thông tin tài khoản liên kết ngân hàng, và lưu thông tin mã token bảo mật.
- **Security**: Tuyệt đối không trả về thông tin các token nhạy cảm từ SePay cho phía Front-End.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 400 | BAD_REQUEST | Mã trạng thái `state` không hợp lệ hoặc bị thiếu, phía SePay trả về lỗi |
| 500 | INTERNAL_ERROR | Lỗi gọi sang dịch vụ hoặc đổi token từ phía SePay |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/BankConnection/Service.cs` |
| Provider Client | `Personal_Finance_Management.Service/Sepay/SepayClient.cs` |
| Entity | `BankConnectionSession`, `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-FA-003 | API này có tính năng callback với người dùng ẩn danh (anonymous) nên đang bị cấu hình ẩn trên Swagger | FE không cần xem trên Swagger nhưng cần hiểu về luồng chuyển hướng (redirect) |

## Checklist

- [ ] Tham số `state` được kiểm tra chặt chẽ để chống tấn công CSRF.
- [ ] Thông tin token nhạy cảm đã được bảo vệ mã hóa trước khi lưu vào DB.
- [ ] Phía FE chỉ nhận trạng thái kết quả hoặc lệnh chuyển hướng.
