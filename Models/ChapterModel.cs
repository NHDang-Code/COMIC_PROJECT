using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class ChapterModel
    {
        [Key]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "Mã truyện không được bỏ trống.")]

        public int? ComicId { get; set; }

        public virtual ComicModel? Comic { get; set; } = null;

        [Range(1, int.MaxValue, ErrorMessage = "Số thứ tự chương phải lớn hơn 0.")]
        public int ChapterNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá Coin không được âm.")]
        public decimal? Coin { get; set; }

        public bool IsPaid { get; set; }

        public virtual ICollection<ChapterImageModel> ChapterImages { get; set; } = new List<ChapterImageModel>();

        public virtual ICollection<PaymentHistoryModel> PaymentHistories { get; set; } = new List<PaymentHistoryModel>();

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                if (!CreatedAt.HasValue)
                    return "Không xác định";

                var timeSpan = DateTime.Now - CreatedAt.Value;

                if (timeSpan.TotalMinutes < 1)
                    return "Vừa xong";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} giờ trước";
                return $"{(int)timeSpan.TotalDays} ngày trước";
            }
        }
    }
}
