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
                .Include(c => c.Comic)
                .FirstOrDefaultAsync(m => m.ChapterId == id);

            if (chapter == null) return NotFound();

            // Lấy danh sách ảnh của chương
            var images = await _context.ChapterImages
                .Where(ci => ci.ChapterId == id)
                .ToListAsync();

            ViewData["ChapterImages"] = images;
            ViewData["ComicId"] = chapter.ComicId;

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

            
            var comic = _context.Comics
                .FirstOrDefault(c => c.ComicId == ComicId);

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

           
            var lastChapter = _context.Chapters
                                     .Where(ch => ch.ComicId == ComicId)
                                     .OrderByDescending(ch => ch.ChapterNumber)
                                     .FirstOrDefault();

            
            int nextChapterNumber = lastChapter != null ? lastChapter.ChapterNumber + 1 : 1;

            
            ViewData["NextChapterNumber"] = nextChapterNumber;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChapterId,ComicId,CreatedAt,ChapterNumber,IsPaid")] ChapterModel chapter)
        {
            if (ModelState.IsValid)
            {
                
                chapter.Coin = chapter.IsPaid ? 20 : 0;

                
                chapter.CreatedAt ??= DateTime.Now;

                
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

                TempData["success"] = "Chapter đã được thêm thành công.";

                return RedirectToAction("Details", "Profile", new { area = "Admin", id = chapter.ComicId });
            }

           
            var comic = _context.Comics
                .FirstOrDefault(c => c.ComicId == chapter.ComicId);

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
                TempData["error"] = "Chương không hợp lệ!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                TempData["error"] = "Không tìm thấy chương!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            var comic = await _context.Comics.FindAsync(chapter.ComicId);
            if (comic == null)
            {
                TempData["error"] = "Truyện không tồn tại!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            ViewBag.ComicId = comic.ComicId;
            ViewBag.ComicName = comic.ComicName;

            return View(chapter);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChapterId,ComicId,ChapterNumber,CreatedAt,IsPaid")] ChapterModel chapter)
        {
            if (id != chapter.ChapterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var chapterToUpdate = await _context.Chapters.FindAsync(id);
                    if (chapterToUpdate == null)
                    {
                        return NotFound();
                    }

                    chapterToUpdate.ChapterNumber = chapter.ChapterNumber;
                    chapterToUpdate.CreatedAt = chapter.CreatedAt ?? DateTime.Now;
                    chapterToUpdate.IsPaid = chapter.IsPaid;
                    chapterToUpdate.Coin = chapter.IsPaid ? 20 : 0;

                    _context.Update(chapterToUpdate);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Chapter đã được cập nhật thành công.";
                    return RedirectToAction("Details", "Profile", new { area = "Admin", id = chapter.ComicId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Chapters.Any(e => e.ChapterId == chapter.ChapterId))
                        return NotFound();
                    throw;
                }
            }

            var comic = await _context.Comics.FindAsync(chapter.ComicId);
            if (comic == null)
            {
                TempData["error"] = "Truyện không tồn tại!";
                return RedirectToAction("Index", "Profile", new { area = "Admin" });
            }

            ViewBag.ComicId = comic.ComicId;
            ViewBag.ComicName = comic.ComicName;

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


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);

            if (chapter != null)
            {
                var comicId = chapter.ComicId;

                
                var chaptersToUpdate = await _context.Chapters
                    .Where(c => c.ComicId == comicId && c.ChapterNumber > chapter.ChapterNumber)
                    .ToListAsync();

                
                foreach (var ch in chaptersToUpdate)
                {
                    ch.ChapterNumber -= 1;
                }

                
                _context.Chapters.Remove(chapter);

               
                await _context.SaveChangesAsync();

                TempData["success"] = "Xoá chương thành công!";

                
                return RedirectToAction("Details", "Profile", new { area = "Admin", id = comicId });
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
