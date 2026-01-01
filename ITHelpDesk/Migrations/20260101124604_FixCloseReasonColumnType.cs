using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class FixCloseReasonColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the column if it exists with wrong type (nvarchar)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns 
                           WHERE object_id = OBJECT_ID(N'[Tickets]') 
                           AND name = 'CloseReason' 
                           AND system_type_id != 56) -- 56 = int
                BEGIN
                    ALTER TABLE [Tickets] DROP COLUMN [CloseReason];
                END
            ");

            // Add the column as int (enum) if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                               WHERE object_id = OBJECT_ID(N'[Tickets]') 
                               AND name = 'CloseReason')
                BEGIN
                    ALTER TABLE [Tickets] ADD [CloseReason] int NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns 
                           WHERE object_id = OBJECT_ID(N'[Tickets]') 
                           AND name = 'CloseReason')
                BEGIN
                    ALTER TABLE [Tickets] DROP COLUMN [CloseReason];
                END
            ");
        }
    }
}
