using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class LevelTypeController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public LevelTypeController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 4;

            var query = _context.LevelTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.TypeName.Contains(search));
            }

            int totalItem = await query.CountAsync();
            var levelTypes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewmodel = new LevelTypeViewModel
            {
                LevelTypes = levelTypes,
                Pagination = new Paginate(totalItem, page, pageSize),
                SearchTerm = search,
            };

            return View(viewmodel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id, string search, int page = 1, int pageSize = 4)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelType = await _context.LevelTypes
                .FirstOrDefaultAsync(m => m.LevelTypeId == id);

            if (levelType == null)
            {
                return NotFound();
            }

            // Lọc LevelMappings theo LevelTypeId trước
            var levelMappingsQuery = _context.LevelMappings
                                      .Where(lm => lm.LevelTypeId == id);

            // Áp dụng tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                levelMappingsQuery = levelMappingsQuery.Where(lm => lm.DisplayName.Contains(search));
            }

            // Đếm tổng số kết quả sau khi lọc
            int totalItems = await levelMappingsQuery.CountAsync();

            // Tạo đối tượng Paginate để tính toán trang hợp lệ
            var paginate = new Paginate(totalItems, page, pageSize);

            // Lấy danh sách LevelMappings theo phân trang
            var levelMappings = await levelMappingsQuery
                .OrderBy(lm => lm.LevelId)  // Đảm bảo có thứ tự rõ ràng
                .Skip((paginate.CurrentPage - 1) * paginate.PageSize)
                .Take(paginate.PageSize)
                .ToListAsync();

            // Trả về ViewModel chứa dữ liệu
            var viewModel = new LevelTypeDetailsViewModel
            {
                LevelTypeId = levelType.LevelTypeId,
                TypeName = levelType.TypeName,
                LevelMappings = levelMappings,
                SearchTerm = search,
                Pagination = paginate
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("TypeName")] LevelTypeModel levelType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(levelType);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            return View(levelType);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelType = await _context.LevelTypes.FindAsync(id);

            if (levelType == null)
            {
                return NotFound();
            }

            return View(levelType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("LevelTypeId, TypeName")] LevelTypeModel levelType)
        {
            if (id != levelType.LevelTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(levelType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LevelTypeModelExists(levelType.LevelTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Sửa thành công";
                return RedirectToAction("Index");
            }
            return View(levelType);

        }

        private bool LevelTypeModelExists(int id)
        {
            return _context.LevelTypes.Any(e => e.LevelTypeId == id);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelType = await _context.LevelTypes
                .FirstOrDefaultAsync(m => m.LevelTypeId == id);
            if (levelType == null)
            {
                return NotFound();
            }

            return View(levelType);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var levelType = await _context.LevelTypes.FindAsync(id);

            _context.LevelTypes.Remove(levelType);

            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa Thành Công";

            return RedirectToAction(nameof(Index));
        }
    }
}
