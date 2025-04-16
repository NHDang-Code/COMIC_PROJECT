using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class ChapterImageModel
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int ChapterId { get; set; }

        // Lưu danh sách ảnh dưới dạng JSON (khi không dùng HTTPS)
        public string? ImageUrls { get; set; }

        public virtual ChapterModel? Chapter { get; set; }

        // Convert JSON thành List<string>
        [NotMapped]
        public List<string> ImageUrlList
        {
            get => string.IsNullOrEmpty(ImageUrls) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(ImageUrls);
            set => ImageUrls = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<IFormFile>? UploadedImages { get; set; }
    }
}
