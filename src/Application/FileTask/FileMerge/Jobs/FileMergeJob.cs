using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class FileMergeJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;

    public FileMergeJob(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(FileMergeJob));

    public async Task Execute(IJobExecutionContext context)
    {
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);
            if (downloadTaskKey is null)
            {
                ResultExtensions.IsNull(nameof(DownloadTaskKey)).LogError();
                return;
            }

            _log.Here()
                .Debug(
                    "Executing job: {NameOfFileMergeJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                    nameof(FileMergeJob),
                    nameof(downloadTaskKey),
                    downloadTaskKey.Id
                );

            await _mediator.Send(new MergeFilesFromFileTaskCommand(downloadTaskKey), context.CancellationToken);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
