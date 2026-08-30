using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FincaNova.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Hito2_LoteEstadoCambiadoPor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoCambiadoPor",
                table: "Lotes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoCambiadoPor",
                table: "Lotes");
        }
    }
}
