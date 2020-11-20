using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class addTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                table: "User");

            migrationBuilder.AddColumn<Guid>(
                name: "LableId",
                table: "User",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "IsShow",
                table: "tb_TabMenu",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "tb_TabMenu",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MenuType",
                table: "tb_TabMenu",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tb_RoleMenu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false),
                    MenuIds = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_RoleMenu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_SystemMenu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    ParentGid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Icon = table.Column<int>(maxLength: 200, nullable: false),
                    SortNum = table.Column<int>(nullable: false),
                    Location = table.Column<string>(maxLength: 50, nullable: true),
                    IsShow = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_SystemMenu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_UserComment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    ReplyId = table.Column<Guid>(nullable: false),
                    CommentTxt = table.Column<string>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_UserComment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_UserLable",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    LabelName = table.Column<string>(maxLength: 20, nullable: false),
                    ImgPath = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_UserLable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_UserOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    OrderCode = table.Column<string>(maxLength: 50, nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 100, nullable: false),
                    CoverPath = table.Column<string>(maxLength: 200, nullable: false),
                    Price = table.Column<double>(nullable: false),
                    DiscountPrice = table.Column<double>(nullable: false),
                    ActualPay = table.Column<double>(nullable: false),
                    PayState = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_UserOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    RoleName = table.Column<string>(maxLength: 50, nullable: false),
                    RoleDescribe = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_UserStudent",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<int>(nullable: false),
                    NickName = table.Column<string>(maxLength: 50, nullable: false),
                    LoginName = table.Column<string>(maxLength: 50, nullable: false),
                    LoginPwd = table.Column<string>(maxLength: 50, nullable: false),
                    Salt = table.Column<string>(maxLength: 20, nullable: false),
                    Telephone = table.Column<string>(maxLength: 20, nullable: false),
                    IsVIP = table.Column<int>(nullable: false),
                    LableId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_UserStudent", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_RoleMenu");

            migrationBuilder.DropTable(
                name: "tb_SystemMenu");

            migrationBuilder.DropTable(
                name: "tb_UserComment");

            migrationBuilder.DropTable(
                name: "tb_UserLable");

            migrationBuilder.DropTable(
                name: "tb_UserOrder");

            migrationBuilder.DropTable(
                name: "tb_UserRole");

            migrationBuilder.DropTable(
                name: "tb_UserStudent");

            migrationBuilder.DropColumn(
                name: "LableId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsShow",
                table: "tb_TabMenu");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "tb_TabMenu");

            migrationBuilder.DropColumn(
                name: "MenuType",
                table: "tb_TabMenu");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginTime",
                table: "User",
                nullable: true);
        }
    }
}
