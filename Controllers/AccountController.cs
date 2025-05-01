using K21CNT2_NguyenHaiDang_2110900067_DATN.Constants;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class AccountController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public AccountController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool emailExists = await _context.Users.AnyAsync(u => u.UserEmail == model.UserEmail);
            if (emailExists)
            {
                TempData["error"] = "Email đã được sử dụng.";
                return View(model);
            }

            var user = new UserModel
            {
                UserName = model.UserName,
                UserEmail = model.UserEmail,
                Password = model.Password,
                RoleId = RoleConstants.User,
                IsLocked = false,
                Coin = 0,
                Points = 1000
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserEmail.ToLower() == model.Email.ToLower() && u.Password == model.Password);

            if (user == null || user.IsLocked)
            {
                TempData["error"] = "Email hoặc mật khẩu không chính xác.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                 new Claim("UserImage", user.UserImage ?? "media/user_image/logo_dragon.jpg"),
                new Claim("CurrentFrameId", user.CurrentFrameId?.ToString() ?? "0")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "UserScheme");
            

            await HttpContext.SignInAsync("UserScheme", new ClaimsPrincipal(claimsIdentity));


            TempData["success"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");
            TempData["success"] = "Đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

    }
}
