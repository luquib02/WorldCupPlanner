using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class EliminarShadowFKsIncidencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidencias_Partidos_PartidoId1",
                table: "Incidencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidencias_Partidos_PartidoId2",
                table: "Incidencias");

            migrationBuilder.DropIndex(
                name: "IX_Incidencias_PartidoId1",
                table: "Incidencias");

            migrationBuilder.DropIndex(
                name: "IX_Incidencias_PartidoId2",
                table: "Incidencias");

            migrationBuilder.DropColumn(
                name: "PartidoId1",
                table: "Incidencias");

            migrationBuilder.DropColumn(
                name: "PartidoId2",
                table: "Incidencias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartidoId1",
                table: "Incidencias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartidoId2",
                table: "Incidencias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_PartidoId1",
                table: "Incidencias",
                column: "PartidoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_PartidoId2",
                table: "Incidencias",
                column: "PartidoId2");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidencias_Partidos_PartidoId1",
                table: "Incidencias",
                column: "PartidoId1",
                principalTable: "Partidos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidencias_Partidos_PartidoId2",
                table: "Incidencias",
                column: "PartidoId2",
                principalTable: "Partidos",
                principalColumn: "Id");
        }
    }
}
