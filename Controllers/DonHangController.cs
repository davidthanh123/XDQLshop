using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;
using ShopQuanLy.Services;

namespace ShopQuanLy.Controllers;

[Authorize]
public class DonHangController : Controller
{
    private readonly AppDbContext _db;
    private readonly DonHangService _svc;

    public DonHangController(AppDbContext db, DonHangService svc)
    {
        _db = db;
        _svc = svc;
    }

    public async Task<IActionResult> Index(TrangThaiDon? tt, KenhBan? kenh, string? q, int page = 1)
    {
        var query = _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe)
            .AsQueryable();

        if (tt != null) query = query.Where(d => d.TrangThai == tt);
        if (kenh != null) query = query.Where(d => d.Kenh == kenh);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var cleanQ = q.Trim();
            query = query.Where(d => d.MaDon.Contains(cleanQ) ||
                                     d.MaVanDon!.Contains(cleanQ) ||
                                     d.KhachHang.HoTen.Contains(cleanQ) ||
                                     d.KhachHang.SoDienThoai.Contains(cleanQ));
        }

        ViewBag.Tt = tt;
        ViewBag.Kenh = kenh;
        ViewBag.Q = q;

        // Thống kê nhanh trạng thái
        ViewBag.DemMoi = await _db.DonHangs.CountAsync(d => d.TrangThai == TrangThaiDon.Moi);
        ViewBag.DemDaXacNhan = await _db.DonHangs.CountAsync(d => d.TrangThai == TrangThaiDon.DaXacNhan);
        ViewBag.DemDangDong = await _db.DonHangs.CountAsync(d => d.TrangThai == TrangThaiDon.DangDong);
        ViewBag.DemDaGiao = await _db.DonHangs.CountAsync(d => d.TrangThai == TrangThaiDon.DaGiao);

        var list = await query.OrderByDescending(d => d.NgayTao).Take(250).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var don = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe).ThenInclude(b => b.SanPham)
            .FirstOrDefaultAsync(d => d.Id == id);

        return don == null ? NotFound() : View(don);
    }

    private async Task<List<SelectListItem>> DsSku() =>
        (await _db.BienThes.Include(b => b.SanPham)
            .Where(b => b.TonKho > 0)
            .OrderBy(b => b.MaSku)
            .Take(300)
            .ToListAsync())
        .Select(b => new SelectListItem($"{b.MaSku} - {b.Ten} (còn {b.TonKho} | {b.SanPham.GiaBan:N0}đ)", b.Id.ToString()))
        .ToList();

    [Authorize(Roles = VaiTro.ChuShop + "," + VaiTro.ChotDon)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Sku = await DsSku();
        return View(new DonHangCreateVm());
    }

    [Authorize(Roles = VaiTro.ChuShop + "," + VaiTro.ChotDon)]
    [HttpPost]
    public async Task<IActionResult> Create(DonHangCreateVm vm)
    {
        var (don, loi) = await _svc.DatHangAsync(
            vm.Kenh, vm.HoTen, vm.SoDienThoai, vm.DiaChi,
            vm.Dong, User.Identity?.Name ?? "",
            vm.GhiChu, vm.DonViVanChuyen);

        if (don == null)
        {
            ViewBag.Loi = loi;
            ViewBag.Sku = await DsSku();
            return View(vm);
        }

        TempData["ThongBao"] = $"Tạo đơn {don.MaDon} thành công. Tồn kho đã được trừ ngay lập tức!";
        return RedirectToAction(nameof(Details), new { id = don.Id });
    }

    [HttpPost]
    public async Task<IActionResult> DoiTrangThai(int id, TrangThaiDon moi)
    {
        // Phân quyền theo quy trình:
        // - Chủ shop: toàn quyền
        // - Chốt đơn: xác nhận, hủy
        // - Kho: đóng gói, đã giao, khách hoàn
        bool duocPhep = User.IsInRole(VaiTro.ChuShop)
            || (User.IsInRole(VaiTro.Kho) && moi is TrangThaiDon.DangDong or TrangThaiDon.DaGiao or TrangThaiDon.Hoan)
            || (User.IsInRole(VaiTro.ChotDon) && moi is TrangThaiDon.DaXacNhan or TrangThaiDon.Huy);

        if (!duocPhep) return Forbid();

        var loi = await _svc.DoiTrangThaiAsync(id, moi, User.Identity?.Name ?? "");
        if (loi != null) TempData["Loi"] = loi;
        else TempData["ThongBao"] = $"Cập nhật trạng thái đơn sang '{moi}' thành công.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // ==========================================
    // CR-01: TÍNH NĂNG IN VẬN ĐƠN / PHIẾU GIAO HÀNG (DẠNG PDF / IN ẤN)
    // Phát sinh tuần 2, xử lý theo phiếu yêu cầu thay đổi (CR)
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> InVanDon(int id)
    {
        var don = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe).ThenInclude(b => b.SanPham)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (don == null) return NotFound();

        ViewBag.BarcodeMaDonSvg = BarcodeHelper.GenerateSvg(don.MaDon, 42, 2);
        ViewBag.BarcodeVanDonSvg = BarcodeHelper.GenerateSvg(don.MaVanDon ?? don.MaDon, 42, 2);

        return View(don);
    }

    [HttpGet]
    public async Task<IActionResult> InVanDonHangLoat(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            TempData["Loi"] = "Chưa chọn đơn hàng nào để in vận đơn.";
            return RedirectToAction(nameof(Index));
        }

        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
            .Where(n => n > 0).ToList();

        var dons = await _db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTiet).ThenInclude(c => c.BienThe).ThenInclude(b => b.SanPham)
            .Where(d => idList.Contains(d.Id))
            .OrderByDescending(d => d.Id)
            .ToListAsync();

        return View(dons);
    }

    // ==========================================
    // NHẬP ĐƠN HÀNG HÀNG LOẠT TỪ CSV (TIKTOK / FB / ZALO)
    // ==========================================
    [Authorize(Roles = VaiTro.ChuShop + "," + VaiTro.ChotDon)]
    [HttpGet]
    public IActionResult ImportCsv() => View();

    [HttpGet]
    public IActionResult DownloadCsvTemplate()
    {
        var csv = "MaSku,HoTen,SoDienThoai,DiaChi,SoLuong,Kenh,GhiChu,DonViVanChuyen\n" +
                  "AT01-M-DEN,Nguyễn Thu Trang,0912345678,Số 15 ngõ 68 Cầu Giấy Hà Nội,2,TikTokShop,Cho xem hàng không thử,Giao Hàng Tiết Kiệm (GHTK)\n" +
                  "SM01-L-TRANG,Trần Minh Quân,0987654321,245 Lê Văn Sỹ Quận 3 TP HCM,1,Facebook,Giao giờ hành chính,Giao Hàng Nhanh (GHN)\n" +
                  "QJ01-29-XANHNHAT,Lê Phương Linh,0905123456,78 Hùng Vương Hải Châu Đà Nẵng,1,Zalo,Gọi trước khi giao,Viettel Post\n" +
                  "VH01-M-HOAVANG,Phạm Hoàng Yến,0978999888,12 Đinh Tiên Hoàng Hồng Bàng Hải Phòng,1,TikTokShop,Hàng dễ vỡ,J&T Express\n";

        var bytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();

        return File(bytes, "text/csv; charset=utf-8", "Mau_Import_Don_Hang.csv");
    }

    [Authorize(Roles = VaiTro.ChuShop + "," + VaiTro.ChotDon)]
    [HttpPost]
    public async Task<IActionResult> ImportCsv(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Loi = "Vui lòng chọn một file CSV hợp lệ.";
            return View();
        }

        var thanhCong = 0;
        var loiList = new List<string>();

        using var sr = new StreamReader(file.OpenReadStream());
        var dong = 0;
        while (await sr.ReadLineAsync() is { } line)
        {
            dong++;
            if (dong == 1 && line.StartsWith("MaSku", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split(',', StringSplitOptions.TrimEntries);
            if (p.Length < 6 || !int.TryParse(p[4], out var sl) || !Enum.TryParse<KenhBan>(p[5], true, out var kenh))
            {
                loiList.Add($"Dòng {dong}: Sai định dạng dữ liệu (Yêu cầu: MaSku,HoTen,SoDienThoai,DiaChi,SoLuong,Kenh).");
                continue;
            }

            var skuInput = p[0].ToUpper();
            var bt = await _db.BienThes.FirstOrDefaultAsync(b => b.MaSku == skuInput);
            if (bt == null)
            {
                loiList.Add($"Dòng {dong}: Mã SKU '{p[0]}' không tồn tại trong danh mục.");
                continue;
            }

            string ghiChu = p.Length >= 7 ? p[6] : "Cho xem hàng, không thử";
            string dvvc = p.Length >= 8 ? p[7] : "Giao Hàng Tiết Kiệm (GHTK)";

            var (don, loi) = await _svc.DatHangAsync(
                kenh, p[1], p[2], p[3],
                new[] { new DongDat { BienTheId = bt.Id, SoLuong = sl } },
                User.Identity?.Name ?? "import_csv",
                ghiChu, dvvc);

            if (don == null)
            {
                loiList.Add($"Dòng {dong} (SKU {p[0]}): {loi}");
            }
            else
            {
                thanhCong++;
            }
        }

        ViewBag.ThanhCong = thanhCong;
        ViewBag.LoiList = loiList;
        return View();
    }
}
