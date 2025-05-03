using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class PaymentHistoryModel
    {
        [Key]
        public int PaymentHistoryId { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual UserModel User { get; set; }

        public int? ChapterId { get; set; }

        public virtual ChapterModel? Chapter { get; set; }

        public int? FrameId { get; set; }

        public virtual FrameModel? Frame { get; set; }

        public int? TeamId { get; set; }
        public virtual TranslationTeamModel? TranslationTeam { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Số coin thanh toán không thể âm.")]
        public decimal Amount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";  // "Pending", "Success", "Failed"

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
