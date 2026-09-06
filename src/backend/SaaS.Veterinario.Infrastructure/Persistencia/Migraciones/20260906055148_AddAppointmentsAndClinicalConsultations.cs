using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AddAppointmentsAndClinicalConsultations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "citas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    veterinario_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_hora_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_hora_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_citas", x => x.id);
                    table.ForeignKey(
                        name: "FK_citas_clientes_veterinaria_cliente_veterinaria_id",
                        column: x => x.cliente_veterinaria_id,
                        principalTable: "clientes_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_mascotas_veterinaria_mascota_veterinaria_id",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_usuarios_actualizado_por_usuario_id",
                        column: x => x.actualizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_usuarios_veterinario_usuario_id",
                        column: x => x.veterinario_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consultas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cita_id = table.Column<Guid>(type: "uuid", nullable: true),
                    veterinario_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    motivo_consulta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    peso = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    temperatura = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    frecuencia_cardiaca = table.Column<int>(type: "integer", nullable: true),
                    observaciones_clinicas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    indicaciones_propietario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    proxima_fecha_control = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_proximo_control = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_finalizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    finalizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_anulacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    anulado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultas", x => x.id);
                    table.ForeignKey(
                        name: "FK_consultas_citas_cita_id",
                        column: x => x.cita_id,
                        principalTable: "citas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_mascotas_veterinaria_mascota_veterinaria_id",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_actualizado_por_usuario_id",
                        column: x => x.actualizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_anulado_por_usuario_id",
                        column: x => x.anulado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_finalizado_por_usuario_id",
                        column: x => x.finalizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_veterinario_usuario_id",
                        column: x => x.veterinario_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consultas_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "diagnosticos_consulta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consulta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    es_principal = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnosticos_consulta", x => x.id);
                    table.ForeignKey(
                        name: "FK_diagnosticos_consulta_consultas_consulta_id",
                        column: x => x.consulta_id,
                        principalTable: "consultas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000016"), "citas.ver", null, "Ver citas" },
                    { new Guid("22222222-0000-0000-0000-000000000017"), "citas.crear", null, "Crear citas" },
                    { new Guid("22222222-0000-0000-0000-000000000018"), "citas.editar", null, "Editar citas" },
                    { new Guid("22222222-0000-0000-0000-000000000019"), "citas.cancelar", null, "Cancelar citas" },
                    { new Guid("22222222-0000-0000-0000-000000000020"), "citas.cambiar_estado", null, "Cambiar estado de citas" },
                    { new Guid("22222222-0000-0000-0000-000000000021"), "consultas.ver", null, "Ver consultas" },
                    { new Guid("22222222-0000-0000-0000-000000000022"), "consultas.crear", null, "Crear consultas" },
                    { new Guid("22222222-0000-0000-0000-000000000023"), "consultas.editar_borrador", null, "Editar consultas en borrador" },
                    { new Guid("22222222-0000-0000-0000-000000000024"), "consultas.finalizar", null, "Finalizar consultas" },
                    { new Guid("22222222-0000-0000-0000-000000000025"), "consultas.anular", null, "Anular consultas" },
                    { new Guid("22222222-0000-0000-0000-000000000026"), "diagnosticos.ver", null, "Ver diagnósticos" },
                    { new Guid("22222222-0000-0000-0000-000000000027"), "diagnosticos.crear", null, "Crear diagnósticos" },
                    { new Guid("22222222-0000-0000-0000-000000000028"), "diagnosticos.editar", null, "Editar diagnósticos" },
                    { new Guid("22222222-0000-0000-0000-000000000029"), "historial.ver", null, "Ver historial clínico" }
                });

            migrationBuilder.InsertData(
                table: "roles_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000017"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000018"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000019"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000026"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000025"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000026"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000027"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000028"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000017"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000018"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000019"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_citas_actualizado_por_usuario_id",
                table: "citas",
                column: "actualizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_citas_cliente_veterinaria_id",
                table: "citas",
                column: "cliente_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_citas_creado_por_usuario_id",
                table: "citas",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_citas_veterinario_usuario_id",
                table: "citas",
                column: "veterinario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_citas_mascota_veterinaria_fecha_inicio",
                table: "citas",
                columns: new[] { "mascota_veterinaria_id", "fecha_hora_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_citas_veterinaria_fecha_inicio",
                table: "citas",
                columns: new[] { "veterinaria_id", "fecha_hora_inicio" });

            migrationBuilder.CreateIndex(
                name: "ix_citas_veterinaria_veterinario_fecha_inicio",
                table: "citas",
                columns: new[] { "veterinaria_id", "veterinario_usuario_id", "fecha_hora_inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_consultas_actualizado_por_usuario_id",
                table: "consultas",
                column: "actualizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_anulado_por_usuario_id",
                table: "consultas",
                column: "anulado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_cita_id",
                table: "consultas",
                column: "cita_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_creado_por_usuario_id",
                table: "consultas",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_finalizado_por_usuario_id",
                table: "consultas",
                column: "finalizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_mascota_veterinaria_id",
                table: "consultas",
                column: "mascota_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_veterinario_usuario_id",
                table: "consultas",
                column: "veterinario_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_consultas_veterinaria_mascota_veterinaria_fecha",
                table: "consultas",
                columns: new[] { "veterinaria_id", "mascota_veterinaria_id", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ix_consultas_veterinaria_veterinario_fecha",
                table: "consultas",
                columns: new[] { "veterinaria_id", "veterinario_usuario_id", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ux_diagnosticos_consulta_principal",
                table: "diagnosticos_consulta",
                column: "consulta_id",
                unique: true,
                filter: "es_principal = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diagnosticos_consulta");

            migrationBuilder.DropTable(
                name: "consultas");

            migrationBuilder.DropTable(
                name: "citas");

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000017"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000018"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000019"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000026"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000025"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000026"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000027"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000028"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000016"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000017"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000018"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000019"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000029"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000029"));
        }
    }
}
