using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_SubCategoryMaster_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_VendorMaster_VendorId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategoryMaster_CategoryMaster_CategoryId",
                table: "SubCategoryMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMasters_RoleMaster_RoleId",
                table: "UserMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorMaster_UserMasters_UserId",
                table: "VendorMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VendorMaster",
                table: "VendorMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubCategoryMaster",
                table: "SubCategoryMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleMaster",
                table: "RoleMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryMaster",
                table: "CategoryMaster");

            migrationBuilder.RenameTable(
                name: "VendorMaster",
                newName: "VendorMasters");

            migrationBuilder.RenameTable(
                name: "SubCategoryMaster",
                newName: "SubCategoryMasters");

            migrationBuilder.RenameTable(
                name: "RoleMaster",
                newName: "RoleMasters");

            migrationBuilder.RenameTable(
                name: "CategoryMaster",
                newName: "CategoryMasters");

            migrationBuilder.RenameIndex(
                name: "IX_VendorMaster_UserId",
                table: "VendorMasters",
                newName: "IX_VendorMasters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SubCategoryMaster_CategoryId",
                table: "SubCategoryMasters",
                newName: "IX_SubCategoryMasters_CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "RoleMasterId",
                table: "UserMasters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VendorMasters",
                table: "VendorMasters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubCategoryMasters",
                table: "SubCategoryMasters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleMasters",
                table: "RoleMasters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryMasters",
                table: "CategoryMasters",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_ProductMasters_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_RoleMasterId",
                table: "UserMasters",
                column: "RoleMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters",
                column: "SubCategoryId",
                principalTable: "SubCategoryMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_VendorMasters_VendorId",
                table: "ProductMasters",
                column: "VendorId",
                principalTable: "VendorMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategoryMasters_CategoryMasters_CategoryId",
                table: "SubCategoryMasters",
                column: "CategoryId",
                principalTable: "CategoryMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMasters_RoleMasters_RoleId",
                table: "UserMasters",
                column: "RoleId",
                principalTable: "RoleMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMasters_RoleMasters_RoleMasterId",
                table: "UserMasters",
                column: "RoleMasterId",
                principalTable: "RoleMasters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorMasters_UserMasters_UserId",
                table: "VendorMasters",
                column: "UserId",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_SubCategoryMasters_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_VendorMasters_VendorId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategoryMasters_CategoryMasters_CategoryId",
                table: "SubCategoryMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMasters_RoleMasters_RoleId",
                table: "UserMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMasters_RoleMasters_RoleMasterId",
                table: "UserMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_VendorMasters_UserMasters_UserId",
                table: "VendorMasters");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_UserMasters_RoleMasterId",
                table: "UserMasters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VendorMasters",
                table: "VendorMasters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubCategoryMasters",
                table: "SubCategoryMasters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleMasters",
                table: "RoleMasters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryMasters",
                table: "CategoryMasters");

            migrationBuilder.DropColumn(
                name: "RoleMasterId",
                table: "UserMasters");

            migrationBuilder.RenameTable(
                name: "VendorMasters",
                newName: "VendorMaster");

            migrationBuilder.RenameTable(
                name: "SubCategoryMasters",
                newName: "SubCategoryMaster");

            migrationBuilder.RenameTable(
                name: "RoleMasters",
                newName: "RoleMaster");

            migrationBuilder.RenameTable(
                name: "CategoryMasters",
                newName: "CategoryMaster");

            migrationBuilder.RenameIndex(
                name: "IX_VendorMasters_UserId",
                table: "VendorMaster",
                newName: "IX_VendorMaster_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SubCategoryMasters_CategoryId",
                table: "SubCategoryMaster",
                newName: "IX_SubCategoryMaster_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VendorMaster",
                table: "VendorMaster",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubCategoryMaster",
                table: "SubCategoryMaster",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleMaster",
                table: "RoleMaster",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryMaster",
                table: "CategoryMaster",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_SubCategoryMaster_SubCategoryId",
                table: "ProductMasters",
                column: "SubCategoryId",
                principalTable: "SubCategoryMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductMasters_VendorMaster_VendorId",
                table: "ProductMasters",
                column: "VendorId",
                principalTable: "VendorMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategoryMaster_CategoryMaster_CategoryId",
                table: "SubCategoryMaster",
                column: "CategoryId",
                principalTable: "CategoryMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMasters_RoleMaster_RoleId",
                table: "UserMasters",
                column: "RoleId",
                principalTable: "RoleMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VendorMaster_UserMasters_UserId",
                table: "VendorMaster",
                column: "UserId",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
