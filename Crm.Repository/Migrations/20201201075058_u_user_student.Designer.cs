﻿// <auto-generated />
using System;
using Crm.Repository.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Crm.Repository.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20201201075058_u_user_student")]
    partial class u_user_student
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Crm.Repository.TbEntity.HotNewsEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ChildrenGid");

                    b.Property<string>("CoverUrl");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("InformationSource")
                        .HasMaxLength(50);

                    b.Property<int>("IsDelete");

                    b.Property<string>("NewsContent")
                        .IsRequired();

                    b.Property<Guid>("ParentGid");

                    b.Property<DateTime?>("ShowTime");

                    b.Property<string>("Subtitle")
                        .HasMaxLength(100);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("tb_HotNews");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.HotSpotEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContentUrl")
                        .HasMaxLength(200);

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("ImgPath")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("ImgTitle")
                        .HasMaxLength(50);

                    b.Property<int>("IsDelete");

                    b.HasKey("Id");

                    b.ToTable("tb_HotSpot");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.NoticeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("FileDownload");

                    b.Property<string>("FileName");

                    b.Property<int>("IsDelete");

                    b.Property<string>("NewsContent")
                        .IsRequired();

                    b.Property<Guid>("NoticeType");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("tb_Notice");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.OperationLogEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<string>("OpentionContext");

                    b.Property<string>("OpentionControllerStr");

                    b.Property<string>("OperationEvent");

                    b.Property<DateTime>("OperationTime");

                    b.Property<string>("OperationUser")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("tb_OperationLog");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.ProductEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CoverPath")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTime>("CreateTime");

                    b.Property<double>("Discount");

                    b.Property<double>("DiscountPrice");

                    b.Property<double>("FloorPrice");

                    b.Property<int>("HotNum");

                    b.Property<int>("IsDelete");

                    b.Property<DateTime>("IssueDateTime");

                    b.Property<int>("OnShelfStatus");

                    b.Property<double>("Price");

                    b.Property<string>("ProductContent");

                    b.Property<string>("Subtitle");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("tb_Product");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.ProductVideoEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CoverPath")
                        .HasMaxLength(200);

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<Guid>("ProId");

                    b.Property<string>("TimeLength")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<string>("TypeName")
                        .HasMaxLength(50);

                    b.Property<string>("VideoName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("VideoPath")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<string>("VideoSize");

                    b.HasKey("Id");

                    b.ToTable("tb_ProductVideo");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.RoleMenuEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<string>("MenuIds");

                    b.Property<Guid>("RoleId");

                    b.HasKey("Id");

                    b.ToTable("tb_RoleMenu");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.SystemMenuEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Icon")
                        .HasMaxLength(200);

                    b.Property<int>("IsDelete");

                    b.Property<int>("IsShow");

                    b.Property<string>("Location")
                        .HasMaxLength(50);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<Guid>("ParentGid");

                    b.Property<int>("SortNum");

                    b.HasKey("Id");

                    b.ToTable("tb_SystemMenu");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.TabMenuEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<int>("IsShow");

                    b.Property<string>("Location")
                        .HasMaxLength(50);

                    b.Property<int>("MenuType");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<Guid>("ParentGid");

                    b.Property<int>("SortNum");

                    b.HasKey("Id");

                    b.ToTable("tb_TabMenu");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<Guid>("LabelId");

                    b.Property<string>("LoginName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("LoginPwd")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<Guid>("RoleId");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<DateTime?>("UpdateTime");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserCommentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommentTxt")
                        .IsRequired();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<Guid>("ReplyId");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.ToTable("tb_UserComment");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserLabel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("ImgPath")
                        .HasMaxLength(200);

                    b.Property<int>("IsDelete");

                    b.Property<string>("LabelName")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.HasKey("Id");

                    b.ToTable("tb_UserLable");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserLoginLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Ip");

                    b.Property<int>("IsDelete");

                    b.Property<Guid>("UserId");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.ToTable("UserLoginLog");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserOrderEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("ActualPay");

                    b.Property<string>("CoverPath")
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property<DateTime>("CreateTime");

                    b.Property<double>("DiscountPrice");

                    b.Property<int>("IsDelete");

                    b.Property<string>("OrderCode")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("PayState");

                    b.Property<double>("Price");

                    b.Property<Guid>("ProductId");

                    b.Property<string>("Remarks")
                        .HasMaxLength(200);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.ToTable("tb_UserOrder");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserRoleEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<string>("RoleDescribe")
                        .HasMaxLength(200);

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("tb_UserRole");
                });

            modelBuilder.Entity("Crm.Repository.TbEntity.UserStudentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<int>("IsDelete");

                    b.Property<int>("IsVIP");

                    b.Property<Guid>("LabelId");

                    b.Property<string>("LoginName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("LoginPwd")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("RecentlyIp");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<string>("Telephone")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.HasKey("Id");

                    b.ToTable("tb_UserStudent");
                });
#pragma warning restore 612, 618
        }
    }
}
