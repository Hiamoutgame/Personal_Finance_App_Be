03/05/2026
- ghi ở dưới đây cho tui bằng tiếng việt 

# Context Dự Án FinJar

## 1. Tổng Quan

FinJar là hệ thống quản lý tài chính cá nhân tập trung vào việc theo dõi chi tiêu, tiết kiệm và quản lý tiền theo phương pháp đa hũ. Ứng dụng không giữ tiền của người dùng, chỉ hỗ trợ ghi nhận, phân loại, cảnh báo và đưa ra gợi ý quản lý tài chính.

Mục tiêu chính của hệ thống là giảm thao tác tính toán thủ công, giúp người dùng dễ theo dõi tình hình tài chính, nhận cảnh báo khi gần vượt hạn mức và lập kế hoạch tiết kiệm theo mục tiêu.

Đối tượng hướng đến:

- Người nội trợ.
- Người không có nhiều thời gian hoặc kỹ năng quản lý tiền bạc.
- Cá nhân hoặc nhóm nhỏ cần giải pháp thay thế Excel, sổ tay hoặc cách tính thủ công.
- Doanh nghiệp nhỏ cần công cụ hỗ trợ quản lý chi tiêu cơ bản.

## 2. Vấn Đề Cần Giải Quyết

Các phương pháp truyền thống như Excel hoặc sổ tay có nhiều hạn chế:

- Tốn thời gian nhập liệu và tính toán thủ công.
- Dễ bỏ sót các khoản chi nhỏ.
- Khó tổng hợp lại lịch sử chi tiêu.
- Thiếu cảnh báo trước khi vượt hạn mức.
- Chưa gắn việc chi tiêu hằng ngày với mục tiêu tiết kiệm.
- Phân loại giao dịch thủ công gây nhàm chán.
- Người dùng thường chỉ biết mình đã tiêu quá nhiều sau khi sự việc đã xảy ra.

FinJar cần giải quyết các vấn đề này bằng cách tự động hóa nhập liệu, phân loại giao dịch, tính toán số dư hũ, cảnh báo hạn mức và đề xuất kế hoạch tài chính phù hợp.

## 3. Giải Pháp Sản Phẩm

Hệ thống cung cấp một web app/PWA giúp người dùng:

- Thực hiện khảo sát ban đầu để hiểu thói quen tài chính.
- Chọn phương pháp quản lý tiền như Six Jars, 50/30/20 hoặc Custom.
- Tạo tài khoản tài chính như tiền mặt, ngân hàng, ví điện tử.
- Ghi nhận giao dịch thu/chi bằng nhập tay hoặc import sao kê.
- Phân loại giao dịch theo hũ và danh mục.
- Đặt hạn mức chi tiêu theo ngày/tháng, theo hũ hoặc danh mục.
- Tạo mục tiêu tiết kiệm có số tiền và hạn hoàn thành.
- Nhận nhắc nhở cho các khoản định kỳ.
- Nhận thông báo và gợi ý từ hệ thống/AI.

Ứng dụng ưu tiên hai nguồn dữ liệu khả thi:

- Nhập tay: dành cho tiền mặt hoặc giao dịch nhỏ.
- Import sao kê/OCR/AI: dành cho giao dịch ngân hàng hoặc dữ liệu có file sao kê.

Các phương án như API ngân hàng, SMS banking, SePay/Casso chưa ưu tiên vì rào cản chính sách, chi phí, bảo mật hoặc độ phủ tại Việt Nam.

## 4. Vai Trò Người Dùng

Hệ thống có 2 vai trò chính:

- User: quản lý tài chính cá nhân.
- Admin: quản trị người dùng, thông báo, dashboard hệ thống và cấu hình AI.

## 5. Nhóm Chức Năng Chính

### User

- Đăng ký, đăng nhập, quản lý hồ sơ cá nhân.
- Hoàn thành onboarding profile.
- Chọn phương pháp quản lý hũ.
- Quản lý tài khoản tài chính.
- Quản lý danh mục chi tiêu.
- Quản lý hũ tiền.
- CRUD giao dịch thu/chi.
- Import sao kê và review giao dịch nháp.
- Đặt hạn mức chi tiêu.
- Tạo mục tiêu tiết kiệm.
- Ghi nhận đóng góp vào mục tiêu.
- Tạo nhắc nhở định kỳ.
- Nhận notification về cảnh báo chi tiêu, mục tiêu, reminder, broadcast và hệ thống.
- Nhận gợi ý hoặc lời khuyên tài chính từ AI.

### Admin

- Quản lý tài khoản người dùng.
- Khóa/mở khóa tài khoản.
- Xem dashboard thống kê người dùng, giao dịch và hoạt động hệ thống.
- Gửi broadcast notification cho người dùng.
- Cấu hình model AI, system prompt, temperature, max tokens và API key.
- Theo dõi audit logs.

## 6. Context Database

Database được chia theo các nhóm nghiệp vụ sau.

### Nhóm Tài Khoản Và Phân Quyền

- `roles`: lưu vai trò hệ thống, ví dụ User và Admin.
- `accounts`: lưu tài khoản người dùng, thông tin đăng nhập, hồ sơ cơ bản, trạng thái tài khoản và tiền tệ ưu tiên.
- `audit_logs`: lưu lịch sử hành động quan trọng của admin hoặc user để phục vụ kiểm tra và truy vết.

Quan hệ chính:

- Mỗi `account` thuộc một `role`.
- `audit_logs.actor_account_id` tham chiếu tới `accounts.id`.

### Nhóm Onboarding Và Thiết Lập Hũ

- `onboarding_profiles`: lưu dữ liệu khảo sát ban đầu như thu nhập tháng, nghề nghiệp, mục tiêu tài chính, khó khăn chi tiêu và phương pháp được đề xuất.
- `jar_setups`: lưu phương pháp quản lý hũ mà user chọn, gồm SixJars, Rule503020, Custom hoặc Undecided.
- `jars`: lưu từng hũ tiền của user, phần trăm phân bổ, số dư, màu, icon và trạng thái.

Quan hệ chính:

- Mỗi user có tối đa một `onboarding_profile`.
- Mỗi user có tối đa một `jar_setup`.
- Một `jar_setup` có thể có nhiều `jars`.
- Mỗi `jar` thuộc về một user.

### Nhóm Tài Khoản Tài Chính

- `financial_accounts`: lưu các nguồn tiền của user như tiền mặt, ngân hàng, ví điện tử hoặc nguồn khác.

Các trường quan trọng:

- `account_type`: Cash, Bank, EWallet, Other.
- `connection_mode`: Manual hoặc LinkedApi.
- `current_balance`: số dư hiện tại.
- `sync_status`: NeverSynced, Synced, Syncing, Error, Disconnected.
- Các trường provider/token/cursor dùng để mở rộng tích hợp API sau này.

Hiện tại hệ thống không giữ tiền thật, chỉ lưu dữ liệu phục vụ quản lý và theo dõi.

### Nhóm Danh Mục

- `categories`: lưu danh mục chi tiêu/thu nhập.

Danh mục có thể là:

- Mặc định của hệ thống nếu `is_default = true`.
- Tùy chỉnh bởi user nếu có `owner_user_id`.

Danh mục hỗ trợ icon, màu, thứ tự hiển thị, active/deleted mềm.

### Nhóm Giao Dịch

- `transactions`: lưu giao dịch thu/chi chính thức.
- `import_jobs`: lưu thông tin job import sao kê.
- `import_transaction_drafts`: lưu các dòng giao dịch nháp sau khi parse/import để user review trước khi tạo giao dịch thật.

Luồng import dự kiến:

1. User upload file sao kê.
2. Tạo `import_jobs` với trạng thái Pending hoặc Processing.
3. Parser/OCR/AI đọc file và sinh các dòng `import_transaction_drafts`.
4. User review, chỉnh category/jar/note nếu cần.
5. Hệ thống tạo `transactions` chính thức.
6. Cập nhật trạng thái `import_jobs` thành Completed hoặc Failed.

Các trường quan trọng trong `transactions`:

- `type`: Income hoặc Expense.
- `transactions_amount`: số tiền giao dịch; Income lưu số dương, Expense lưu số âm.
- `financial_account_id`: nguồn tiền liên quan.
- `from_jar_id` / `to_jar_id`: hũ nguồn/hũ đích cho thao tác nội bộ với hũ, có thể null.
- `jar_balance_after_allocation`: số dư hũ sau thao tác phân bổ, nullable.
- `category_id`: danh mục liên quan, có thể null.
- `source_type`: Manual, Imported, OCR, Jar hoặc System.
- `is_deleted`: hỗ trợ xóa mềm.

### Nhóm Phân Bổ Và Chuyển Hũ

- Scope MVP 1 tháng không còn bảng riêng `jar_allocations`, `jar_allocation_items`, `jar_transfers`.
- Phân bổ/chuyển hũ được ghi nhận qua `transactions` với `source_type = 'Jar'`.
- `from_jar_id`, `to_jar_id` và `jar_balance_after_allocation` cho biết chiều chuyển/phân bổ và số dư hũ sau thao tác.

Ý nghĩa nghiệp vụ:

- Khi có thu nhập, user có thể phân bổ tổng số tiền vào nhiều hũ.
- Mỗi record jar transaction có thể ghi nhận `jar_balance_after_allocation` để biết số dư hũ sau phân bổ.
- Chuyển tiền giữa hũ không nhất thiết là giao dịch ngân hàng thật, mà là thao tác quản lý nội bộ trong app.

### Nhóm Hạn Mức Chi Tiêu

- `spending_limits`: lưu hạn mức chi tiêu theo user, có thể gắn với hũ, danh mục hoặc cả hai.

Các trường quan trọng:

- `limit_amount`: số tiền giới hạn.
- `period`: Daily hoặc Monthly.
- `alert_at_percentage`: phần trăm ngưỡng cảnh báo.
- `is_active`: bật/tắt hạn mức.

Hệ thống dùng bảng này để phát hiện khi chi tiêu gần vượt hoặc đã vượt giới hạn, sau đó tạo notification dạng SpendingAlert.

### Nhóm Mục Tiêu Tiết Kiệm

- `goals`: lưu mục tiêu tiết kiệm.
- `goal_contributions`: lưu các lần đóng góp vào mục tiêu.

Một goal có:

- `target_amount`: số tiền cần đạt.
- `saved_amount`: số tiền đã tiết kiệm.
- `due_date`: hạn hoàn thành.
- `status`: Active, Completed hoặc Cancelled.
- `linked_jar_id`: hũ liên kết nếu có.

Khi user đóng góp vào goal, hệ thống tạo `goal_contributions` và cập nhật `goals.saved_amount`.

### Nhóm Nhắc Nhở

- `reminders`: lưu các khoản cần nhắc định kỳ như tiền nhà, học phí, bảo hiểm hoặc hóa đơn.

Các trường quan trọng:

- `frequency`: Daily, Weekly, Monthly, Quarterly, Yearly.
- `start_date`: ngày bắt đầu.
- `day_of_month`: ngày trong tháng nếu reminder lặp theo tháng/năm.
- `notify_days_before`: số ngày báo trước.
- `status`: Active, Paused, Completed hoặc Cancelled.

Hệ thống dùng bảng này để tạo notification dạng Reminder.

### Nhóm Thông Báo

- `broadcasts`: lưu thông báo do admin gửi tới nhiều người dùng.
- `notifications`: lưu thông báo cụ thể của từng user.

Notification có các loại:

- SpendingAlert.
- GoalUpdate.
- Reminder.
- System.
- Broadcast.

`broadcasts` dùng để quản lý thông báo hàng loạt, còn `notifications` là bản ghi hiển thị cho từng user.

### Nhóm Cấu Hình AI

- `ai_settings`: lưu cấu hình AI do admin quản lý.

Các trường quan trọng:

- `model_name`: tên model đang dùng.
- `system_prompt`: system prompt cho AI.
- `temperature`: mức sáng tạo.
- `max_tokens`: giới hạn output.
- `api_key_encrypted`: API key đã mã hóa.
- `is_enabled`: bật/tắt AI.

AI được dùng để:

- Gợi ý phân loại giao dịch.
- Gợi ý hũ phù hợp.
- Đưa lời khuyên dựa trên tiêu dùng thực tế.
- Cá nhân hóa phản hồi theo onboarding profile.
- Hỗ trợ user lập kế hoạch đạt mục tiêu tiết kiệm.

## 7. Luồng Nghiệp Vụ Quan Trọng

### Onboarding

1. User đăng ký tài khoản.
2. User trả lời khảo sát ban đầu.
3. Hệ thống lưu `onboarding_profiles`.
4. Hệ thống đề xuất phương pháp quản lý tiền.
5. User chọn hoặc chỉnh phương pháp.
6. Hệ thống tạo `jar_setups` và các `jars` mặc định.
7. Cập nhật `accounts.is_onboarding_completed = true`.

### Nhập Giao Dịch Thủ Công

1. User chọn tài khoản tài chính.
2. User nhập loại giao dịch Income hoặc Expense.
3. User chọn category và jar nếu cần.
4. Hệ thống tạo `transactions`.
5. Hệ thống cập nhật số dư tài khoản/hũ theo nghiệp vụ.
6. Hệ thống kiểm tra spending limit, goal hoặc reminder liên quan.
7. Nếu cần, tạo `notifications`.

### Import Sao Kê

1. User upload sao kê.
2. Tạo `import_jobs`.
3. Hệ thống parse/OCR file.
4. AI hoặc rule gợi ý note, category, jar.
5. Lưu kết quả vào `import_transaction_drafts`.
6. User review.
7. Các draft hợp lệ được chuyển thành `transactions`.
8. Cập nhật tiến độ, số dòng thành công/thất bại trong `import_jobs`.

### Cảnh Báo Chi Tiêu

1. User tạo `spending_limits`.
2. Khi có giao dịch Expense mới, hệ thống tính tổng chi theo period.
3. Nếu tổng chi đạt ngưỡng `alert_at_percentage`, tạo notification SpendingAlert.
4. Nếu vượt hạn mức, vẫn tạo notification hoặc nâng mức cảnh báo tùy logic backend.

### Mục Tiêu Tiết Kiệm

1. User tạo `goals`.
2. User đóng góp tiền từ hũ hoặc tài khoản tài chính.
3. Tạo `goal_contributions`.
4. Cập nhật `saved_amount`.
5. Nếu đạt `target_amount`, cập nhật goal thành Completed và tạo notification GoalUpdate.

## 8. Quy Ước Nghiệp Vụ

- Tiền tệ mặc định là VND.
- Các số tiền dùng `numeric(18,2)`.
- Giao dịch thu/chi lưu ở `transactions_amount`: Income dương, Expense âm; loại giao dịch nằm ở `type`.
- Xóa mềm áp dụng cho `transactions` và `categories`.
- Trạng thái tài khoản gồm Active và Banned.
- Trạng thái hũ gồm Active, Paused, Archived.
- Hệ thống không giữ tiền thật của user.
- Các trường LinkedApi trong `financial_accounts` là nền tảng mở rộng, không phải luồng chính hiện tại.
- AI chỉ hỗ trợ gợi ý, không tự động quyết định thay user với các thay đổi tài chính quan trọng.

## 9. Ghi Chú Khi Phát Triển Backend

- Luôn kiểm tra `user_id` để tránh user truy cập dữ liệu của người khác.
- Các thao tác tạo giao dịch, phân bổ hũ, chuyển hũ, đóng góp mục tiêu nên chạy trong transaction database.
- Khi cập nhật số dư, cần đảm bảo tính nhất quán giữa `transactions`, `jars`, `financial_accounts`, `goals`.
- Các enum hiện đang lưu dạng `varchar`, backend nên validate chặt để tránh giá trị sai.
- Các bảng có `updated_at` cần được cập nhật khi sửa dữ liệu.
- Import sao kê nên tách rõ job, draft và transaction chính thức.
- API admin cần kiểm tra role Admin.
- API user chỉ thao tác trên dữ liệu thuộc chính user đó.
- Với AI settings, API key phải được mã hóa trước khi lưu vào `api_key_encrypted`.
