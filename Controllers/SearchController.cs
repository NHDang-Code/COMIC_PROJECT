using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class SearchController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public SearchController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<ComicChapterViewModel>());
            }

            var results = await _context.Comics
                .Where(c => c.ComicName.Contains(query))
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new ComicChapterViewModel
                {
                    Comic = c,
                    LatestChapter = c.Chapters
                        .OrderByDescending(ch => ch.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            ViewBag.Query = query;

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> SearchAjax(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { success = false });
            }

            var results = await _context.Comics
                .Where(c => c.ComicName.Contains(query))
                .Select(c => new
                {
                    c.ComicId,
                    c.ComicName,
                    c.ComicImage
                })
                .ToListAsync();

            return Json(new { success = true, data = results });
        }

    }
}
