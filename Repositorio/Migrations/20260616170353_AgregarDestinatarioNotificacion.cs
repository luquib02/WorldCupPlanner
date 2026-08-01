using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositorio.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDestinatarioNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las notificaciones previas eran globales (sin destinatario) y no encajan en el
            // nuevo modelo por periodista; se eliminan para poder crear la FK a Usuarios.
            migrationBuilder.Sql("DELETE FROM Notificaciones");

            migrationBuilder.AddColumn<int>(
                name: "DestinatarioId",
                table: "Notificaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_DestinatarioId",
                table: "Notificaciones",
                column: "DestinatarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificaciones_Usuarios_DestinatarioId",
                table: "Notificaciones",
                column: "DestinatarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificaciones_Usuarios_DestinatarioId",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_DestinatarioId",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "DestinatarioId",
                table: "Notificaciones");
        }
    }
}
