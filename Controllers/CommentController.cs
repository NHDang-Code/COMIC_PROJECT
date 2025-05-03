using Azure.Core;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Constants;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;


namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Controllers
{
    public class CommentController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public CommentController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentConstants constants)
        {
            if (string.IsNullOrWhiteSpace(constants.Content))
            {
                return BadRequest("Nội dung bình luận không được để trống.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Bạn cần đăng nhập để bình luận.");
            }

            int.TryParse(userIdClaim, out int userId);

            var user = await _context.Users.FindAsync(userId);

            if (user == null) return Unauthorized("Tài khoản không hợp lệ.");

            int? parentCommentId = null;

            int? repliedToUserId = null;

            if (constants.ParentId.HasValue)
            {
                var parentComment = await _context.Comments.FindAsync(constants.ParentId.Value);

                if (parentComment != null)
                {
                    repliedToUserId = parentComment.UserId;
                    parentCommentId = parentComment.ParentCommentId ?? parentComment.CommentId;
                }
            }


            string cleanedContent = constants.Content;

            
            cleanedContent = Regex.Replace(cleanedContent, @"^@\p{L}[\p{L}\s]*", "").Trim();

            if (cleanedContent.Length > 5000)
            {
                return BadRequest($"Nội dung bình luận không được vượt quá 5000 ký tự");
            }

            var comment = new CommentModel
            {
                ComicId = constants.ComicId,
                UserId = user.UserId,
                Content = cleanedContent,
                ParentCommentId = parentCommentId,
                RepliedToUserId = repliedToUserId,
                UserName = user.UserName,
                CreateAt = DateTime.UtcNow.AddHours(7)
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            string notificationHtml = "<div class='alert alert-success notification'>" + "Bình luận thành công!" + "</div>";

            return Json(new { success = true, html = notificationHtml, message = "Bình luận thành công!" });
        }

    }
}
