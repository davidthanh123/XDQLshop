using System.ComponentModel.DataAnnotations;

namespace ShopQuanLy.Models;

public enum KenhBan { TikTokShop, Facebook, Zalo, TrucTiep }
public enum TrangThaiDon { Moi, DaXacNhan, DangDong, DaGiao, Huy, Hoan }
public enum LoaiKho { Nhap, Xuat, Hoan, KiemKe }

public static class VaiTro
{
    public const string ChuShop = "ChuShop";
    public const string ChotDon = "NhanVienChotDon";
    public const string Kho = "NhanVienKho";
}

public class TaiKhoan
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string TenDangNhap { get; set; } = "";
    [Required] public string MatKhauHash { get; set; } = "";
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [Required, StringLength(30)] public string VaiTro { get; set; } = "";
}

public class SanPham
{
    public int Id { get; set; }
    [Required, StringLength(30), Display(Name = "Mã SP")] public string MaSp { get; set; } = "";
    [Required, StringLength(150), Display(Name = "Tên sản phẩm")] public string TenSp { get; set; } = "";
    [Range(0, 100000000), Display(Name = "Giá bán")] public decimal GiaBan { get; set; }
    [Range(0, 100000000), Display(Name = "Giá vốn")] public decimal GiaVon { get; set; }
    public ICollection<BienThe> BienThes { get; set; } = new List<BienThe>();
}

public class BienThe
{
    public int Id { get; set; }
    public int SanPhamId { get; set; }
    public SanPham SanPham { get; set; } = null!;
    [Required, StringLength(50)] public string MaSku { get; set; } = "";
    [StringLength(10)] public string Size { get; set; } = "";
    [StringLength(30)] public string MauSac { get; set; } = "";
    public int TonKho { get; set; }
    public int NguongCanhBao { get; set; } = 5;
    [StringLength(50)] public string? MaVach { get; set; }
    [Timestamp] public byte[]? RowVersion { get; set; }
    public string Ten => $"{SanPham?.TenSp} - {Size}/{MauSac}";
}

public class KhachHang
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string HoTen { get; set; } = "";
    [Required, StringLength(20)] public string SoDienThoai { get; set; } = "";
    [StringLength(250)] public string DiaChi { get; set; } = "";
}

public class DonHang
{
    public int Id { get; set; }
    [StringLength(20)] public string MaDon { get; set; } = "";
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public KenhBan Kenh { get; set; }
    public TrangThaiDon TrangThai { get; set; } = TrangThaiDon.Moi;
    public int KhachHangId { get; set; }
    public KhachHang KhachHang { get; set; } = null!;
    [StringLength(100)] public string NguoiTao { get; set; } = "";
    [StringLength(250)] public string? GhiChu { get; set; } = "Cho xem hàng, không thử";
    [StringLength(50)] public string? DonViVanChuyen { get; set; } = "Giao Hàng Tiết Kiệm (GHTK)";
    [StringLength(50)] public string? MaVanDon { get; set; }
    public decimal TienThuHo { get; set; }
    public ICollection<ChiTietDonHang> ChiTiet { get; set; } = new List<ChiTietDonHang>();
    public decimal TongTien => ChiTiet.Sum(c => c.SoLuong * c.DonGia);
}

public class ChiTietDonHang
{
    public int Id { get; set; }
    public int DonHangId { get; set; }
    public DonHang DonHang { get; set; } = null!;
    public int BienTheId { get; set; }
    public BienThe BienThe { get; set; } = null!;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
}

public class LichSuKho
{
    public int Id { get; set; }
    public int BienTheId { get; set; }
    public BienThe BienThe { get; set; } = null!;
    public LoaiKho Loai { get; set; }
    public int SoLuong { get; set; }
    public DateTime NgayTao { get; set; } = DateTime.Now;
    [StringLength(200)] public string GhiChu { get; set; } = "";
    [StringLength(100)] public string NguoiThucHien { get; set; } = "";
}
