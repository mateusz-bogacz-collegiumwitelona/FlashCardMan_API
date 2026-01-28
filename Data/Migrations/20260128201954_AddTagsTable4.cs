using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsTable4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_FlashCards_CardId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CardId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "FlashCardTag",
                columns: table => new
                {
                    FlashCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashCardTag", x => new { x.FlashCardId, x.TagId });
                    table.ForeignKey(
                        name: "FK_FlashCardTag_FlashCards_FlashCardId",
                        column: x => x.FlashCardId,
                        principalTable: "FlashCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashCardTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashCardTag_TagId",
                table: "FlashCardTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashCardTag");

            migrationBuilder.AddColumn<Guid>(
                name: "CardId",
                table: "Tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CardId",
                table: "Tags",
                column: "CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_FlashCards_CardId",
                table: "Tags",
                column: "CardId",
                principalTable: "FlashCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
