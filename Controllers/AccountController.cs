using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopQuanLy.Data;
using ShopQuanLy.Models;

namespace ShopQuanLy.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string tenDangNhap, string matKhau)
    {
        var tk = await _db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
        if (tk != null)
        {
            var kq = new PasswordHasher<TaiKhoan>().VerifyHashedPassword(tk, tk.MatKhauHash, matKhau ?? "");
            if (kq != PasswordVerificationResult.Failed)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, tk.HoTen),
                    new("user", tk.TenDangNhap),
                    new(ClaimTypes.Role, tk.VaiTro)
                };
                var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(new ClaimsPrincipal(id));
                return RedirectToAction("Index", "Home");
            }
        }
        ViewBag.Loi = "Sai tên đăng nhập hoặc mật khẩu.";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Denied() => View();
}
