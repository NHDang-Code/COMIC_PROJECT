using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]

    public class ComicController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public ComicController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 5)
        {
            var query = _context.Comics.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ComicName.Contains(search) || c.TranslationTeam.TeamName.Contains(search));
            }
            int totalItems = await query.CountAsync(); // Đếm tổng số truyện sau khi lọc

            // Tính toán phân trang
            var paginate = new Paginate(totalItems, page, pageSize);

            // Lấy danh sách truyện trong phạm vi phân trang
            var comics = await query
                .Include(c => c.TranslationTeam)
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.ComicId)
                .Skip((paginate.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo ViewModel chứa danh sách truyện và thông tin phân trang
            var viewModel = new ComicViewModel
            {
                Comics = comics,
                Pagination = paginate,
                SearchTerm = search
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Tải danh sách thể loại từ cơ sở dữ liệu
            var genres = await _context.Genres.ToListAsync();

            var comicModel = new ComicModel
            {
                AvailableGenres = genres // Truyền danh sách thể loại vào view
            };

            ViewBag.TeamId = new SelectList(_context.TranslationTeams, "TeamId", "TeamName");

            return View(comicModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComicModel comics)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp ComicName
                var existingComic = await _context.Comics
                    .FirstOrDefaultAsync(p => p.ComicName == comics.ComicName && p.ComicId != comics.ComicId);

                if (existingComic != null)
                {
                    ModelState.AddModelError("ComicName", "Truyện này đã tồn tại.");
                }

                if (!ModelState.IsValid)
                {
                    return View(comics);
                }

                // Xử lý ảnh ComicImage
                var files = HttpContext.Request.Form.Files;
                var comicImageFile = files.FirstOrDefault(f => f.Name == "ComicImage");
                if (comicImageFile != null && comicImageFile.Length > 0)
                {
                    var comicImageFileName = comicImageFile.FileName;
                    var comicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "comic_image", comicImageFileName);

                    if (System.IO.File.Exists(comicImagePath))
                    {
                        ModelState.AddModelError("ComicImage", "Hình ảnh truyện đã tồn tại trên hệ thống.");
                        return View(comics);
                    }

                    using (var stream = new FileStream(comicImagePath, FileMode.Create))
                    {
                        comicImageFile.CopyTo(stream);
                        comics.ComicImage = "media/comic_image/" + comicImageFileName;
                    }
                }

                // Gán giá trị mặc định cho Views
                comics.Views = 0;

                comics.CreatedDate = DateTime.Now;

                comics.Slug = ComicSlug(comics.ComicName);
                // Lưu truyện vào cơ sở dữ liệu
                _context.Add(comics);
                await _context.SaveChangesAsync();

                // Lưu thông tin thể loại cho truyện
                if (comics.SelectedGenreIds != null && comics.SelectedGenreIds.Any())
                {
                    // Lặp qua danh sách GenreId đã chọn và thêm vào ComicGenreModel
                    foreach (var genreId in comics.SelectedGenreIds)
                    {
                        var comicGenre = new ComicGenreModel
                        {
                            ComicId = comics.ComicId,  // ComicId từ đối tượng comic
                            GenreId = genreId          // GenreId từ danh sách được chọn
                        };

                        _context.ComicGenres.Add(comicGenre);  // Thêm comicGenre vào bảng ComicGenres
                    }

                    // Lưu tất cả thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();
                }

                TempData["success"] = "Thêm truyện thành công";
                return RedirectToAction(nameof(Index));
            }

            return View(comics);
        }

        private string ComicSlug(string phrase)
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var existingComic = await _context.Comics
                .Include(c => c.ComicGenres)
                .FirstOrDefaultAsync(c => c.ComicId == id);

            if (existingComic == null)
            {
                return NotFound();
            }

            // Lấy danh sách thể loại
            ViewBag.AvailableGenres = await _context.Genres.ToListAsync();

            // Lấy danh sách thể loại của truyện để check vào checkbox
            ViewBag.SelectedGenreIds = existingComic.ComicGenres.Select(g => g.GenreId).ToList();

            ViewBag.TeamId = new SelectList(_context.TranslationTeams, "TeamId", "TeamName", existingComic.TeamId);


            return View(existingComic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComicId,ComicName,ComicImage,ComicAuthor,Slug,ComicDescription,Views,Status,SelectedGenreIds,TeamId")] ComicModel comics)
        {
            if (id != comics.ComicId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ từ database
                    var existingComic = await _context.Comics
                                               .Include(c => c.ComicGenres) // Bao gồm ComicGenres để cập nhật thể loại
                                               .FirstOrDefaultAsync(c => c.ComicId == id);

                    if (existingComic == null)
                    {
                        return NotFound();
                    }

                    // Xử lý ảnh ComicImage
                    var files = HttpContext.Request.Form.Files;
                    var comicImageFile = files.FirstOrDefault(f => f.Name == "ComicImage");

                    if (comicImageFile != null && comicImageFile.Length > 0)
                    {
                        var comicImageFileName = comicImageFile.FileName;
                        var comicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "comic_image", comicImageFileName);

                        // Kiểm tra nếu ảnh đã tồn tại
                        if (System.IO.File.Exists(comicImagePath))
                        {
                            ModelState.AddModelError("ComicImage", "Hình ảnh truyện đã tồn tại trên hệ thống.");
                            return View(comics);
                        }

                        // Lưu ảnh mới
                        using (var stream = new FileStream(comicImagePath, FileMode.Create))
                        {
                            await comicImageFile.CopyToAsync(stream);
                            comics.ComicImage = "media/comic_image/" + comicImageFileName;
                        }
                    }
                    else
                    {
                        comics.ComicImage = existingComic.ComicImage; // Giữ nguyên nếu không thay đổi
                    }

                    // Giữ nguyên ngày tạo
                    comics.CreatedDate = existingComic.CreatedDate;


                    comics.Slug = ComicSlug(comics.ComicName);
                    // Cập nhật thể loại
                    _context.ComicGenres.RemoveRange(existingComic.ComicGenres);
                    if (comics.SelectedGenreIds != null && comics.SelectedGenreIds.Any())
                    {
                        foreach (var genreId in comics.SelectedGenreIds.Distinct())
                        {
                            var comicGenre = new ComicGenreModel
                            {
                                ComicId = comics.ComicId,
                                GenreId = genreId
                            };
                            _context.ComicGenres.Add(comicGenre);
                        }
                    }

                    // Cập nhật thông tin truyện
                    existingComic.Slug = comics.Slug;
                    existingComic.ComicName = comics.ComicName;
                    existingComic.ComicAuthor = comics.ComicAuthor;
                    existingComic.ComicDescription = comics.ComicDescription;
                    existingComic.Views = comics.Views;
                    existingComic.Status = comics.Status;
                    existingComic.ComicImage = comics.ComicImage;
                    existingComic.TeamId = comics.TeamId;


                    _context.Update(existingComic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComicExists(comics.ComicId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["success"] = "Cập nhật truyện thành công";
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, gán lại các ViewBag để View hiển thị đầy đủ
            ViewBag.AvailableGenres = await _context.Genres.ToListAsync();
            ViewBag.SelectedGenreIds = comics.SelectedGenreIds ?? new List<int>();
            ViewBag.TeamId = new SelectList(_context.TranslationTeams, "TeamId", "TeamName", comics.TeamId);

            return View(comics);

        }


        // Kiểm tra sự tồn tại của truyện
        private bool ComicExists(int id)
        {
            return _context.Comics.Any(e => e.ComicId == id);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var story = await _context.Comics
                .FirstOrDefaultAsync(m => m.ComicId == id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comic = await _context.Comics.FindAsync(id);
            if (comic != null)
            {
                // Xóa ComicImage nếu có
                if (!string.IsNullOrEmpty(comic.ComicImage))
                {
                    var comicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "comic_image", Path.GetFileName(comic.ComicImage));

                    if (System.IO.File.Exists(comicImagePath))
                    {
                        System.IO.File.Delete(comicImagePath); // Xóa ComicImage khỏi thư mục
                    }
                }

                // Xóa truyện trong cơ sở dữ liệu
                _context.Comics.Remove(comic);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xóa truyện thành công";
            }
            else
            {
                TempData["error"] = "Không tìm thấy truyện để xóa";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
