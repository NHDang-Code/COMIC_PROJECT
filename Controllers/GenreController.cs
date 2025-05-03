using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class GenreController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public GenreController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.ComicGenres)
                .ThenInclude(cg => cg.Comic)
                .FirstOrDefaultAsync(g => g.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            ViewBag.GenreName = genre.GenreName;

            var comics = genre.ComicGenres
                .Select(cg => new ComicChapterViewModel
                {
                    Comic = cg.Comic,
                    LatestChapter = _context.Chapters
                        .Where(c => c.ComicId == cg.Comic.ComicId)
                        .OrderByDescending(c => c.ChapterNumber)
                        .FirstOrDefault()
                })
                .ToList();

            return View(comics);
        }
    }
}
