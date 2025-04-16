using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class UserFrameModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual UserModel User { get; set; }

        [Required]
        public int FrameId { get; set; }

        public virtual FrameModel Frame { get; set; }
    }
}
