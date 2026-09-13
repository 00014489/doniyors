using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <summary>
    /// Data only. Users.LanguageCode used to take Telegram's raw client locale,
    /// so rows can hold values such as "ru-RU" or "de". Maps tags onto the
    /// supported codes and clears anything else to "" ("not chosen yet"), so
    /// the bot asks those members to pick a language instead of guessing.
    /// </summary>
    public partial class NormalizeUserLanguageCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "LanguageCode" =
                    CASE lower(split_part(replace(trim("LanguageCode"), '_', '-'), '-', 1))
                        WHEN 'uz' THEN 'uz'
                        WHEN 'ru' THEN 'ru'
                        WHEN 'en' THEN 'en'
                        ELSE ''
                    END
                WHERE "LanguageCode" NOT IN ('', 'uz', 'ru', 'en');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The original values are not recoverable, and nothing depends on them.
        }
    }
}
