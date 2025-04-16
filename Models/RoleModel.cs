using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class RoleModel
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [StringLength(255, ErrorMessage = "Tên vai trò không được vượt quá 255 ký tự.")]
        public string RoleName { get; set; }
        public virtual ICollection<UserModel> Users { get; set; } = new List<UserModel>();// Quan hệ với User (Một vai trò có nhiều người dùng)
    }
}
