using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        private readonly DbContextOptions<K21CNT2_NguyenHaiDang_2110900067_DATNContext> _viewCtxOptions;

        public HomeController(ILogger<HomeController> logger, K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
            _logger = logger;

            var optionsBuilder = new DbContextOptionsBuilder<K21CNT2_NguyenHaiDang_2110900067_DATNContext>()
            .UseSqlServer(_context.Database.GetDbConnection().ConnectionString);
            _viewCtxOptions = optionsBuilder.Options;
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
        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            if (id == null) return NotFound();

           
            await IncrementViewCountAsync(id.Value);

            
            var comic = await _context.Comics
                .Include(c => c.ComicGenres).ThenInclude(sg => sg.Genre)
                .Include(c => c.Chapters)
                .Include(c => c.TranslationTeam)
                .FirstOrDefaultAsync(c => c.ComicId == id);
            if (comic == null) return NotFound();

            
            comic.Chapters = comic.Chapters
                .OrderByDescending(ch => ch.CreatedAt)
                .ToList();

            
            const int pageSize = 5;

            int totalParentComments = await _context.Comments
                .CountAsync(c => c.ComicId == id && c.ParentCommentId == null);

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalParentComments / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.ComicId = comic.ComicId;

            
            var parentComments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.ComicId == id && c.ParentCommentId == null)
                .Include(c => c.User)
                    .ThenInclude(u => u.CurrentFrame)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                    .ThenInclude(u => u.CurrentFrame)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.RepliedToUser)
                .OrderByDescending(c => c.CreateAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            comic.Comments = parentComments;

            
            var levelMappings = await _context.LevelMappings.OrderBy(lm => lm.PointsRequired).ToListAsync();
            var userDisplayNames = new Dictionary<int, string>();
            foreach (var parent in parentComments)
            {
                if (parent.User != null)
                    userDisplayNames[parent.User.UserId] =
                        GetLevelDisplayName(parent.User.LevelTypeId ?? 0, parent.User.Points, levelMappings);

                foreach (var reply in parent.Replies)
                {
                    if (reply.User != null && !userDisplayNames.ContainsKey(reply.User.UserId))
                        userDisplayNames[reply.User.UserId] =
                            GetLevelDisplayName(reply.User.LevelTypeId ?? 0, reply.User.Points, levelMappings);
                }
            }
            ViewBag.UserDisplayNames = userDisplayNames;

            
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int? userId = null;

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var parsedUser))
                userId = parsedUser;

            
            ViewBag.IsFollowed = userId.HasValue && await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ComicId == id);
            ViewBag.TotalFollows = await _context.Favorites.CountAsync(f => f.ComicId == id);

            ViewBag.IsLiked = userId.HasValue && await _context.Likes.AnyAsync(f => f.UserId == userId && f.ComicId == id);
            ViewBag.TotalLikes = await _context.Likes.CountAsync(f => f.ComicId == id);

            var chapterPurchase = new Dictionary<int, bool>();

            var chapterFree = new Dictionary<int, bool>();

            foreach (var ch in comic.Chapters)
            {
                chapterFree[ch.ChapterId] = !ch.IsPaid;
                chapterPurchase[ch.ChapterId] = userId.HasValue &&
                    await _context.PaymentHistories.AnyAsync(p => p.UserId == userId && p.ChapterId == ch.ChapterId && p.Status == "Success");
            }
            ViewBag.ChapterFreeStatus = chapterFree;
            ViewBag.ChapterPurchaseStatus = chapterPurchase;

            ViewBag.ShowLoginModal = TempData["error"] != null;

            
            ViewBag.HasStartedReading = userId.HasValue &&
                await _context.Histories.AnyAsync(h => h.UserId == userId && h.ComicId == id);

            return View(comic);
        }

        private async Task IncrementViewCountAsync(int comicId)
        {
            string cookieKey = $"Viewed_Comic_{comicId}";
            if (Request.Cookies[cookieKey] != null) return;

            
            using var viewCtx = new K21CNT2_NguyenHaiDang_2110900067_DATNContext(_viewCtxOptions);

            var tmp = new ComicModel { ComicId = comicId };

            viewCtx.Comics.Attach(tmp);

            
            var oldViews = await _context.Comics
                .Where(c => c.ComicId == comicId)
                .Select(c => c.Views)
                .FirstAsync();
            tmp.Views = oldViews + 1;

            viewCtx.Entry(tmp).Property(c => c.Views).IsModified = true;
            await viewCtx.SaveChangesAsync();

            Response.Cookies.Append(cookieKey, "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(10),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true
            });
        }

        private string GetLevelDisplayName(int levelTypeId, int points, List<LevelMappingModel> levelMappings)
        {
            var lm = levelMappings
                .Where(m => m.LevelTypeId == levelTypeId && points >= m.PointsRequired)
                .LastOrDefault();
            return lm?.DisplayName ?? " ";
        }

        [HttpPost]
        public async Task<IActionResult> Follow(int comicId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ComicId == comicId);

            if (existingFavorite != null)
            {
                _context.Favorites.Remove(existingFavorite);
            }
            else
            {
                _context.Favorites.Add(new FavoriteModel
                {
                    UserId = userId,
                    ComicId = comicId,
                    AddedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = comicId });
        }

        [HttpGet]
        public async Task<IActionResult> ReadFromStart(int comicId)
        {
            var firstChapter = await _context.Chapters
                .Where(ch => ch.ComicId == comicId)
                .OrderBy(ch => ch.CreatedAt)
                .FirstOrDefaultAsync();

            if (firstChapter == null) return NotFound();
            return RedirectToAction("Index", "Chapter", new { id = firstChapter.ChapterId });
        }

        [HttpGet]
        public async Task<IActionResult> ContinueReading(int comicId)
        {
            int? userId = null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            if (userId == null)
                return Unauthorized();

            var lastRead = await _context.Histories
                .Where(h => h.UserId == userId && h.ComicId == comicId)
                .OrderByDescending(h => h.LastReadAt)
                .FirstOrDefaultAsync();

            if (lastRead != null)
            {
                return RedirectToAction("Index", "Chapter", new { id = lastRead.ChapterId });
            }
            else
            {
                return await ReadFromStart(comicId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Like(int comicId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ComicId == comicId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                _context.Likes.Add(new LikeModel
                {
                    UserId = userId,
                    ComicId = comicId,
                    LikeAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = comicId });
        }

    }
}
