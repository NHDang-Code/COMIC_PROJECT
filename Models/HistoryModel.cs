using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class HistoryModel
    {
        [Key]
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public int ComicId { get; set; }

        public int ChapterId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastReadAt { get; set; }

        public virtual UserModel? User { get; set; } = null!;
        public virtual ComicModel? Comic { get; set; } = null!;
        public virtual ChapterModel? Chapter { get; set; } = null!;
    }
}
