using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopQuanLy.Models;

namespace ShopQuanLy.Controllers;

[Authorize]
public class DuAnController : Controller
{
    public IActionResult Index()
    {
        var vm = new DuAnVm();
        return View(vm);
    }
}
