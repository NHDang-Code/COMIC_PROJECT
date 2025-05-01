using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class ComicModel
    {
        [Key]
        public int ComicId { get; set; }

        [Required(ErrorMessage = "Hãy nhập tên truyện.")]
        [StringLength(255, ErrorMessage = "Tên truyện không được vượt quá 255 ký tự.")]
        public string ComicName { get; set; }

        public string Slug { get; set; }

        [Required(ErrorMessage = "Hãy nhập tên tác giả.")]
        [StringLength(255, ErrorMessage = "Tên tác giả không được vượt quá 255 ký tự.")]
        public string ComicAuthor { get; set; }

        public string? ComicImage { get; set; } = "media/item_image/default_comic.png"; // URL ảnh truyện hoặc file ảnh

        [Required(ErrorMessage = "Hãy nhập mô tả truyện.")]
        [StringLength(5000, ErrorMessage = "Mô tả không được vượt quá 5000 ký tự.")]
        public string ComicDescription { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "Lượt xem không thể âm.")]
        public int Views { get; set; } = 0; // Bắt đầu bằng 0


        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;


        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Đang tiến hành";

        public int? TeamId { get; set; } // Nhóm dịch sở hữu
        public virtual TranslationTeamModel? TranslationTeam { get; set; }

        public int? NationId {  get; set; }

        public virtual NationModel? Nation { get; set; }

        public virtual ICollection<ComicGenreModel> ComicGenres { get; set; } = new List<ComicGenreModel>();

        public List<GenreModel> AvailableGenres { get; set; } = new List<GenreModel>();

        [NotMapped]
        public List<int> SelectedGenreIds { get; set; } = new List<int>();

        public virtual ICollection<ChapterModel> Chapters { get; set; } = new List<ChapterModel>();

        [NotMapped]
        public int ChapterCount => Chapters?.Count ?? 0;

        [NotMapped]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedDate;

                if (timeSpan.TotalMinutes < 1)
                    return "Vừa xong";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} giờ trước";
                return $"{(int)timeSpan.TotalDays} ngày trước";
            }
        }

    }
}
