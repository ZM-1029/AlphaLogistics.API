using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_UserId",
                table: "VendorMasters");

            migrationBuilder.AddColumn<int>(
                name: "ProductMasterId",
                table: "ProductImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CartMasters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_Email",
                table: "VendorMasters",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_Phone",
                table: "VendorMasters",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_UserId",
                table: "VendorMasters",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_Email",
                table: "UserMasters",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_UserName",
                table: "UserMasters",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductMasterId",
                table: "ProductImages",
                column: "ProductMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CartMasters_UserId_ProductId",
                table: "CartMasters",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartMasters_UserMasters_UserId",
                table: "CartMasters",
                column: "UserId",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductMasters_ProductMasterId",
                table: "ProductImages",
                column: "ProductMasterId",
                principalTable: "ProductMasters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters",
                column: "SubCategoryId",
                principalTable: "SubCategoryMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartMasters_UserMasters_UserId",
                table: "CartMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductMasters_ProductMasterId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_Email",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_Phone",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_UserId",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_UserMasters_Email",
                table: "UserMasters");

            migrationBuilder.DropIndex(
                name: "IX_UserMasters_UserName",
                table: "UserMasters");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductMasterId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_CartMasters_UserId_ProductId",
                table: "CartMasters");

            migrationBuilder.DropColumn(
                name: "ProductMasterId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CartMasters");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_UserId",
                table: "VendorMasters",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters",
                column: "SubCategoryId",
                principalTable: "SubCategoryMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
