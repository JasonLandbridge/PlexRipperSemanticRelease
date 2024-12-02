using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class MadePercentageRunTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DownloadPercentage", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "FileTransferPercentage", table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(name: "DownloadPercentage", table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(name: "FileTransferPercentage", table: "DownloadTaskMovieFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AddColumn<decimal>(
                    name: "DownloadPercentage",
                    table: "DownloadTaskTvShowEpisodeFile",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: 0m
                )
                .Annotation("Relational:ColumnOrder", 4);

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
                .AddColumn<decimal>(
                    name: "DownloadPercentage",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: 0m
                )
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder
                .AddColumn<decimal>(
                    name: "FileTransferPercentage",
                    table: "DownloadTaskMovieFile",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: 0m
                )
                .Annotation("Relational:ColumnOrder", 4);
        }
    }
}
