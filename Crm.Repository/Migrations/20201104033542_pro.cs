using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class pro : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    Subtitle = table.Column<string>(nullable: true),
                    FloorPrice = table.Column<double>(nullable: false),
                    Price = table.Column<double>(nullable: false),
                    Discount = table.Column<double>(nullable: false),
                    DiscountPrice = table.Column<double>(nullable: false),
                    ProductContent = table.Column<string>(nullable: true),
                    IssueDateTime = table.Column<DateTime>(nullable: false),
                    OnShelfStatus = table.Column<int>(nullable: false),
                    HotNum = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_ProductVideo",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    ProId = table.Column<Guid>(nullable: false),
                    TypeName = table.Column<string>(maxLength: 50, nullable: false),
                    VideoName = table.Column<string>(maxLength: 50, nullable: false),
                    VideoSize = table.Column<string>(nullable: true),
                    TimeLength = table.Column<string>(maxLength: 30, nullable: false),
                    VideoPath = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_ProductVideo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_Product");

            migrationBuilder.DropTable(
                name: "tb_ProductVideo");
        }
    }
}
