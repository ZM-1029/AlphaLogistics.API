using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "VendorMasters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "VendorMasters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_CreatedBy",
                table: "VendorMasters",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_UpdatedBy",
                table: "VendorMasters",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorMasters_UserMasters_CreatedBy",
                table: "VendorMasters",
                column: "CreatedBy",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorMasters_UserMasters_UpdatedBy",
                table: "VendorMasters",
                column: "UpdatedBy",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorMasters_UserMasters_CreatedBy",
                table: "VendorMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorMasters_UserMasters_UpdatedBy",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_CreatedBy",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_UpdatedBy",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VendorMasters");
        }
    }
}
