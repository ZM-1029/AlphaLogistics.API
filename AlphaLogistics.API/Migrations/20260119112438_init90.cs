using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class init90 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "UserMasters",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

         

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OrderMasters",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

          
            migrationBuilder.CreateTable(
                name: "PradeshMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    Charge = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PradeshMasters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PradeshMasters");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "VendorMasters");

            migrationBuilder.DropColumn(
                name: "PradeshId",
                table: "UserMasters");

            migrationBuilder.DropColumn(
                name: "DeliveryCharge",
                table: "OrderMasters");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "UserMasters",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OrderMasters",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
