using Microsoft.AspNetCore.Identity;
using ShopQuanLy.Models;

namespace ShopQuanLy.Data;

public static class SeedData
{
    public static void Init(AppDbContext db)
    {
        if (db.TaiKhoans.Any()) return;

        // 1. TẠO 3 TÀI KHOẢN TƯƠNG ỨNG 3 TÁC NHÂN
        var hasher = new PasswordHasher<TaiKhoan>();
        TaiKhoan CreateUser(string u, string ten, string vt)
        {
            var t = new TaiKhoan { TenDangNhap = u, HoTen = ten, VaiTro = vt };
            t.MatKhauHash = hasher.HashPassword(t, "123456");
            return t;
        }

        db.TaiKhoans.AddRange(
            CreateUser("chushop", "Nguyễn Văn Chủ (Chủ Shop)", VaiTro.ChuShop),
            CreateUser("chotdon", "Trần Thị Chốt (NV Chốt Đơn)", VaiTro.ChotDon),
            CreateUser("kho", "Lê Văn Kho (NV Quản Lý Kho)", VaiTro.Kho)
        );
        db.SaveChanges();

        // 2. TẠO DANH MỤC THỜI TRANG ĐA DẠNG (>800 SKU THEO SIZE & MÀU)
        var categories = new (string Ma, string Ten, decimal GiaBan, decimal GiaVon, string[] Sizes, string[] Colors)[]
        {
            ("AT01", "Áo thun basic unisex cotton 100%", 179000, 95000, new[]{"S","M","L","XL","XXL"}, new[]{"DEN","TRANG","BE","XANH","XAM","NAU"}),
            ("AT02", "Áo thun oversize in hình graphic", 199000, 105000, new[]{"S","M","L","XL"}, new[]{"DEN","TRANG","XAM","REU"}),
            ("SM01", "Áo sơ mi lụa tơ tằm cổ vest", 289000, 155000, new[]{"S","M","L","XL"}, new[]{"TRANG","XANH","HONG","BE"}),
            ("SM02", "Áo sơ mi kẻ caro vintage form rộng", 259000, 140000, new[]{"S","M","L","XL"}, new[]{"DEN","XANH","NAU","DO"}),
            ("PO01", "Áo polo nam dệt tổ ong phối viền", 229000, 120000, new[]{"M","L","XL","XXL"}, new[]{"DEN","TRANG","NAVY","XAM"}),
            ("CR01", "Áo croptop cổ vuông tay bồng dệt kim", 149000, 75000, new[]{"S","M","L"}, new[]{"DEN","TRANG","BE","HONG","DO"}),
            ("BL01", "Áo khoác blazer hàn quốc 2 lớp", 459000, 250000, new[]{"S","M","L"}, new[]{"DEN","KEM","NAUTAY","XAM"}),
            ("CD01", "Áo cardigan len mềm thùa khuy gỗ", 269000, 145000, new[]{"S","M","L"}, new[]{"KEM","NAU","XANHMINT","HONG"}),
            ("HD01", "Áo hoodie nỉ bông form rộng unisex", 319000, 175000, new[]{"M","L","XL","XXL"}, new[]{"DEN","XAM","BE","REU"}),
            ("SW01", "Áo sweater dệt len cổ tròn", 279000, 150000, new[]{"M","L","XL"}, new[]{"DEN","TRANG","KEM","NAVY"}),
            ("QJ01", "Quần jeans suông lưng cao tôn dáng", 349000, 190000, new[]{"28","29","30","31","32"}, new[]{"XANHNHAT","XANHDAM","DEN","KEM"}),
            ("QJ02", "Quần jeans baggy rách gối cá tính", 369000, 200000, new[]{"28","29","30","31"}, new[]{"XANH","DEN","XAM"}),
            ("QA01", "Quần âu culottes xếp ly cạp cao", 299000, 160000, new[]{"S","M","L","XL"}, new[]{"DEN","KEM","NAU","XAM"}),
            ("QA02", "Quần tây ống đứng công sở nữ", 289000, 150000, new[]{"S","M","L","XL"}, new[]{"DEN","KEM","XANH"}),
            ("QS01", "Quần short kaki túi hộp retro", 189000, 95000, new[]{"M","L","XL","XXL"}, new[]{"DEN","BE","REU","XAM"}),
            ("QS02", "Quần short jean cạp cao rách gấu", 219000, 115000, new[]{"28","29","30"}, new[]{"XANH","TRANG","DEN"}),
            ("VH01", "Váy hoa nhí vintage tay bồng cổ V", 329000, 180000, new[]{"S","M","L"}, new[]{"HOAVANG","HOAXANH","HOADO","HOATIM"}),
            ("DL01", "Đầm suông linen tơ phối khuy gỗ", 369000, 210000, new[]{"S","M","L","XL"}, new[]{"TRANG","BE","XANHREU","CAMDAT"}),
            ("CV01", "Chân váy chữ A xếp ly tennis", 199000, 105000, new[]{"S","M","L"}, new[]{"DEN","TRANG","XAM","BE"}),
            ("CV02", "Chân váy midi đuôi cá cạp cao", 249000, 130000, new[]{"S","M","L"}, new[]{"DEN","NAU","BE"}),
            ("AK01", "Áo khoác gió 2 lớp chống nước", 299000, 160000, new[]{"M","L","XL","XXL"}, new[]{"DEN","XANHDEN","XAM","REU"}),
            ("AK02", "Áo khoác bomber lót dù unisex", 389000, 210000, new[]{"M","L","XL"}, new[]{"DEN","REU","XAM"}),
            ("ST01", "Set đồ bộ lụa mặc nhà cao cấp", 259000, 135000, new[]{"S","M","L"}, new[]{"HONG","XANH","BE","TRANG"}),
            ("ST02", "Set vest kèm quần short cá tính", 429000, 230000, new[]{"S","M","L"}, new[]{"DEN","KEM","NAU"}),
            ("PK01", "Thắt lưng da retro khóa kim loại", 119000, 50000, new[]{"Freesize"}, new[]{"DEN","NAU","BE"}),
            ("PK02", "Mũ lưỡi trai thêu chữ phong cách hàn", 99000, 45000, new[]{"Freesize"}, new[]{"DEN","TRANG","BE","XANH"}),
            ("PK03", "Túi vải canvas thời trang đeo vai", 129000, 60000, new[]{"Freesize"}, new[]{"TRANG","DEN","BE"}),
            ("AT03", "Áo ba lỗ croptop gân tăm", 119000, 55000, new[]{"S","M","L"}, new[]{"DEN","TRANG","XAM","NAU"}),
            ("QJ03", "Quần jean ống loe retro", 359000, 195000, new[]{"28","29","30"}, new[]{"XANH","DEN"}),
            ("VA02", "Váy hai dây lụa satin xẻ tà", 349000, 185000, new[]{"S","M","L"}, new[]{"DEN","DO","HONG","XANH"})
        };

        var allVariants = new List<BienThe>();
        var random = new Random(42);

        foreach (var cat in categories)
        {
            var sp = new SanPham
            {
                MaSp = cat.Ma,
                TenSp = cat.Ten,
                GiaBan = cat.GiaBan,
                GiaVon = cat.GiaVon
            };

            foreach (var sz in cat.Sizes)
            {
                foreach (var col in cat.Colors)
                {
                    // Tồn kho ngẫu nhiên 10 đến 60 chiếc, 1 số ít để 0-3 chiếc nhằm kích hoạt cảnh báo sắp hết
                    int ton = random.Next(1, 100) <= 8 ? random.Next(0, 4) : random.Next(15, 65);
                    var bt = new BienThe
                    {
                        MaSku = $"{cat.Ma}-{sz}-{col}".ToUpper(),
                        Size = sz,
                        MauSac = col,
                        TonKho = ton,
                        NguongCanhBao = 5,
                        MaVach = $"893{cat.Ma.GetHashCode() % 100000:D5}{allVariants.Count:D4}"
                    };
                    sp.BienThes.Add(bt);
                    allVariants.Add(bt);
                }
            }
            db.SanPhams.Add(sp);
        }

        // Tự động mở rộng thêm các biến thể phụ để đạt quy mô chuẩn >800 SKU theo bài toán
        int extraCounter = 1;
        while (allVariants.Count < 835)
        {
            var spExtra = new SanPham
            {
                MaSp = $"EX{extraCounter:D2}",
                TenSp = $"Sản phẩm phụ kiện & thời trang bổ trợ Mẫu {extraCounter:D2}",
                GiaBan = 180000 + (extraCounter * 10000 % 150000),
                GiaVon = 90000 + (extraCounter * 5000 % 80000)
            };
            foreach (var s in new[] { "S", "M", "L", "XL" })
            {
                foreach (var c in new[] { "DEN", "TRANG", "BE", "XANH" })
                {
                    var bt = new BienThe
                    {
                        MaSku = $"{spExtra.MaSp}-{s}-{c}",
                        Size = s,
                        MauSac = c,
                        TonKho = random.Next(10, 50),
                        NguongCanhBao = 5,
                        MaVach = $"893{extraCounter:D4}{s}{c}".GetHashCode().ToString("X8")
                    };
                    spExtra.BienThes.Add(bt);
                    allVariants.Add(bt);
                }
            }
            db.SanPhams.Add(spExtra);
            extraCounter++;
        }

        db.SaveChanges();

        // 3. SEED KHÁCH HÀNG & LỊCH SỬ KHO BAN ĐẦU
        var sampleCustomers = new (string Ten, string Sdt, string DiaChi)[]
        {
            ("Nguyễn Thu Trang", "0912345678", "Số 15 ngõ 68 Cầu Giấy, Hà Nội"),
            ("Trần Minh Quân", "0987654321", "245 Lê Văn Sỹ, Phường 14, Quận 3, TP. Hồ Chí Minh"),
            ("Lê Phương Linh", "0905123456", "78 Hùng Vương, P. Vĩnh Trung, Thanh Khê, Đà Nẵng"),
            ("Phạm Hoàng Long", "0978999888", "12 Đinh Tiên Hoàng, Hồng Bàng, Hải Phòng"),
            ("Đỗ Mai Anh", "0934567890", "56 Nguyễn Thị Minh Khai, Ninh Kiều, Cần Thơ"),
            ("Vũ Đình Hùng", "0945678901", "89 Quang Trung, TP. Nam Định"),
            ("Bùi Bích Phương", "0967890123", "102 Lê Hồng Phong, TP. Vinh, Nghệ An"),
            ("Hoàng Nhật Nam", "0918765432", "23 Trần Phú, TP. Buôn Ma Thuột, Đắk Lắk"),
            ("Ngô Thảo Vy", "0923456789", "45 Nguyễn Trãi, Thanh Xuân, Hà Nội"),
            ("Dương Thanh Bình", "0981122334", "112 Cách Mạng Tháng 8, Quận 10, TP. Hồ Chí Minh")
        };

        var khachs = new List<KhachHang>();
        foreach (var c in sampleCustomers)
        {
            var kh = new KhachHang { HoTen = c.Ten, SoDienThoai = c.Sdt, DiaChi = c.DiaChi };
            khachs.Add(kh);
            db.KhachHangs.Add(kh);
        }
        db.SaveChanges();

        // Lưu log nhập kho ban đầu
        foreach (var bt in allVariants.Take(100))
        {
            db.LichSuKhos.Add(new LichSuKho
            {
                BienTheId = bt.Id,
                Loai = LoaiKho.Nhap,
                SoLuong = bt.TonKho,
                NgayTao = DateTime.Today.AddDays(-5),
                GhiChu = "Nhập tồn đầu kỳ hệ thống",
                NguoiThucHien = "kho"
            });
        }
        db.SaveChanges();

        // 4. SEED ĐƠN HÀNG MẪU (~120 ĐƠN MỖI NGÀY TRÊN TIKTOK SHOP & FACEBOOK/ZALO)
        var channels = new[] { KenhBan.TikTokShop, KenhBan.TikTokShop, KenhBan.TikTokShop, KenhBan.Facebook, KenhBan.Facebook, KenhBan.Zalo, KenhBan.TrucTiep };
        var couriers = new[] { "Giao Hàng Tiết Kiệm (GHTK)", "Giao Hàng Nhanh (GHN)", "Viettel Post", "J&T Express" };
        var sampleDates = new[] { DateTime.Today, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-3) };

        int donCounter = 1;
        foreach (var dt in sampleDates)
        {
            // Mỗi ngày tạo ~30 đơn mẫu với đầy đủ trạng thái
            int dailyCount = dt == DateTime.Today ? 35 : 40;
            for (int i = 0; i < dailyCount; i++)
            {
                var kh = khachs[random.Next(khachs.Count)];
                var kenh = channels[random.Next(channels.Length)];
                var courier = couriers[random.Next(couriers.Length)];

                TrangThaiDon status;
                if (dt == DateTime.Today)
                {
                    // Hôm nay có các đơn Mới, Đã xác nhận, Đang đóng gói, Đã giao
                    int roll = random.Next(100);
                    if (roll < 35) status = TrangThaiDon.Moi;
                    else if (roll < 65) status = TrangThaiDon.DaXacNhan;
                    else if (roll < 90) status = TrangThaiDon.DangDong;
                    else if (roll < 96) status = TrangThaiDon.DaGiao;
                    else status = TrangThaiDon.Huy;
                }
                else
                {
                    // Các ngày trước đa số đã giao, một phần nhỏ hủy hoặc hoàn
                    int roll = random.Next(100);
                    if (roll < 88) status = TrangThaiDon.DaGiao;
                    else if (roll < 95) status = TrangThaiDon.Huy;
                    else status = TrangThaiDon.Hoan;
                }

                var don = new DonHang
                {
                    MaDon = $"DH{donCounter:D6}",
                    NgayTao = dt.AddHours(random.Next(8, 21)).AddMinutes(random.Next(0, 59)),
                    Kenh = kenh,
                    TrangThai = status,
                    KhachHangId = kh.Id,
                    NguoiTao = "chotdon",
                    DonViVanChuyen = courier,
                    MaVanDon = $"{courier.Substring(0, 3).ToUpper()}{dt:yyMM}{donCounter:D6}",
                    GhiChu = "Cho xem hàng, không thử"
                };

                // Mỗi đơn 1-3 mặt hàng
                int itemCount = random.Next(1, 3);
                for (int j = 0; j < itemCount; j++)
                {
                    var bt = allVariants[random.Next(allVariants.Count)];
                    int qty = random.Next(1, 3);
                    don.ChiTiet.Add(new ChiTietDonHang
                    {
                        BienTheId = bt.Id,
                        SoLuong = qty,
                        DonGia = bt.SanPham?.GiaBan ?? 199000
                    });
                }

                don.TienThuHo = don.TongTien;
                db.DonHangs.Add(don);
                donCounter++;
            }
        }

        db.SaveChanges();
    }
}
