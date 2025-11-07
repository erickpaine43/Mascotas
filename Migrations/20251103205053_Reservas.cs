using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class Reservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiracionReserva",
                table: "Productos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockDisponible",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockReservado",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockTotal",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockVendido",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracionReserva",
                table: "Ordenes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ReservaActiva",
                table: "Ordenes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiracionReserva",
                table: "Animales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Reservado",
                table: "Animales",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiracionReserva",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockDisponible",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockReservado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockTotal",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "StockVendido",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaExpiracionReserva",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ReservaActiva",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ExpiracionReserva",
                table: "Animales");

            migrationBuilder.DropColumn(
                name: "Reservado",
                table: "Animales");
        }
    }
}
