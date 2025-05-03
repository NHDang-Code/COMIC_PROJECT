using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class TranslationTeamController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public TranslationTeamController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 6)
        {

            var query = _context.TranslationTeams
                        .Include(t => t.Members)
                        .AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.TeamName.Contains(search));
            }


            int totalItems = await query.CountAsync();

            // Tính toán phân trang
            var paginate = new Paginate(totalItems, page, pageSize);

            var teams = await query
                .OrderByDescending(t => t.TeamId)
                .Skip((paginate.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new TranslationTeamViewModel
            {
                TranslationTeams = teams,
                Pagination = paginate,
                SearchTerm = search
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.TranslationTeams
                .Include(t => t.Comics)
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }

            // Lấy danh sách user chưa nằm trong nhóm
            var availableUsers = await _context.Users
                .Where(u => !_context.TeamMembers.Any(m => m.TeamId == id && m.UserId == u.UserId))
                .ToListAsync();

            ViewBag.AvailableUsers = new SelectList(availableUsers, "UserId", "UserName");

            return View(team);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TranslationTeamModel team)
        {
            var files = HttpContext.Request.Form.Files;

            // Xử lý ảnh đại diện
            var avatarFile = files.FirstOrDefault(f => f.Name == "TeamAvatar");

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var avatarFileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);

                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "team_image", avatarFileName);

                using (var stream = new FileStream(avatarPath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);

                    team.TeamAvatar = "media/team_image/" + avatarFileName;
                }
            }
            else
            {
                team.TeamAvatar = "media/team_image/logo_dragon.jpg";
            }

            if (ModelState.IsValid)
            {
                team.TotalEarnings = 0;

                team.CreatedAt = DateTime.Now;

                _context.Add(team);

                await _context.SaveChangesAsync();

                TempData["success"] = "Tạo nhóm dịch thành công!";

                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.";

            return View(team);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.TranslationTeams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TranslationTeamModel updatedTeam)
        {
            var existingTeam = await _context.TranslationTeams.FindAsync(id);

            if (existingTeam == null)
            {
                return NotFound();
            }

            // Lấy file mới (nếu có)
            var file = HttpContext.Request.Form.Files.FirstOrDefault(f => f.Name == "TeamAvatar");

            if (file != null && file.Length > 0)
            {
                // Tạo tên file ngẫu nhiên để tránh trùng
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "team_image", fileName);

                // Lưu file mới
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Gán đường dẫn mới
                existingTeam.TeamAvatar = Path.Combine("media/team_image", fileName).Replace("\\", "/");
            }

            // Cập nhật các trường khác
            if (ModelState.IsValid)
            {
                existingTeam.TeamName = updatedTeam.TeamName;

                existingTeam.Description = updatedTeam.Description;
                // Không cập nhật TotalEarnings và CreatedAt nếu không cho phép chỉnh

                _context.TranslationTeams.Update(existingTeam);

                await _context.SaveChangesAsync();

                TempData["success"] = "Cập nhật nhóm dịch thành công!";

                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Có lỗi xảy ra khi cập nhật nhóm dịch.";

            return View(updatedTeam);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.TranslationTeams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.TranslationTeams.FindAsync(id);

            if (team == null)
            {
                TempData["error"] = "Không tìm thấy nhóm dịch.";

                return RedirectToAction(nameof(Index));
            }

            // Xóa ảnh nếu không phải ảnh mặc định
            if (!string.IsNullOrEmpty(team.TeamAvatar) && !team.TeamAvatar.EndsWith("logo_dragon.jpg"))
            {
                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", team.TeamAvatar.Replace("/", "\\"));
                if (System.IO.File.Exists(avatarPath))
                {
                    System.IO.File.Delete(avatarPath);
                }
            }

            _context.TranslationTeams.Remove(team);

            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa nhóm dịch thành công!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddMember(int teamId)
        {
            // Lọc ra những người dùng chưa có trong nhóm hiện tại và chưa tham gia nhóm khác
            var usersNotInTeam = await _context.Users
               .Where(u => !_context.TeamMembers.Any(m => m.UserId == u.UserId) // Người dùng chưa tham gia bất kỳ nhóm nào
                           && !_context.TeamMembers.Any(m => m.UserId == u.UserId && m.TeamId == teamId))  // Người dùng chưa có trong nhóm hiện tại
               .ToListAsync();


            ViewBag.UserList = new SelectList(usersNotInTeam, "UserId", "UserName");

            // Truyền teamId vào View
            ViewBag.TeamId = teamId;

            return View(new TeamMemberModel { TeamId = teamId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(TeamMemberModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng đã là thành viên của nhóm chưa
                var exists = await _context.TeamMembers
                    .AnyAsync(m => m.UserId == model.UserId && m.TeamId == model.TeamId);

                if (exists)
                {
                    TempData["error"] = "Người dùng này đã là thành viên của nhóm.";
                    return RedirectToAction("Details", new { id = model.TeamId });
                }

                // Nếu chọn làm trưởng nhóm, kiểm tra xem đã có trưởng nhóm chưa
                if (model.IsLeader)
                {
                    var hasLeader = await _context.TeamMembers
                        .AnyAsync(m => m.TeamId == model.TeamId && m.IsLeader);

                    if (hasLeader)
                    {
                        TempData["error"] = "Nhóm đã có trưởng nhóm rồi. Không thể thêm thành viên mới làm trưởng nhóm.";
                        return RedirectToAction("Details", new { id = model.TeamId });
                    }
                }



                model.JoinedAt = DateTime.Now;

                _context.TeamMembers.Add(model);

                // Cập nhật RoleId của user thành 2 nếu không phải admin
                var user = await _context.Users.FindAsync(model.UserId);
                if (user != null && user.RoleId != 1)
                {
                    user.RoleId = 2;
                    _context.Users.Update(user);
                }

                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm thành viên vào nhóm thành công!";
                return RedirectToAction("Details", new { id = model.TeamId });
            }

            TempData["error"] = "Có lỗi xảy ra. Vui lòng thử lại.";

            // Load lại danh sách người dùng để hiển thị
            var usersNotInTeam = await _context.Users
                .Where(u => !_context.TeamMembers.Any(m => m.UserId == u.UserId && m.TeamId == model.TeamId))
                .ToListAsync();

            ViewBag.UserList = new SelectList(usersNotInTeam, "UserId", "UserName");
            ViewBag.TeamId = model.TeamId;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int teamId, int userId)
        {
            var member = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);

            if (member == null)
            {
                return NotFound();
            }

            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã xóa thành viên khỏi nhóm dịch!";
            return RedirectToAction("Details", new { id = teamId });
        }








    }
}
