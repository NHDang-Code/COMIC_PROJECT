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
    public class LevelMappingController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public LevelMappingController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var levelMappings = await _context.LevelMappings.Include(lm => lm.LevelType).ToListAsync();

            // Nhóm dữ liệu theo LevelTypeId
            var groupedLevelMappings = levelMappings
                .GroupBy(lm => lm.LevelTypeId)
                .ToList();

            return View(groupedLevelMappings);
        }


        [HttpGet]
        public IActionResult Create(int? levelTypeId)
        {
            var levelTypes = _context.LevelTypes.ToList();

            if (!levelTypes.Any())
            {
                TempData["error"] = "Không có loại cấp độ nào để chọn.";
                return RedirectToAction("Index", "LevelType", new { area = "Admin" });
            }

            // Kiểm tra xem levelTypeId có hợp lệ không
            if (levelTypeId == null || !_context.LevelTypes.Any(l => l.LevelTypeId == levelTypeId))
            {
                TempData["error"] = "Loại cấp độ không hợp lệ.";
                return RedirectToAction("Index", "LevelType", new { area = "Admin" });
            }

            var levelMapping = new LevelMappingModel
            {
                LevelTypeId = levelTypeId.Value
            };

            ViewBag.LevelTypes = levelTypes;
            return View(levelMapping);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LevelTypeId, PointsRequired, DisplayName")] LevelMappingModel levelMapping)
        {
            // Kiểm tra nếu LevelTypeId không hợp lệ
            if (!_context.LevelTypes.Any(l => l.LevelTypeId == levelMapping.LevelTypeId))
            {
                ModelState.AddModelError("LevelTypeId", "Loại cấp độ không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(levelMapping);
                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm nhật tên cấp độ thành công!";

                return RedirectToAction("Details", "LevelType", new { id = levelMapping.LevelTypeId });
            }

            // Nếu có lỗi, cần load lại danh sách LevelTypes
            ViewBag.LevelTypes = _context.LevelTypes.ToList();
            return View(levelMapping);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelMapping = await _context.LevelMappings.FindAsync(id);
            if (levelMapping == null)
            {
                return NotFound();
            }

            // Lấy danh sách LevelTypes từ cơ sở dữ liệu và truyền vào ViewBag
            ViewBag.LevelTypes = await _context.LevelTypes.ToListAsync();


            return View(levelMapping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LevelId,LevelTypeId,PointsRequired,DisplayName")] LevelMappingModel levelMapping)
        {
            if (id != levelMapping.LevelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật LevelMapping trong cơ sở dữ liệu
                    _context.Update(levelMapping);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Kiểm tra nếu LevelMapping vẫn tồn tại trong cơ sở dữ liệu
                    if (!LevelMappingModelExists(levelMapping.LevelId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Cập nhật tên cấp độ thành công!";

                return RedirectToAction("Details", "LevelType", new { id = levelMapping.LevelTypeId });

            }

            // Cập nhật lại ViewData nếu ModelState không hợp lệ
            ViewData["LevelTypeId"] = new SelectList(_context.LevelTypes, "LevelTypeId", "TypeName", levelMapping.LevelTypeId);
            return View(levelMapping);
        }

        private bool LevelMappingModelExists(int id)
        {
            return _context.LevelMappings.Any(e => e.LevelId == id);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelMapping = await _context.LevelMappings
                .Include(lm => lm.LevelType)
                .FirstOrDefaultAsync(m => m.LevelId == id);
            if (levelMapping == null)
            {
                return NotFound();
            }

            return View(levelMapping);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var levelMapping = await _context.LevelMappings
                .FirstOrDefaultAsync(lm => lm.LevelId == id);

            if (levelMapping == null)
            {
                return NotFound();
            }

            int? levelTypeId = levelMapping.LevelTypeId; // Lưu LevelTypeId để kiểm tra sau khi xóa

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa LevelMapping trước
                    _context.LevelMappings.Remove(levelMapping);
                    await _context.SaveChangesAsync();

                    // Kiểm tra xem còn LevelMapping nào khác có cùng LevelTypeId không
                    bool hasOtherMappings = await _context.LevelMappings
                        .AnyAsync(lm => lm.LevelTypeId == levelTypeId);

                    if (!hasOtherMappings)
                    {
                        var levelType = await _context.LevelTypes.FindAsync(levelTypeId);
                        if (levelType != null)
                        {
                            _context.LevelTypes.Remove(levelType);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi khi xóa, vui lòng thử lại.");

                    return View(levelMapping);
                }
            }
            TempData["success"] = "Xóa Thành Công";

            return RedirectToAction("Details", "LevelType", new { id = levelMapping.LevelTypeId });
        }
    }
}
