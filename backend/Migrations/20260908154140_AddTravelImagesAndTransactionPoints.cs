using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelImagesAndTransactionPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Travels_TravelId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TravelImages_TravelId",
                table: "TravelImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "TravelImages",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "TravelImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "TravelId",
                table: "Transactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TravelImages_TravelId_SortOrder",
                table: "TravelImages",
                columns: new[] { "TravelId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Travels_TravelId",
                table: "Transactions",
                column: "TravelId",
                principalTable: "Travels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Travels_TravelId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TravelImages_TravelId_SortOrder",
                table: "TravelImages");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "TravelImages");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "TravelImages");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "TravelId",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelImages_TravelId",
                table: "TravelImages",
                column: "TravelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Travels_TravelId",
                table: "Transactions",
                column: "TravelId",
                principalTable: "Travels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
