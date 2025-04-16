using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class LevelTypeModel
    {
        [Key]
        public int LevelTypeId { get; set; }

        [Required(ErrorMessage = "Hãy nhập tên cho level")]
        [StringLength(500)]
        public string TypeName { get; set; }
        //1 LevelTyPe có nhiều User
        public virtual ICollection<UserModel> Users { get; set; } = new List<UserModel>();

        public virtual ICollection<LevelMappingModel> LevelMappings { get; set; } = new List<LevelMappingModel>();
    }
}
