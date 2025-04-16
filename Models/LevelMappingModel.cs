using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class LevelMappingModel
    {
        [Key]
        public int LevelId { get; set; }

        public int? LevelTypeId { get; set; }

        [Required(ErrorMessage = "Hãy nhập điểm yêu cầu")]
        public int PointsRequired { get; set; }

        [Required(ErrorMessage = "Hãy nhập tên cấp độ")]
        [StringLength(500)]
        public string DisplayName { get; set; }

        // Mối quan hệ với LevelTypeModel
        public virtual LevelTypeModel? LevelType { get; set; } // Thêm thuộc tính quan hệ với LevelTypeModel
    }
}
