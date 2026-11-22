using GeekShopping.OrderAPI.Model.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeekShopping.OrderAPI.Migrations
{
    // Orders saved before this change hold the full card number and the CVV. Keep only the last four digits.
    [DbContext(typeof(SqlServerContext))]
    [Migration("20261121120000_MaskStoredCardData")]
    public partial class MaskStoredCardData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE order_header
SET card_number = CASE
        WHEN card_number IS NULL THEN NULL
        WHEN LEN(REPLACE(card_number, ' ', '')) = 0 THEN NULL
        ELSE '**** **** **** ' + RIGHT(REPLACE(REPLACE(card_number, ' ', ''), '-', ''), 4)
    END,
    cvv = NULL
WHERE (card_number IS NOT NULL AND card_number NOT LIKE '**** **** **** %') OR cvv IS NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The removed digits cannot be restored.
        }
    }
}
