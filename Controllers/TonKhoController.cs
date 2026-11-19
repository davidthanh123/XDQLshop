using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;

namespace ShopQuanLy.Controllers;

[Authorize(Roles = VaiTro.ChuShop + "," + VaiTro.Kho)]
public class TonKhoController : Controller
{
    private readonly AppDbContext _db;
    public TonKhoController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, bool? sapHet, int page = 1)
    {
        var query = _db.BienThes.Include(b => b.SanPham).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var cleanQ = q.Trim();
            query = query.Where(b => b.MaSku.Contains(cleanQ) || b.SanPham.TenSp.Contains(cleanQ));
        }

        if (sapHet == true)
        {
            query = query.Where(b => b.TonKho <= b.NguongCanhBao);
        }

        ViewBag.Q = q;
        ViewBag.SapHet = sapHet;

        ViewBag.TongSku = await _db.BienThes.CountAsync();
        ViewBag.DemSapHet = await _db.BienThes.CountAsync(b => b.TonKho <= b.NguongCanhBao);
        ViewBag.DemHetHang = await _db.BienThes.CountAsync(b => b.TonKho == 0);
        ViewBag.TongSoLuongTon = await _db.BienThes.SumAsync(b => b.TonKho);

        var list = await query.OrderBy(b => b.TonKho).ThenBy(b => b.MaSku).Take(250).ToListAsync();
        return View(list);
    }

    private async Task<List<SelectListItem>> DsSku() =>
        (await _db.BienThes.Include(b => b.SanPham).OrderBy(b => b.MaSku).Take(500).ToListAsync())
        .Select(b => new SelectListItem($"{b.MaSku} - {b.Ten} (Tồn hiện tại: {b.TonKho})", b.Id.ToString()))
        .ToList();

    [HttpGet]
    public async Task<IActionResult> Nhap()
    {
        ViewBag.Sku = await DsSku();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Nhap(int bienTheId, int soLuong, string? ghiChu)
    {
        var bt = await _db.BienThes.FindAsync(bienTheId);
        if (bt == null || soLuong <= 0)
        {
            ViewBag.Loi = "Vui lòng chọn SKU hợp lệ và số lượng nhập > 0.";
            ViewBag.Sku = await DsSku();
            return View();
        }

        bt.TonKho += soLuong;
        _db.LichSuKhos.Add(new LichSuKho
        {
            BienTheId = bt.Id,
            Loai = LoaiKho.Nhap,
            SoLuong = soLuong,
            GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "Nhập bổ sung kho hàng" : ghiChu.Trim(),
            NguoiThucHien = User.Identity?.Name ?? "kho"
        });

        try
        {
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Nhập kho thành công {soLuong} sản phẩm cho SKU {bt.MaSku}. Tồn mới: {bt.TonKho}";
        }
        catch (DbUpdateConcurrencyException)
        {
            ViewBag.Loi = "Dữ liệu tồn kho vừa bị sửa đổi bởi giao dịch khác. Vui lòng tải lại.";
            ViewBag.Sku = await DsSku();
            return View();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> KiemKe()
    {
        ViewBag.Sku = await DsSku();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> KiemKe(int bienTheId, int thucTe, string? ghiChu)
    {
        var bt = await _db.BienThes.FindAsync(bienTheId);
        if (bt == null || thucTe < 0)
        {
            ViewBag.Loi = "Vui lòng chọn SKU và nhập số lượng thực tế kiểm đếm (>= 0).";
            ViewBag.Sku = await DsSku();
            return View();
        }

        var chenhLech = thucTe - bt.TonKho;
        int tonCu = bt.TonKho;
        bt.TonKho = thucTe;

        _db.LichSuKhos.Add(new LichSuKho
        {
            BienTheId = bt.Id,
            Loai = LoaiKho.KiemKe,
            SoLuong = chenhLech,
            GhiChu = $"Kiểm kê kho (Hệ thống: {tonCu}, Thực tế: {thucTe}, Lệch: {(chenhLech >= 0 ? "+" : "")}{chenhLech}). " + (ghiChu ?? ""),
            NguoiThucHien = User.Identity?.Name ?? "kho"
        });

        await _db.SaveChangesAsync();
        TempData["ThongBao"] = $"Đã cập nhật tồn kho SKU {bt.MaSku} theo số kiểm kê thực tế ({thucTe}). Chênh lệch ghi nhận: {(chenhLech >= 0 ? "+" : "")}{chenhLech}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> LichSu(LoaiKho? loai, int? bienTheId)
    {
        var query = _db.LichSuKhos.Include(l => l.BienThe).ThenInclude(b => b.SanPham).AsQueryable();

        if (loai != null) query = query.Where(l => l.Loai == loai);
        if (bienTheId != null) query = query.Where(l => l.BienTheId == bienTheId);

        ViewBag.Loai = loai;
        var list = await query.OrderByDescending(l => l.NgayTao).Take(150).ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var list = await _db.BienThes.Include(b => b.SanPham).OrderBy(b => b.MaSku).ToListAsync();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("MaSku,TenSanPham,Size,MauSac,TonKho,NguongCanhBao,GiaBan,TrangThaiTon");

        foreach (var b in list)
        {
            string tt = b.TonKho == 0 ? "HetHang" : (b.TonKho <= b.NguongCanhBao ? "SapHet" : "BinhThuong");
            sb.AppendLine($"\"{b.MaSku}\",\"{b.SanPham.TenSp}\",\"{b.Size}\",\"{b.MauSac}\",{b.TonKho},{b.NguongCanhBao},{b.SanPham.GiaBan},\"{tt}\"");
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        return File(bytes, "text/csv; charset=utf-8", $"TonKho_{DateTime.Now:yyyyMMdd_HHmm}.csv");
    }
}
