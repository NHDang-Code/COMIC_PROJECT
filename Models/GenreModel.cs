using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class GenreModel
    {
        [Key]
        public int GenreId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên thể loại.")]
        [StringLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự.")]
        public string GenreName { get; set; } = null!;

        public string Slug { get; set; }

        public virtual ICollection<ComicGenreModel> ComicGenres { get; set; } = new List<ComicGenreModel>();
    }
}
