using Microsoft.EntityFrameworkCore.Migrations;

namespace Crm.Repository.Migrations
{
    public partial class u_user_student : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LableId",
                table: "tb_UserStudent",
                newName: "LabelId");

            migrationBuilder.AddColumn<string>(
                name: "RecentlyIp",
                table: "tb_UserStudent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecentlyIp",
                table: "tb_UserStudent");

            migrationBuilder.RenameColumn(
                name: "LabelId",
                table: "tb_UserStudent",
                newName: "LableId");
        }
    }
}
