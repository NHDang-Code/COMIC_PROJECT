using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.ViewModels
{
    public class UserRegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string UserEmail { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
