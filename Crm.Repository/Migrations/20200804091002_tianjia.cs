using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class tianjia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotNews");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "UserLoginLog",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "User",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "tb_HotNews",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    Subtitle = table.Column<string>(maxLength: 100, nullable: true),
                    InformationSource = table.Column<string>(maxLength: 50, nullable: true),
                    NewsContent = table.Column<string>(nullable: false),
                    CoverUrl = table.Column<string>(nullable: true),
                    ShowTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_HotNews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_HotSpot",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    ImgTitle = table.Column<string>(maxLength: 50, nullable: true),
                    ImgPath = table.Column<string>(maxLength: 200, nullable: false),
                    ContentUrl = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_HotSpot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Notice",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    NoticeType = table.Column<Guid>(nullable: false),
                    NewsContent = table.Column<string>(nullable: false),
                    FileDownload = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Notice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_OperationLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    OperationUser = table.Column<string>(nullable: false),
                    OpentionControllerStr = table.Column<string>(nullable: true),
                    OperationEvent = table.Column<string>(nullable: true),
                    OpentionContext = table.Column<string>(nullable: true),
                    OperationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_OperationLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_TabMenu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    ParentGid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_TabMenu", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_HotNews");

            migrationBuilder.DropTable(
                name: "tb_HotSpot");

            migrationBuilder.DropTable(
                name: "tb_Notice");

            migrationBuilder.DropTable(
                name: "tb_OperationLog");

            migrationBuilder.DropTable(
                name: "tb_TabMenu");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "UserLoginLog",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "User",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.CreateTable(
                name: "HotNews",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CoverUrl = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<int>(nullable: false),
                    NewsContent = table.Column<string>(maxLength: 300, nullable: true),
                    ShowTime = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotNews", x => x.Id);
                });
        }
    }
}
