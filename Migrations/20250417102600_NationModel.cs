using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Migrations
{
    /// <inheritdoc />
    public partial class NationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NationId",
                table: "Comics",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Nations",
                columns: table => new
                {
                    NationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nations", x => x.NationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comics_NationId",
                table: "Comics",
                column: "NationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comics_Nations_NationId",
                table: "Comics",
                column: "NationId",
                principalTable: "Nations",
                principalColumn: "NationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comics_Nations_NationId",
                table: "Comics");

            migrationBuilder.DropTable(
                name: "Nations");

            migrationBuilder.DropIndex(
                name: "IX_Comics_NationId",
                table: "Comics");

            migrationBuilder.DropColumn(
                name: "NationId",
                table: "Comics");
        }
    }
}
