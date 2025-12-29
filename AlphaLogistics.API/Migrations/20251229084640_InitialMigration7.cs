using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductMasters_ProductMasterId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_VendorMasters_Phone",
                table: "VendorMasters");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductMasterId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ProductMasterId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "VendorMasters",
                newName: "VAT");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "VendorMasters",
                newName: "VendorName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "VendorMasters",
                newName: "PrimaryAddress");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "VendorMasters",
                newName: "PAN");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "VendorMasters",
                newName: "BankName");

            migrationBuilder.RenameIndex(
                name: "IX_VendorMasters_Email",
                table: "VendorMasters",
                newName: "IX_VendorMasters_PAN");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VendorMasters",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorMasters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "AccHolderName",
                table: "VendorMasters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNo",
                table: "VendorMasters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerType",
                table: "VendorMasters",
                type: "text",
                nullable: false,
                defaultValue: "Basic");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "VendorMasters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "VendorMasters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryAddress",
                table: "VendorMasters",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserMasters",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserMasters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProductMasters",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductMasters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CartMasters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "DocumentMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    DocumentName = table.Column<string>(type: "text", nullable: false),
                    DocumentUrl = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentMasters_VendorMasters_VendorId",
                        column: x => x.VendorId,
                        principalTable: "VendorMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMasters_VendorId",
                table: "DocumentMasters",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentMasters");

            migrationBuilder.DropColumn(
                name: "AccHolderName",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "BankAccountNo",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "SecondaryAddress",
                table: "VendorMasters");

            migrationBuilder.RenameColumn(
                name: "VendorName",
                table: "VendorMasters",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "VAT",
                table: "VendorMasters",
                newName: "ProfileImage");

            migrationBuilder.RenameColumn(
                name: "PrimaryAddress",
                table: "VendorMasters",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PAN",
                table: "VendorMasters",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "VendorMasters",
                newName: "Address");

            migrationBuilder.RenameIndex(
                name: "IX_VendorMasters_PAN",
                table: "VendorMasters",
                newName: "IX_VendorMasters_Email");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VendorMasters",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "VendorMasters",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "UserMasters",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserMasters",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ProductMasters",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductMasters",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<int>(
                name: "ProductMasterId",
                table: "ProductImages",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CartMasters",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.CreateIndex(
                name: "IX_VendorMasters_Phone",
                table: "VendorMasters",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductMasterId",
                table: "ProductImages",
                column: "ProductMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductMasters_ProductMasterId",
                table: "ProductImages",
                column: "ProductMasterId",
                principalTable: "ProductMasters",
                principalColumn: "Id");
        }
    }
}
