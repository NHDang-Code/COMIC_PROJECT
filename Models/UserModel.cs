using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(255)]
        public string UserName { get; set; }

        [Required, EmailAddress, StringLength(255)]
        public string UserEmail { get; set; }

        [Required, StringLength(255, MinimumLength = 6)]
        public string Password { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Coin { get; set; } = 0;
        public int Points { get; set; } = 0;

        public string? UserImage { get; set; } = "media/user_image/logo_dragon.jpg";
        public string? CoverUserImage { get; set; } = "media/cover_image/cover_image.jpg";

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public virtual RoleModel? Role { get; set; }

        public bool IsLocked { get; set; } = false;

        public int? LevelTypeId { get; set; }

        public virtual LevelTypeModel? LevelType { get; set; }

        public int? CurrentFrameId { get; set; }

        public virtual FrameModel? CurrentFrame { get; set; }

        public virtual ICollection<UserFrameModel> UserFrames { get; set; } = new List<UserFrameModel>();

        public virtual ICollection<TeamMemberModel> Members { get; set; } = new List<TeamMemberModel>();

    }
}
