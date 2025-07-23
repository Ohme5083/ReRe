using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReRe.Data.Migrations
{
    /// <inheritdoc />
    public partial class NouveauxModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RessourceModelUserModel_Ressources_RessourcesLikedId",
                table: "RessourceModelUserModel");

            migrationBuilder.DropForeignKey(
                name: "FK_RessourceModelUserModel_Utilisateurs_UtilisateursId",
                table: "RessourceModelUserModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RessourceModelUserModel",
                table: "RessourceModelUserModel");

            migrationBuilder.RenameTable(
                name: "RessourceModelUserModel",
                newName: "RessourceUtilisateurs");

            migrationBuilder.RenameIndex(
                name: "IX_RessourceModelUserModel_UtilisateursId",
                table: "RessourceUtilisateurs",
                newName: "IX_RessourceUtilisateurs_UtilisateursId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RessourceUtilisateurs",
                table: "RessourceUtilisateurs",
                columns: new[] { "RessourcesLikedId", "UtilisateursId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RessourceUtilisateurs_Ressources_RessourcesLikedId",
                table: "RessourceUtilisateurs",
                column: "RessourcesLikedId",
                principalTable: "Ressources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RessourceUtilisateurs_Utilisateurs_UtilisateursId",
                table: "RessourceUtilisateurs",
                column: "UtilisateursId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RessourceUtilisateurs_Ressources_RessourcesLikedId",
                table: "RessourceUtilisateurs");

            migrationBuilder.DropForeignKey(
                name: "FK_RessourceUtilisateurs_Utilisateurs_UtilisateursId",
                table: "RessourceUtilisateurs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RessourceUtilisateurs",
                table: "RessourceUtilisateurs");

            migrationBuilder.RenameTable(
                name: "RessourceUtilisateurs",
                newName: "RessourceModelUserModel");

            migrationBuilder.RenameIndex(
                name: "IX_RessourceUtilisateurs_UtilisateursId",
                table: "RessourceModelUserModel",
                newName: "IX_RessourceModelUserModel_UtilisateursId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RessourceModelUserModel",
                table: "RessourceModelUserModel",
                columns: new[] { "RessourcesLikedId", "UtilisateursId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RessourceModelUserModel_Ressources_RessourcesLikedId",
                table: "RessourceModelUserModel",
                column: "RessourcesLikedId",
                principalTable: "Ressources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RessourceModelUserModel_Utilisateurs_UtilisateursId",
                table: "RessourceModelUserModel",
                column: "UtilisateursId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
