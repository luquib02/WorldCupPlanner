using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFixtureGrupoJornada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixtureEliminatoriasId",
                table: "Partidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JornadaId",
                table: "Partidos",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mensaje",
                table: "Notificaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Fixtures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CrucesGenerados = table.Column<bool>(type: "bit", nullable: false),
                    SemillaUtilizada = table.Column<int>(type: "int", nullable: false),
                    NombreMotorSimulacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fixtures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Etiqueta = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FixtureId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_Fixtures_FixtureId",
                        column: x => x.FixtureId,
                        principalTable: "Fixtures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoEquipos",
                columns: table => new
                {
                    EquiposId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoEquipos", x => new { x.EquiposId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_GrupoEquipos_Equipos_EquiposId",
                        column: x => x.EquiposId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoEquipos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jornadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jornadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jornadas_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_FixtureEliminatoriasId",
                table: "Partidos",
                column: "FixtureEliminatoriasId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_JornadaId",
                table: "Partidos",
                column: "JornadaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoEquipos_GrupoId",
                table: "GrupoEquipos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_FixtureId",
                table: "Grupos",
                column: "FixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_Jornadas_GrupoId",
                table: "Jornadas",
                column: "GrupoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_Fixtures_FixtureEliminatoriasId",
                table: "Partidos",
                column: "FixtureEliminatoriasId",
                principalTable: "Fixtures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Partidos_Jornadas_JornadaId",
                table: "Partidos",
                column: "JornadaId",
                principalTable: "Jornadas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_Fixtures_FixtureEliminatoriasId",
                table: "Partidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Partidos_Jornadas_JornadaId",
                table: "Partidos");

            migrationBuilder.DropTable(
                name: "GrupoEquipos");

            migrationBuilder.DropTable(
                name: "Jornadas");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Fixtures");

            migrationBuilder.DropIndex(
                name: "IX_Partidos_FixtureEliminatoriasId",
                table: "Partidos");

            migrationBuilder.DropIndex(
                name: "IX_Partidos_JornadaId",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "FixtureEliminatoriasId",
                table: "Partidos");

            migrationBuilder.DropColumn(
                name: "JornadaId",
                table: "Partidos");

            migrationBuilder.AlterColumn<string>(
                name: "Mensaje",
                table: "Notificaciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
