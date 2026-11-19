using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;

namespace ShopQuanLy.Services;

/// <summary>
/// Nghiệp vụ lõi: Đặt hàng (trừ kho ngay trong 1 Transaction, chặn bán lố, kiểm soát đồng thời RowVersion),
/// đổi trạng thái theo vòng đời và tự động hoàn kho khi Hủy/Hoàn.
/// Đáp ứng KPI: Giảm sai đơn < 2% và Cập nhật tồn kho tức thì < 5 phút.
/// </summary>
public class DonHangService
{
    private readonly AppDbContext _db;
    public DonHangService(AppDbContext db) => _db = db;

    public async Task<(DonHang? Don, string? Loi)> DatHangAsync(
        KenhBan kenh, string hoTen, string sdt, string diaChi,
        IEnumerable<DongDat> dong, string nguoi,
        string? ghiChu = null, string? donViVanChuyen = null, string? maVanDon = null)
    {
        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(sdt))
            return (null, "Thiếu họ tên hoặc số điện thoại khách hàng.");

        var items = dong.Where(d => d.BienTheId > 0 && d.SoLuong > 0)
            .GroupBy(d => d.BienTheId)
            .Select(g => new DongDat { BienTheId = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
            .ToList();

        if (items.Count == 0) return (null, "Đơn hàng phải có ít nhất một sản phẩm hợp lệ.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var ids = items.Select(i => i.BienTheId).ToList();
            var bts = await _db.BienThes.Include(b => b.SanPham)
                .Where(b => ids.Contains(b.Id)).ToListAsync();

            var khach = await _db.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == sdt.Trim());
            if (khach == null)
            {
                khach = new KhachHang { HoTen = hoTen.Trim(), SoDienThoai = sdt.Trim(), DiaChi = diaChi?.Trim() ?? "" };
                _db.KhachHangs.Add(khach);
            }
            else if (!string.IsNullOrWhiteSpace(diaChi))
            {
                khach.DiaChi = diaChi.Trim();
            }

            var don = new DonHang
            {
                Kenh = kenh,
                KhachHang = khach,
                NguoiTao = nguoi,
                NgayTao = DateTime.Now,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "Cho xem hàng, không thử" : ghiChu.Trim(),
                DonViVanChuyen = string.IsNullOrWhiteSpace(donViVanChuyen) ? "Giao Hàng Tiết Kiệm (GHTK)" : donViVanChuyen.Trim()
            };

            foreach (var it in items)
            {
                var bt = bts.FirstOrDefault(b => b.Id == it.BienTheId);
                if (bt == null) return (null, "Có mã sản phẩm không tồn tại trong hệ thống.");

                // CHẶN BÁN LỐ / OVERSOLD
                if (bt.TonKho < it.SoLuong)
                    return (null, $"SKU {bt.MaSku} hiện chỉ còn {bt.TonKho} sản phẩm, không đủ số lượng yêu cầu ({it.SoLuong}). Hệ thống đã chặn bán lố!");

                // TRỪ KHO NGAY LẬP TỨC
                bt.TonKho -= it.SoLuong;
                don.ChiTiet.Add(new ChiTietDonHang
                {
                    BienThe = bt,
                    SoLuong = it.SoLuong,
                    DonGia = bt.SanPham.GiaBan
                });

                // LƯU LỊCH SỬ BIẾN ĐỘNG KHO
                _db.LichSuKhos.Add(new LichSuKho
                {
                    BienThe = bt,
                    Loai = LoaiKho.Xuat,
                    SoLuong = it.SoLuong,
                    GhiChu = $"Xuất kho bán hàng qua {kenh}",
                    NguoiThucHien = nguoi
                });
            }

            don.TienThuHo = don.TongTien; // Mặc định thu hộ COD toàn bộ giá trị đơn
            _db.DonHangs.Add(don);
            await _db.SaveChangesAsync();

            don.MaDon = $"DH{don.Id:D6}";
            don.MaVanDon = string.IsNullOrWhiteSpace(maVanDon) ? $"VN{DateTime.Now:yyMM}{don.Id:D6}" : maVanDon.Trim();
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return (don, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (null, "Xung đột đồng thời: Tồn kho của một số sản phẩm vừa bị thay đổi bởi giao dịch khác. Vui lòng tải lại và thao tác lại.");
        }
    }

    public async Task<string?> DoiTrangThaiAsync(int id, TrangThaiDon moi, string nguoi)
    {
        var don = await _db.DonHangs.Include(d => d.ChiTiet).ThenInclude(c => c.BienThe)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (don == null) return "Không tìm thấy đơn hàng.";
        if (don.TrangThai is TrangThaiDon.Huy or TrangThaiDon.Hoan)
            return "Đơn hàng đã đóng ở trạng thái Hủy hoặc Hoàn, không thể chuyển đổi tiếp.";

        bool hopLe = moi switch
        {
            TrangThaiDon.DaXacNhan => don.TrangThai == TrangThaiDon.Moi,
            TrangThaiDon.DangDong => don.TrangThai == TrangThaiDon.DaXacNhan,
            TrangThaiDon.DaGiao => don.TrangThai == TrangThaiDon.DangDong,
            TrangThaiDon.Huy => don.TrangThai is TrangThaiDon.Moi or TrangThaiDon.DaXacNhan or TrangThaiDon.DangDong,
            TrangThaiDon.Hoan => don.TrangThai == TrangThaiDon.DaGiao,
            _ => false
        };

        if (!hopLe)
            return $"Không thể chuyển trạng thái từ '{don.TrangThai}' sang '{moi}'. Vui lòng tuân thủ đúng quy trình vận hành.";

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // TỰ ĐỘNG CỘNG LẠI KHO KHI HỦY HOẶC KHÁCH HOÀN HÀNG
            if (moi is TrangThaiDon.Huy or TrangThaiDon.Hoan)
            {
                foreach (var c in don.ChiTiet)
                {
                    c.BienThe.TonKho += c.SoLuong;
                    _db.LichSuKhos.Add(new LichSuKho
                    {
                        BienTheId = c.BienTheId,
                        Loai = LoaiKho.Hoan,
                        SoLuong = c.SoLuong,
                        GhiChu = $"Hoàn trả kho do {moi} đơn hàng {don.MaDon}",
                        NguoiThucHien = nguoi
                    });
                }
            }

            don.TrangThai = moi;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Dữ liệu tồn kho vừa bị thay đổi đồng thời. Vui lòng thử lại.";
        }
    }
}
