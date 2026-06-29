using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnreadMessagesTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First create the trigger function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION increment_unread_messages()
                RETURNS TRIGGER AS '
                BEGIN
                    UPDATE ""ConversationParticipants""
                    SET ""NumberOfUnreadMessages"" = ""NumberOfUnreadMessages"" + 1
                    WHERE ""ConversationId"" = NEW.""ConversationId""
                    AND ""UserId"" != NEW.""AuthorId"";
    
                    RETURN NEW;
                END;
            ' LANGUAGE plpgsql");

            // Then create the trigger
            migrationBuilder.Sql(@"
                CREATE TRIGGER tr_messages_increment_unread_count
                AFTER INSERT ON ""Messages""
                FOR EACH ROW
                EXECUTE FUNCTION increment_unread_messages();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS tr_messages_increment_unread_count ON ""Messages"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS increment_unread_messages();");
        }
    }
}