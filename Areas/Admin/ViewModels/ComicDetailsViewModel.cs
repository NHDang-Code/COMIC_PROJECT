using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels
{
    public class ComicDetailsViewModel
    {
        public ComicModel Comic { get; set; }
        public List<ChapterModel> Chapters { get; set; }
        public Paginate Pagination { get; set; }
        public string ChapterSearchTerm { get; set; }
    }

}
