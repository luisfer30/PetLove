using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AddClientesAndPets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes_veterinaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_eliminacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes_veterinaria", x => x.id);
                    table.ForeignKey(
                        name: "FK_clientes_veterinaria_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clientes_veterinaria_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "especies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_especies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "razas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    especie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_razas", x => x.id);
                    table.ForeignKey(
                        name: "FK_razas_especies_especie_id",
                        column: x => x.especie_id,
                        principalTable: "especies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mascotas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_publico = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    especie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    raza_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sexo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_nacimiento_aproximada = table.Column<bool>(type: "boolean", nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    numero_microchip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    esterilizado = table.Column<bool>(type: "boolean", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_eliminacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mascotas", x => x.id);
                    table.ForeignKey(
                        name: "FK_mascotas_especies_especie_id",
                        column: x => x.especie_id,
                        principalTable: "especies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mascotas_razas_raza_id",
                        column: x => x.raza_id,
                        principalTable: "razas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mascotas_veterinaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_historia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    observaciones_internas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_eliminacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mascotas_veterinaria", x => x.id);
                    table.ForeignKey(
                        name: "FK_mascotas_veterinaria_mascotas_mascota_id",
                        column: x => x.mascota_id,
                        principalTable: "mascotas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mascotas_veterinaria_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "responsables_mascota",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mascota_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_veterinaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    tipo_relacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    es_principal = table.Column<bool>(type: "boolean", nullable: false),
                    puede_ver_historial = table.Column<bool>(type: "boolean", nullable: false),
                    puede_gestionar_tratamientos = table.Column<bool>(type: "boolean", nullable: false),
                    estado_vinculacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_actualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsables_mascota", x => x.id);
                    table.ForeignKey(
                        name: "FK_responsables_mascota_clientes_veterinaria_cliente_veterinar~",
                        column: x => x.cliente_veterinaria_id,
                        principalTable: "clientes_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_responsables_mascota_mascotas_veterinaria_mascota_veterinar~",
                        column: x => x.mascota_veterinaria_id,
                        principalTable: "mascotas_veterinaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_responsables_mascota_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_responsables_mascota_veterinarias_veterinaria_id",
                        column: x => x.veterinaria_id,
                        principalTable: "veterinarias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "especies",
                columns: new[] { "id", "codigo", "estado", "nombre" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-0000-0000-000000000001"), "PERRO", "ACTIVO", "Perro" },
                    { new Guid("33333333-0000-0000-0000-000000000002"), "GATO", "ACTIVO", "Gato" },
                    { new Guid("33333333-0000-0000-0000-000000000003"), "AVE", "ACTIVO", "Ave" },
                    { new Guid("33333333-0000-0000-0000-000000000004"), "CONEJO", "ACTIVO", "Conejo" },
                    { new Guid("33333333-0000-0000-0000-000000000005"), "OTRO", "ACTIVO", "Otro" }
                });

            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000007"), "clientes.ver", null, "Ver clientes" },
                    { new Guid("22222222-0000-0000-0000-000000000008"), "clientes.crear", null, "Crear clientes" },
                    { new Guid("22222222-0000-0000-0000-000000000009"), "clientes.editar", null, "Editar clientes" },
                    { new Guid("22222222-0000-0000-0000-000000000010"), "mascotas.ver", null, "Ver mascotas" },
                    { new Guid("22222222-0000-0000-0000-000000000011"), "mascotas.crear", null, "Crear mascotas" },
                    { new Guid("22222222-0000-0000-0000-000000000012"), "mascotas.editar", null, "Editar mascotas" },
                    { new Guid("22222222-0000-0000-0000-000000000013"), "responsables.ver", null, "Ver responsables" },
                    { new Guid("22222222-0000-0000-0000-000000000014"), "responsables.crear", null, "Crear responsables" },
                    { new Guid("22222222-0000-0000-0000-000000000015"), "responsables.editar", null, "Editar responsables" }
                });

            migrationBuilder.InsertData(
                table: "roles_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000008"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000009"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000015"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000008"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000009"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000015"), new Guid("11111111-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_veterinaria_usuario_id",
                table: "clientes_veterinaria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_veterinaria_veterinaria_id",
                table: "clientes_veterinaria",
                column: "veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "ix_especies_codigo_unico",
                table: "especies",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mascotas_especie_id",
                table: "mascotas",
                column: "especie_id");

            migrationBuilder.CreateIndex(
                name: "IX_mascotas_raza_id",
                table: "mascotas",
                column: "raza_id");

            migrationBuilder.CreateIndex(
                name: "ix_mascotas_codigo_publico_unico",
                table: "mascotas",
                column: "codigo_publico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_mascotas_numero_microchip",
                table: "mascotas",
                column: "numero_microchip",
                unique: true,
                filter: "numero_microchip IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_mascotas_veterinaria_mascota_id",
                table: "mascotas_veterinaria",
                column: "mascota_id");

            migrationBuilder.CreateIndex(
                name: "ux_mascotas_veterinaria_veterinaria_mascota",
                table: "mascotas_veterinaria",
                columns: new[] { "veterinaria_id", "mascota_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_razas_especie_nombre",
                table: "razas",
                columns: new[] { "especie_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_responsables_mascota_cliente_veterinaria_id",
                table: "responsables_mascota",
                column: "cliente_veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "IX_responsables_mascota_usuario_id",
                table: "responsables_mascota",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_responsables_mascota_veterinaria_id",
                table: "responsables_mascota",
                column: "veterinaria_id");

            migrationBuilder.CreateIndex(
                name: "ux_responsables_mascota_principal_activo",
                table: "responsables_mascota",
                column: "mascota_veterinaria_id",
                unique: true,
                filter: "es_principal AND estado = 'ACTIVO'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "responsables_mascota");

            migrationBuilder.DropTable(
                name: "clientes_veterinaria");

            migrationBuilder.DropTable(
                name: "mascotas_veterinaria");

            migrationBuilder.DropTable(
                name: "mascotas");

            migrationBuilder.DropTable(
                name: "razas");

            migrationBuilder.DropTable(
                name: "especies");

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000008"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000009"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000015"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000008"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000009"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000015"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000015"));
        }
    }
}
