using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaaS.Veterinario.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AddClinicaEjercerVeterinarioPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permisos",
                columns: new[] { "id", "codigo", "descripcion", "nombre" },
                values: new object[] { new Guid("22222222-0000-0000-0000-000000000030"), "clinica.ejercer_veterinario", null, "Ejercer como veterinario responsable" });

            migrationBuilder.InsertData(
                table: "roles_permisos",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[] { new Guid("22222222-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000002") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "roles_permisos",
                keyColumns: new[] { "permiso_id", "rol_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "permisos",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000030"));
        }
    }
}
