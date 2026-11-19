using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;

namespace ShopQuanLy.Controllers;

[Authorize(Roles = VaiTro.ChuShop)]
public class BaoCaoController : Controller
{
    private readonly AppDbContext _db;
    public BaoCaoController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int ngay = 7, DateTime? ngayChot = null)
    {
        var sw = Stopwatch.StartNew();

        DateTime targetChot = ngayChot ?? DateTime.Today;
        var tu = DateTime.Today.AddDays(-(Math.Max(1, ngay) - 1));

        var dons = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe).ThenInclude(b => b.SanPham)
            .Where(d => d.NgayTao >= tu)
            .ToListAsync();

        var hieuLuc = dons.Where(d => d.TrangThai is not (TrangThaiDon.Huy or TrangThaiDon.Hoan)).ToList();
        var donChotHomNay = dons.Where(d => d.NgayTao.Date == targetChot.Date).ToList();
        var hieuLucHomNay = donChotHomNay.Where(d => d.TrangThai is not (TrangThaiDon.Huy or TrangThaiDon.Hoan)).ToList();

        // Thống kê theo ngày
        var theoNgay = hieuLuc.GroupBy(d => d.NgayTao.Date)
            .OrderBy(g => g.Key)
            .Select(g => (g.Key, g.Count(), g.Sum(d => d.TongTien)))
            .ToList();

        // Thống kê theo kênh bán
        var theoKenh = hieuLuc.GroupBy(d => d.Kenh)
            .Select(g => (g.Key, g.Count(), g.Sum(d => d.TongTien)))
            .OrderByDescending(x => x.Item3)
            .ToList();

        // Top 10 SKU bán chạy
        var topSkuData = hieuLuc.SelectMany(d => d.ChiTiet)
            .GroupBy(c => c.BienTheId)
            .Select(g => new
            {
                BienTheId = g.Key,
                SoLuong = g.Sum(c => c.SoLuong),
                ThanhTien = g.Sum(c => c.SoLuong * c.DonGia)
            })
            .OrderByDescending(x => x.SoLuong)
            .Take(10)
            .ToList();

        var btIds = topSkuData.Select(x => x.BienTheId).ToList();
        var btMap = await _db.BienThes.Include(b => b.SanPham)
            .Where(b => btIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id);

        var topSku = topSkuData.Select(t =>
        {
            var bt = btMap.GetValueOrDefault(t.BienTheId);
            return (
                Sku: bt?.MaSku ?? $"ID-{t.BienTheId}",
                TenSp: bt?.Ten ?? "Sản phẩm",
                SoLuong: t.SoLuong,
                ThanhTien: t.ThanhTien
            );
        }).ToList();

        // Danh sách hàng sắp hết
        var sapHet = await _db.BienThes
            .Include(b => b.SanPham)
            .Where(b => b.TonKho <= b.NguongCanhBao)
            .OrderBy(b => b.TonKho)
            .Take(50)
            .ToListAsync();

        // Tính toán các chỉ số KPI chất lượng theo đề bài
        int tongDon = dons.Count;
        int donHuyHoan = dons.Count - hieuLuc.Count;
        decimal tiLeSaiDon = tongDon > 0 ? Math.Round((decimal)donHuyHoan * 100m / tongDon, 2) : 0m;

        // Thống kê tỷ lệ lệch kho từ lịch sử kiểm kê
        int tongSku = await _db.BienThes.CountAsync();
        var kiemKeGiaoDich = await _db.LichSuKhos
            .Where(l => l.Loai == LoaiKho.KiemKe && l.NgayTao >= tu)
            .Select(l => l.BienTheId)
            .Distinct()
            .CountAsync();
        decimal tiLeLechKho = tongSku > 0 ? Math.Round((decimal)kiemKeGiaoDich * 100m / tongSku, 2) : 0m;

        sw.Stop();

        var vm = new BaoCaoVm
        {
            TongDon = tongDon,
            DonHieuLuc = hieuLuc.Count,
            DonHuyHoan = donHuyHoan,
            DoanhThu = hieuLuc.Sum(d => d.TongTien),
            DoanhThuChuaGiao = hieuLuc.Where(d => d.TrangThai is TrangThaiDon.Moi or TrangThaiDon.DaXacNhan or TrangThaiDon.DangDong).Sum(d => d.TongTien),
            TiLeSaiDon = tiLeSaiDon,
            TiLeLechKho = tiLeLechKho,
            ThoiGianChotDoanhThuGiay = sw.Elapsed.TotalSeconds,
            NgayChot = targetChot,
            TheoNgay = theoNgay,
            TheoKenh = theoKenh,
            TopSku = topSku,
            SapHet = sapHet
        };

        ViewBag.Ngay = ngay;
        ViewBag.NgayChot = targetChot;
        ViewBag.DonChotHomNay = donChotHomNay;
        ViewBag.DoanhThuHomNay = hieuLucHomNay.Sum(d => d.TongTien);
        ViewBag.SoDonHomNay = donChotHomNay.Count;
        ViewBag.SoDonThanhCongHomNay = hieuLucHomNay.Count;

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> InBienBanChotCa(DateTime? ngay)
    {
        var targetDate = (ngay ?? DateTime.Today).Date;
        var dons = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe)
            .Where(d => d.NgayTao.Date == targetDate)
            .OrderByDescending(d => d.NgayTao)
            .ToListAsync();

        var hieuLuc = dons.Where(d => d.TrangThai is not (TrangThaiDon.Huy or TrangThaiDon.Hoan)).ToList();

        ViewBag.Ngay = targetDate;
        ViewBag.TongDon = dons.Count;
        ViewBag.DonHieuLuc = hieuLuc.Count;
        ViewBag.DonHuy = dons.Count(d => d.TrangThai == TrangThaiDon.Huy);
        ViewBag.DonHoan = dons.Count(d => d.TrangThai == TrangThaiDon.Hoan);
        ViewBag.DoanhThu = hieuLuc.Sum(d => d.TongTien);
        ViewBag.TienCod = hieuLuc.Sum(d => d.TienThuHo);

        ViewBag.DoanhThuKenh = hieuLuc.GroupBy(d => d.Kenh)
            .Select(g => (Kenh: g.Key, SoDon: g.Count(), DoanhThu: g.Sum(x => x.TongTien)))
            .ToList();

        return View(dons);
    }
}
