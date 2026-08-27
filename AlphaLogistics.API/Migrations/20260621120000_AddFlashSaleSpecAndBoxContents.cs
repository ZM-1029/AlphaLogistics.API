using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AlphaLogisticsContext))]
    [Migration("20260621120000_AddFlashSaleSpecAndBoxContents")]
    public partial class AddFlashSaleSpecAndBoxContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFlashSale",
                table: "ProductMasters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Specification",
                table: "ProductMasters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsInTheBox",
                table: "ProductMasters",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFlashSale",
                table: "ProductMasters");

            migrationBuilder.DropColumn(
                name: "Specification",
                table: "ProductMasters");

            migrationBuilder.DropColumn(
                name: "WhatsInTheBox",
                table: "ProductMasters");
        }
    }
}
