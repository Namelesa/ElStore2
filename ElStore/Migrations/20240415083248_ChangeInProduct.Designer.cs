﻿// <auto-generated />
using ElStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ElStore.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240415083248_ChangeInProduct")]
    partial class ChangeInProduct
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ElStore.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("ElStore.Models.DescriptionPC", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BackCamera")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FrontCamera")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RAM")
                        .HasColumnType("integer");

                    b.Property<int>("ROM")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DescriptionPC");
                });

            modelBuilder.Entity("ElStore.Models.HearphoneDescriptions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Design")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TypeConnections")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("speaker")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.ToTable("HearphoneDescriptions");
                });

            modelBuilder.Entity("ElStore.Models.Images", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string[]>("Image")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("Video")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ElStore.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Battery")
                        .HasColumnType("integer");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<int>("DescriptioHId")
                        .HasColumnType("integer");

                    b.Property<int>("DescriptioPCId")
                        .HasColumnType("integer");

                    b.Property<int>("DescriptionHId")
                        .HasColumnType("integer");

                    b.Property<int>("DescriptionPCId")
                        .HasColumnType("integer");

                    b.Property<int>("ImageId")
                        .HasColumnType("integer");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Price")
                        .HasColumnType("double precision");

                    b.Property<string>("ShortDescription")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("DescriptioHId");

                    b.HasIndex("DescriptioPCId");

                    b.HasIndex("ImageId");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("ElStore.Models.Product", b =>
                {
                    b.HasOne("ElStore.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ElStore.Models.HearphoneDescriptions", "HearphoneDescriptions")
                        .WithMany()
                        .HasForeignKey("DescriptioHId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ElStore.Models.DescriptionPC", "DescriptionPC")
                        .WithMany()
                        .HasForeignKey("DescriptioPCId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ElStore.Models.Images", "Images")
                        .WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("DescriptionPC");

                    b.Navigation("HearphoneDescriptions");

                    b.Navigation("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
