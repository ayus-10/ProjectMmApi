﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProjectMmApi.Data;

#nullable disable

namespace ProjectMmApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250125130632_UpdateMessageModel")]
    partial class UpdateMessageModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Conversation", b =>
                {
                    b.Property<Guid>("ConversationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("FriendId")
                        .HasColumnType("char(36)");

                    b.Property<bool>("IsSeen")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid>("LastMessageId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("LastMessageTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("ConversationId");

                    b.HasIndex("FriendId")
                        .IsUnique();

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Friend", b =>
                {
                    b.Property<Guid>("FriendId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("ConversationId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("ReceiverId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("FriendId");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("SenderId");

                    b.ToTable("Friends");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Message", b =>
                {
                    b.Property<Guid>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("ConversationId")
                        .HasColumnType("char(36)");

                    b.Property<string>("MessageText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("char(36)");

                    b.HasKey("MessageId");

                    b.HasIndex("ConversationId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Conversation", b =>
                {
                    b.HasOne("ProjectMmApi.Models.Entities.Friend", "ByFriend")
                        .WithOne("HasConversation")
                        .HasForeignKey("ProjectMmApi.Models.Entities.Conversation", "FriendId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ByFriend");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Friend", b =>
                {
                    b.HasOne("ProjectMmApi.Models.Entities.User", "Receiver")
                        .WithMany("ReceivedRequests")
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ProjectMmApi.Models.Entities.User", "Sender")
                        .WithMany("SentRequests")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Message", b =>
                {
                    b.HasOne("ProjectMmApi.Models.Entities.Conversation", "FromConversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ProjectMmApi.Models.Entities.User", "Sender")
                        .WithMany("SentMessages")
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FromConversation");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Conversation", b =>
                {
                    b.Navigation("Messages");
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.Friend", b =>
                {
                    b.Navigation("HasConversation")
                        .IsRequired();
                });

            modelBuilder.Entity("ProjectMmApi.Models.Entities.User", b =>
                {
                    b.Navigation("ReceivedRequests");

                    b.Navigation("SentMessages");

                    b.Navigation("SentRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
