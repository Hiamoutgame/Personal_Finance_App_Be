# TÀI LIỆU NGHIỆP VỤ
# DỰ ÁN FINJAR — ỨNG DỤNG QUẢN LÝ TÀI CHÍNH CÁ NHÂN

**Phiên bản:** 1.0  
**Ngày phát hành:** 26/05/2026  
**Đối tượng đọc:** Ban điều hành, đội ngũ kinh doanh, marketing, vận hành

---

## MỤC LỤC

1. Giới thiệu sản phẩm
2. Bài toán và giá trị mang lại
3. Đối tượng người dùng
4. Các tính năng chính
5. Quy tắc nghiệp vụ
6. Vai trò và quyền hạn
7. Tích hợp & đối tác
8. Lộ trình phát triển

---

## 1. GIỚI THIỆU SẢN PHẨM

**FinJar** là ứng dụng **quản lý tài chính cá nhân** thế hệ mới, giúp người dùng theo dõi dòng tiền, phân bổ ngân sách theo "hũ" (jars), thiết lập mục tiêu tiết kiệm và nhận cảnh báo sớm khi sắp vượt chi.

Sản phẩm hướng đến trải nghiệm **tự động hóa cao**: tích hợp ngân hàng, nhận diện hóa đơn bằng OCR, tư vấn bằng trí tuệ nhân tạo, qua đó giảm tối đa thời gian người dùng phải nhập liệu thủ công.

### Tầm nhìn
> "Mỗi người Việt đều có một trợ lý tài chính cá nhân thông minh trong túi."

---

## 2. BÀI TOÁN VÀ GIÁ TRỊ MANG LẠI

### 2.1 Vấn đề người dùng đang gặp
| Vấn đề | Hậu quả |
|---|---|
| Cộng/trừ chi tiêu bằng tay, ghi sổ rời rạc | Tốn thời gian, dễ sai số |
| Khó theo dõi các khoản chi nhỏ phát sinh liên tục | Mất kiểm soát, không biết tiền "đi đâu" |
| Không có cảnh báo sớm khi sắp vượt ngân sách | Vượt chi cuối tháng, ảnh hưởng kế hoạch |
| Chi tiêu hằng ngày không gắn với mục tiêu dài hạn | Mục tiêu tiết kiệm bị bỏ quên |
| Dữ liệu nằm rải rác Excel/sổ tay/ứng dụng cũ | Khó nhìn tổng quan, khó ra quyết định |

### 2.2 Giá trị FinJar mang lại
1. **Tiết kiệm thời gian** — sao kê tự đồng bộ, hóa đơn tự nhận diện.
2. **Chủ động ngân sách** — chia tiền vào hũ theo phương pháp đã được kiểm chứng (6 Hũ, 50-30-20…).
3. **Cảnh báo sớm** — thông báo khi sắp vượt hạn mức chi tiêu, nhắc lịch thanh toán hóa đơn.
4. **Mục tiêu rõ ràng** — gắn từng khoản tiết kiệm với mục tiêu cụ thể (mua xe, du học, mua nhà…).
5. **Tư vấn cá nhân hóa** — trợ lý AI gợi ý dựa trên hành vi chi tiêu thực tế của người dùng.

---

## 3. ĐỐI TƯỢNG NGƯỜI DÙNG

| Nhóm | Đặc điểm | Nhu cầu chính |
|---|---|---|
| **Người nội trợ** | Quản lý chi tiêu gia đình | Đơn giản, dễ dùng, theo dõi đa nguồn tiền |
| **Người đi làm bận rộn** | Thu nhập đều, nhiều giao dịch ngân hàng | Tự động hóa, ít thao tác, đồng bộ ngân hàng |
| **Người trẻ mới quản lý tài chính** | Thu nhập trung bình, đang xây thói quen | Gợi ý phương pháp, học tài chính cơ bản |
| **Hộ gia đình / nhóm nhỏ** | Quản lý ví chung | Hũ chung, phân quyền (lộ trình V2) |
| **Quản trị viên** | Vận hành hệ thống | Quản lý người dùng, gửi thông báo, cấu hình AI |

---

## 4. CÁC TÍNH NĂNG CHÍNH

### 4.1 Khảo sát đầu vào (Onboarding)
- Người dùng mới được hướng dẫn qua wizard 3–5 bước: thu nhập, nghề nghiệp, độ tuổi, mục tiêu tài chính, thách thức chi tiêu.
- Hệ thống **đề xuất phương pháp ngân sách phù hợp**:
  - **6 Hũ** (T.Harv Eker): chia thu nhập theo 6 mục đích.
  - **Quy tắc 50-30-20**: 50% nhu cầu thiết yếu, 30% cá nhân, 20% tiết kiệm.
  - **Tùy chỉnh** (Custom) nếu người dùng đã có hệ thống riêng.

### 4.2 Quản lý nguồn tiền
- Người dùng có thể có **nhiều nguồn tiền**: tiền mặt, tài khoản ngân hàng, ví điện tử.
- Hai chế độ:
  - **Thủ công**: tự nhập số dư, tự cập nhật khi cần.
  - **Liên kết ngân hàng**: qua OAuth Casso → giao dịch tự động đồng bộ.
- Mỗi người dùng có **một nguồn tiền mặc định** (dùng khi tạo nhanh giao dịch).

### 4.3 Hũ ngân sách (Jars)
- "Hũ" là khái niệm cốt lõi để **phân bổ tiền** theo mục đích.
- Người dùng có thể tạo, đặt màu, đặt biểu tượng, tạm dừng hoặc lưu trữ hũ.
- Số dư hũ **do hệ thống tự tính** dựa trên các thao tác phân bổ và giao dịch — người dùng không sửa số dư trực tiếp (đảm bảo dữ liệu nhất quán).

### 4.4 Giao dịch
- Hai loại giao dịch trong phiên bản hiện tại: **Thu nhập (Income)** và **Chi tiêu (Expense)**.
- Mỗi giao dịch gắn với: nguồn tiền, hũ, danh mục, ngày giờ, ghi chú.
- Nguồn dữ liệu giao dịch:
  - **Manual**: người dùng tự nhập.
  - **Imported**: từ sao kê (CSV/Excel).
  - **OCR**: từ ảnh hóa đơn.
  - **Linked API**: tự đồng bộ từ ngân hàng (Casso).

### 4.5 Danh mục chi tiêu
- Hệ thống cung cấp **danh mục mặc định** (ăn uống, đi lại, mua sắm, hóa đơn…) do quản trị viên duy trì.
- Người dùng có thể **tạo danh mục riêng** với màu, biểu tượng tùy ý.

### 4.6 Hạn mức chi tiêu (Limits)
- Người dùng đặt hạn mức theo **hũ** hoặc theo **danh mục**.
- Chu kỳ: **Ngày** hoặc **Tháng**.
- Có ngưỡng cảnh báo theo phần trăm (vd: cảnh báo khi đạt 80%).
- Hệ thống **tự gửi thông báo** khi giao dịch mới làm vượt ngưỡng.

### 4.7 Mục tiêu tiết kiệm (Goals)
- Người dùng đặt mục tiêu: số tiền cần đạt, ngày đến hạn, ghi chú.
- Có thể **gắn với một hũ** để theo dõi tiến độ trực quan.
- Mỗi lần "đóng góp vào mục tiêu", số dư mục tiêu tăng — nếu đóng góp từ hũ thì số dư hũ tương ứng giảm.
- Trạng thái: Đang chạy / Hoàn thành / Đã hủy.

### 4.8 Nhắc lịch (Reminders)
- Tạo nhắc nhở cho hóa đơn định kỳ: điện, nước, internet, học phí, bảo hiểm…
- Tần suất: Ngày / Tuần / Tháng / Quý / Năm.
- Chọn số ngày báo trước (vd: nhắc trước 3 ngày).

### 4.9 Nhập sao kê & nhận diện hóa đơn
- **Nhập sao kê**: tải lên file CSV/Excel từ ngân hàng → hệ thống phân tích, người dùng **xem trước (preview)** và chỉnh sửa trước khi xác nhận tạo giao dịch.
- **OCR ảnh hóa đơn**: chụp/upload ảnh → hệ thống tự trích **số tiền, ngày, tên cửa hàng** → người dùng kiểm tra và lưu.

### 4.10 Liên kết ngân hàng (Casso)
- Người dùng đăng nhập ngân hàng qua dịch vụ Casso (OAuth).
- Sau khi đồng ý, hệ thống tự đồng bộ giao dịch mới về.
- Có thể **bật/tắt đồng bộ tự động** cho từng kết nối.

### 4.11 Tổng quan tài chính (Dashboard)
Dashboard cá nhân hiển thị:
- Tổng số dư, số đã phân bổ vào hũ, số chưa phân bổ.
- Thu nhập, chi tiêu, lợi nhuận ròng tháng hiện tại.
- Tóm tắt từng hũ (số dư, % chi tiêu so với phân bổ).
- Phân tích chi theo danh mục.
- Tiến độ các mục tiêu đang theo đuổi.
- Danh sách giao dịch gần đây.

### 4.12 Thông báo (Notifications)
- Hộp thư trong ứng dụng tập hợp:
  - Cảnh báo vượt hạn mức.
  - Cập nhật tiến độ mục tiêu.
  - Nhắc thanh toán.
  - Thông báo hệ thống.
  - Tin nhắn broadcast từ quản trị viên.

### 4.13 Trợ lý AI (AI Chat)
- Người dùng đặt câu hỏi về tài chính cá nhân (vd: "tháng này em tiêu gì nhiều nhất?", "làm sao tiết kiệm 50 triệu trong 1 năm?").
- Trợ lý sử dụng dữ liệu chi tiêu thật để gợi ý cá nhân hóa.
- Nội dung phản hồi gồm: câu trả lời, gợi ý hành động.

---

## 5. QUY TẮC NGHIỆP VỤ

### 5.1 Quy tắc về tài khoản
- **Mỗi email/username chỉ tạo được một tài khoản**.
- Trạng thái tài khoản:
  - **Active**: được sử dụng bình thường.
  - **Banned**: bị quản trị viên khóa, không đăng nhập được. Khi khóa, phải nêu rõ lý do.
- Người dùng **bắt buộc hoàn tất Onboarding** trước khi sử dụng các tính năng nghiệp vụ.

### 5.2 Quy tắc về tiền
- **Số tiền hiển thị/nhập trên giao diện luôn là số dương.**
- Hệ thống tự quyết định "âm/dương" trong dữ liệu nội bộ dựa trên loại giao dịch (Thu / Chi).
- **Đơn vị tiền tệ mặc định**: VND. Hệ thống lưu mã tiền theo chuẩn ISO (VND, USD…).

### 5.3 Quy tắc về hũ và phân bổ
- Số dư hũ **không thể âm**.
- Người dùng **không sửa được số dư hũ trực tiếp** — phải qua thao tác **phân bổ tiền** hoặc qua giao dịch.
- Một hũ ở trạng thái **Archived** (lưu trữ) thì không nhận thêm phân bổ mới.

### 5.4 Quy tắc về giao dịch
- Mỗi giao dịch **bắt buộc** gắn với một **nguồn tiền** cụ thể.
- Giao dịch chi tiêu (Expense) **nên** (nhưng không bắt buộc) gắn với một **hũ** và một **danh mục** để báo cáo chính xác.
- **Xóa giao dịch** là xóa mềm — dữ liệu vẫn lưu trong hệ thống, mặc định ẩn khỏi danh sách. Quản trị viên có thể xem lại khi cần.
- Phiên bản hiện tại **chưa hỗ trợ chuyển khoản giữa hai hũ** — sẽ có ở phiên bản sau.

### 5.5 Quy tắc về hạn mức
- Một hạn mức phải **gắn với hũ HOẶC danh mục** (không thể trống cả hai).
- `Ngưỡng cảnh báo` nằm trong khoảng 1% – 100%.
- Khi đạt ngưỡng, hệ thống **tự sinh thông báo** trong vòng tối đa 1 giờ.

### 5.6 Quy tắc về mục tiêu
- `Số tiền mục tiêu` > 0.
- `Số tiền đã tiết kiệm` ≥ 0 và **không vượt quá** số tiền mục tiêu khi trạng thái còn `Active`.
- Khi đạt 100%, mục tiêu có thể tự chuyển sang trạng thái `Completed`.
- Mục tiêu **đã hủy/đã hoàn thành** không nhận thêm khoản đóng góp.

### 5.7 Quy tắc về nhắc lịch
- Nhắc theo tháng có thể chọn ngày trong tháng (1–31). Nếu tháng không có ngày đó (vd: 31/2), hệ thống sẽ dời sang ngày cuối cùng của tháng.
- Người dùng có thể **tạm dừng** nhắc lịch mà không cần xóa.

### 5.8 Quy tắc về nhập sao kê & OCR
- Mỗi lần upload là một "Phiên nhập" có vòng đời: `Đang chờ → Đang xử lý → Chờ duyệt → Hoàn tất / Thất bại`.
- Người dùng phải **xác nhận** thì giao dịch mới được tạo chính thức.
- Phiên nhập có thể bị hủy trước khi xác nhận.
- Hệ thống chống trùng lặp giao dịch theo mã giao dịch ngân hàng (đối với Casso) — đồng bộ nhiều lần không tạo bản trùng.

### 5.9 Quy tắc về Casso (liên kết ngân hàng)
- Kết nối có hạn sử dụng theo token mà ngân hàng/Casso cấp. Khi hết hạn, người dùng **phải xác thực lại**.
- Token được mã hóa trước khi lưu trữ — không ai (kể cả nhân viên kỹ thuật) đọc được trực tiếp.
- Khi người dùng **hủy kết nối**, dữ liệu giao dịch đã đồng bộ vẫn giữ — chỉ ngừng đồng bộ tiếp.

### 5.10 Quy tắc về thông báo broadcast
- Chỉ **Admin** mới được tạo broadcast.
- Broadcast có thể **gửi ngay** hoặc **lên lịch** cho thời điểm trong tương lai.
- Sau khi gửi, hệ thống lưu số người nhận để báo cáo.

### 5.11 Quy tắc về dữ liệu cá nhân
- Mỗi người dùng **chỉ thấy dữ liệu của chính mình** — không có cách nào người dùng A thấy hũ/giao dịch của người dùng B.
- Quản trị viên **không thấy nội dung giao dịch** của người dùng cuối; chỉ thấy thông tin tổng quan, trạng thái tài khoản.
- Mọi thao tác quản trị quan trọng (khóa user, đổi role, tạo broadcast…) đều được **ghi log audit** với người thực hiện, thời điểm, IP.

---

## 6. VAI TRÒ VÀ QUYỀN HẠN

| Vai trò | Phạm vi |
|---|---|
| **User (Người dùng cuối)** | Quản lý tài chính cá nhân: nguồn tiền, hũ, giao dịch, danh mục riêng, hạn mức, mục tiêu, nhắc lịch, dùng AI chat |
| **Admin (Quản trị viên)** | Quản lý người dùng (khóa/mở/đổi role), danh mục mặc định, gửi broadcast, xem dashboard vận hành, audit log, cấu hình AI |

**Nguyên tắc:**
- Admin **không thao tác trên dữ liệu nghiệp vụ** của người dùng (không tạo/sửa giao dịch hộ user).
- Mọi hành động Admin đều được ghi vào audit log.

---

## 7. TÍCH HỢP & ĐỐI TÁC

| Đối tác | Vai trò |
|---|---|
| **Casso** | Dịch vụ trung gian cho phép FinJar đọc giao dịch ngân hàng của người dùng (qua sự đồng ý của họ) |
| **Google Gemini** | Mô hình AI cung cấp tư vấn tài chính |
| **Cloudinary** | Dịch vụ lưu trữ hình ảnh (avatar, ảnh hóa đơn) |
| **PaddleOCR** | Công nghệ nhận diện chữ trong ảnh hóa đơn (chạy trên server, không gửi ảnh ra ngoài) |
| **Gmail SMTP** | Gửi email thông báo, reset mật khẩu |

---

## 8. LỘ TRÌNH PHÁT TRIỂN

### Phiên bản V1 — Đang triển khai (Q2/2026)
- Toàn bộ tính năng cốt lõi mô tả ở mục 4.
- Liên kết Casso, OCR, AI Chat Gemini.
- Dashboard cá nhân & dashboard Admin, broadcast, audit log.

### Phiên bản V2 — Lộ trình
- **Chuyển khoản giữa hai hũ** (Transfer Jar-to-Jar).
- **Hũ chung cho hộ gia đình / nhóm bạn** (Shared Jar).
- **Mời thành viên cùng quản lý ví chung**, phân quyền xem/sửa.
- **Báo cáo nâng cao**: xuất Excel/PDF lịch sử giao dịch theo kỳ.
- **Kế hoạch tiết kiệm thông minh** do AI đề xuất tự động cho từng mục tiêu.

### Tính năng tùy chọn (Optional, đang đánh giá)
- Trợ lý AI dạng chat 24/7 chủ động nhắc nhở thói quen tài chính.
- Tích hợp thêm nhiều ngân hàng/ví khác ngoài Casso.
- Hỗ trợ đa tiền tệ (multi-currency) cho người sống/làm việc ở nước ngoài.

---

## 9. TÓM TẮT CÁC GIÁ TRỊ ĐIỂM (KEY TAKEAWAYS)

1. **FinJar = Quản lý theo "Hũ"**: trực quan, đã được kiểm chứng bằng các phương pháp 6 Hũ / 50-30-20.
2. **Tự động hóa**: đồng bộ ngân hàng + OCR hóa đơn ⇒ giảm 70–80% thao tác nhập tay.
3. **Cảnh báo chủ động**: thông báo trước khi vượt hạn mức, trước hạn thanh toán.
4. **Mục tiêu gắn với chi tiêu**: từng đồng tiết kiệm đi vào một mục tiêu cụ thể.
5. **AI cá nhân hóa**: tư vấn dựa trên dữ liệu thật, không phải lời khuyên chung chung.
6. **An toàn dữ liệu**: token ngân hàng mã hóa, mỗi user chỉ thấy dữ liệu của mình, audit đầy đủ.
7. **Sẵn sàng mở rộng**: kiến trúc hỗ trợ thêm ngân hàng, thêm tính năng nhóm/gia đình ở V2.

---

*Hết tài liệu nghiệp vụ.*
