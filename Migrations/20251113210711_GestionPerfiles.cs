using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class GestionPerfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerfilesUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Direcciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Calle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Direcciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Direcciones_PerfilesUsuarios_PerfilUsuarioId",
                        column: x => x.PerfilUsuarioId,
                        principalTable: "PerfilesUsuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MascotasClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Especie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Raza = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Peso = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Esterilizado = table.Column<bool>(type: "bit", nullable: false),
                    NotasMedicas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Alergias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MascotasClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MascotasClientes_PerfilesUsuarios_PerfilUsuarioId",
                        column: x => x.PerfilUsuarioId,
                        principalTable: "PerfilesUsuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilUsuarioId = table.Column<int>(type: "int", nullable: false),
                    RecibirNotificacionesEmail = table.Column<bool>(type: "bit", nullable: false),
                    RecibirNotificacionesSMS = table.Column<bool>(type: "bit", nullable: false),
                    RecibirOfertasEspeciales = table.Column<bool>(type: "bit", nullable: false),
                    CategoriaFavorita = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Idioma = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferenciasUsuarios_PerfilesUsuarios_PerfilUsuarioId",
                        column: x => x.PerfilUsuarioId,
                        principalTable: "PerfilesUsuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_PerfilUsuarioId",
                table: "Direcciones",
                column: "PerfilUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_PerfilUsuarioId_EsPrincipal",
                table: "Direcciones",
                columns: new[] { "PerfilUsuarioId", "EsPrincipal" });

            migrationBuilder.CreateIndex(
                name: "IX_MascotasClientes_Especie",
                table: "MascotasClientes",
                column: "Especie");

            migrationBuilder.CreateIndex(
                name: "IX_MascotasClientes_PerfilUsuarioId",
                table: "MascotasClientes",
                column: "PerfilUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesUsuarios_UsuarioId",
                table: "PerfilesUsuarios",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreferenciasUsuarios_PerfilUsuarioId",
                table: "PreferenciasUsuarios",
                column: "PerfilUsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Direcciones");

            migrationBuilder.DropTable(
                name: "MascotasClientes");

            migrationBuilder.DropTable(
                name: "PreferenciasUsuarios");

            migrationBuilder.DropTable(
                name: "PerfilesUsuarios");
        }
    }
}
