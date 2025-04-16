using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public UserController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 4;  // Number of items per page
            var usersQuery = _context.Users
                                     .Include(u => u.Role)
                                     .AsQueryable();

            // Apply search filter if there's a query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Ensure that the Contains method is applied correctly
                usersQuery = usersQuery.Where(u => EF.Functions.Like(u.UserName, $"%{searchQuery}%") || u.UserId.ToString().Contains(searchQuery));
            }

            // Get the total count of users after applying the search filter
            int totalUsers = await usersQuery.CountAsync();

            // Paginate the users
            var users = await usersQuery
                .OrderByDescending(u => u.UserId)  // Or any other field you'd like to order by
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate total pages
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Create a ViewModel to pass data to the view
            var viewModel = new UserViewModel
            {
                Users = users,
                TotalPages = totalPages,
                CurrentPage = page,
                SearchQuery = searchQuery
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new List<object>());
            }

            var users = await _context.Users
                .Where(u => EF.Functions.Like(u.UserName, $"%{term}%") || u.UserId.ToString().Contains(term))
                .Select(u => new { id = u.UserId, name = u.UserName })
                .Take(10) // Limit to 10 results
                .ToListAsync();

            return Json(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserId, UserName, UserEmail, Password, Coin, Points, RoleId, UserImage, CoverUserImage")] UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (user.RoleId == null || user.RoleId == 0)
                    {
                        ModelState.AddModelError("RoleId", "Vai trò là bắt buộc");

                        ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");

                        return View(user);
                    }

                    //Kiểm tra trùng lặp Email
                    var existingUser = await _context.Users

                        .FirstOrDefaultAsync(u => u.UserEmail == user.UserEmail && u.UserId != user.UserId);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserEmail", "Người dùng với email này đã tồn tại.");

                        ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");

                        return View(user);
                    }

                    //Xử lý ảnh đại diện
                    var files = HttpContext.Request.Form.Files;

                    // Xử lý ảnh đại diện
                    var userImageFile = files.FirstOrDefault(f => f.Name == "UserImage");

                    if (userImageFile != null && userImageFile.Length > 0)
                    {
                        var userImageFileName = Guid.NewGuid() + Path.GetExtension(userImageFile.FileName);

                        var userImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "user_image", userImageFileName);

                        using (var stream = new FileStream(userImagePath, FileMode.Create))
                        {
                            await userImageFile.CopyToAsync(stream);

                            user.UserImage = "media/user_image/" + userImageFileName;
                        }
                    }
                    else
                    {
                        user.UserImage = "media/user_image/logo_dragon.jpg";
                    }

                    // Xử lý ảnh bìa
                    var coverUserImageFile = files.FirstOrDefault(f => f.Name == "CoverUserImage");

                    if (coverUserImageFile != null && coverUserImageFile.Length > 0)
                    {
                        var coverUserImageFileName = Guid.NewGuid() + Path.GetExtension(coverUserImageFile.FileName);

                        var coverUserImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "cover_image", coverUserImageFileName);

                        using (var stream = new FileStream(coverUserImagePath, FileMode.Create))
                        {
                            await coverUserImageFile.CopyToAsync(stream);

                            user.CoverUserImage = "media/cover_image/" + coverUserImageFileName;
                        }
                    }
                    else
                    {
                        user.CoverUserImage = "media/cover_image/cover_image.jpg";
                    }
                    // Gán Coin = 0 nếu không có giá trị
                    if (user.Coin == null)
                    {
                        user.Coin = 0;
                    }

                    user.Points = 1000;

                    user.CreatedAt = DateTime.Now;

                    // Lưu user vào database
                    _context.Add(user);

                    await _context.SaveChangesAsync();

                    TempData["success"] = "Thêm người dùng thành công!";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Đã có lỗi xảy ra: " + ex.Message;
                }

            }
            else
            {
                // Tạo chuỗi lỗi chi tiết từ ModelState nếu có
                string errorMessage = "Có trường thông tin không hợp lệ. Vui lòng kiểm tra lại:\n";

                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        errorMessage += $"- {error.ErrorMessage}\n";
                    }
                }
                TempData["error"] = errorMessage;
            }

            // Nếu có lỗi, truyền lại danh sách vai trò vào View
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Truyền danh sách vai trò vào ViewBag
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserEmail,UserName,Password,Points,Coin,RoleId,UserImage,CoverUserImage,CreatedAt")] UserModel user)
        {
            if (id != user.UserId)
            {
                TempData["error"] = $"ID không khớp: {id} != {user.UserId}.";

                return RedirectToAction(nameof(Index)); // Quay lại danh sách người dùng
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra trùng lặp theo UserEmail
                    var existingUser = await _context.Users

                        .FirstOrDefaultAsync(u => u.UserEmail == user.UserEmail && u.UserId != user.UserId);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserEmail", "Người dùng với email này đã tồn tại.");

                        ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);

                        return View(user);
                    }

                    var currentUser = await _context.Users.FindAsync(id);

                    if (currentUser == null)
                    {
                        TempData["error"] = "Người dùng không tồn tại.";

                        return RedirectToAction(nameof(Index));
                    }

                    // Cập nhật các trường như Coin và RoleId
                    currentUser.Coin = user.Coin;

                    currentUser.UserName = user.UserName;

                    currentUser.Password = user.Password;

                    currentUser.RoleId = user.RoleId;

                    currentUser.Points = user.Points;


                    // Xử lý ảnh đại diện (UserImage)
                    var files = HttpContext.Request.Form.Files;

                    var userImageFile = files.FirstOrDefault(f => f.Name == "UserImage");

                    if (userImageFile != null && userImageFile.Length > 0)
                    {
                        var userImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(userImageFile.FileName);

                        var userImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "user_image", userImageFileName);

                        using (var stream = new FileStream(userImagePath, FileMode.Create))
                        {
                            await userImageFile.CopyToAsync(stream);

                            currentUser.UserImage = "media/user_image/" + userImageFileName;
                        }
                    }

                    // Xử lý ảnh bìa (CoverUserImage)
                    var coverUserImageFile = files.FirstOrDefault(f => f.Name == "CoverUserImage");

                    if (coverUserImageFile != null && coverUserImageFile.Length > 0)
                    {
                        var coverUserImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(coverUserImageFile.FileName);

                        var coverUserImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "cover_image", coverUserImageFileName);

                        using (var stream = new FileStream(coverUserImagePath, FileMode.Create))
                        {
                            await coverUserImageFile.CopyToAsync(stream);

                            currentUser.CoverUserImage = "media/cover_image/" + coverUserImageFileName;
                        }
                    }


                    // Cập nhật thông tin người dùng vào database
                    _context.Update(currentUser);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Đã có lỗi xảy ra: " + ex.Message;
                }
            }

            // Nếu có lỗi, truyền lại danh sách vai trò vào View
            ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        private bool UserModelExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Xóa ảnh đại diện nếu không phải ảnh mặc định
            if (!string.IsNullOrEmpty(user.UserImage) && !user.UserImage.EndsWith("logo_dragon.jpg"))
            {
                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.UserImage.Replace("/", "\\"));
                if (System.IO.File.Exists(avatarPath))
                {
                    System.IO.File.Delete(avatarPath);
                }
            }

            // Xóa ảnh bìa nếu có
            if (!string.IsNullOrEmpty(user.CoverUserImage) && !user.CoverUserImage.EndsWith("cover_image.jpg"))
            {
                var coverPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.CoverUserImage.Replace("/", "\\"));
                if (System.IO.File.Exists(coverPath))
                {
                    System.IO.File.Delete(coverPath);
                }
            }
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
