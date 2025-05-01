using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class DashboardController : Controller
    {
        private K21CNT2_NguyenHaiDang_2110900067_DATNContext  _context;
        public DashboardController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }
        public async Task<ActionResult> Index()
        {
            int totalComics = await _context.Comics.CountAsync();

            int totalChapters = await _context.Chapters.CountAsync();

            int totalViews = await _context.Comics.SumAsync(c => c.Views);

            ViewData["TotalComics"] = totalComics;
            ViewData["TotalChapters"] = totalChapters;
            ViewData["TotalViews"] = totalViews;

            return View();
        }
    }
}
