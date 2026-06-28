# Financial Accounts — Create Manual

## Endpoint

| Thuộc tính | Giá trị |
|------------|---------|
| Method | `POST /api/v1/financial-accounts/Manual` |
| Auth | Bearer User |
| Status thành công | `201 Created` |
| Status hiện tại (code) | `200 OK` |

## Mục đích

Tạo nguồn tiền thủ công như tiền mặt (cash), ngân hàng tự theo dõi, ví điện tử (e-wallet) hoặc loại khác (other) cho người dùng hiện tại.

## Request

```json
{
  "name": "string",
  "accountType": "string",
  "currentBalance": "decimal",
  "currency": "string | null",
  "isDefault": "boolean"
}
```

| Trường | Kiểu | Bắt buộc | Ghi chú |
|--------|------|----------|---------|
| name | string | ✅ | Tên nguồn tiền |
| accountType | string | ✅ | Phân loại nguồn tiền |
| currentBalance | decimal | ✅ | Số dư ban đầu |
| currency | string hay null | ❌ | Tiền tệ sử dụng (ví dụ: VND) |
| isDefault | boolean | ✅ | Đặt làm nguồn tiền mặc định |

## Response

```json
{
  "id": "guid",
  "name": "string",
  "accountType": "string",
  "connectionMode": "string",
  "currentBalance": "decimal",
  "currency": "string",
  "isDefault": "boolean",
  "isActive": "boolean"
}
```

| Trường | Kiểu | Ghi chú |
|--------|------|---------|
| id | guid | Định danh duy nhất |
| name | string | Tên nguồn tiền |
| accountType | string | Loại nguồn tiền |
| connectionMode | string | Chế độ kết nối (`Manual`) |
| currentBalance | decimal | Số dư hiện tại |
| currency | string | Đơn vị tiền tệ |
| isDefault | boolean | Cờ đánh dấu là nguồn tiền mặc định |
| isActive | boolean | Trạng thái hoạt động |

## Luồng xử lý

1. `CreateManualFinancialAccount` gọi tới phương thức tương ứng trong Service.
2. Dịch vụ phân tách ID của người dùng từ token.
3. Validate tính hợp lệ của phân loại (`accountType`), tiền tệ (`currency`) và số dư.
4. Tạo `FinancialAccount` với `ConnectionMode = Manual` và trạng thái đồng bộ mặc định.
5. Nếu thuộc tính `isDefault` là `true`, hệ thống sẽ gỡ bỏ cờ mặc định của các nguồn tiền khác của cùng người dùng đó.

## Quy tắc nghiệp vụ

- **Ownership**: Nguồn tiền mới tạo phải thuộc quyền sở hữu của chính người dùng hiện tại.
- **Validation**: `currentBalance` là kiểu số thập phân, `accountType` phải thuộc Enum được phép, tên không được để trống.
- **Side effects**: Thêm bản ghi vào bảng `financial_accounts`, có thể cập nhật lại cờ mặc định của tài khoản khác.
- **Security**: Tuyệt đối không cho phép người dùng tự tạo nguồn tiền thay mặt cho người dùng khác.

## Lỗi

| HTTP | Code | Khi nào |
|------|------|---------|
| 401 | UNAUTHORIZED | Token xác thực thiếu, hết hạn hoặc không hợp lệ |
| 422 | VALIDATION_FAILED | Loại nguồn tiền hoặc số dư cung cấp không hợp lệ |

## File liên quan

| Loại | Đường dẫn |
|------|-----------|
| Controller | `Personal_Finance_Management.Api/Controllers/FinancialAccountController.cs` |
| Service | `Personal_Finance_Management.Service/FinancialAccount/Service.cs` |
| DTO | `Personal_Finance_Management.Service/FinancialAccount/Request.cs`, `Response.cs` |
| Entity | `FinancialAccount` |

## Drift

| ID | Mô tả | Ảnh hưởng |
|----|--------|-----------|
| DRIFT-011 | Trạng thái mong muốn khi khởi tạo là 201 Created nhưng bộ điều khiển hiện tại trả về 200 OK | Cần đồng bộ mã trạng thái trên toàn hệ thống |

## Checklist

- [ ] Đối tượng nguồn tiền tạo thủ công phải luôn được gán thuộc tính `ConnectionMode=Manual`.
- [ ] Tính duy nhất của cờ mặc định (`isDefault`) trên phạm vi một người dùng được bảo đảm.
- [ ] Tính năng số dư ban đầu (`currentBalance` tự cấu hình) chỉ áp dụng cho tài khoản thuộc dạng thủ công (Manual).
