using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class NationModel
    {
        [Key]
        public int NationId {  get; set; }
        [Required(ErrorMessage = "Tên quốc gia không được để trống.")]
        [StringLength(255, ErrorMessage = "Tên quốc gia không được vượt quá 255 ký tự.")]
        public string NationName { get; set; }

        public virtual ICollection<ComicModel> Comics { get; set; } = new List<ComicModel>();

    }
}
