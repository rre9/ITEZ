using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddCloseReasonToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop column if it exists with wrong type (nvarchar)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns 
                           WHERE object_id = OBJECT_ID(N'[Tickets]') 
                           AND name = 'CloseReason' 
                           AND system_type_id != 56) -- 56 = int
                BEGIN
                    ALTER TABLE [Tickets] DROP COLUMN [CloseReason];
                END
            ");

            // Add CloseReason as int (enum) - nullable
            migrationBuilder.AddColumn<int>(
                name: "CloseReason",
                table: "Tickets",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseReason",
                table: "Tickets");
        }
    }
}
