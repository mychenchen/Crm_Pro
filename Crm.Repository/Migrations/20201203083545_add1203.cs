using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class add1203 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecentlyIp",
                table: "tb_UserStudent",
                newName: "MyIntroduce");

            migrationBuilder.AddColumn<string>(
                name: "HeadImg",
                table: "User",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MyIntroduce",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "User",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HeadImg",
                table: "tb_UserStudent",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sex",
                table: "tb_UserStudent",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadImg",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MyIntroduce",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HeadImg",
                table: "tb_UserStudent");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "tb_UserStudent");

            migrationBuilder.RenameColumn(
                name: "MyIntroduce",
                table: "tb_UserStudent",
                newName: "RecentlyIp");
        }
    }
}
