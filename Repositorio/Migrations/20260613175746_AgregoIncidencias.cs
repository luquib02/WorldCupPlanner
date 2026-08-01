using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class AgregoIncidencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Incidencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinutoJuego = table.Column<int>(type: "int", nullable: false),
                    EsLocal = table.Column<bool>(type: "bit", nullable: false),
                    PartidoId = table.Column<int>(type: "int", nullable: true),
                    PartidoId1 = table.Column<int>(type: "int", nullable: true),
                    PartidoId2 = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidencias_Partidos_PartidoId",
                        column: x => x.PartidoId,
                        principalTable: "Partidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Incidencias_Partidos_PartidoId1",
                        column: x => x.PartidoId1,
                        principalTable: "Partidos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Incidencias_Partidos_PartidoId2",
                        column: x => x.PartidoId2,
                        principalTable: "Partidos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_PartidoId",
                table: "Incidencias",
                column: "PartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_PartidoId1",
                table: "Incidencias",
                column: "PartidoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_PartidoId2",
                table: "Incidencias",
                column: "PartidoId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidencias");
        }
    }
}
