using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;

namespace ShopQuanLy.Controllers;

[Authorize(Roles = VaiTro.ChuShop)]
public class SanPhamController : Controller
{
    private readonly AppDbContext _db;
    public SanPhamController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.SanPhams.Include(s => s.BienThes).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var cleanQ = q.Trim();
            query = query.Where(s => s.MaSp.Contains(cleanQ) || s.TenSp.Contains(cleanQ));
        }

        ViewBag.Q = q;
        ViewBag.TongSanPham = await _db.SanPhams.CountAsync();
        ViewBag.TongSku = await _db.BienThes.CountAsync();

        return View(await query.OrderBy(s => s.MaSp).Take(200).ToListAsync());
    }

    [HttpGet]
    public IActionResult Create() => View(new SanPham());

    // bienThe: mỗi dòng "Size,Màu,SốLượngBanĐầu" ví dụ: M,Den,20
    [HttpPost]
    public async Task<IActionResult> Create(SanPham sp, string? bienThe)
    {
        if (await _db.SanPhams.AnyAsync(s => s.MaSp == sp.MaSp))
            ModelState.AddModelError(nameof(sp.MaSp), "Mã sản phẩm đã tồn tại trong hệ thống.");

        foreach (var dong in (bienThe ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var p = dong.Split(',', StringSplitOptions.TrimEntries);
            if (p.Length < 3 || !int.TryParse(p[2], out var sl) || sl < 0)
            {
                ModelState.AddModelError("", $"Dòng biến thể không hợp lệ: \"{dong}\" (Định dạng chuẩn: Size,Màu,SốLượng).");
                continue;
            }
            sp.BienThes.Add(new BienThe
            {
                Size = p[0],
                MauSac = p[1],
                TonKho = sl,
                NguongCanhBao = 5,
                MaSku = $"{sp.MaSp}-{p[0]}-{p[1]}".ToUpper(),
                MaVach = $"893{sp.MaSp.GetHashCode() % 100000:D5}{sp.BienThes.Count:D4}"
            });
        }

        if (!ModelState.IsValid) return View(sp);

        if (sp.GiaVon <= 0) sp.GiaVon = sp.GiaBan * 0.6m;

        _db.SanPhams.Add(sp);
        await _db.SaveChangesAsync();

        foreach (var bt in sp.BienThes.Where(b => b.TonKho > 0))
        {
            _db.LichSuKhos.Add(new LichSuKho
            {
                BienTheId = bt.Id,
                Loai = LoaiKho.Nhap,
                SoLuong = bt.TonKho,
                GhiChu = "Tồn đầu kỳ khi khởi tạo sản phẩm mới",
                NguoiThucHien = User.Identity?.Name ?? "chushop"
            });
        }
        await _db.SaveChangesAsync();

        TempData["ThongBao"] = $"Thêm mới sản phẩm {sp.TenSp} ({sp.MaSp}) thành công với {sp.BienThes.Count} mã SKU biến thể.";
        return RedirectToAction(nameof(Index));
    }
}
