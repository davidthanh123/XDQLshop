using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Models;

namespace ShopQuanLy.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<BienThe> BienThes => Set<BienThe>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<DonHang> DonHangs => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
    public DbSet<LichSuKho> LichSuKhos => Set<LichSuKho>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<TaiKhoan>().HasIndex(x => x.TenDangNhap).IsUnique();
        b.Entity<SanPham>().HasIndex(x => x.MaSp).IsUnique();
        b.Entity<BienThe>().HasIndex(x => x.MaSku).IsUnique();
        b.Entity<KhachHang>().HasIndex(x => x.SoDienThoai);

        b.Entity<SanPham>().Property(x => x.GiaBan).HasColumnType("decimal(18,0)");
        b.Entity<SanPham>().Property(x => x.GiaVon).HasColumnType("decimal(18,0)");
        b.Entity<ChiTietDonHang>().Property(x => x.DonGia).HasColumnType("decimal(18,0)");
        b.Entity<DonHang>().Property(x => x.TienThuHo).HasColumnType("decimal(18,0)");

        b.Entity<DonHang>().Ignore(x => x.TongTien);
        b.Entity<BienThe>().Ignore(x => x.Ten);

        b.Entity<DonHang>().Property(x => x.Kenh).HasConversion<string>().HasMaxLength(20);
        b.Entity<DonHang>().Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(20);
        b.Entity<LichSuKho>().Property(x => x.Loai).HasConversion<string>().HasMaxLength(20);
    }
}
