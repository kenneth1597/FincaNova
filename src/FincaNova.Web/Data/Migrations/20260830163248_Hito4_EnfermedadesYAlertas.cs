using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FincaNova.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Hito4_EnfermedadesYAlertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductoTexto",
                table: "Tratamientos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenciaSecundariaId",
                table: "Alertas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductoTexto",
                table: "Tratamientos");

            migrationBuilder.DropColumn(
                name: "ReferenciaSecundariaId",
                table: "Alertas");
        }
    }
}
