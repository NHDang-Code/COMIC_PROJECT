using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class ComicGenreModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Đảm bảo ComicGenreId tự động tăng
        public int ComicGenreId { get; set; }

        [Required]
        public int ComicId { get; set; }

        [Required]
        public int GenreId { get; set; }

        public virtual ComicModel? Comic { get; set; }
        public virtual GenreModel? Genre { get; set; }
    }
}
