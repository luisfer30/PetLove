using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InitialIdentityAndTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permisos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    correo_verificado = table.Column<bool>(type: "boolean", nullable: false),
                    telefono_verificado = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "veterinarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_publico = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre_comercial = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    razon_social = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ruc = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pais = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zona_horaria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_eliminacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veterinarias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles_permisos",
                columns: table => new
                {
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permiso_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_permisos", x => new { x.rol_id, x.permiso_id });
                    table.ForeignKey(
                        name: "FK_roles_permisos_permisos_permiso_id",
                        column: x => x.permiso_id,
                        principalTable: "permisos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roles_permisos_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "membresias_veterinaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_ingreso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_finalizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membresias_veterinaria", x => x.id);
                    table.ForeignKey(
                        name: "FK_membresias_veterinaria_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membresias_veterinaria_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membresias_roles",
                columns: table => new
                {
                    membresia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membresias_roles", x => new { x.membresia_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_membresias_roles_membresias_veterinaria_membresia_id",
                        column: x => x.membresia_id,
                        principalTable: "membresias_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_membresias_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), "veterinaria.ver", null, "Ver veterinaria" },
                    { new Guid("22222222-0000-0000-0000-000000000002"), "veterinaria.editar", null, "Editar veterinaria" },
                    { new Guid("22222222-0000-0000-0000-000000000003"), "personal.ver", null, "Ver personal" },
                    { new Guid("22222222-0000-0000-0000-000000000004"), "personal.invitar", null, "Invitar personal" },
                    { new Guid("22222222-0000-0000-0000-000000000005"), "personal.editar", null, "Editar personal" },
                    { new Guid("22222222-0000-0000-0000-000000000006"), "personal.finalizar", null, "Finalizar personal" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "codigo", "descripcion", "estado", "nombre" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "ADMINISTRADOR_VETERINARIA", null, "ACTIVO", "Administrador de veterinaria" },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "VETERINARIO", null, "ACTIVO", "Veterinario" },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "ASISTENTE", null, "ACTIVO", "Asistente" }
                });

            migrationBuilder.InsertData(
                table: "roles_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_membresias_roles_rol_id",
                table: "membresias_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "ix_membresias_veterinaria_usuario_id",
                table: "membresias_veterinaria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_membresias_veterinaria_veterinaria_id",
                table: "membresias_veterinaria",
                column: "veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "ux_membresias_veterinaria_usuario_activa",
                table: "membresias_veterinaria",
                column: "usuario_id",
                unique: true,
                filter: "estado = 'ACTIVA'");

            migrationBuilder.CreateIndex(
                name: "ix_permisos_codigo_unico",
                table: "permisos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_codigo_unico",
                table: "roles",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_permisos_permiso_id",
                table: "roles_permisos",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_correo_unico",
                table: "usuarios",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_veterinarias_codigo_publico_unico",
                table: "veterinarias",
                column: "codigo_publico",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membresias_roles");

            migrationBuilder.DropTable(
                name: "roles_permisos");

            migrationBuilder.DropTable(
                name: "membresias_veterinaria");

            migrationBuilder.DropTable(
                name: "permisos");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "veterinarias");
        }
    }
}
