using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Constants;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public AccountController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;

        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["error"] = "Bạn không có quyền truy cập vào khu vực này!";

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserEmail == model.Email && u.Password == model.Password);

            if (user == null || user.IsLocked)
            {
                TempData["error"] = "Đăng nhập không thành công.";
                return View(model);
            }

            if (user.RoleId == RoleConstants.Admin || user.RoleId == RoleConstants.Translator)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.UserEmail),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("UserImage", user.UserImage ?? "media/user_image/logo_dragon.jpg")
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AdminScheme");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync("AdminScheme", new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["success"] = "Đăng nhập thành công!";

                // Điều hướng người dùng theo vai trò
                if (user.RoleId == RoleConstants.Admin)
                {
                    // Admin vào Dashboard/Index
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.RoleId == RoleConstants.Translator)
                {
                    // Translator vào Comic/Index
                    return RedirectToAction("Index", "Profile", new { area = "Admin" });
                }
            }

            TempData["error"] = "Bạn không có quyền truy cập vào khu vực này.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất người dùng khỏi hệ thống
            await HttpContext.SignOutAsync("AdminScheme");

            // Chuyển hướng đến trang đăng nhập hoặc trang chính sau khi đăng xuất
            TempData["success"] = "Bạn đã đăng xuất thành công!";
            return RedirectToAction("Login", "Account", new { area = "Admin" }); // Hoặc trang chính của bạn
        }

    }
}
