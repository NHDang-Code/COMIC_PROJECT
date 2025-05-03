using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class CommentModel
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Hãy nhập ID người dùng.")]
        public int UserId { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Hãy nhập ID truyện.")]
        public int ComicId { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận không thể để trống.")]
        [StringLength(5000, ErrorMessage = "Nội dung bình luận không được vượt quá 5000 ký tự.")]
        public string Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreateAt { get; set; } = DateTime.Now;
        [NotMapped]
        public string TimeAgo
        {
            get
            {
                if (!CreateAt.HasValue) return "Không xác định";

                TimeSpan timeSpan = DateTime.Now - CreateAt.Value;

                if (timeSpan.TotalSeconds < 60)
                    return $"{timeSpan.Seconds} giây trước";
                if (timeSpan.TotalMinutes < 60)
                    return $"{timeSpan.Minutes} phút trước";
                if (timeSpan.TotalHours < 24)
                    return $"{timeSpan.Hours} giờ trước";
                if (timeSpan.TotalDays < 30)
                    return $"{timeSpan.Days} ngày trước";
                if (timeSpan.TotalDays < 365)
                    return $"{timeSpan.Days / 30} tháng trước";

                return $"{timeSpan.Days / 365} năm trước";
            }
        }
        
        public virtual UserModel? User { get; set; } = null!;
        public virtual ComicModel? Comic { get; set; } = null!;

        
        public int? ParentCommentId { get; set; }
        public int? RepliedToUserId { get; set; }

        public virtual UserModel? RepliedToUser { get; set; }
        public virtual CommentModel? ParentComment { get; set; }

        public virtual ICollection<CommentModel> Replies { get; set; } = new List<CommentModel>();
    }
}
