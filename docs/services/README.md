# Tài liệu Dịch vụ FinJar (FinJar Service Docs)

Thư mục này là lớp tài liệu ngắn (micro-docs) cho từng API/service của backend FinJar. Mục tiêu là giúp các sub-agent/developer đọc nhanh contract, code hiện tại và drift trước khi sửa code.

## Hướng dẫn chung

1. Đọc `_meta/source-priority.md` để biết nguồn nào được ưu tiên.
2. Đọc `_meta/template.md` trước khi tạo/sửa tài liệu endpoint.
3. Đọc `_meta/service-map.md` để ánh xạ controller -> service -> DTO -> entity/schema.
4. Đọc `_meta/drift-register.md` nếu endpoint có mâu thuẫn giữa tài liệu và code.
5. Đọc file endpoint trong module cần làm.

---

## Lưu ý chung cho Front-End (FE)

Để tránh lặp lại thông tin ở mỗi file endpoint, tất cả các tích hợp FE cần tuân thủ các quy tắc sau:

1. **Xác thực (Authentication)**:
   - Gửi header `Authorization: Bearer <accessToken>` cho tất cả các endpoint yêu cầu quyền truy cập (Auth ghi `Bearer User` hoặc `Bearer Admin`).
   - Các endpoint ghi `Public` không cần truyền token.

2. **Quy ước đặt tên (Naming Convention)**:
   - Payload gửi lên (Request body) và dữ liệu nhận về (Response body) luôn sử dụng kiểu **camelCase** (ví dụ: `monthlyIncome`, `preferredCurrency`, `isOnboardingCompleted`) theo đúng [conventions.md](../conventions.md).

3. **Cấu trúc phản hồi lỗi (Error Envelope)**:
   - Khi API trả về lỗi (mã trạng thái 4xx/5xx), cấu trúc lỗi luôn tuân theo định dạng chuẩn:
     ```json
     {
       "success": false,
       "error": "Mô tả lỗi ngắn gọn cho user",
       "details": {
         "field": "tên_trường_bị_lỗi",
         "code": "MÃ_LỖI"
       },
       "traceId": "guid"
     }
     ```

4. **Số tiền (Amount Sign Convention)**:
   - FE luôn gửi giá trị số tiền là **số dương** cho cả Thu nhập (Income) và Chi phí (Expense). Hệ thống sẽ tự động xử lý dấu âm/dương tương ứng khi lưu trữ vào cơ sở dữ liệu.

---

## Trạng thái module

| Module | Trạng thái (Status) | Ghi chú |
| --- | --- | --- |
| Meta | Xong (Done) | Đã có template mới, nguồn ưu tiên, danh sách lệch và bản đồ dịch vụ. |
| Auth | Xong (Done) | Đăng ký/đăng nhập/đăng xuất đã tách theo endpoint. |