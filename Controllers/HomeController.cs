using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public HomeController(ILogger<HomeController> logger, K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comicChapters = await _context.Comics
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new ComicChapterViewModel
                {
                    Comic = c,
                    LatestChapter = c.Chapters
                        .OrderByDescending(ch => ch.CreatedAt)
                        .FirstOrDefault()
                })
                .Take(18)
                .ToListAsync();

            var hotComics = await _context.Comics
                .Include(c => c.Chapters)
                .Where(c => c.Views >= 500)
                .OrderByDescending(c => c.Views)
                .Select(c => new ComicChapterViewModel
                {
                    Comic = c,
                    LatestChapter = c.Chapters
                    .OrderByDescending(ch => ch.CreatedAt)
                    .FirstOrDefault()
                    
                })
                .Take(10)
                .ToListAsync();

            ViewBag.HotComics = hotComics;

            return View(comicChapters);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var comics = await _context.Comics
                .Include(s => s.ComicGenres)
                    .ThenInclude(sg => sg.Genre)
                .Include(ch => ch.Chapters)
                .FirstOrDefaultAsync(s => s.ComicId == id);

            if(comics == null)
            {
                return NotFound();
            }

            comics.Chapters = comics.Chapters.OrderByDescending(c => c.CreatedAt).ToList();

            ViewBag.ComicId = comics.ComicId;
            

            return View(comics);

        }




    }
}
