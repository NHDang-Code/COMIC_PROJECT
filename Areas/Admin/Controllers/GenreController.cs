using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class GenreController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public GenreController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;  // Khởi tạo _context để sử dụng
        }

        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 6)
        {
            var query = _context.Genres.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.GenreName.Contains(search));
            }
            int totalItems = await query.CountAsync(); // Đếm tổng số thể loại sau khi lọc

            // Tính toán phân trang
            var paginate = new Paginate(totalItems, page, pageSize);

            // Lấy danh sách thể loại trong phạm vi phân trang
            var genres = await query
                .OrderByDescending(g => g.GenreId)
                .Skip((paginate.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo ViewModel chứa danh sách thể loại và thông tin phân trang
            var viewModel = new GenresViewModel
            {
                Genres = genres,
                Pagination = paginate,
                SearchTerm = search
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GenreId,GenreName,Slug")] GenreModel genre)
        {
            // Kiểm tra xem thể loại đã tồn tại chưa (so sánh không phân biệt chữ hoa chữ thường)
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.GenreName.ToLower() == genre.GenreName.ToLower());

            if (existingGenre != null)
            {
                // Thêm thông báo lỗi nếu thể loại đã tồn tại
                TempData["error"] = $"Thể loại \"{genre.GenreName}\" đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(Create));  // Quay lại trang Create để người dùng sửa
            }
            // Tạo slug từ GenreName
            genre.Slug = GenerateSlug(genre.GenreName);

            if (ModelState.IsValid)
            {
                _context.Add(genre);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm Thể Loại Thành Công";
                return RedirectToAction(nameof(Index));  // Chuyển hướng đến danh sách
            }

            return View(genre);  // Nếu có lỗi, quay lại trang tạo thể loại
        }

        private string GenerateSlug(string phrase)
        {
            string str = RemoveDiacritics(phrase).ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Bỏ ký tự đặc biệt
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();  // Rút gọn khoảng trắng
            str = str.Replace(" ", "-"); // Dùng gạch ngang thay khoảng trắng
            return str;
        }

        private string RemoveDiacritics(string text)
        {
            text = text.Replace("Đ", "D").Replace("đ", "d");
            var normalized = text.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GenreId,GenreName,Slug")] GenreModel genre)
        {
            if (id != genre.GenreId)
            {
                return NotFound();
            }

            // Tạo lại Slug từ GenreName
            genre.Slug = GenerateSlug(genre.GenreName);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genre.GenreId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Sửa Thể Loại Thành Công";
                return RedirectToAction(nameof(Index));
            }

            return View(genre);
        }
        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.GenreId == id);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var genre = await _context.Genres
                .FirstOrDefaultAsync(m => m.GenreId == id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa Thể Loại Thành Công";
            return RedirectToAction(nameof(Index));
        }
    }
}
