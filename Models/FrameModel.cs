using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class FrameModel
    {
        [Key]
        public int FrameId { get; set; }

        [Required, StringLength(100)]
        public string FrameName { get; set; }

        public string? FrameImage { get; set; }

        public bool IsFree { get; set; } // True: miễn phí, False: phải mua

        [Range(0, double.MaxValue)]
        public int? Coin { get; set; } = 0; // Giá của khung nếu phải mua

        // Những người dùng sở hữu khung này
        public virtual ICollection<UserFrameModel> UserFrames { get; set; } = new List<UserFrameModel>();
    }
}
