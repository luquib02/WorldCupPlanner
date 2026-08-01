using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConstraintsYAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsuarioRoles_UsuarioId",
                table: "UsuarioRoles");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_FixtureId",
                table: "Grupos");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioEmail",
                table: "Logs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_UsuarioId_Rol",
                table: "UsuarioRoles",
                columns: new[] { "UsuarioId", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_PartidoId",
                table: "Notificaciones",
                column: "PartidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_FixtureId_Etiqueta",
                table: "Grupos",
                columns: new[] { "FixtureId", "Etiqueta" },
                unique: true,
                filter: "[FixtureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoEquipos_EquiposId",
                table: "GrupoEquipos",
                column: "EquiposId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notificaciones_Partidos_PartidoId",
                table: "Notificaciones",
                column: "PartidoId",
                principalTable: "Partidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificaciones_Partidos_PartidoId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioRoles_UsuarioId_Rol",
                table: "UsuarioRoles");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_PartidoId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_FixtureId_Etiqueta",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_GrupoEquipos_EquiposId",
                table: "GrupoEquipos");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioEmail",
                table: "Logs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_UsuarioId",
                table: "UsuarioRoles",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_FixtureId",
                table: "Grupos",
                column: "FixtureId");
        }
    }
}
