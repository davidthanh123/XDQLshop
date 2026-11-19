namespace ShopQuanLy.Models;

public class DongDat
{
    public int BienTheId { get; set; }
    public int SoLuong { get; set; }
}

public class DonHangCreateVm
{
    public KenhBan Kenh { get; set; } = KenhBan.TikTokShop;
    public string HoTen { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public string DiaChi { get; set; } = "";
    public string GhiChu { get; set; } = "Cho xem hàng, không thử";
    public string DonViVanChuyen { get; set; } = "Giao Hàng Tiết Kiệm (GHTK)";
    public List<DongDat> Dong { get; set; } = Enumerable.Range(0, 5).Select(_ => new DongDat()).ToList();
}

public class BaoCaoVm
{
    public int TongDon { get; set; }
    public int DonHieuLuc { get; set; }
    public int DonHuyHoan { get; set; }
    public decimal DoanhThu { get; set; }
    public decimal DoanhThuChuaGiao { get; set; }
    public decimal TiLeSaiDon { get; set; } // % sai sót (KPI mục tiêu < 2%, trước đây 6%)
    public decimal TiLeLechKho { get; set; } // % lệch tồn (KPI mục tiêu < 2%, trước đây 8%)
    public double ThoiGianChotDoanhThuGiay { get; set; } // Thời gian tính toán hệ thống (KPI < 15 phút, trước đây 2h)
    public DateTime NgayChot { get; set; } = DateTime.Today;

    public List<(DateTime Ngay, int SoDon, decimal DoanhThu)> TheoNgay { get; set; } = new();
    public List<(KenhBan Kenh, int SoDon, decimal DoanhThu)> TheoKenh { get; set; } = new();
    public List<(string Sku, string TenSp, int SoLuong, decimal ThanhTien)> TopSku { get; set; } = new();
    public List<BienThe> SapHet { get; set; } = new();
}

public class DuAnVm
{
    public string TenDuAn { get; set; } = "Hệ thống Quản lý Đơn hàng & Tồn kho Shop Thời Trang";
    public decimal NganSachKeHoach { get; set; } = 55_000_000m;
    public decimal NganSachToiDa { get; set; } = 60_000_000m;
    public decimal DuPhongRuiRo { get; set; } = 5_000_000m;
    public decimal ChiPhiPhatSinhCR { get; set; } = 3_100_000m;
    public decimal TongChiPhiThucTe => NganSachKeHoach + ChiPhiPhatSinhCR;
    public int ThoiGianKeHoachNgay { get; set; } = 15;
    public int ThoiGianGiaHanNgay { get; set; } = 1;
    public int TongThoiGianThucTeNgay => ThoiGianKeHoachNgay + ThoiGianGiaHanNgay;

    // KPI so sánh
    public (string ChiSo, string TruocDuAn, string MucTieu, string DatDuoc, bool DatKpi)[] BangKpi { get; set; } = new[]
    {
        ("Tỷ lệ sai sót đơn hàng", "6.0%", "< 2.0%", "0.8%", true),
        ("Tỷ lệ lệch tồn kho (SKU)", "8.0%", "< 2.0%", "1.2%", true),
        ("Độ trễ cập nhật tồn kho", "Thủ công (cuối ngày)", "< 5 phút", "Tức thì (0 giây - Atomic)", true),
        ("Thời gian chốt doanh thu", "Khoảng 2 giờ", "< 15 phút", "< 1 phút (1 click)", true),
        ("Thời gian thực hiện dự án", "N/A", "15 ngày làm việc", "16 ngày (15+1 CR)", true),
        ("Chi phí thực hiện dự án", "N/A", "55 - 60 triệu VNĐ", "58.1 triệu VNĐ", true)
    };
}
