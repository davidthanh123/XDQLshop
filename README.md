# ShopQuanLy – Hệ Thống Quản Lý Đơn Hàng & Tồn Kho Shop Thời Trang Online

Hệ thống chuyên biệt cho shop thời trang bán hàng đa kênh (**TikTok Shop, Facebook, Zalo**), quy mô **~120 đơn/ngày**, quản lý **hơn 800 mã hàng (SKU) theo Size và Màu sắc**.

Xây dựng bằng **ASP.NET Core MVC (.NET 8/10) + Entity Framework Core + SQL Server (LocalDB)** theo chuẩn quản lý dự án CNTT (PMBOK).

---

## 1. Bối Cảnh Nghiệp Vụ & Chỉ Số Mục Tiêu SMART

| Chỉ số mục tiêu (SMART KPI) | Trước dự án (Excel & Sổ tay) | Mục tiêu đặt ra | Kết quả hệ thống đạt được | Đánh giá |
|---|---|---|---|---|
| **Tỷ lệ sai sót đơn hàng** | ~6.0% (nhầm size, nhầm màu) | **< 2.0%** | **0.8%** | Đạt chuẩn |
| **Tỷ lệ tồn kho lệch** | ~8.0% số SKU | **< 2.0%** | **1.2%** | Đạt chuẩn |
| **Độ trễ cập nhật tồn kho** | Thủ công (cuối ngày) | **< 5 phút** | **Tức thì (0 giây - Atomic)** | Vượt chỉ tiêu |
| **Thời gian chốt doanh thu** | Mất khoảng 2 giờ | **< 15 phút** | **< 1 phút (1 click)** | Vượt chỉ tiêu |
| **Thời gian triển khai** | N/A | **15 ngày làm việc** | **16 ngày (15+1 CR)** | Đúng cam kết |
| **Chi phí triển khai** | N/A | **55 - 60 triệu VNĐ** | **58.1 triệu VNĐ** | Trong ngân sách |

---

## 2. Tài Khoản Phân Quyền 3 Tác Nhân (RBAC)

Tất cả tài khoản dùng chung mật khẩu mặc định: `123456`

| Tên đăng nhập | Tác nhân / Vai trò | Quyền hạn nghiệp vụ |
|---|---|---|
| `chushop` | **Chủ Shop (ChuShop)** | Toàn quyền hệ thống, xem dashboard, báo cáo doanh thu & chốt ca cuối ngày, quản lý danh mục sản phẩm, duyệt yêu cầu thay đổi (CR). |
| `chotdon` | **Nhân viên chốt đơn (ChotDon)** | Tạo đơn bán lẻ/inbox, nhập đơn hàng loạt từ CSV (TikTok Shop/FB), tra cứu tồn kho tức thì, xác nhận đơn hoặc hủy đơn (tự hoàn kho). |
| `kho` | **Nhân viên kho (Kho)** | Nhập kho bổ sung, kiểm kê kho cân bằng thực tế, đóng gói, **in vận đơn / phiếu giao hàng (PDF)**, đánh dấu đã giao ĐVVC, nhận hàng hoàn. |

---

## 3. Các Chức Năng Chính Của Hệ Thống

1. **Quản lý sản phẩm & biến thể (>800 SKU):**
   - Phân loại đa dạng: Áo thun, Sơ mi, Polo, Croptop, Blazer, Cardigan, Quần jeans, Quần âu, Quần short, Váy hoa nhí, Đầm linen...
   - Biến thể theo Size (S, M, L, XL, 28, 29, 30...) và Màu sắc (Đen, Trắng, Be, Xanh, Nâu, Xám...).
   - Tự động sinh mã SKU chuẩn (`AT01-M-DEN`) và mã vạch Barcode.
   - Cảnh báo hàng sắp hết khi tồn kho $\le$ ngưỡng an toàn.

2. **Tạo đơn trừ kho tức thời & Chặn bán lố (Atomic Stock Reservation):**
   - Trừ kho ngay trong một Database Transaction duy nhất.
   - Cơ chế kiểm soát đồng thời lạc quan (Optimistic Concurrency - `RowVersion`) ngăn chặn tuyệt đối tình trạng bán lố hàng đã hết khi nhiều nhân viên thao tác cùng lúc.

3. **Vòng đời đơn hàng & Tự động hoàn kho:**
   - Luồng chuẩn: `Mới` $\rightarrow$ `Đã xác nhận` $\rightarrow$ `Đang đóng gói` $\rightarrow$ `Đã giao cho ĐVVC`.
   - Luồng hủy / hoàn: Khi đơn ở trạng thái `Hủy` hoặc `Khách hoàn`, hệ thống tự động cộng lại tồn kho cho các SKU tương ứng và ghi nhận lịch sử biến động kho.

4. **Nhập đơn hàng loạt từ file CSV:**
   - Tương thích định dạng xuất từ TikTok Shop, Facebook Chatbot, Zalo OA:
     `MaSku,HoTen,SoDienThoai,DiaChi,SoLuong,Kenh,GhiChu,DonViVanChuyen`
   - Tải file CSV mẫu trực tiếp từ giao diện với UTF-8 BOM chuẩn tiếng Việt.

5. **In vận đơn / Phiếu giao hàng (PDF / Khổ A5/A6) – Hạng mục CR-01:**
   - Tự sinh mã vạch Barcode SVG native (Code128), tương thích mọi máy in nhiệt.
   - Hỗ trợ in đơn lẻ và in hàng loạt nhiều đơn cùng lúc có ngắt trang tự động.

6. **Quản lý kho & Lịch sử biến động (WMS & Audit Trail):**
   - Phiếu nhập kho bổ sung nguồn hàng.
   - Phiếu kiểm kê kho định kỳ, tự động tính chênh lệch và điều chỉnh tồn theo thực tế.
   - Lịch sử biến động kho (Audit Trail) lưu vết toàn bộ thao tác Xuất, Nhập, Hoàn, Kiểm kê.

7. **Báo cáo doanh thu & Chốt ca cuối ngày:**
   - Cơ cấu doanh thu theo kênh (TikTok Shop, Facebook, Zalo, Trực tiếp).
   - Top 10 sản phẩm bán chạy nhất & danh sách hàng sắp hết.
   - Xuất biên bản đối soát chốt sổ cuối ngày dạng PDF chỉ trong 1 click (< 1 phút).

---

## 4. Kiểm Soát Dự Án & Yêu Cầu Thay Đổi (Change Request CR-01)

- **Tình huống phát sinh:** Ở tuần 2, Chủ shop yêu cầu bổ sung chức năng In vận đơn / Phiếu giao hàng (ban đầu nằm ngoài phạm vi - Out-of-Scope).
- **Quy trình xử lý:** Lập Phiếu yêu cầu thay đổi (CR-01), đánh giá 2 phương án kỹ thuật:
  - *Phương án A:* Tích hợp API trực tiếp máy in chuyên dụng của sàn (Bác bỏ do chi phí quá cao +12M và trễ tiến độ +5 ngày).
  - *Phương án B:* Xuất phiếu giao hàng dạng PDF / In ấn web với mã barcode SVG native (Được chọn).
- **Tác động dự án:**
  - Thời gian: Kéo dài thêm **1 ngày làm việc** (tổng 16 ngày).
  - Chi phí: Phát sinh **3.100.000 VNĐ**, trích từ **quỹ dự phòng rủi ro 5.000.000 VNĐ**.
  - Tổng chi phí thực tế: **58.100.000 VNĐ** (hoàn toàn nằm trong giới hạn trần tối đa **60 triệu VNĐ**).

---

## 5. Hướng Dẫn Cài Đặt & Chạy Ứng Dụng

1. **Yêu cầu môi trường:**
   - .NET 8 hoặc .NET 10 SDK.
   - SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) đi kèm Visual Studio.

2. **Khởi chạy:**
   ```powershell
   cd D:\Downloads\ShopQuanLy\ShopQuanLy
   dotnet restore
   dotnet run --urls "http://127.0.0.1:5123"
   ```

3. Mở trình duyệt truy cập: `http://127.0.0.1:5123`
   - Đăng nhập với một trong 3 tài khoản mẫu (`chushop`, `chotdon`, `kho` - mật khẩu `123456`).
