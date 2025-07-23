using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReRe.Data.Migrations
{
    /// <inheritdoc />
    public partial class NouveauxModels3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RoleModels",
                columns: new[] { "Id", "Libelle" },
                values: new object[,]
                {
                    { 1, "Utilisateur" },
                    { 2, "Modérateur" },
                    { 3, "Administrateur" }
                });

            migrationBuilder.InsertData(
                table: "TypeModel",
                columns: new[] { "Id", "Libelle" },
                values: new object[,]
                {
                    { 1, "Privée" },
                    { 2, "Partagée" },
                    { 3, "Publique" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleModels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RoleModels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RoleModels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TypeModel",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TypeModel",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TypeModel",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
