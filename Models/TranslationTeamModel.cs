using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class TranslationTeamModel
    {
        [Key]
        public int TeamId { get; set; }

        [Required, StringLength(255)]
        public string TeamName { get; set; }

        public string? Description { get; set; }
        public string? TeamAvatar { get; set; } = "media/team_image/logo_dragon.jpg";

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEarnings { get; set; } = 0;// Tổng thu nhập

        public virtual ICollection<TeamMemberModel> Members { get; set; } = new List<TeamMemberModel>();

        public virtual ICollection<ComicModel> Comics { get; set; } = new List<ComicModel>();

        public virtual ICollection<PaymentHistoryModel> PaymentHistories { get; set; } = new List<PaymentHistoryModel>();

        [NotMapped]
        public int MemberCount => Members?.Count ?? 0;
    }
}
