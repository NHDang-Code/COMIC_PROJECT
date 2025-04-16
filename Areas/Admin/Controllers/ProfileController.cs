using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin, Translator")]
    public class ProfileController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public ProfileController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.LevelType)
                .Include(u => u.CurrentFrame)
                .Include(u => u.UserFrames)
                    .ThenInclude(uf => uf.Frame)
                .Include(u => u.Members)
                    .ThenInclude(m => m.TranslationTeam)
                        .ThenInclude(t => t.Comics)
                .FirstOrDefaultAsync(u => u.UserEmail == email);

            if (user == null)
            {
                return NotFound();
            }

            var teamComics = user.Members
                .Where(m => m.TranslationTeam != null && m.TranslationTeam.Comics != null)
                .SelectMany(m => m.TranslationTeam.Comics)
                .GroupBy(c => c.ComicId) // Dùng GroupBy để tránh truyện trùng lặp
                .Select(g => g.First())
                .ToList();

            ViewBag.TeamId = user.Members.FirstOrDefault(m => m.TranslationTeam != null)?.TranslationTeam.TeamId;
            ViewBag.TeamComics = teamComics;            

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id, string genreSearch, string searchTerm, int page = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comic = await _context.Comics
                .FirstOrDefaultAsync(m => m.ComicId == id);

            if (comic == null)
            {
                return NotFound();
            }

            int pageSize = 5; // Số chương trên mỗi trang
            var totalChapters = await _context.Chapters
                .Where(c => c.ComicId == id && (string.IsNullOrEmpty(searchTerm)))
                .CountAsync();

            var chapters = await _context.Chapters
                .Where(c => c.ComicId == id && (string.IsNullOrEmpty(searchTerm)))
                .OrderBy(c => c.ChapterNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy toàn bộ thể loại của truyện,
            var comicGenres = await _context.ComicGenres
                .Where(cg => cg.ComicId == id && (string.IsNullOrEmpty(genreSearch) || cg.Genre.GenreName.Contains(genreSearch)))
                .Include(cg => cg.Genre)
                .OrderBy(cg => cg.Genre.GenreName)
                .ToListAsync(); // Bỏ Skip() và Take()

            var pagination = new Paginate(totalChapters, page, pageSize);

            ViewData["ComicName"] = comic.ComicName;
            ViewData["ComicId"] = comic.ComicId;

            var viewModel = new ComicDetailsViewModel
            {
                Comic = comic,
                Chapters = chapters,
                ComicGenres = comicGenres,
                Pagination = pagination, // Giữ phân trang cho Chapters
                ChapterSearchTerm = searchTerm,
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddComic(int? teamId)
        {
            var genres = await _context.Genres.ToListAsync();

            var comicModel = new ComicModel
            {
                AvailableGenres = genres
            };

            ViewBag.TeamId = teamId;

            // Lấy tên nhóm dịch dựa trên teamId (nếu có)
            if (teamId != null)
            {
                var team = await _context.TranslationTeams
                    .Where(t => t.TeamId == teamId)
                    .Select(t => new { t.TeamId, t.TeamName })
                    .FirstOrDefaultAsync();

                ViewBag.TeamId = team?.TeamId ?? 0;
                ViewBag.TeamName = team?.TeamName ?? "Không xác định";
            }



            return View(comicModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComic(ComicModel comics, int? teamId)
        {
            // Kiểm tra trùng lặp ComicName trước
            if (ModelState.IsValid)
            {
                var existingComic = await _context.Comics
                    .FirstOrDefaultAsync(p => p.ComicName == comics.ComicName && p.ComicId != comics.ComicId);

                if (existingComic != null)
                {
                    ModelState.AddModelError("ComicName", "Truyện này đã tồn tại.");
                }
            }

            // Nếu ModelState vẫn không hợp lệ sau kiểm tra trùng tên hoặc do lỗi ban đầu (như thiếu tên, ảnh, thể loại,...)
            if (!ModelState.IsValid)
            {
                // Lấy lại danh sách thể loại
                var genres = await _context.Genres.ToListAsync();
                comics.AvailableGenres = genres;

                // Gửi lại danh sách thành viên và nhóm dịch
                if (teamId != null)
                {
                    var team = await _context.TranslationTeams
                        .Where(t => t.TeamId == teamId)
                        .Select(t => new { t.TeamId, t.TeamName })
                        .FirstOrDefaultAsync();

                    ViewBag.TeamId = team?.TeamId;
                    ViewBag.TeamName = team?.TeamName ?? "Không xác định";
                }
                else
                {
                    ViewBag.TeamId = null;
                    ViewBag.TeamName = "Không xác định";
                    ViewBag.TeamMembers = new SelectList(Enumerable.Empty<object>());
                }

                return View(comics);
            }

            // Gán lại TeamId nếu chưa có
            comics.TeamId = comics.TeamId ?? teamId;

            // Xử lý ảnh
            var files = HttpContext.Request.Form.Files;
            var comicImageFile = files.FirstOrDefault(f => f.Name == "ComicImage");
            if (comicImageFile != null && comicImageFile.Length > 0)
            {
                var comicImageFileName = Path.GetFileName(comicImageFile.FileName);
                var comicImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "comic_image", comicImageFileName);

                if (System.IO.File.Exists(comicImagePath))
                {
                    ModelState.AddModelError("ComicImage", "Hình ảnh truyện đã tồn tại trên hệ thống.");
                    return View(comics);
                }

                using (var stream = new FileStream(comicImagePath, FileMode.Create))
                {
                    await comicImageFile.CopyToAsync(stream);
                    comics.ComicImage = "media/comic_image/" + comicImageFileName;
                }
            }

            comics.Views = 0;
            comics.CreatedDate = DateTime.Now;
            comics.Slug = ComicSlug(comics.ComicName);

            _context.Add(comics);
            await _context.SaveChangesAsync();

            // Lưu thể loại
            if (comics.SelectedGenreIds != null && comics.SelectedGenreIds.Any())
            {
                foreach (var genreId in comics.SelectedGenreIds)
                {
                    var comicGenre = new ComicGenreModel
                    {
                        ComicId = comics.ComicId,
                        GenreId = genreId
                    };

                    _context.ComicGenres.Add(comicGenre);
                }

                await _context.SaveChangesAsync();
            }

            TempData["success"] = "Thêm truyện thành công";

            return RedirectToAction("Index", "Profile", new { id = comics.TeamId });
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

        // Sửa Truyện
        [HttpGet]
        public async Task<IActionResult> EditComic(int id)
        {
            var existingComic = await _context.Comics
                .Include(c => c.ComicGenres)
                .Include(c => c.TranslationTeam)
                .FirstOrDefaultAsync(c => c.ComicId == id);

            if (existingComic == null)
            {
                return NotFound();
            }

            // Lấy danh sách thể loại
            ViewBag.AvailableGenres = await _context.Genres.ToListAsync();

            // Lấy danh sách thể loại của truyện để check vào checkbox
            ViewBag.SelectedGenreIds = existingComic.ComicGenres.Select(g => g.GenreId).ToList();

            ViewBag.TeamName = existingComic.TranslationTeam?.TeamName;


            return View(existingComic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComic(int id, [Bind("ComicId,ComicName,ComicImage,ComicAuthor,Slug,ComicDescription,Views,Status,SelectedGenreIds,TeamId")] ComicModel comics)
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
                        .Include(c => c.ComicGenres)
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

                            // Lấy lại thông tin để hiển thị nếu lỗi
                            await PopulateEditViewData(comics);
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
                        comics.ComicImage = existingComic.ComicImage; // Giữ nguyên ảnh cũ nếu không thay đổi
                    }

                    // Giữ nguyên ngày tạo và TeamId
                    comics.CreatedDate = existingComic.CreatedDate;
                    comics.TeamId = existingComic.TeamId;

                    comics.Slug = ComicSlug(comics.ComicName);

                    // Cập nhật lại thể loại: xóa cũ, thêm mới
                    _context.ComicGenres.RemoveRange(existingComic.ComicGenres);
                    if (comics.SelectedGenreIds != null && comics.SelectedGenreIds.Any())
                    {
                        foreach (var genreId in comics.SelectedGenreIds.Distinct())
                        {
                            _context.ComicGenres.Add(new ComicGenreModel
                            {
                                ComicId = comics.ComicId,
                                GenreId = genreId
                            });
                        }
                    }

                    // Cập nhật các trường khác
                    existingComic.Slug = comics.Slug;
                    existingComic.ComicName = comics.ComicName;
                    existingComic.ComicAuthor = comics.ComicAuthor;
                    existingComic.ComicDescription = comics.ComicDescription;
                    existingComic.Views = comics.Views;
                    existingComic.Status = comics.Status;
                    existingComic.ComicImage = comics.ComicImage;

                    _context.Update(existingComic);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Cập nhật truyện thành công";
                    return RedirectToAction("Index", "Profile", new { id = comics.TeamId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComicExists(comics.ComicId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Nếu ModelState không hợp lệ
            await PopulateEditViewData(comics);
            return View(comics);
        }

        private async Task PopulateEditViewData(ComicModel comics)
        {
            ViewBag.AvailableGenres = await _context.Genres.ToListAsync();
            ViewBag.SelectedGenreIds = comics.SelectedGenreIds ?? new List<int>();
            ViewBag.TeamName = await _context.TranslationTeams
                                    .Where(t => t.TeamId == comics.TeamId)
                                    .Select(t => t.TeamName)
                                    .FirstOrDefaultAsync();
        }
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
