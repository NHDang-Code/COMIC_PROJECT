using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels
{
    public class NationViewModel
    {
        public List<NationModel> Nations { get; set; }
        public Paginate Paginate { get; set; }

        public string SearchTerm { get; set; }
    }
}
