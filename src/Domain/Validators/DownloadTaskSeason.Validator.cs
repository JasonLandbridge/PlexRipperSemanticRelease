using FluentValidation;

namespace PlexRipper.Domain.Validators;

public class DownloadTaskSeasonValidator : AbstractValidator<DownloadTaskTvShowSeason>
{
    public DownloadTaskSeasonValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        RuleFor(x => x.Key).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.FullTitle).NotEmpty();
        RuleFor(x => x.Year).GreaterThan(0);
        RuleFor(x => x.DataTotal).GreaterThan(0);
        RuleFor(x => x.DownloadStatus).Equal(DownloadStatus.Queued);
        RuleFor(x => x.Created).GreaterThan(DateTime.MinValue);

        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.PlexServerId).GreaterThan(0);

        RuleFor(x => x.DownloadTaskType).Equal(DownloadTaskType.Season);
        RuleFor(x => x.MediaType).Equal(PlexMediaType.Season);
        RuleFor(x => x.DataTotal).GreaterThan(0);
        RuleForEach(x => x.Children).SetValidator(new DownloadTaskEpisodeValidator());
    }
}