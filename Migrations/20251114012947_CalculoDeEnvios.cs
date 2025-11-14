using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class CalculoDeEnvios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Alto",
                table: "Productos",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Ancho",
                table: "Productos",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EsFragil",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Largo",
                table: "Productos",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Peso",
                table: "Productos",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoEnvio",
                table: "Ordenes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiasEntregaEstimados",
                table: "Ordenes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DireccionEnvioId",
                table: "Ordenes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEstimadaEntrega",
                table: "Ordenes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetodoEnvioId",
                table: "Ordenes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetodoEnvios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetiroEnLocal = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoEnvios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonaEnvios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RangoCodigosPostales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarifaBaseEstandar = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TarifaBaseExpress = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CostoPorKiloExtra = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RecargoFragil = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiasEstandar = table.Column<int>(type: "int", nullable: false),
                    DiasExpress = table.Column<int>(type: "int", nullable: false),
                    MontoMinimoEnvioGratis = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonaEnvios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_DireccionEnvioId",
                table: "Ordenes",
                column: "DireccionEnvioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_MetodoEnvioId",
                table: "Ordenes",
                column: "MetodoEnvioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ordenes_Direcciones_DireccionEnvioId",
                table: "Ordenes",
                column: "DireccionEnvioId",
                principalTable: "Direcciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ordenes_MetodoEnvios_MetodoEnvioId",
                table: "Ordenes",
                column: "MetodoEnvioId",
                principalTable: "MetodoEnvios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ordenes_Direcciones_DireccionEnvioId",
                table: "Ordenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ordenes_MetodoEnvios_MetodoEnvioId",
                table: "Ordenes");

            migrationBuilder.DropTable(
                name: "MetodoEnvios");

            migrationBuilder.DropTable(
                name: "ZonaEnvios");

            migrationBuilder.DropIndex(
                name: "IX_Ordenes_DireccionEnvioId",
                table: "Ordenes");

            migrationBuilder.DropIndex(
                name: "IX_Ordenes_MetodoEnvioId",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "Alto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Ancho",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EsFragil",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Largo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CostoEnvio",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "DiasEntregaEstimados",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "DireccionEnvioId",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "FechaEstimadaEntrega",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "MetodoEnvioId",
                table: "Ordenes");
        }
    }
}
