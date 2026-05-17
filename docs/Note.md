# Note xử lý lỗi nghiệp vụ tài chính - BE/FE/Test

Ngày rà soát: 2026-05-16

Phạm vi đọc hiện tại: repo này đang có BE `Personal_Finance_Management`; chưa thấy source FE trong workspace. Vì vậy phần FE bên dưới là checklist cần áp dụng ở repo FE hoặc app FE tương ứng.

## 1. Nhóm thời gian giao dịch

### Vấn đề

- `POST /api/v1/transactions` hiện nhận `date` từ FE và lưu thẳng vào `TransactionDate`, nên FE có thể tạo giao dịch thu nhập/chi tiêu ở tương lai.
- Với `type = Transfer` cho các flow chuyển hũ sang hũ, tài khoản sang hũ, hũ sang tài khoản, code cũng đang dùng `request.date`; user không nên tự chọn ngày quá khứ/tương lai cho nghiệp vụ chuyển tiền nội bộ này.
- `GET /api/v1/transactions` đang filter/sort bằng `TransactionDate` nhưng response lại trả `date = CreatedAt`, dễ làm FE hiển thị sai timeline.

### Quyết định nghiệp vụ

- `Income` và `Expense`: cho phép `TransactionDate <= now`; không cho tạo trong tương lai.
- `Transfer`: không dùng ngày user nhập. BE set `TransactionDate = CreatedAt = DateTimeOffset.UtcNow` để phản ánh thời điểm thao tác thật.
- Nếu public contract vẫn chỉ chốt `Income | Expense`, cần quyết định rõ `Transfer` là internal capability hay expose chính thức trước khi FE dùng rộng.

### BE cần làm

- Thêm validation trong `Personal_Finance_Management.Service/Transaction/Service.cs`:
  - reject `request.date > DateTimeOffset.UtcNow` cho `Income` và `Expense`;
  - normalize hoặc ignore `request.date` cho `Transfer`, set transaction date bằng `now`;
  - validate amount dương, type hợp lệ, ownership jar/account/category trước khi update balance.
- Sửa response `GET /api/v1/transactions` trả `date = x.TransactionDate`, không dùng `CreatedAt`.
- Nên gom create transaction vào EF database transaction vì đang update balance jar/account và tạo notification trong cùng flow tiền.

### FE cần làm

- Date picker cho thu nhập/chi tiêu: disable ngày > hôm nay, không cho nhập tay ngày tương lai.
- Flow transfer: không hiển thị input ngày hoặc hiển thị read-only "Hôm nay/Bây giờ"; payload không cần gửi `date` nếu BE đổi contract, hoặc gửi hiện tại nếu contract chưa đổi.
- Khi BE trả lỗi future date, show inline error ở field ngày và không retry âm thầm.

### Test BE

- Tạo `Income` với `date = now + 1 day` trả 400, không tạo transaction, không đổi balance.
- Tạo `Expense` với `date = now + 1 day` trả 400, không trừ jar/account.
- Tạo `Income`/`Expense` với hôm nay và quá khứ hợp lệ.
- Tạo `Transfer` với payload cố tình gửi quá khứ/tương lai vẫn lưu `TransactionDate` gần bằng `CreatedAt/now`.
- `GET /transactions` trả `date` đúng `TransactionDate`.

### Test FE

- Date picker disable future date cho thu/chi.
- Nhập tay future date bị chặn ở client hoặc hiển thị lỗi từ BE.
- Transfer không có date picker chỉnh được; sau khi tạo, transaction hiển thị thời điểm hiện tại.

## 2. Nhóm limit và notification chuông

### Vấn đề

- `GET /api/v1/limits` hiện chỉ tính `CurrentSpent` cho limit theo `Jar`; limit theo `Category` chưa cộng chi tiêu.
- `Transaction.CheckLimit(...)` hiện chỉ nhận `jarId`, chỉ evaluate limit theo jar sau expense từ hũ; category limit không được đánh giá khi tạo chi tiêu có `categoryId`.
- Notification API có `UnreadCount`, nhưng FE cần làm nổi bật chuông khi có notification chưa đọc.

### BE cần làm

- Trong limit service, tính `CurrentSpent` cho cả:
  - target `Jar`: tổng expense theo `FromJarId`;
  - target `Category`: tổng expense theo `CategoryId`;
  - chỉ tính transaction `!IsDeleted`, đúng user, đúng kỳ `ResetAt` hoặc period hiện hành.
- Khi tạo/update/delete transaction expense, evaluate lại limit bị ảnh hưởng:
  - limit theo jar nếu có `FromJarId`;
  - limit theo category nếu có `CategoryId`;
  - tránh tạo duplicate notification bằng metadata `limitId`, `jarId/categoryId`, threshold type.
- Notification tạo ra phải để `IsRead = false`, `Type = SpendingAlert`, `MetadataJson` đủ thông tin để FE navigate.

### FE cần làm

- Chuông notification phải dựa trên `unreadCount > 0` để hiển thị badge/màu nổi bật.
- Sau khi user tạo transaction expense thành công, refresh hoặc invalidate query:
  - `/api/v1/limits`;
  - `/api/v1/notifications`;
  - unread count ở header/bell.
- Limit card cho category phải show `currentSpent`, `%`, trạng thái cảnh báo/vượt ngưỡng giống jar limit.

### Test BE

- Tạo category limit, tạo expense cùng category, gọi `GET /limits` thấy `CurrentSpent` tăng.
- Expense chạm `AlertAtPercentage` tạo 1 notification `SpendingAlert`.
- Expense vượt `LimitAmount` tạo notification vượt hạn mức.
- Tạo thêm expense sau đó không duplicate cùng notification threshold.
- Mark notification read làm `UnreadCount` giảm.

### Test FE

- Khi có unread notification, chuông hiển thị badge/màu nổi bật.
- Sau khi mark read, badge giảm hoặc biến mất.
- Tạo expense thuộc category có limit: UI limit category cập nhật progress.
- Notification spending alert click được và điều hướng tới limit hoặc transaction liên quan.

## 3. Nhóm goal trong hũ

### Vấn đề

- Khi tạo goal có `TargetAmount <= Jar.Balance`, cần notification ngay.
- Notification chỉ là record thông báo; không được xóa goal do việc tạo notification.
- `GetGoals()` hiện query `Status == "Active" && Status == "Completed"` nên điều kiện luôn sai, có nguy cơ FE không thấy goal nào.
- `CreateGoal` chưa validate `DueDate`, nên tạo goal trong quá khứ vẫn được.

### Quyết định nghiệp vụ

- Goal chỉ được tạo với `dueDate >= today`; không cho ngày quá khứ.
- Nếu goal vừa tạo đã đạt target vì tiền trong hũ đủ:
  - tạo notification `GoalUpdate` ngay;
  - cập nhật goal `Completed` nếu sản phẩm xem mục tiêu đã đạt;
  - không xóa goal và không xóa notification. FE vẫn phải xem được goal completed hoặc ít nhất xem chi tiết qua notification.

### BE cần làm

- Validate create/update goal:
  - `TargetAmount > 0`;
  - `DueDate.Date >= today`;
  - `LinkedJarId` nếu có phải thuộc user.
- Sửa `GetGoals()` filter thành danh sách trạng thái hợp lệ, ví dụ `Active || Completed`, hoặc hỗ trợ query `status`.
- Khi tạo goal đủ tiền trong hũ, tạo notification append-only với metadata `{ goalId, jarId }`.
- Không hard delete goal; delete vẫn nên map thành `Cancelled`.

### FE cần làm

- Date picker goal disable ngày < hôm nay.
- Nếu BE trả lỗi dueDate quá khứ, show inline error ở field deadline.
- Goal list cần hiển thị hoặc có tab/filter cho `Completed` để goal vừa hoàn thành không biến mất.
- Notification `GoalUpdate` click tới goal detail hoặc jar detail.

### Test BE

- Tạo goal với dueDate hôm qua trả 400.
- Tạo goal hôm nay/tương lai thành công.
- Tạo goal target nhỏ hơn hoặc bằng jar balance: goal không bị xóa, notification `GoalUpdate` được tạo, unread count tăng.
- `GET /goals` trả được goal active và goal completed theo contract đã chọn.
- Update goal sang dueDate quá khứ trả 400.

### Test FE

- Không chọn được ngày quá khứ khi tạo/sửa goal.
- Goal đủ tiền hiện thông báo trong chuông.
- Click notification mở đúng goal/jar.
- Goal completed vẫn có nơi để user xem lại.

## 4. Nhóm reminder/nhắc lịch

### Vấn đề

- Reminder có thể tạo với `StartDate` trong quá khứ; validation hiện mới kiểm tra title, amount, frequency, dayOfMonth, notifyDaysBefore.

### Quyết định nghiệp vụ

- Không cho tạo reminder trong quá khứ.
- `StartDate.Date >= today`; nếu cần giờ cụ thể thì `StartDate >= now` cho reminder trong ngày.
- `NextDueDate` vẫn được tính trong service từ `StartDate`, `Frequency`, `DayOfMonth`, `NotifyDaysBefore`.

### BE cần làm

- Thêm validation trong `ValidateCreateReminderRequest`:
  - `StartDate.Date >= DateTimeOffset.UtcNow.Date`;
  - nếu sản phẩm dùng giờ cụ thể, reject `StartDate < now`.
- Nếu update reminder sau này cho phép đổi `StartDate`, thêm rule tương tự trong update DTO/service.
- Validate category nếu `CategoryId` có giá trị thì phải thuộc user hoặc là category default active.

### FE cần làm

- Date picker reminder disable ngày < hôm nay.
- Với reminder trong hôm nay, time picker disable giờ đã qua nếu UI có chọn giờ.
- Hiển thị lỗi field `startDate` từ BE.

### Test BE

- Tạo reminder với ngày hôm qua trả 400.
- Tạo reminder hôm nay/tương lai thành công.
- Tạo reminder hôm nay nhưng giờ đã qua: test theo rule cuối cùng của sản phẩm.
- `GET /reminders` trả `nextDueDate` không nằm trong quá khứ.

### Test FE

- Không chọn được ngày quá khứ.
- Không chọn được giờ quá khứ nếu UI có time picker.
- Reminder vừa tạo xuất hiện ở list với next due date đúng.

## 5. Thứ tự ưu tiên xử lý

1. BE validation ngày cho transaction, goal, reminder để chặn dữ liệu sai ngay cả khi FE lỗi.
2. BE fix limit category current spent và evaluate notification theo category.
3. BE fix goal query/status và notification completed goal.
4. FE chặn date picker và refresh notification/limit state.
5. Viết test BE trước cho rule nghiệp vụ; sau đó test FE theo luồng UI.

## 6. Ghi chú rủi ro

- Public docs hiện có chỗ nói transaction public chỉ `Income | Expense`, nhưng code đang có `Transfer`. Nếu FE cần transfer, nên cập nhật contract rõ ràng trước khi implement tiếp.
- Cần thống nhất timezone hiển thị: BE dùng UTC, FE hiển thị theo local timezone của user.
- Một số query hiện dùng navigation optional như `FinancialAccount`, `FromJar`, `Category`; khi sửa transaction nên kiểm tra null để tránh lỗi projection.

---

# Note xử lý lỗi UI/API tiếp theo - 2026-05-16

## 1. Auth và global status

### BE

- Login sai email/mật khẩu phải trả HTTP 400, không được rơi vào global 500.
- Các lỗi validate nên đi qua `AppValidationException` để response thống nhất:

```json
{
  "success": false,
  "error": "Email hoặc mật khẩu không đúng.",
  "message": "Email hoặc mật khẩu không đúng.",
  "details": {
    "field": "email",
    "code": "INVALID_LOGIN_CREDENTIALS"
  },
  "traceId": "..."
}
```

- Không dùng `throw new Exception(...)` cho lỗi nghiệp vụ/user input như login sai, không tìm thấy dữ liệu của user, dữ liệu không hợp lệ.

### FE

- Với auth/login, nếu HTTP 400 và code `INVALID_LOGIN_CREDENTIALS`, hiển thị tiếng Việt thân thiện: "Email hoặc mật khẩu không đúng."
- Không hiển thị text kỹ thuật như `Bad Request`, `INVALID_LOGIN_CREDENTIALS`, stack trace, hoặc field name thô cho user cuối.

### Test

- BE: login sai password trả 400, không trả 500.
- FE: form login hiển thị lỗi tiếng Việt dưới form hoặc toast rõ ràng.

## 2. Lỗi transaction phải hiển thị chi tiết hơn

### BE

- Response lỗi transaction phải có `error/message` thân thiện và `details.field`, `details.code` phẳng, không lồng `details.details`.
- Ví dụ future date:

```json
{
  "success": false,
  "error": "Không thể tạo giao dịch trong tương lai.",
  "message": "Không thể tạo giao dịch trong tương lai.",
  "details": {
    "field": "date",
    "code": "TRANSACTION_DATE_IN_FUTURE"
  },
  "traceId": "..."
}
```

### FE

- Form thêm giao dịch cần đọc ưu tiên:
  - `response.error` hoặc `response.message` để show tổng quan;
  - `response.details.field` để gắn lỗi vào input tương ứng;
  - `response.details.code` để map thông báo tiếng Việt nếu BE chưa đủ rõ.
- Mapping tối thiểu:
  - `TRANSACTION_DATE_IN_FUTURE`: "Không thể tạo giao dịch trong tương lai."
  - `INVALID_TRANSACTION_AMOUNT`: "Số tiền giao dịch phải lớn hơn 0."
  - `CATEGORY_NOT_FOUND`: "Danh mục không còn khả dụng."
  - `JAR_NOT_FOUND`: "Hũ không còn khả dụng."
  - `FINANCIAL_ACCOUNT_NOT_FOUND`: "Tài khoản tài chính không còn khả dụng."

### Test

- BE: tạo transaction tương lai trả 400 và details phẳng.
- FE: lỗi hiện ngay ở field ngày, không chỉ hiện "400".

## 3. Transaction pagination và chi tiết giao dịch

### BE

- `GET /api/v1/transactions` phải phân trang thật:
  - validate `pageIndex > 0`;
  - validate `1 <= pageSize <= 100`;
  - `totalCount` tính trước `Skip/Take`;
  - `totalPages` dựa trên toàn bộ filter result.
- Thêm `GET /api/v1/transactions/{id}` để FE xem chi tiết giao dịch.

### FE

- Màn danh sách giao dịch cần gọi API với `pageIndex`, `pageSize`, và render pagination từ `pagination.totalCount/totalPages`.
- Màn chi tiết transaction:
  - từ list click vào một transaction;
  - gọi `GET /api/v1/transactions/{id}`;
  - hiển thị: loại giao dịch, số tiền, ngày, danh mục, hũ/tài khoản liên quan, ghi chú.

### Test

- BE: tạo nhiều transaction, gọi page 1/page 2 không trùng data và totalCount đúng.
- BE: gọi transaction id không thuộc user trả 404.
- FE: chuyển trang giữ filter hiện tại; refresh detail vẫn load đúng transaction.

## 4. Dashboard category expense và limit phải cùng logic

### BE

- Dashboard category breakdown, limit category, và transaction expense phải cùng dùng logic:
  - `UserId == currentUser`;
  - `!IsDeleted`;
  - `Type == "Expense"`;
  - đúng `CategoryId`;
  - limit còn tính thêm kỳ hiện hành bằng `ResetAt`.
- Dashboard phải tính tổng thu/chi theo user hiện tại, không cộng toàn hệ thống.
- Category breakdown phải gồm cả default category và category riêng của user.

### FE

- Khi tạo expense có category hoặc hũ, cần invalidate/refetch:
  - dashboard;
  - limits;
  - transactions;
  - notifications.
- UI không dùng từ kỹ thuật như `Expense`, `Income`, `Limit`, `Transaction` nếu đang hiển thị cho user cuối; dùng:
  - "Chi tiêu"
  - "Thu nhập"
  - "Hạn mức"
  - "Giao dịch"
  - "Danh mục"
  - "Hũ"

### Test

- BE: expense theo category làm dashboard category breakdown tăng.
- BE: cùng expense đó làm category limit currentSpent tăng.
- BE: expense theo jar làm jar limit currentSpent tăng.
- FE: sau khi thêm chi tiêu, dashboard và limit cập nhật sau khi refetch.

# Note kế hoạch xử lý giao dịch, category, thời gian và xoá hũ - 2026-05-16

## 1. Chi tiêu từ hũ bị thiếu tiền đang trả 500

### Vấn đề

- Khi tạo giao dịch thủ công loại chi tiêu từ hũ, nếu `Jar.Balance < transactionsAmount`, BE hiện có thể ném lỗi thường `Insufficient funds` và global middleware trả 500.
- Đây là lỗi nghiệp vụ do dữ liệu người dùng nhập, không phải lỗi hệ thống.

### BE cần làm

- Trong `CreateTransaction` và các flow update/delete liên quan balance, thay toàn bộ lỗi thiếu tiền bằng `AppValidationException.BadRequest`.
- Response mong muốn:

```json
{
  "success": false,
  "error": "Số tiền trong hũ không đủ để thực hiện giao dịch.",
  "message": "Số tiền trong hũ không đủ để thực hiện giao dịch.",
  "details": {
    "field": "fromJarId",
    "code": "INSUFFICIENT_JAR_BALANCE"
  },
  "traceId": "..."
}
```

- Không dùng `throw new Exception("Insufficient funds")` cho thiếu tiền.
- Nếu chi tiêu vừa có `fromJarId` vừa có `categoryId`, vẫn phải:
  - chặn thiếu tiền bằng 400;
  - nếu hợp lệ thì trừ balance hũ;
  - cập nhật/evaluate limit theo hũ;
  - cập nhật/evaluate limit theo danh mục.

### FE cần làm

- Khi nhận code `INSUFFICIENT_JAR_BALANCE`, hiển thị lỗi thân thiện ở form thêm giao dịch:
  - "Số tiền trong hũ không đủ."
- Nên gắn lỗi vào phần chọn hũ hoặc ô số tiền, không chỉ hiện toast `400`.

### Test

- BE: tạo expense từ hũ có balance nhỏ hơn amount trả 400, không tạo transaction, không đổi balance.
- BE: response có `details.field = fromJarId` và `details.code = INSUFFICIENT_JAR_BALANCE`.
- FE: form hiển thị lỗi tiếng Việt rõ ràng.

## 2. Chuyển tiền từ tài khoản sang hũ bị thiếu tiền đang trả 500

### Vấn đề

- Khi tạo giao dịch chuyển từ tài khoản tài chính sang hũ, nếu `FinancialAccount.CurrentBalance < transactionsAmount`, BE có thể trả 500.
- FE cần status 400 và mã lỗi rõ để debug và map UI.

### BE cần làm

- Với flow tài khoản sang hũ, validate số dư tài khoản trước khi trừ tiền.
- Nếu thiếu tiền, trả 400:

```json
{
  "success": false,
  "error": "Số dư tài khoản không đủ để chuyển vào hũ.",
  "message": "Số dư tài khoản không đủ để chuyển vào hũ.",
  "details": {
    "field": "financialAccountId",
    "code": "INSUFFICIENT_ACCOUNT_BALANCE"
  },
  "traceId": "..."
}
```

- Áp dụng cùng rule cho các flow có trừ tiền từ tài khoản, ví dụ chi tiêu trực tiếp từ tài khoản nếu có.
- Đảm bảo transaction DB atomic: thiếu tiền thì rollback toàn bộ, không cộng hũ, không tạo transaction.

### FE cần làm

- Map code `INSUFFICIENT_ACCOUNT_BALANCE` thành:
  - "Số dư tài khoản không đủ."
- Gắn lỗi vào vùng chọn tài khoản hoặc số tiền.
- Không hiển thị thông báo kỹ thuật như `Internal server error`, `Insufficient funds`.

### Test

- BE: account balance nhỏ hơn amount trả 400.
- BE: hũ không được cộng tiền khi tài khoản thiếu tiền.
- BE: transaction không được lưu khi trả lỗi.
- FE: lỗi hiện đúng ở form, không chỉ hiện status code.

## 3. Tạo category không nên bắt user nhập icon và mã màu thủ công

### Vấn đề

- FE hiện bắt người dùng nhập icon và mã màu bằng text, không thân thiện.

### FE cần làm

- Thay input text icon bằng bộ chọn icon:
  - hiển thị danh sách icon phổ biến theo nhóm chi tiêu/thu nhập;
  - user click để chọn;
  - payload vẫn gửi `icon` theo format BE đang lưu.
- Thay input text màu bằng color picker hoặc bảng màu có sẵn:
  - user chọn màu bằng swatch;
  - payload vẫn gửi mã màu hex hợp lệ như `#22C55E`.
- Validate FE:
  - icon không rỗng nếu BE vẫn bắt buộc;
  - màu phải là hex hợp lệ;
  - thông báo lỗi bằng tiếng Việt.

### BE cần kiểm tra

- Nếu BE đang bắt buộc icon/color, giữ validation nhưng trả lỗi 400 rõ ràng.
- Nếu muốn thân thiện hơn, có thể set default icon/color khi FE không gửi.

### Test

- FE: tạo category bằng cách chọn icon và màu, không cần gõ thủ công.
- FE: màu chọn được hiển thị preview trước khi lưu.
- BE: payload hợp lệ tạo category thành công.

## 4. Hạn mức theo hũ không thay đổi sau khi có chi tiêu trong hũ

### Vấn đề

- Khi tạo hạn mức dành cho hũ, sau đó tạo giao dịch chi tiêu trong hũ đó, UI không thấy hạn mức thay đổi.

### BE cần làm

- Kiểm tra `GET /limits` tính `CurrentSpent` cho jar limit theo cùng logic:
  - đúng user;
  - transaction chưa bị xoá;
  - `Type == "Expense"`;
  - `FromJarId == limit.JarId`;
  - `TransactionDate >= limit.ResetAt`.
- Khi tạo/update/delete expense có `FromJarId`, evaluate lại jar limit và tạo notification nếu chạm/vượt ngưỡng.
- Nếu expense vừa có hũ vừa có danh mục, phải update cả jar limit và category limit.

### FE cần làm

- Sau khi tạo/sửa/xoá giao dịch chi tiêu trong hũ, refetch/invalidate:
  - danh sách hạn mức;
  - dashboard;
  - transaction list;
  - notification/unread count.
- Limit card phải hiển thị lại `CurrentSpent` và phần trăm mới từ BE, không tự cache giá trị cũ.

### Test

- BE: tạo jar limit, tạo expense trong hũ, `GET /limits` thấy `CurrentSpent` tăng.
- BE: sửa amount expense, `GET /limits` phản ánh amount mới.
- BE: xoá mềm expense, `GET /limits` giảm lại.
- FE: sau khi thêm chi tiêu trong hũ, màn hạn mức cập nhật không cần reload thủ công.

## 5. UI chọn giờ/phút giao dịch đang nhảy 5 phút

### Vấn đề

- UI thời gian giao dịch đang chọn phút theo bước 5 phút.
- User cần chọn chính xác từng phút.

### FE cần làm

- Đổi cấu hình time picker minute step từ `5` sang `1`.
- Nếu dùng HTML input/time picker library:
  - set `step = 60` giây, tương ứng 1 phút;
  - hoặc cấu hình `minuteStep = 1` theo thư viện đang dùng.
- Kiểm tra cả form thêm và sửa giao dịch nếu có.

### Test

- FE: time picker cho phép chọn 10:01, 10:02, 10:03, không chỉ 10:00, 10:05.
- FE: payload gửi lên BE giữ đúng giờ/phút user chọn.
- BE: lưu `TransactionDate` đúng phút nhận được, miễn không nằm trong tương lai.

## 6. FE chưa có xoá hũ

### Vấn đề

- FE chưa có thao tác xoá hũ trong giao diện.

### FE cần làm

- Thêm nút icon thùng rác ở danh sách hũ hoặc màn chi tiết hũ.
- Trước khi xoá, hiển thị confirm dialog tiếng Việt:
  - "Bạn có chắc muốn xoá hũ này không?"
- Sau khi xoá thành công:
  - refetch danh sách hũ;
  - refetch dashboard;
  - refetch limit nếu hũ có hạn mức liên quan.
- Nếu BE trả lỗi hũ đang có dữ liệu liên quan hoặc không thể xoá, hiển thị message thân thiện.

### BE cần kiểm tra

- API xoá hũ nên là soft delete/archive nếu hũ đã có transaction, goal hoặc limit liên quan.
- Nếu không cho xoá, trả 400/409 rõ ràng thay vì 500.
- Response lỗi nên có code như:
  - `JAR_HAS_RELATED_DATA`
  - `JAR_NOT_FOUND`

### Test

- FE: click thùng rác mở confirm, cancel không xoá.
- FE: confirm xoá thành công thì hũ biến mất khỏi list.
- BE: xoá hũ không thuộc user trả 404.
- BE: hũ có dữ liệu liên quan không trả 500.

---

# Refactor service helpers

## 2026-05-15

- Them helper dung chung trong `Personal_Finance_Management.Service/Base`:
  - `ServiceClaimHelper`: lay `UserId`, `AdminId`, `AccountId` tu JWT claim `id`.
  - `ServiceTextHelper`: normalize required/optional text, truncate string, mask secret, mask so tai khoan, normalize enum.
- Da thay cac logic lap lai trong service layer:
  - parse claim `id` trong cac service nhu `Auth`, `User`, `Jar`, `Transaction`, `FinancialAccount`, `Dashboard`, `Onboarding`, `Goal`, `Limit`, `Notification`, `Reminder`, `AI`, `Import`, `Admin`, `Broadcast`, `Category`.
  - normalize text/enum/mask trong `AI`, `Category`, `Import`, `Reminder`, `User`, `FinancialAccount`, `ValidationServices`.
- Muc tieu: cac service tai su dung helper chung, giam private helper trung lap va giu behavior hien tai.
- Verification: `dotnet build Personal_Finance_Management/Personal_Finance_Management.sln --no-restore -m:1 -v:m` build thanh cong.
- Build con warning khong lien quan den refactor:
