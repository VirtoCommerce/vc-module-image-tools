﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.ImageToolsModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.ImageToolsModule.Data.MySql.Migrations
{
    [DbContext(typeof(ThumbnailDbContext))]
    partial class ThumbnailDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailOptionEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("AnchorPosition")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("BackgroundColor")
                        .HasColumnType("longtext");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FileSuffix")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int?>("Height")
                        .HasColumnType("int");

                    b.Property<string>("JpegQuality")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("ResizeMethod")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int?>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ThumbnailOption", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailTaskEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("LastRun")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("WorkPath")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("varchar(2048)");

                    b.HasKey("Id");

                    b.ToTable("ThumbnailTask", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailTaskOptionEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ThumbnailOptionId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ThumbnailTaskId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("ThumbnailOptionId");

                    b.HasIndex("ThumbnailTaskId");

                    b.ToTable("ThumbnailTaskOption", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailTaskOptionEntity", b =>
                {
                    b.HasOne("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailOptionEntity", "ThumbnailOption")
                        .WithMany()
                        .HasForeignKey("ThumbnailOptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailTaskEntity", "ThumbnailTask")
                        .WithMany("ThumbnailTaskOptions")
                        .HasForeignKey("ThumbnailTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ThumbnailOption");

                    b.Navigation("ThumbnailTask");
                });

            modelBuilder.Entity("VirtoCommerce.ImageToolsModule.Data.Models.ThumbnailTaskEntity", b =>
                {
                    b.Navigation("ThumbnailTaskOptions");
                });
#pragma warning restore 612, 618
        }
    }
}
