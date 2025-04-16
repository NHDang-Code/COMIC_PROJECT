using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels
{
    public class FrameViewModel
    {
        public List<FrameModel> Frame { get; set; }
        public Paginate Pagination { get; set; }
        public string SearchTerm { get; set; } // Lưu từ khóa tìm kiếm
    }
}
