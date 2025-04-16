using System.ComponentModel.DataAnnotations;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Models
{
    public class TeamMemberModel
    {
        [Key]
        public int MemberId { get; set; }
        public int UserId { get; set; }
        public virtual UserModel? User { get; set; }

        public int TeamId { get; set; }
        public virtual TranslationTeamModel? TranslationTeam { get; set; }
        public bool IsLeader { get; set; } = false; // Trưởng nhóm hay không

        [DataType(DataType.DateTime)]
        public DateTime JoinedAt { get; set; } = DateTime.Now; // Ngày tham gia nhóm
    }
}
