using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class busquedasAvanzadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasEntrega",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dimensiones",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DuracionTratamiento",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EntregaRapida",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnvioGratis",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EspecieDestinada",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtapaVida",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NecesidadesEspeciales",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Preorden",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RazaDestinada",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RetiroEnTienda",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoTratamiento",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DiasEntrega",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Dimensiones",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DuracionTratamiento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EntregaRapida",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EnvioGratis",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EspecieDestinada",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EtapaVida",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NecesidadesEspeciales",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Preorden",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "RazaDestinada",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "RetiroEnTienda",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TipoTratamiento",
                table: "Productos");
        }
    }
}
