using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFileTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FileTasks");

            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "DownloadPercentage"
            );

            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "DownloadTaskMovieFile",
                newName: "DownloadPercentage"
            );

            migrationBuilder.AddColumn<long>(
                name: "CurrentFileTransferBytesOffset",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<int>(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder
                .AddColumn<decimal>(
                    name: "FileTransferPercentage",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: 0m
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<long>(
                    name: "FileDataTransferred",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<long>(
                name: "CurrentFileTransferBytesOffset",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<int>(
                name: "CurrentFileTransferPathIndex",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder
                .AddColumn<decimal>(
                    name: "FileTransferPercentage",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: 0m
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<long>(
                    name: "FileDataTransferred",
                    table: "DownloadTaskMovieFile",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: 0L
                )
                .Annotation("Relational:ColumnOrder", 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CurrentFileTransferBytesOffset", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "CurrentFileTransferPathIndex", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "FileTransferPercentage", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "FileDataTransferred", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "CurrentFileTransferBytesOffset", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "CurrentFileTransferPathIndex", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "FileTransferPercentage", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "FileDataTransferred", table: "DownloadTaskMovieFile");

            migrationBuilder.RenameColumn(
                name: "DownloadPercentage",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "Percentage"
            );

            migrationBuilder.RenameColumn(
                name: "DownloadPercentage",
                table: "DownloadTaskMovieFile",
                newName: "Percentage"
            );

            migrationBuilder.CreateTable(
                name: "FileTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexServerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentBytesOffset = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentFilePathIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationDirectory = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadTaskType = table.Column<string>(
                        type: "TEXT",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePathsCompressed = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTasks_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_FileTasks_PlexServers_PlexServerId",
                        column: x => x.PlexServerId,
                        principalTable: "PlexServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_FileTasks_PlexLibraryId",
                table: "FileTasks",
                column: "PlexLibraryId"
            );

            migrationBuilder.CreateIndex(name: "IX_FileTasks_PlexServerId", table: "FileTasks", column: "PlexServerId");
        }
    }
}
