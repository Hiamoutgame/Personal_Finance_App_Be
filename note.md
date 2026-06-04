Rõ, tôi chỉ đưa giải pháp, không động vào code. Dưới đây là phương án cho từng vấn đề, sắp theo thứ tự nên làm trước.

🔴 Nhóm 1 — Gom nghiệp vụ Limit về một chỗ
Vấn đề 1, 4 (logic tính currentSpent nhân đôi + code chết).
Giải pháp: tạo một service nghiệp vụ dùng chung, ví dụ ISpendingLimitEvaluator đặt trong feature Limit/, chịu trách nhiệm duy nhất cho:

tính số đã chi theo jar/category trong kỳ,
so ngưỡng Alert/Exceeded,
sinh notification (qua Notification service).
Sau đó:

Limit/Service.GetLimits() gọi cùng hàm tính đã chi này.
Transaction/Service không còn GetCurrentSpentByJar/ByCategory/CheckJarLimit/CheckCategoryLimit nữa, chỉ gọi evaluator.EvaluateAsync(jarId/categoryId, userId) sau khi tạo transaction.
Xoá AddLimitNotificationIfNotExists (code chết).
Lợi ích: đổi quy tắc limit chỉ sửa 1 file; Transaction service mỏng lại.

Vấn đề 3 (dedup notification bằng chuỗi).
Giải pháp: định danh notification bằng cột có cấu trúc, không phải so Body/JSON text. Thêm vào entity Notification các cột như LimitId, TargetType, ThresholdType (Alert/Exceeded) + PeriodKey (ví dụ 2026-06). Dedup = query đúng các cột này. Khi đó đổi câu chữ thông báo không làm hỏng logic, và mỗi kỳ vẫn cảnh báo lại được. Cần 1 migration thêm cột.

Vấn đề 2 (Period không reset).
Cần xác nhận trước có job reset không. Hai hướng:

(Khuyên dùng) Tính theo cửa sổ kỳ động: bỏ phụ thuộc ResetAt cứng, suy ra mốc đầu kỳ từ Period + thời điểm hiện tại (đầu ngày/đầu tuần/đầu tháng) ngay lúc tính. Không cần job, không có trạng thái lệch.
Hoặc giữ ResetAt nhưng thêm HostedService (giống BroadcastDispatchBackgroundService đã có) reset định kỳ — phức tạp hơn, dễ trôi.
🟠 Nhóm 2 — An toàn tiền bạc
Vấn đề 5 (race condition số dư).
Giải pháp: thêm optimistic concurrency token ([Timestamp] byte[] RowVersion hoặc xmin của Postgres) lên Jar, FinancialAccount. EF sẽ ném DbUpdateConcurrencyException khi có ghi đè → bắt lại và retry/báo lỗi. Cần migration + cấu hình IsRowVersion()/UseXminAsConcurrencyToken() trong AppDbContext.

Vấn đề 7 (God method CreateTransaction/UpdateTransaction).
Giải pháp: tách ma trận transfer thành các handler theo ý đồ, ví dụ enum MovementKind { JarToJar, AccountToJar, JarToAccount, ExpenseFromJar, IncomeToAccount } + mỗi loại một method nhỏ ApplyXxx(transaction). CreateTransaction chỉ: validate → xác định kind → gọi handler → save → evaluate limit/goal. UpdateTransaction tái dùng cùng handler. Dễ đọc, dễ test từng nhánh.

Vấn đề 6 (không có test).
Giải pháp: thêm project test (xUnit) dùng EF Core InMemory hoặc SQLite in-memory, phủ trước cho: balance sau mỗi loại transfer, chặn số dư âm, và evaluate limit (alert/exceeded/dedup). Ưu tiên đúng phần mutate tiền vì rủi ro cao nhất. (Việc số 7 tách nhỏ sẽ làm việc này dễ hơn nhiều.)

🟡 Nhóm 3 — Nhất quán & quy ước (rẻ, làm dần)
Vấn đề 8 (xử lý lỗi lẫn lộn).
Giải pháp: quy ước chỉ dùng AppValidationException.BadRequest/NotFound cho lỗi nghiệp vụ. Thay hết throw new Exception(...), throw new (...), KeyNotFoundException bằng exception chuẩn để GlobalExceptionHandlerMiddleware trả response đồng nhất. Có thể bổ sung handler bắt cả KeyNotFoundException như lưới an toàn trong giai đoạn chuyển tiếp.

Vấn đề 11 (ICurrentUserAccessor bị bỏ phí + GetCurrentUserId() lặp ở mọi service).
Giải pháp: dùng ICurrentUserAccessor đã đăng ký sẵn — hoặc tạo một BaseService abstract chứa GetCurrentUserId() để các service kế thừa. Xoá phần private Guid GetCurrentUserId() lặp ở từng file. Inject ICurrentUserAccessor thay cho IHttpContextAccessor thô.

Vấn đề 9, 10 (namespace casing + tên Service/IService chung chung).
Giải pháp (làm khi rảnh, dùng rename của IDE):

Chuẩn hoá namespace về PascalCase nhất quán (Service.Limit, Service.Goal…). Sau đó xoá phần lớn alias using trong Program.cs.
Đổi tên class/interface theo feature: LimitService : ILimitService thay vì Service : IService. Stack trace và điều hướng rõ hơn hẳn. Thống nhất IService (số ít), bỏ IServices.
Vấn đề 12 (chuỗi hiển thị + file thừa).
Giải pháp: đưa hết message ra ErrorMessages/Defaults (đã có sẵn), thống nhất tiếng Việt có dấu (sửa "Thong bao vuot nguong" → "Thông báo vượt ngưỡng"). Xoá WeatherForecast.cs.

Thứ tự đề xuất
Nhóm 1 (gom Limit, sửa dedup, làm rõ Period) — đụng đúng nhánh fix-be-limit bạn đang làm.
Nhóm 2 (concurrency + tách method + test) — rủi ro tiền bạc.
Nhóm 3 — dọn dần, an toàn, dùng rename tự động.
Mỗi nhóm 1/2 nên là một PR riêng để dễ review. Bạn muốn tôi soạn một tài liệu kế hoạch chi tiết (có các bước thực thi cụ thể) cho Nhóm 1 để dành cho lúc bắt tay vào sửa không?
