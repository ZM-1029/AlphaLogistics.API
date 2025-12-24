using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "UserMasters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "ProductMasters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "ProductMasters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CategoryMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: true),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorMaster_UserMasters_UserId",
                        column: x => x.UserId,
                        principalTable: "UserMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryMaster_CategoryMaster_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CategoryMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_RoleId",
                table: "UserMasters",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMasters_SubCategoryId",
                table: "ProductMasters",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMasters_VendorId",
                table: "ProductMasters",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryMaster_CategoryId",
                table: "SubCategoryMaster",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMaster_UserId",
                table: "VendorMaster",
                column: "UserId");

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
                name: "FK_UserMasters_RoleMaster_RoleId",
                table: "UserMasters",
                column: "RoleId",
                principalTable: "RoleMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_SubCategoryMaster_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductMasters_VendorMaster_VendorId",
                table: "ProductMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMasters_RoleMaster_RoleId",
                table: "UserMasters");

            migrationBuilder.DropTable(
                name: "RoleMaster");

            migrationBuilder.DropTable(
                name: "SubCategoryMaster");

            migrationBuilder.DropTable(
                name: "VendorMaster");

            migrationBuilder.DropTable(
                name: "CategoryMaster");

            migrationBuilder.DropIndex(
                name: "IX_UserMasters_RoleId",
                table: "UserMasters");

            migrationBuilder.DropIndex(
                name: "IX_ProductMasters_SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropIndex(
                name: "IX_ProductMasters_VendorId",
                table: "ProductMasters");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserMasters");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "ProductMasters");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "ProductMasters");
        }
    }
}
