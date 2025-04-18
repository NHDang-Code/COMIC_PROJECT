using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Data
{
    public class K21CNT2_NguyenHaiDang_2110900067_DATNContext : DbContext
    {
        public K21CNT2_NguyenHaiDang_2110900067_DATNContext(DbContextOptions<K21CNT2_NguyenHaiDang_2110900067_DATNContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<RoleModel> Roles { get; set; }

        public DbSet<TranslationTeamModel> TranslationTeams { get; set; }

        public DbSet<TeamMemberModel> TeamMembers { get; set; }

        public DbSet<FrameModel> Frames { get; set; }

        public DbSet<UserFrameModel> UserFrames { get; set; }

        public DbSet<LevelTypeModel> LevelTypes { get; set; }

        public DbSet<LevelMappingModel> LevelMappings { get; set; }

        public DbSet<ComicModel> Comics { get; set; }

        public DbSet<ComicGenreModel> ComicGenres { get; set; }

        public DbSet<GenreModel> Genres { get; set; }

        public DbSet<ChapterModel> Chapters { get; set; }
        public DbSet<ChapterImageModel> ChapterImages { get; set; }

        public DbSet<NationModel> Nations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ giữa User và Role
            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa UserModel và TeamMemberModel (Người dùng tham gia nhóm dịch)
            modelBuilder.Entity<TeamMemberModel>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.Members)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Quan hệ giữa TeamMemberModel và TranslationTeamModel 
            modelBuilder.Entity<TeamMemberModel>()
                .HasOne(tm => tm.TranslationTeam)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa User và LevelType
            modelBuilder.Entity<UserModel>()
               .HasOne(u => u.LevelType)
               .WithMany(lt => lt.Users)
               .HasForeignKey(u => u.LevelTypeId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LevelMappingModel>()
                 .HasOne(lm => lm.LevelType)
                 .WithMany(lt => lt.LevelMappings)  // Định nghĩa quan hệ một-nhiều
                 .HasForeignKey(lm => lm.LevelTypeId)
                 .OnDelete(DeleteBehavior.Restrict);  // Ngăn chặn xóa nếu còn dữ liệu liên quan

            modelBuilder.Entity<UserFrameModel>()
              .HasKey(uf => new { uf.UserId, uf.FrameId });

            modelBuilder.Entity<UserFrameModel>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFrames)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFrameModel>()
                .HasOne(uf => uf.Frame)
                .WithMany(f => f.UserFrames)
                .HasForeignKey(uf => uf.FrameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Comic và Genre qua ComicGenre
            modelBuilder.Entity<ComicGenreModel>()
                .HasKey(cg => new { cg.ComicId, cg.GenreId });

            modelBuilder.Entity<ComicGenreModel>()
                .HasOne(cg => cg.Comic)
                .WithMany(c => c.ComicGenres)
                .HasForeignKey(cg => cg.ComicId)
                .HasConstraintName("FK_Comic_ComicGenres") // Tùy chỉnh tên khóa ngoại
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComicGenreModel>()
                .HasOne(cg => cg.Genre)
                .WithMany(g => g.ComicGenres)
                .HasForeignKey(cg => cg.GenreId)
                .HasConstraintName("FK_Genre_ComicGenres") // Tùy chỉnh tên khóa ngoại
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Comic và TranslationTeam (mỗi truyện thuộc về một nhóm dịch)
            modelBuilder.Entity<ComicModel>()
                .HasOne(c => c.TranslationTeam)
                .WithMany(t => t.Comics)
                .HasForeignKey(c => c.TeamId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu nhóm dịch bị xóa, truyện vẫn tồn tại nhưng không có nhóm dịch

            modelBuilder.Entity<ComicModel>()
                .HasOne(c => c.Nation)
                .WithMany(n => n.Comics)
                .HasForeignKey(c => c.NationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ giữa Chapter và Comic
            modelBuilder.Entity<ChapterModel>()
                .HasOne(c => c.Comic)
                .WithMany(cm => cm.Chapters)
                .HasForeignKey(c => c.ComicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ giữa Chapter và ChapterImage
            modelBuilder.Entity<ChapterImageModel>()
                .HasOne(ci => ci.Chapter)
                .WithMany(c => c.ChapterImages)
                .HasForeignKey(ci => ci.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
