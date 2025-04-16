using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels
{
    public class LevelTypeDetailsViewModel
    {
        public int LevelTypeId { get; set; }
        public string TypeName { get; set; }
        public string SearchTerm { get; set; }
        public List<LevelMappingModel> LevelMappings { get; set; }
        public Paginate Pagination { get; set; }
    }
}
