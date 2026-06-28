# Mức độ ưu tiên của nguồn (Source Priority)

Tài liệu service phải tách rõ 2 loại sự thật (truth):

- **Contract mong muốn**: những gì FE/consumer nên nhìn thấy.
- **Implementation hiện tại**: những gì controller/service/schema đang làm trong mã nguồn thực tế (checkout).

## Thứ tự đối chiếu

1. `docs/conventions.md`: quy ước chung về route, JSON naming, dấu của số tiền (amount sign), cấu trúc lỗi (error envelope), enum, phân trang, bảo mật.
2. `docs/API V2.md`: endpoint contract công khai hiện tại.
3. `docs/flow.md`: luồng hướng người dùng và cách FE gọi API.
4. `docs/schema/finjar_schema.sql`: schema mục tiêu. Nếu kho lưu trữ có schema cũ ở `docs/finjar_schema.sql`, ưu tiên sử dụng đường dẫn `docs/schema/finjar_schema.sql` trong mã nguồn hiện tại.
5. Code controller/service/DTO/entity trong `Personal_Finance_Management`.
6. Ngữ cảnh vận hành/AGENTS trong cuộc trò chuyện (conversation): dùng để phát hiện sự sai lệch (drift) và cạm bẫy (pitfall), không tự động ghi đè tài liệu trong kho (repo docs) nếu chưa có quyết định của chủ dự án (owner decision).

## Quy tắc khi mâu thuẫn

- Không sửa nghiệp vụ trong endpoint doc bằng cách đoán.
- Ghi rõ `Contract mong muốn` và `Implementation hiện tại`.
- Thêm một dòng vào `drift-register.md` nếu độ lệch (drift) có ảnh hưởng đến FE, DB, bảo mật, luồng tiền (money movement) hoặc route công khai.
- Endpoint đang chờ (pending) trong docs nhưng chưa có controller thì không tạo tài liệu endpoint hoạt động (active endpoint doc); ghi vào drift/backlog.
- Endpoint có controller nhưng thiếu trong `API V2.md` thì vẫn tạo tài liệu theo implementation và ghi nhận sự sai lệch (drift).

## Điều không được làm

- Không hồi sinh (revive) các endpoint cũ: jar setup/transfer/transfers, endpoint lấy số dư tài khoản riêng lẻ, Casso legacy route.
- Không sao chép API key, chuỗi kết nối (connection string), provider secret hoặc token vào tài liệu.
- Không để lộ (expose) `ApiKeyEncrypted`, raw provider token, password hash, thông báo nhắc nội bộ (internal prompt/secret).
- Không coi health endpoint là API nghiệp vụ; health là ngoại lệ chỉ dành cho hệ thống (ops-only) nằm ngoài đường dẫn `/api/v1`.
