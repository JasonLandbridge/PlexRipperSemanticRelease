using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static Faker<FileTask> GetFileTask(int seed, int filePathsCount = 4) =>
        GetFileTask(new Seed(seed), filePathsCount);

    public static Faker<FileTask> GetFileTask(Seed seed, int filePathsCount = 4)
    {
        return new Faker<FileTask>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
            .RuleFor(x => x.DestinationDirectory, f => f.System.DirectoryPath())
            .RuleFor(
                x => x.FilePathsCompressed,
                f =>
                {
                    var paths = new List<string>();
                    for (var i = 0; i < filePathsCount; i++)
                    {
                        paths.Add(f.System.FilePath());
                    }

                    return string.Join(";", paths);
                }
            )
            .RuleFor(x => x.CurrentFilePathIndex, _ => 0)
            .RuleFor(x => x.FileName, f => f.System.FileName())
            .RuleFor(x => x.FileSize, f => f.Random.Long(1_000_000, 10_000_000))
            .RuleFor(x => x.DownloadTaskId, f => f.Random.Guid())
            .RuleFor(x => x.DownloadTaskType, f => f.PickRandom<DownloadTaskType>())
            .RuleFor(x => x.CurrentBytesOffset, f => f.Random.Long(0, 1_000_000))
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexServerId, f => f.Random.Int(1, 100))
            .RuleFor(x => x.PlexLibrary, _ => null)
            .RuleFor(x => x.PlexLibraryId, f => f.Random.Int(1, 100));
    }
}
