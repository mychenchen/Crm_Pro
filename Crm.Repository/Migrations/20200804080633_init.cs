using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotNews",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 50, nullable: false),
                    NewsContent = table.Column<string>(maxLength: 300, nullable: true),
                    CoverUrl = table.Column<string>(nullable: true),
                    ShowTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotNews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<int>(nullable: false),
                    NickName = table.Column<string>(maxLength: 50, nullable: false),
                    LoginName = table.Column<string>(maxLength: 50, nullable: false),
                    LoginPwd = table.Column<string>(maxLength: 50, nullable: false),
                    Salt = table.Column<string>(maxLength: 50, nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: true),
                    LastLoginTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Ip = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginLog", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotNews");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserLoginLog");
        }
    }
}
