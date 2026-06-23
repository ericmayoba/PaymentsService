using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentsService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReversedMovementId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReversedMovementId",
                table: "Movements",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReversedMovementId",
                table: "Movements");
        }
    }
}
