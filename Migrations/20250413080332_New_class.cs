using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Migrations
{
    /// <inheritdoc />
    public partial class New_class : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentFrameId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LevelTypeId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Comics",
                columns: table => new
                {
                    ComicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComicName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComicAuthor = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ComicImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComicDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comics", x => x.ComicId);
                    table.ForeignKey(
                        name: "FK_Comics_TranslationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "TranslationTeams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Frames",
                columns: table => new
                {
                    FrameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrameName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FrameImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFree = table.Column<bool>(type: "bit", nullable: false),
                    Coin = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frames", x => x.FrameId);
                });

            migrationBuilder.CreateTable(
                name: "LevelTypes",
                columns: table => new
                {
                    LevelTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelTypes", x => x.LevelTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComicModelComicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.GenreId);
                    table.ForeignKey(
                        name: "FK_Genres_Comics_ComicModelComicId",
                        column: x => x.ComicModelComicId,
                        principalTable: "Comics",
                        principalColumn: "ComicId");
                });

            migrationBuilder.CreateTable(
                name: "UserFrames",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FrameId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFrames", x => new { x.UserId, x.FrameId });
                    table.ForeignKey(
                        name: "FK_UserFrames_Frames_FrameId",
                        column: x => x.FrameId,
                        principalTable: "Frames",
                        principalColumn: "FrameId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFrames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevelMappings",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelTypeId = table.Column<int>(type: "int", nullable: true),
                    PointsRequired = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelMappings", x => x.LevelId);
                    table.ForeignKey(
                        name: "FK_LevelMappings_LevelTypes_LevelTypeId",
                        column: x => x.LevelTypeId,
                        principalTable: "LevelTypes",
                        principalColumn: "LevelTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComicGenres",
                columns: table => new
                {
                    ComicId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false),
                    ComicGenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComicGenres", x => new { x.ComicId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_Comic_ComicGenres",
                        column: x => x.ComicId,
                        principalTable: "Comics",
                        principalColumn: "ComicId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Genre_ComicGenres",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "GenreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CurrentFrameId",
                table: "Users",
                column: "CurrentFrameId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LevelTypeId",
                table: "Users",
                column: "LevelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComicGenres_GenreId",
                table: "ComicGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Comics_TeamId",
                table: "Comics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_ComicModelComicId",
                table: "Genres",
                column: "ComicModelComicId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelMappings_LevelTypeId",
                table: "LevelMappings",
                column: "LevelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFrames_FrameId",
                table: "UserFrames",
                column: "FrameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Frames_CurrentFrameId",
                table: "Users",
                column: "CurrentFrameId",
                principalTable: "Frames",
                principalColumn: "FrameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_LevelTypes_LevelTypeId",
                table: "Users",
                column: "LevelTypeId",
                principalTable: "LevelTypes",
                principalColumn: "LevelTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Frames_CurrentFrameId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_LevelTypes_LevelTypeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ComicGenres");

            migrationBuilder.DropTable(
                name: "LevelMappings");

            migrationBuilder.DropTable(
                name: "UserFrames");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "LevelTypes");

            migrationBuilder.DropTable(
                name: "Frames");

            migrationBuilder.DropTable(
                name: "Comics");

            migrationBuilder.DropIndex(
                name: "IX_Users_CurrentFrameId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LevelTypeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CurrentFrameId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LevelTypeId",
                table: "Users");
        }
    }
}
