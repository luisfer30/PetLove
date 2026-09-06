using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AddContextoVeterinarioAndInvitaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitaciones_personal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_expiracion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_aceptacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitaciones_personal", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitaciones_personal_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitaciones_personal_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invitaciones_personal_roles",
                columns: table => new
                {
                    invitacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitaciones_personal_roles", x => new { x.invitacion_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_invitaciones_personal_roles_invitaciones_personal_invitacio~",
                        column: x => x.invitacion_id,
                        principalTable: "invitaciones_personal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitaciones_personal_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitaciones_personal_creado_por_usuario_id",
                table: "invitaciones_personal",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitaciones_personal_token_hash_unico",
                table: "invitaciones_personal",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitaciones_personal_veterinaria_id",
                table: "invitaciones_personal",
                column: "veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "ux_invitaciones_personal_veterinaria_correo_pendiente",
                table: "invitaciones_personal",
                columns: new[] { "veterinaria_id", "correo" },
                unique: true,
                filter: "estado = 'PENDIENTE'");

            migrationBuilder.CreateIndex(
                name: "IX_invitaciones_personal_roles_rol_id",
                table: "invitaciones_personal_roles",
                column: "rol_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitaciones_personal_roles");

            migrationBuilder.DropTable(
                name: "invitaciones_personal");
        }
    }
}
