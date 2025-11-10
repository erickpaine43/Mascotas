using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mascotas.Migrations
{
    /// <inheritdoc />
    public partial class Notificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertaPrecios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    PrecioObjetivo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Notificado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActivacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertaPrecios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertaPrecios_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusquedaGuardadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParametrosBusqueda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VecesUtilizada = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoUso = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusquedaGuardadas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiltroGuardados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParametrosBusqueda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsFavorito = table.Column<bool>(type: "bit", nullable: false),
                    VecesUtilizado = table.Column<int>(type: "int", nullable: false),
                    CategoriaFiltro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoUso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonitorearNuevosProductos = table.Column<bool>(type: "bit", nullable: false),
                    MonitorearBajasPrecio = table.Column<bool>(type: "bit", nullable: false),
                    MonitorearStock = table.Column<bool>(type: "bit", nullable: false),
                    PorcentajeBajaMinima = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FechaUltimaRevision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalNotificacionesEnviadas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiltroGuardados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: true),
                    FiltroGuardadoId = table.Column<int>(type: "int", nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    Enviada = table.Column<bool>(type: "bit", nullable: false),
                    EnviarEmail = table.Column<bool>(type: "bit", nullable: false),
                    EnviarPush = table.Column<bool>(type: "bit", nullable: false),
                    MostrarEnWeb = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_FiltroGuardados_FiltroGuardadoId",
                        column: x => x.FiltroGuardadoId,
                        principalTable: "FiltroGuardados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResultadoCambios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiltroGuardadoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    TipoCambio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrecioAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PrecioNuevo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaDetectado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoCambios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadoCambios_FiltroGuardados_FiltroGuardadoId",
                        column: x => x.FiltroGuardadoId,
                        principalTable: "FiltroGuardados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultadoCambios_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertaPrecios_ProductoId",
                table: "AlertaPrecios",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaPrecios_UsuarioId_Activa",
                table: "AlertaPrecios",
                columns: new[] { "UsuarioId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_FiltroGuardados_UsuarioId",
                table: "FiltroGuardados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FiltroGuardados_UsuarioId_EsFavorito",
                table: "FiltroGuardados",
                columns: new[] { "UsuarioId", "EsFavorito" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_FiltroGuardadoId",
                table: "Notificaciones",
                column: "FiltroGuardadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ProductoId",
                table: "Notificaciones",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_Leida",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoCambios_FechaDetectado",
                table: "ResultadoCambios",
                column: "FechaDetectado");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoCambios_FiltroGuardadoId",
                table: "ResultadoCambios",
                column: "FiltroGuardadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadoCambios_ProductoId",
                table: "ResultadoCambios",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertaPrecios");

            migrationBuilder.DropTable(
                name: "BusquedaGuardadas");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ResultadoCambios");

            migrationBuilder.DropTable(
                name: "FiltroGuardados");
        }
    }
}
