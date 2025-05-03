using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class FavoriteModel
    {
        [Key]
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int ComicId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? AddedAt { get; set; }

        public virtual UserModel? User { get; set; } = null!;
        public virtual ComicModel? Comic { get; set; } = null!;
    }
}
