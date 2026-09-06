using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AddTreatmentsAndFollowUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "planes_tratamiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consulta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fecha_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_fin_estimada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    suspendido_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_suspension = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_suspension = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    completado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_completado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancelado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_cancelacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_cancelacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes_tratamiento", x => x.id);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_consultas_consulta_id",
                        column: x => x.consulta_id,
                        principalTable: "consultas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_mascotas_veterinaria_mascota_veterinaria~",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_usuarios_actualizado_por_usuario_id",
                        column: x => x.actualizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_usuarios_cancelado_por_usuario_id",
                        column: x => x.cancelado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_usuarios_completado_por_usuario_id",
                        column: x => x.completado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_usuarios_suspendido_por_usuario_id",
                        column: x => x.suspendido_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planes_tratamiento_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "items_tratamiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_tratamiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    dosis_cantidad = table.Column<decimal>(type: "numeric(8,3)", precision: 8, scale: 3, nullable: true),
                    dosis_unidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    via_administracion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    frecuencia_tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    intervalo_horas = table.Column<int>(type: "integer", nullable: true),
                    veces_por_dia = table.Column<int>(type: "integer", nullable: true),
                    fecha_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    duracion_dias = table.Column<int>(type: "integer", nullable: true),
                    instrucciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_tratamiento", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_tratamiento_planes_tratamiento_plan_tratamiento_id",
                        column: x => x.plan_tratamiento_id,
                        principalTable: "planes_tratamiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "seguimientos_clinicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consulta_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plan_tratamiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_objetivo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fecha_agendada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cita_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_realizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    es_generado_desde_proximo_control = table.Column<bool>(type: "boolean", nullable: false),
                    creado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguimientos_clinicos", x => x.id);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_citas_cita_id",
                        column: x => x.cita_id,
                        principalTable: "citas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_consultas_consulta_origen_id",
                        column: x => x.consulta_origen_id,
                        principalTable: "consultas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_mascotas_veterinaria_mascota_veterina~",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_planes_tratamiento_plan_tratamiento_id",
                        column: x => x.plan_tratamiento_id,
                        principalTable: "planes_tratamiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_usuarios_actualizado_por_usuario_id",
                        column: x => x.actualizado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_usuarios_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seguimientos_clinicos_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horarios_items_tratamiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_tratamiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hora = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios_items_tratamiento", x => x.id);
                    table.ForeignKey(
                        name: "FK_horarios_items_tratamiento_items_tratamiento_item_tratamien~",
                        column: x => x.item_tratamiento_id,
                        principalTable: "items_tratamiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programaciones_tratamiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_tratamiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_tratamiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_hora_programada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_realizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    registrado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacion_realizacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programaciones_tratamiento", x => x.id);
                    table.ForeignKey(
                        name: "FK_programaciones_tratamiento_items_tratamiento_item_tratamien~",
                        column: x => x.item_tratamiento_id,
                        principalTable: "items_tratamiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programaciones_tratamiento_mascotas_veterinaria_mascota_vet~",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programaciones_tratamiento_planes_tratamiento_plan_tratamie~",
                        column: x => x.plan_tratamiento_id,
                        principalTable: "planes_tratamiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programaciones_tratamiento_usuarios_registrado_por_usuario_~",
                        column: x => x.registrado_por_usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programaciones_tratamiento_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000031"), "tratamientos.ver", null, "Ver tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000032"), "tratamientos.crear", null, "Crear tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000033"), "tratamientos.editar", null, "Editar tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000034"), "tratamientos.activar", null, "Activar tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000035"), "tratamientos.suspender", null, "Suspender tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000036"), "tratamientos.completar", null, "Completar tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000037"), "tratamientos.cancelar", null, "Cancelar tratamientos" },
                    { new Guid("22222222-0000-0000-0000-000000000038"), "programaciones.ver", null, "Ver programaciones" },
                    { new Guid("22222222-0000-0000-0000-000000000039"), "programaciones.registrar_realizacion", null, "Registrar realización de programaciones" },
                    { new Guid("22222222-0000-0000-0000-000000000040"), "programaciones.registrar_omision", null, "Registrar omisión de programaciones" },
                    { new Guid("22222222-0000-0000-0000-000000000041"), "seguimientos.ver", null, "Ver seguimientos" },
                    { new Guid("22222222-0000-0000-0000-000000000042"), "seguimientos.crear", null, "Crear seguimientos" },
                    { new Guid("22222222-0000-0000-0000-000000000043"), "seguimientos.editar", null, "Editar seguimientos" },
                    { new Guid("22222222-0000-0000-0000-000000000044"), "seguimientos.realizar", null, "Realizar seguimientos" },
                    { new Guid("22222222-0000-0000-0000-000000000045"), "seguimientos.cancelar", null, "Cancelar seguimientos" }
                });

            migrationBuilder.InsertData(
                table: "roles_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000035"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000036"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000037"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000039"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000043"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000044"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000045"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000039"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "ux_horarios_items_tratamiento_item_hora",
                table: "horarios_items_tratamiento",
                columns: new[] { "item_tratamiento_id", "hora" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_items_tratamiento_plan_tratamiento_id",
                table: "items_tratamiento",
                column: "plan_tratamiento_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_actualizado_por_usuario_id",
                table: "planes_tratamiento",
                column: "actualizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_cancelado_por_usuario_id",
                table: "planes_tratamiento",
                column: "cancelado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_completado_por_usuario_id",
                table: "planes_tratamiento",
                column: "completado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_consulta_id",
                table: "planes_tratamiento",
                column: "consulta_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_creado_por_usuario_id",
                table: "planes_tratamiento",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_mascota_veterinaria_id",
                table: "planes_tratamiento",
                column: "mascota_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_planes_tratamiento_suspendido_por_usuario_id",
                table: "planes_tratamiento",
                column: "suspendido_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_planes_tratamiento_veterinaria_estado",
                table: "planes_tratamiento",
                columns: new[] { "veterinaria_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_planes_tratamiento_veterinaria_mascota_veterinaria",
                table: "planes_tratamiento",
                columns: new[] { "veterinaria_id", "mascota_veterinaria_id" });

            migrationBuilder.CreateIndex(
                name: "IX_programaciones_tratamiento_mascota_veterinaria_id",
                table: "programaciones_tratamiento",
                column: "mascota_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_programaciones_tratamiento_registrado_por_usuario_id",
                table: "programaciones_tratamiento",
                column: "registrado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_programaciones_tratamiento_plan_estado",
                table: "programaciones_tratamiento",
                columns: new[] { "plan_tratamiento_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_programaciones_tratamiento_veterinaria_fecha",
                table: "programaciones_tratamiento",
                columns: new[] { "veterinaria_id", "fecha_hora_programada" });

            migrationBuilder.CreateIndex(
                name: "ux_programaciones_tratamiento_item_fecha",
                table: "programaciones_tratamiento",
                columns: new[] { "item_tratamiento_id", "fecha_hora_programada" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seguimientos_clinicos_actualizado_por_usuario_id",
                table: "seguimientos_clinicos",
                column: "actualizado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguimientos_clinicos_cita_id",
                table: "seguimientos_clinicos",
                column: "cita_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguimientos_clinicos_creado_por_usuario_id",
                table: "seguimientos_clinicos",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguimientos_clinicos_mascota_veterinaria_id",
                table: "seguimientos_clinicos",
                column: "mascota_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_seguimientos_clinicos_plan_tratamiento_id",
                table: "seguimientos_clinicos",
                column: "plan_tratamiento_id");

            migrationBuilder.CreateIndex(
                name: "ix_seguimientos_clinicos_veterinaria_mascota_veterinaria_fecha",
                table: "seguimientos_clinicos",
                columns: new[] { "veterinaria_id", "mascota_veterinaria_id", "fecha_objetivo" });

            migrationBuilder.CreateIndex(
                name: "ux_seguimientos_clinicos_consulta_origen_auto_generado",
                table: "seguimientos_clinicos",
                column: "consulta_origen_id",
                unique: true,
                filter: "es_generado_desde_proximo_control = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "horarios_items_tratamiento");

            migrationBuilder.DropTable(
                name: "programaciones_tratamiento");

            migrationBuilder.DropTable(
                name: "seguimientos_clinicos");

            migrationBuilder.DropTable(
                name: "items_tratamiento");

            migrationBuilder.DropTable(
                name: "planes_tratamiento");

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000035"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000036"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000037"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000039"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000043"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000044"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000045"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000038"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000039"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000045"));
        }
    }
}
