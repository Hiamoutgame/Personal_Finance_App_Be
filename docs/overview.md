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

## 5.1. Mô hình quản lý theo nhiều hũ chi tiêu

Ứng dụng được xây dựng dựa trên tư duy quản lý tài chính theo nhiều "hũ" hoặc nhóm ngân sách. Mỗi hũ đại diện cho một mục đích cụ thể, ví dụ:

- sinh hoạt;
- ăn uống;
- di chuyển;
- mua sắm;
- tiết kiệm;
- quỹ mục tiêu.

Việc chia tiền theo từng hũ giúp người dùng nhìn rõ khoản nào đang tiêu vượt mức, khoản nào còn dư và khoản nào cần ưu tiên giữ lại.

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
- Nền tảng mục tiêu là **Web App**, có thể phát triển theo hướng **PWA** để thuận tiện sử dụng trên thiết bị di động.

## 7. Nguồn dữ liệu đầu vào

Sau khi cân nhắc các phương thức lấy dữ liệu, hướng triển khai phù hợp nhất hiện tại là:

### 7.1. Import sao kê

Đây là nguồn dữ liệu quan trọng cho các giao dịch qua ngân hàng. Hệ thống có thể kết hợp:

- import file sao kê;
- OCR để đọc dữ liệu từ ảnh hoặc tài liệu;
- AI để gợi ý phân mục chi tiêu.

### 7.2. Nhập tay

Phù hợp với:

- các khoản chi tiền mặt;
- các khoản chi nhỏ phát sinh nhanh;
- dữ liệu người dùng muốn bổ sung thủ công.

### 7.3. Các phương án chưa ưu tiên

Một số phương thức được xem xét nhưng chưa phù hợp ở giai đoạn hiện tại:

- API ngân hàng: khó triển khai do ràng buộc bảo mật, pháp lý và hợp tác.
- SePay/Casso: có chi phí và giới hạn hỗ trợ chưa tối ưu cho bài toán hiện tại.
- SMS Banking: tiềm ẩn rủi ro bảo mật thông tin.

## 8. Tính năng chính cho người dùng

Theo phạm vi sản phẩm, nhóm tính năng của User có thể được tổ chức lại như sau:

### 8.1. Quản lý giao dịch và danh mục

- CRUD dữ liệu chi tiêu.
- Tạo mới hoặc sử dụng danh mục mặc định như ăn uống, di chuyển, mua sắm.
- Chọn phân mục khi nhập dữ liệu.
- Hỗ trợ liên kết tài khoản ngân hàng ở mức phục vụ theo dõi dữ liệu, không giữ tiền.

### 8.2. Theo dõi tài chính tổng quan

- Dashboard hiển thị số dư, biểu đồ thu chi và các giao dịch gần đây.
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

## 14. Kết luận

Theo cách mình diễn giải, đây không chỉ là một ứng dụng ghi chép thu chi, mà là một **trợ lý quản lý tài chính cá nhân** có tính tự động hóa cao.

Giá trị cốt lõi của hệ thống nằm ở 4 điểm:

- giảm thao tác thủ công cho người dùng;
- giúp nhìn rõ tình hình tài chính theo từng nhóm mục tiêu;
- cảnh báo sớm trước khi phát sinh vấn đề;
- tận dụng AI để biến dữ liệu chi tiêu thành hành động cụ thể và dễ áp dụng.

Nếu triển khai đúng trọng tâm, sản phẩm có thể tạo ra khác biệt rõ rệt so với các ứng dụng chỉ dừng ở mức ghi chép và thống kê đơn thuần.
