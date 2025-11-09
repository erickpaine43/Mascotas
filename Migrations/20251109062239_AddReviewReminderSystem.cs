using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewReminderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnimalId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: true),
                    AnimalId = table.Column<int>(type: "int", nullable: true),
                    TipoItem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreItem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrimerRecordatorioEnviado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SegundoRecordatorioEnviado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResenaCompletada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReminders_Animales_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewReminders_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewReminders_Ordenes_OrdenId",
                        column: x => x.OrdenId,
                        principalTable: "Ordenes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewReminders_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AnimalId",
                table: "Reviews",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_AnimalId",
                table: "ReviewReminders",
                column: "AnimalId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_ClienteId",
                table: "ReviewReminders",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_OrdenId",
                table: "ReviewReminders",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReminders_ProductoId",
                table: "ReviewReminders",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Animales_AnimalId",
                table: "Reviews",
                column: "AnimalId",
                principalTable: "Animales",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Animales_AnimalId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "ReviewReminders");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AnimalId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "AnimalId",
                table: "Reviews");
        }
    }
}
