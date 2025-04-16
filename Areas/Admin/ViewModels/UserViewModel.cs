using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels
{
    public class UserViewModel
    {
        public List<UserModel> Users { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string SearchQuery { get; set; }
    }
}
