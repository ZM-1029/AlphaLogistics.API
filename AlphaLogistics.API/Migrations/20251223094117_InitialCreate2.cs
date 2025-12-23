using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaLogistics.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartMasters_ProductMasters_ProductMasterId",
                table: "CartMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderMasters_OrderMasterId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMasters_UserMasters_UserMasterId",
                table: "OrderMasters");

            migrationBuilder.DropIndex(
                name: "IX_OrderMasters_UserMasterId",
                table: "OrderMasters");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderMasterId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartMasters_ProductMasterId",
                table: "CartMasters");

            migrationBuilder.DropColumn(
                name: "UserMasterId",
                table: "OrderMasters");

            migrationBuilder.DropColumn(
                name: "OrderMasterId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductMasterId",
                table: "CartMasters");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMasters_UserId",
                table: "OrderMasters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CartMasters_ProductId",
                table: "CartMasters",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartMasters_ProductMasters_ProductId",
                table: "CartMasters",
                column: "ProductId",
                principalTable: "ProductMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderMasters_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "OrderMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductMasters_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "ProductMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMasters_UserMasters_UserId",
                table: "OrderMasters",
                column: "UserId",
                principalTable: "UserMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartMasters_ProductMasters_ProductId",
                table: "CartMasters");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_OrderMasters_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductMasters_ProductId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMasters_UserMasters_UserId",
                table: "OrderMasters");

            migrationBuilder.DropIndex(
                name: "IX_OrderMasters_UserId",
                table: "OrderMasters");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartMasters_ProductId",
                table: "CartMasters");

            migrationBuilder.AddColumn<int>(
                name: "UserMasterId",
                table: "OrderMasters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderMasterId",
                table: "OrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductMasterId",
                table: "CartMasters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderMasters_UserMasterId",
                table: "OrderMasters",
                column: "UserMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderMasterId",
                table: "OrderItems",
                column: "OrderMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CartMasters_ProductMasterId",
                table: "CartMasters",
                column: "ProductMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartMasters_ProductMasters_ProductMasterId",
                table: "CartMasters",
                column: "ProductMasterId",
                principalTable: "ProductMasters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_OrderMasters_OrderMasterId",
                table: "OrderItems",
                column: "OrderMasterId",
                principalTable: "OrderMasters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMasters_UserMasters_UserMasterId",
                table: "OrderMasters",
                column: "UserMasterId",
                principalTable: "UserMasters",
                principalColumn: "Id");
        }
    }
}
