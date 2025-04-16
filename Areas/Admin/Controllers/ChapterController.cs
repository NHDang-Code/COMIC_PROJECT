using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin, Translator")]
    public class ChapterController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public ChapterController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;

        }

        public async Task<IActionResult> Index()
        {
            var chapters = _context.Chapters.Include(c => c.Comic);
            return View(await chapters.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var chapter = await _context.Chapters
                .Include(c => c.Comic) // Lấy cả thông tin Comic
                .FirstOrDefaultAsync(m => m.ChapterId == id);

            if (chapter == null) return NotFound();

            // Lấy danh sách ảnh của chương
            var images = await _context.ChapterImages
                .Where(ci => ci.ChapterId == id)
                .ToListAsync();

            ViewData["ChapterImages"] = images; // Truyền danh sách ảnh qua View
            ViewData["ComicId"] = chapter.ComicId; // Truyền ComicId để sử dụng trong View

            return View(chapter);
        }


        [HttpGet]
        public IActionResult Create(int? ComicId)
        {
            if (ComicId == null)
            {
                TempData["error"] = "Truyện không hợp lệ!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            // Lấy ComicName từ ComicId và truyền nó vào ViewBag
            var comic = _context.Comics.FirstOrDefault(c => c.ComicId == ComicId);
            if (comic != null)
            {
                ViewBag.ComicId = comic.ComicId;
                ViewBag.ComicName = comic.ComicName;
            }
            else
            {
                TempData["error"] = "Truyện không tồn tại!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            // Tìm số chapter cao nhất của Comic hiện tại, nếu có
            var lastChapter = _context.Chapters
                                     .Where(ch => ch.ComicId == ComicId)
                                     .OrderByDescending(ch => ch.ChapterNumber)
                                     .FirstOrDefault();

            // Nếu có chapter nào trước đó, lấy số ChapterNumber lớn nhất rồi tăng 1
            int nextChapterNumber = lastChapter != null ? lastChapter.ChapterNumber + 1 : 1;

            // Gán nextChapterNumber cho Chapter mới
            ViewData["NextChapterNumber"] = nextChapterNumber;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChapterId,ComicId,CreatedAt,ChapterNumber")] ChapterModel chapter)
        {
            if (ModelState.IsValid)
            {
                // Gán mặc định giá trị Coin là 0
                chapter.Coin = 0;

                // Gán giá trị CreatedAt nếu không có
                chapter.CreatedAt = chapter.CreatedAt == null ? DateTime.Now : chapter.CreatedAt;

                // Nếu không có giá trị ChapterNumber trong view, gán giá trị mới vào
                if (chapter.ChapterNumber == 0)
                {
                    var lastChapter = _context.Chapters
                                        .Where(ch => ch.ComicId == chapter.ComicId)
                                        .OrderByDescending(ch => ch.ChapterNumber)
                                        .FirstOrDefault();

                    chapter.ChapterNumber = lastChapter != null ? lastChapter.ChapterNumber + 1 : 1;
                }

                _context.Add(chapter);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công
                TempData["success"] = "Chapter đã được thêm thành công.";

                return RedirectToAction("Details", "Profile", new { area = "Admin", id = chapter.ComicId });
            }

            // Nếu không hợp lệ, truyền lại ComicId và ComicName để hiển thị trong view
            var comic = _context.Comics.FirstOrDefault(c => c.ComicId == chapter.ComicId);
            if (comic == null)
            {
                TempData["error"] = "Truyện không tồn tại!";
                return RedirectToAction("Details", "Profile", new { area = "Admin" });
            }

            ViewBag.ComicId = comic.ComicId;
            ViewBag.ComicName = comic.ComicName;

            return View(chapter);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }

            // Get the ComicId from the chapter to fetch ComicName
            var comic = _context.Comics.FirstOrDefault(c => c.ComicId == chapter.ComicId);
            if (comic != null)
            {
                // Set the ComicName in ViewData to display in the view
                ViewData["ComicName"] = comic.ComicName;
                ViewData["ComicId"] = comic.ComicId; // Store ComicId as well
            }
            else
            {
                TempData["error"] = "Truyện này không tồn tại!";
                return RedirectToAction("Details", "Profile", new { area = "Admin" });
            }

            // Create SelectList for dropdown (if needed to change Comic)
            ViewData["Comics"] = new SelectList(_context.Comics, "ComicId", "ComicName", chapter.ComicId);

            return View(chapter);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChapterId,ComicId,ChapterTitle,ChapterNumber,Coin,CreatedAt")] ChapterModel chapter)
        {
            // Kiểm tra ChapterId có khớp không
            if (id != chapter.ChapterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var chapterToUpdate = await _context.Chapters.FindAsync(id);

                    // Kiểm tra xem Chapter có tồn tại hay không
                    if (chapterToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Chỉ thay đổi các trường đã được chỉnh sửa
                    chapterToUpdate.Coin = chapter.Coin;
                   
                    if (chapter.ChapterNumber > 0)
                    {
                        chapterToUpdate.ChapterNumber = chapter.ChapterNumber;
                    }
                    if (chapter.CreatedAt.HasValue)
                    {
                        chapterToUpdate.CreatedAt = chapter.CreatedAt;
                    }

                    // Cập nhật Chapter
                    _context.Update(chapterToUpdate);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Chapter đã được cập nhật thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Nếu không tìm thấy ChapterId trong cơ sở dữ liệu
                    if (!_context.Chapters.Any(c => c.ChapterId == chapter.ChapterId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Profile", new { area = "Admin", id = chapter.ComicId });
            }

            // Trả về lại view nếu model không hợp lệ
            ViewData["ComicId"] = new SelectList(_context.Comics, "ComicId", "ComicName", chapter.ComicId);
            return View(chapter);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Comic)
                .FirstOrDefaultAsync(m => m.ChapterId == id);

            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Xử lý xoá Chapter và chuyển đến trang chi tiết Comic
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);

            if (chapter != null)
            {
                var comicId = chapter.ComicId;  // Lưu ComicId để chuyển hướng sau khi xóa
                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();

                TempData["success"] = "Xoá chương thành công!";

                // Chuyển hướng về trang chi tiết Comic
                return RedirectToAction("Details", "Profile", new { area = "Admin", id = comicId });
            }

            return RedirectToAction(nameof(Index));  // Nếu không tìm thấy chapter, trở về danh sách
        }

    }
}
