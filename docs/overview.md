# Tổng Quan Dự Án

## 1. Giới thiệu

Đây là dự án xây dựng **hệ thống quản lý tài chính cá nhân** theo định hướng tự động hóa, tập trung vào việc giúp người dùng:

- theo dõi dòng tiền hằng ngày;
- quản lý chi tiêu theo từng nhóm ngân sách hoặc "hũ";
- thiết lập mục tiêu tiết kiệm rõ ràng;
- nhận cảnh báo sớm trước khi vượt hạn mức;
- nhận gợi ý và lời khuyên phù hợp với hành vi chi tiêu thực tế.

Khác với cách quản lý truyền thống bằng Excel hoặc sổ tay, hệ thống hướng đến trải nghiệm đơn giản hơn, ít nhập liệu thủ công hơn, nhưng vẫn đủ chi tiết để người dùng kiểm soát được tài chính cá nhân.

## 2. Bối cảnh và bài toán

Trong thực tế, nhiều người biết rằng cần quản lý chi tiêu nhưng lại không duy trì được thói quen đó lâu dài. Nguyên nhân thường không nằm ở việc họ không quan tâm đến tiền bạc, mà là vì quá trình quản lý hiện tại quá tốn thời gian, lặp lại và thiếu tính hỗ trợ.

Từ phần mô tả ban đầu, mình hiểu bài toán cốt lõi của dự án gồm các điểm sau:

- Người dùng phải tự cộng trừ, đối chiếu và thống kê bằng tay, gây mất thời gian.
- Các khoản chi nhỏ, phát sinh liên tục, rất khó theo dõi nếu nhập từng dòng thủ công.
- Dữ liệu có thể được ghi lại, nhưng việc truy xuất lại để hiểu bản thân đang tiêu tiền như thế nào vẫn khó.
- Nhiều ứng dụng chỉ cho biết "đã tiêu bao nhiêu", nhưng chưa hỗ trợ tốt câu hỏi "sắp vượt mức chưa?" hoặc "cần điều chỉnh thế nào để đạt mục tiêu tiết kiệm?".
- Người dùng thường không nhận ra mình đang vượt ngân sách cho đến khi số tiền còn lại đã quá thấp.
- Các khoản chi chưa được gắn chặt với mục tiêu tài chính dài hạn, nên việc tiết kiệm dễ bị đứt quãng.
- Một nhóm người dùng chỉ có tiền mặt hoặc chưa muốn liên kết ngân hàng, nên hệ thống vẫn phải hoạt động tốt ngay cả khi không có bank link.

Nói ngắn gọn, hệ thống này không chỉ ghi nhận giao dịch, mà còn phải đóng vai trò như một công cụ **theo dõi, cảnh báo và định hướng tài chính cá nhân**.

## 3. Mục tiêu sản phẩm

Sản phẩm được xây dựng để giải quyết ba mục tiêu chính:

### 3.1. Tự động hóa việc quản lý chi tiêu

Hệ thống cần giảm tối đa việc tính toán và nhập liệu thủ công, đặc biệt với các giao dịch lặp lại hoặc dữ liệu lấy từ sao kê.

### 3.2. Giúp người dùng ra quyết định sớm

Thay vì chỉ thống kê quá khứ, ứng dụng cần phát hiện xu hướng chi tiêu hiện tại, đưa ra cảnh báo khi sắp vượt hạn mức và đề xuất hướng điều chỉnh.

### 3.3. Kết nối chi tiêu với mục tiêu tài chính

Chi tiêu hằng ngày cần được đặt trong bối cảnh lớn hơn: quỹ tiết kiệm, mục tiêu mua sắm, các khoản thanh toán định kỳ và kế hoạch tài chính theo thời gian.

## 4. Đối tượng người dùng

Theo yêu cầu ban đầu, hệ thống hướng đến các nhóm người dùng sau:

- **Người nội trợ** cần một công cụ dễ dùng để theo dõi tiền ra vào trong gia đình.
- **Người bận rộn hoặc không quen quản lý tiền bạc** nhưng vẫn muốn có một hệ thống hỗ trợ tự động.
- **Nhóm người dùng cần thay thế các cách làm thủ công** như Excel, sổ tay hoặc ghi chú rời rạc.
- **Doanh nghiệp nhỏ hoặc nhóm làm việc đơn giản** có thể tận dụng hệ thống như một giải pháp quản lý tài chính cơ bản, dù trọng tâm chính của sản phẩm vẫn là tài chính cá nhân.

## 5. Định hướng giải pháp

## 5.1. Mô hình nguồn tiền và nhiều hũ chi tiêu

Ứng dụng nên tách rõ hai lớp dữ liệu:

- **nguồn tiền**: tiền mặt, tài khoản ngân hàng, ví điện tử hoặc nguồn tiền khác mà user đang theo dõi;
- **hũ ngân sách**: lớp phân bổ nội bộ để chia tiền theo mục đích sử dụng.

Điều này giúp hệ thống không bị nhầm giữa:

- tài khoản đăng nhập;
- tài khoản/nguồn tiền thực tế của user;
- hũ ngân sách trong ứng dụng.

Trong đó, mỗi hũ đại diện cho một mục đích cụ thể, ví dụ:

- sinh hoạt;
- ăn uống;
- di chuyển;
- mua sắm;
- tiết kiệm;
- quỹ mục tiêu.

Việc chia tiền theo từng hũ giúp người dùng nhìn rõ khoản nào đang tiêu vượt mức, khoản nào còn dư và khoản nào cần ưu tiên giữ lại.

Song song với đó, hệ thống phải cho phép user dùng hoàn toàn bằng **nguồn tiền thủ công** như `Tiền mặt`, ngay cả khi chưa liên kết bất kỳ tài khoản ngân hàng nào.

## 5.2. Khảo sát ban đầu để cá nhân hóa trải nghiệm

Với người dùng mới, hệ thống sẽ có một bước khảo sát ngắn về thói quen tiêu dùng và mục tiêu tài chính. Theo cách mình hiểu, bước này không chỉ để thu thập dữ liệu, mà còn phục vụ 4 mục đích:

- tạo cảm giác ứng dụng hiểu đúng hoàn cảnh của người dùng;
- đề xuất hạn mức và mục tiêu tiết kiệm phù hợp hơn ngay từ đầu;
- cung cấp ngữ cảnh cho AI để đưa ra lời khuyên sát với nhu cầu thực tế;
- cá nhân hóa cách xưng hô, phong cách phản hồi và kế hoạch gợi ý.

## 5.3. Kết hợp AI vào trải nghiệm tài chính

AI trong hệ thống không chỉ đóng vai trò chatbot trả lời câu hỏi, mà còn là lớp hỗ trợ phân tích và gợi ý, bao gồm:

- đề xuất phân loại giao dịch;
- hỗ trợ diễn giải tình hình chi tiêu;
- cảnh báo nguy cơ vượt ngân sách;
- gợi ý kế hoạch để đạt mục tiêu tiết kiệm;
- hỗ trợ xử lý dữ liệu nhập từ hóa đơn hoặc sao kê.

## 6. Phạm vi chức năng

## 6.1. Vai trò trong hệ thống

Hệ thống có 2 vai trò chính:

- **User**: sử dụng ứng dụng để quản lý tài chính cá nhân.
- **Admin**: quản trị người dùng, cấu hình hệ thống và vận hành phần AI.

## 6.2. Giới hạn nghiệp vụ

- Hệ thống **không giữ tiền của người dùng**.
- Phạm vi chính là **quản lý chi tiêu và tiết kiệm**, không phải ngân hàng số hay ví điện tử.
- Liên kết ngân hàng, nếu có, chỉ nhằm **đồng bộ và theo dõi dữ liệu**, không phải để hệ thống nắm quyền giữ hay chuyển tiền thật.
- User không liên kết ngân hàng vẫn phải sử dụng được đầy đủ các luồng cốt lõi thông qua nguồn tiền nhập tay như tiền mặt.
- Nền tảng mục tiêu là **Web App**, có thể phát triển theo hướng **PWA** để thuận tiện sử dụng trên thiết bị di động.

## 7. Nguồn dữ liệu đầu vào

Sau khi cân nhắc các phương thức lấy dữ liệu, hướng triển khai phù hợp nhất hiện tại là:

### 7.1. Nguồn tiền nhập tay

Đây nên là cách khởi tạo dữ liệu mặc định cho mọi user, đặc biệt là:

- học sinh, sinh viên chỉ có tiền mặt;
- người chưa muốn liên kết ngân hàng;
- người muốn bắt đầu rất nhanh mà không cần tích hợp bên ngoài.

User có thể:

- tạo nguồn tiền như `Tiền mặt`;
- nhập số dư hiện tại;
- tự điều chỉnh số dư khi cần;
- phân bổ số dư đó vào các hũ ngân sách.

### 7.2. Import sao kê

Đây là nguồn dữ liệu quan trọng cho các giao dịch qua ngân hàng. Hệ thống có thể kết hợp:

- import file sao kê;
- parse dữ liệu giao dịch;
- map dữ liệu vào một nguồn tiền cụ thể;
- AI để gợi ý phân mục chi tiêu.

### 7.3. Nhập tay giao dịch

Phù hợp với:

- các khoản chi tiền mặt;
- các khoản chi nhỏ phát sinh nhanh;
- dữ liệu người dùng muốn bổ sung thủ công;
- các giao dịch phát sinh từ nguồn tiền thủ công như tiền mặt.

### 7.4. Các phương án chưa ưu tiên

Một số phương thức được xem xét nhưng chưa phù hợp ở giai đoạn hiện tại:

- API ngân hàng: hữu ích về hướng mở rộng, nhưng khó triển khai ngay do ràng buộc bảo mật, pháp lý và hợp tác.
- SePay/Casso: có chi phí và giới hạn hỗ trợ chưa tối ưu cho bài toán hiện tại.
- SMS Banking: tiềm ẩn rủi ro bảo mật thông tin.

## 8. Tính năng chính cho người dùng

Theo phạm vi sản phẩm, nhóm tính năng của User có thể được tổ chức lại như sau:

### 8.1. Quản lý nguồn tiền, giao dịch và danh mục

- Tạo và quản lý các nguồn tiền như tiền mặt hoặc tài khoản theo dõi.
- Hỗ trợ nguồn tiền nhập tay làm mặc định cho user mới.
- CRUD dữ liệu chi tiêu.
- Tạo mới hoặc sử dụng danh mục mặc định như ăn uống, di chuyển, mua sắm.
- Chọn phân mục khi nhập dữ liệu.
- Hỗ trợ liên kết tài khoản ngân hàng ở mức phục vụ theo dõi dữ liệu, không giữ tiền, khi hệ thống đã sẵn sàng tích hợp.

### 8.2. Theo dõi tài chính tổng quan

- Dashboard hiển thị tổng số dư, số dư đã phân bổ vào hũ, số dư chưa phân bổ, biểu đồ thu chi và các giao dịch gần đây.
- Theo dõi mức sử dụng ngân sách theo từng nhóm chi tiêu hoặc từng hũ.
- Quan sát tiến độ thực hiện mục tiêu tiết kiệm.

### 8.3. Hạn mức và cảnh báo

- Đặt hạn mức chi tiêu theo ngày, tháng hoặc theo từng hạng mục.
- Cấu hình ngưỡng cảnh báo do người dùng tự đặt.
- Hệ thống có thể chủ động cảnh báo khi phát hiện dấu hiệu sắp vượt mức.
- Nhắc các khoản đóng tiền định kỳ theo mốc như 30 ngày, 90 ngày hoặc chu kỳ tùy chỉnh.

### 8.4. Nhập liệu thông minh

- Chụp ảnh hóa đơn để tự động điền thông tin.
- Import sao kê để giảm thao tác nhập tay.
- AI hỗ trợ phân loại và điền trước dữ liệu để tăng tốc độ nhập liệu.

### 8.5. Mục tiêu và kế hoạch tài chính

- Tạo quỹ mục tiêu tiết kiệm với thời hạn linh hoạt theo ngày, tháng hoặc năm.
- Nhận đề xuất kế hoạch chi tiêu và tiết kiệm để hoàn thành mục tiêu.
- Liên kết việc chi tiêu hiện tại với mục tiêu tài chính sắp tới.

### 8.6. Tính năng mở rộng

- Mời bạn bè hoặc người thân cùng tham gia quản lý ví chung.

Đây là tính năng tùy chọn, có thể triển khai sau khi các nghiệp vụ cốt lõi đã ổn định.

## 9. Tính năng cho Admin

Admin là vai trò phục vụ vận hành hệ thống, không tham gia vào quản lý tiền của người dùng. Các chức năng chính gồm:

- quản lý người dùng: xem danh sách, tìm kiếm, khóa hoặc mở khóa tài khoản;
- gửi thông báo hệ thống hoặc thông báo hàng loạt;
- theo dõi dashboard quản trị: số lượng người dùng mới, số lượng giao dịch, mức độ sử dụng hệ thống;
- cấu hình system prompt cho AI;
- lựa chọn model AI và quản lý API key phục vụ tích hợp.

## 10. Tính năng ở cấp hệ thống

Ngoài các thao tác trực tiếp từ User và Admin, hệ thống còn cần có các năng lực nền như:

- AI chủ động đưa ra lời khuyên thay vì phụ thuộc hoàn toàn vào thiết lập thủ công;
- đề xuất hành động phù hợp để người dùng đạt mục tiêu tiết kiệm;
- cơ chế cảnh báo nhiều lớp để giảm nguy cơ vượt hạn mức mà không được thông báo;
- xử lý dữ liệu đồng bộ giữa nhiều thiết bị.

## 11. Yêu cầu kỹ thuật

Theo định hướng hiện tại, hệ thống được triển khai với stack sau:

- **Backend**: .NET 8
- **Frontend**: React + Vite
- **Database**: PostgreSQL
- **Deployment**: Docker

Từ lựa chọn này có thể thấy dự án hướng đến một kiến trúc web hiện đại, dễ tách lớp và thuận tiện cho việc triển khai, kiểm thử cũng như mở rộng về sau.

## 12. Mốc triển khai dự kiến

Khoảng thời gian triển khai được chia theo các giai đoạn sau:

| Thời gian     | Giai đoạn         | Mục tiêu                                                    |
| ------------- | ----------------- | ----------------------------------------------------------- |
| 15/04 - 19/04 | Setup & API       | Hoàn thành API giả lập, thiết lập repo và nền tảng ban đầu  |
| 19/04 - 23/04 | Entity & Database | Xây dựng entity, thiết kế cơ sở dữ liệu, chuẩn bị giao diện |
| 23/04 - 07/05 | Implement         | Triển khai các nghiệp vụ chính                              |
| 07/05 - 15/05 | Test & QA         | Kiểm thử, sửa lỗi, tối ưu và hoàn thiện                     |

## 13. Rủi ro và hướng xử lý

Một số rủi ro đáng chú ý của hệ thống gồm:

| Rủi ro                               | Hướng xử lý                                                         |
| ------------------------------------ | ------------------------------------------------------------------- |
| Mất lịch sử giao dịch                | Sao lưu định kỳ, đồng bộ dữ liệu an toàn                            |
| Chậm tiến độ                         | Ưu tiên core feature, theo sát task và điều phối nguồn lực hợp lý   |
| Vượt hạn mức mà không cảnh báo       | Thiết kế cảnh báo nhiều lớp, kiểm thử kỹ luồng ngân sách            |
| Lỗi khi chuyển tiền giữa các hũ      | Có cơ chế rollback và thông báo lỗi rõ ràng                         |
| Nhập sai hoặc dư số tiền             | Validate dữ liệu đầu vào, chặn giao dịch không hợp lệ               |
| Xung đột dữ liệu trên nhiều thiết bị | Đồng bộ realtime hoặc theo phiên bản, có cơ chế conflict resolution |

## 14. Rủi ro kỹ thuật trong quá trình phát triển (góc nhìn senior)

Phần này tập trung vào các rủi ro dễ phát sinh trong lúc code thực tế (đặc biệt khi backend .NET mới ở giai đoạn đầu), dựa trên schema hiện tại và các luồng nghiệp vụ đã mô tả.

| STT | Nhóm           | Rủi ro có thể xuất hiện                                                                                                                | Biểu hiện lỗi thường gặp                                                                | Hướng giảm thiểu sớm                                                                                                     |
| --- | -------------- | -------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| 1   | Backend + DB   | **Race condition khi cập nhật số dư** ở `financial_accounts` và `jars` khi nhiều request cùng lúc (ghi giao dịch, transfer, allocate). | Số dư bị lệch, âm bất thường, hoặc lỗi constraint không ổn định theo thời điểm.         | Dùng transaction + lock cấp row (`SELECT ... FOR UPDATE`), quy ước thứ tự cập nhật, test concurrency ngay từ sprint đầu. |
| 2   | Backend + DB   | **Deadlock khi chuyển tiền giữa hũ** nếu các transaction lock theo thứ tự khác nhau.                                                   | PostgreSQL báo deadlock (`40P01`), request fail ngẫu nhiên khi tải cao.                 | Chuẩn hóa thứ tự lock theo `jar_id`, thêm retry policy có backoff cho lỗi deadlock.                                      |
| 3   | Backend + DB   | **Thiếu idempotency khi import sao kê** dẫn đến tạo trùng transaction.                                                                 | Một file import nhiều lần tạo bản ghi lặp, dashboard tăng sai số.                       | Bắt buộc key chống trùng (external id/hash), lưu import checksum, confirm theo cơ chế idempotent command.                |
| 4   | Backend + DB   | **Lệch enum giữa code và DB** (schema dùng `varchar + check constraint`).                                                              | API trả lỗi insert/update do vi phạm check constraint sau khi FE/BE đổi tên trạng thái. | Tạo enum/domain constant dùng chung, kiểm thử contract API, migration bắt buộc khi đổi trạng thái.                       |
| 5   | Backend + DB   | **Quên filter soft delete** ở bảng `transactions`, `categories`...                                                                     | Dữ liệu đã xóa vẫn hiện trên dashboard/list, sai báo cáo.                               | Chuẩn hóa repository/specification mặc định chỉ lấy bản ghi active, thêm test regression cho truy vấn chính.             |
| 6   | Backend + DB   | **Sai lệch số tiền do làm tròn** giữa `decimal` (.NET/DB) và `number` (JS).                                                            | Chênh 0.01-0.02 sau nhiều phép cộng/trừ, user mất niềm tin dữ liệu.                     | Chuẩn hóa kiểu tiền tệ: BE dùng `decimal`, FE format/display riêng, không tính toán tài chính bằng float JS thuần.       |
| 7   | Backend + DB   | **Lệch múi giờ** cho `transaction_date`, limit ngày/tháng, reminder.                                                                   | Giao dịch rơi sai ngày, cảnh báo vượt mức đến sớm/muộn, nhắc lịch sai ngày.             | Quy chuẩn UTC trong lưu trữ, quy đổi timezone ở biên API/FE, viết test tại ranh giới cuối ngày/cuối tháng.               |
| 8   | Backend + DB   | **N+1 query và truy vấn nặng dashboard** khi dữ liệu tăng nhanh.                                                                       | API dashboard chậm, timeout, CPU DB tăng cao.                                           | Thiết kế query tổng hợp rõ ràng, index đúng cột lọc/sắp xếp, profiling SQL trước khi mở rộng tính năng.                  |
| 9   | Backend (.NET) | **Lỗi phân quyền/ownership** khi chỉ kiểm tra `id` mà thiếu ràng buộc `user_id`.                                                       | User A truy cập/chỉnh được dữ liệu của user B (IDOR).                                   | Mọi query nghiệp vụ phải kèm `user_id`, thêm integration test phân quyền ở endpoint nhạy cảm.                            |
| 10  | Backend (.NET) | **Mất tính nhất quán giữa ghi dữ liệu và gửi notification/event**.                                                                     | Transaction đã ghi thành công nhưng cảnh báo không tạo, hoặc ngược lại.                 | Áp dụng pattern outbox/inbox cho tác vụ bất đồng bộ, có job retry và theo dõi trạng thái phát sự kiện.                   |
| 11  | Frontend UI/UX | **Double submit do UX loading chưa chặt** (click nhiều lần khi mạng chậm).                                                             | Phát sinh giao dịch trùng, user tưởng hệ thống lỗi nặng.                                | Disable nút khi đang submit, hiển thị trạng thái rõ, BE vẫn cần idempotency để chặn trùng từ gốc.                        |
| 12  | Frontend UI/UX | **Thông tin dashboard quá dày trên mobile** (chart + card + filter) gây khó đọc/sai thao tác.                                          | User hiểu nhầm số dư khả dụng, chạm nhầm action, bỏ qua cảnh báo quan trọng.            | Thiết kế mobile-first cho màn tổng quan, ưu tiên thứ bậc thông tin, test usability với kịch bản nhập chi tiêu nhanh.     |

## 15. Kết luận

Theo cách mình diễn giải, đây không chỉ là một ứng dụng ghi chép thu chi, mà là một **trợ lý quản lý tài chính cá nhân** có tính tự động hóa cao.

Giá trị cốt lõi của hệ thống nằm ở 4 điểm:

- giảm thao tác thủ công cho người dùng;
- giúp nhìn rõ tình hình tài chính theo từng nhóm mục tiêu;
- cảnh báo sớm trước khi phát sinh vấn đề;
- tận dụng AI để biến dữ liệu chi tiêu thành hành động cụ thể và dễ áp dụng.

Nếu triển khai đúng trọng tâm, sản phẩm có thể tạo ra khác biệt rõ rệt so với các ứng dụng chỉ dừng ở mức ghi chép và thống kê đơn thuần.
