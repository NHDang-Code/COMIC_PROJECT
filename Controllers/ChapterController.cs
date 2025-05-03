using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{

    public class ChapterController : Controller
    {
        private readonly ILogger<ChapterController> _logger;
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public ChapterController(ILogger<ChapterController> logger, K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int id)
        {
            var chapters = _context.Chapters
                            .Include(c => c.Comic)
                            .Include(c => c.ChapterImages)
                            .FirstOrDefault(c => c.ChapterId == id);

            if (chapters == null)
            {
                return NotFound();
            }

            bool requirePayment = chapters.IsPaid;

            if (requirePayment && chapters.Coin == 0)
            {
                chapters.Coin = 20;
            }

            bool hasPurchased = false;
            int? userId = null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            if (requirePayment && userId.HasValue)
            {
                hasPurchased = _context.PaymentHistories
                                       .Any(p => p.UserId == userId.Value && p.ChapterId == id && p.Status == "Success");
            }


            if (userId.HasValue && chapters.ComicId.HasValue)
            {
                var existingHistory = _context.Histories
                    .FirstOrDefault(h => h.UserId == userId.Value && h.ComicId == chapters.ComicId);

                if (existingHistory != null)
                {
                    var maxChapter = _context.Chapters
                        .Where(c => c.ComicId == chapters.ComicId)
                        .OrderByDescending(c => c.ChapterNumber)
                        .FirstOrDefault();

                    if (maxChapter != null && chapters.ChapterNumber >= maxChapter.ChapterNumber)
                    {
                        existingHistory.ChapterId = id;
                    }

                    existingHistory.LastReadAt = DateTime.Now;
                }
                else
                {
                    _context.Histories.Add(new HistoryModel
                    {
                        UserId = userId.Value,
                        ComicId = chapters.ComicId.Value,
                        ChapterId = id,
                        LastReadAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
            }

            ViewBag.RequirePayment = requirePayment && !hasPurchased;

            List<ChapterModel> chapterList = _context.Chapters
                                                     .Where(c => c.ComicId == chapters.ComicId)
                                                     .OrderBy(c => c.ChapterNumber)
                                                     .ToList();
            ViewBag.Chapters = chapterList;

            ViewBag.PreviousChapter = chapterList.LastOrDefault(c => c.ChapterNumber < chapters.ChapterNumber);

            ViewBag.NextChapter = chapterList.FirstOrDefault(c => c.ChapterNumber > chapters.ChapterNumber);

            if (!ViewBag.RequirePayment)
            {
                List<string> imageUrls = new List<string>();
                foreach (var image in chapters.ChapterImages)
                {
                    if (!string.IsNullOrEmpty(image.ImageUrls))
                    {
                        var images = JsonSerializer.Deserialize<List<string>>(image.ImageUrls);
                        if (images != null)
                        {
                            imageUrls.AddRange(images);
                        }
                    }
                    ViewBag.ImageUrls = imageUrls;
                }
            }
            else
            {
                ViewBag.ImageUrls = new List<string>();
            }

            return View(chapters);
        }

        [HttpPost]
        public IActionResult IncreasePoints()
        {
            int? userId = null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var parsedUserId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để nhận điểm." });
            }
            userId = parsedUserId;

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Người dùng không tồn tại." });
            }

            user.Points += 50;
            _context.SaveChanges();

            return Json(new { success = true, message = "Bạn đã nhận được 50 điểm!" });
        }

        [HttpPost]
        public IActionResult PurchaseChapter(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                TempData["error"] = "Bạn cần đăng nhập để dùng chức năng này!";
                return RedirectToAction("Index", new { id });
            }

            var chapter = _context.Chapters.FirstOrDefault(c => c.ChapterId == id);
            if (chapter == null)
            {
                TempData["error"] = "Chương không tồn tại!";
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.Find(userId);
            if (user == null || user.Coin < chapter.Coin.GetValueOrDefault())
            {
                TempData["error"] = "Bạn không đủ Coin để mua chương này!";
                return RedirectToAction("Index", new { id });
            }

            
            var hasPurchased = _context.PaymentHistories
                .Any(p => p.UserId == userId && p.ChapterId == id && p.Status == "Success");

            if (hasPurchased)
            {
                TempData["success"] = "Bạn đã mua chương này rồi!";
                return RedirectToAction("Index", new { id });
            }

            
            user.Coin -= chapter.Coin.Value;

            
            _context.PaymentHistories.Add(new PaymentHistoryModel
            {
                UserId = userId,
                ChapterId = id,
                Amount = chapter.Coin.Value,
                PaymentType = "ChapterUnlock",
                Status = "Success",
                PaymentDate = DateTime.UtcNow,
                Description = $"Mở khóa chương {chapter.ChapterNumber}"
            });

            
            var comic = _context.Comics.FirstOrDefault(c => c.ComicId == chapter.ComicId);
            if (comic != null)
            {
                var team = _context.TranslationTeams.FirstOrDefault(t => t.TeamId == comic.TeamId);
                if (team != null)
                {
                    team.TotalEarnings += 20;
                }
            }

            _context.SaveChanges();

            TempData["success"] = "Mua chương thành công!";
            return RedirectToAction("Index", new { id });
        }


    }
}
