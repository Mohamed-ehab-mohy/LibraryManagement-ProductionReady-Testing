using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_AvailableCopies_NonNegative",
                table: "Books",
                sql: "[AvailableCopies] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_AvailableCopies_NotExceedTotal",
                table: "Books",
                sql: "[AvailableCopies] <= [TotalCopies]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_AvailableCopies_NonNegative",
                table: "Books");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_AvailableCopies_NotExceedTotal",
                table: "Books");
        }
    }
}
