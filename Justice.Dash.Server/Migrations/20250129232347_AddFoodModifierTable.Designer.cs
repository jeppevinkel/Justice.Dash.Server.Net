﻿// <auto-generated />
using System;
using Justice.Dash.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Justice.Dash.Server.Migrations
{
    [DbContext(typeof(DashboardDbContext))]
    [Migration("20250129232347_AddFoodModifierTable")]
    partial class AddFoodModifierTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Justice.Dash.Server.DataModels.FoodModifier", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("food_modifiers", (string)null);
                });

            modelBuilder.Entity("Justice.Dash.Server.DataModels.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Prompt")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RevisedPrompt")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("images", (string)null);
                });

            modelBuilder.Entity("Justice.Dash.Server.DataModels.MenuItem", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("CorrectedFoodName")
                        .HasColumnType("longtext");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<string>("Day")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("FoodContents")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid?>("FoodModifierId")
                        .HasColumnType("char(36)");

                    b.Property<string>("FoodName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid?>("ImageId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("NeedsDescription")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsFoodContents")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsImageRegeneration")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsNameCorrection")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsVeganDescription")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsVeganImageRegeneration")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("NeedsVeganization")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("VeganizedDescription")
                        .HasColumnType("longtext");

                    b.Property<string>("VeganizedFoodName")
                        .HasColumnType("longtext");

                    b.Property<Guid?>("VeganizedImageId")
                        .HasColumnType("char(36)");

                    b.Property<int>("WeekNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Date")
                        .IsUnique();

                    b.HasIndex("FoodModifierId");

                    b.HasIndex("ImageId");

                    b.HasIndex("VeganizedImageId");

                    b.ToTable("menu_items", (string)null);
                });

            modelBuilder.Entity("Justice.Dash.Server.DataModels.Photo", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<long>("AlbumAddDate")
                        .HasColumnType("bigint");

                    b.Property<int>("Height")
                        .HasColumnType("int");

                    b.Property<long>("ImageUpdateDate")
                        .HasColumnType("bigint");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Uid");

                    b.ToTable("photos", (string)null);
                });

            modelBuilder.Entity("Justice.Dash.Server.DataModels.Surveillance", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("char(36)");

                    b.Property<string>("Responsible")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("Week")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Type", "Week", "Year")
                        .IsUnique();

                    b.ToTable("surveillance", (string)null);
                });

            modelBuilder.Entity("Justice.Dash.Server.DataModels.MenuItem", b =>
                {
                    b.HasOne("Justice.Dash.Server.DataModels.FoodModifier", "FoodModifier")
                        .WithMany()
                        .HasForeignKey("FoodModifierId");

                    b.HasOne("Justice.Dash.Server.DataModels.Image", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId");

                    b.HasOne("Justice.Dash.Server.DataModels.Image", "VeganizedImage")
                        .WithMany()
                        .HasForeignKey("VeganizedImageId");

                    b.Navigation("FoodModifier");

                    b.Navigation("Image");

                    b.Navigation("VeganizedImage");
                });
#pragma warning restore 612, 618
        }
    }
}
