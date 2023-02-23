using Environment;
using Logging.Enricher;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.Console.LogThemes;
using Xunit.Abstractions;

namespace Logging;

public static class LogConfig
{
    #region Properties

    public static readonly string Template =
        $"{{NewLine}}{{Timestamp:HH:mm:ss}} [{{Level}}] [{{{ClassNamePropertyName}}}.{{{MemberNamePropertyName}}}:{{{LineNumberPropertyName}}}] => {{Message}}{{NewLine}}{{Exception}}";

    public static MessageTemplateTextFormatter TemplateTextFormatter => new(Template);

    public static string ClassNamePropertyName => "ClassName";
    public static string MemberNamePropertyName => "MemberName";
    public static string LineNumberPropertyName => "LineNumber";

    #endregion

    #region Methods

    #region Private

    public static LoggerConfiguration GetBaseConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Quartz", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.With<ExternalFrameworkEnricher>()
            .WriteTo.Debug(outputTemplate: Template)
            .WriteTo.Console(theme: LogThemes.SystemColored, outputTemplate: Template);
    }

    #endregion

    #region Public

    public static void SetTestOutputHelper(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    #endregion

    #endregion

    private static ITestOutputHelper _testOutput;

    public static Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        if (_testOutput is null)
        {
            return GetBaseConfiguration()
                .WriteTo.File(
                    TemplateTextFormatter,
                    Path.Combine(PathProvider.LogsDirectory, "log.txt"),
                    LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .MinimumLevel.Is(minimumLogLevel)
                .CreateLogger();
        }

        // Test Logger
        return GetBaseConfiguration()
            .MinimumLevel.Is(minimumLogLevel)
            .WriteTo.TestOutput(_testOutput, TemplateTextFormatter, minimumLogLevel)
            .WriteTo.TestCorrelator(minimumLogLevel)
            .CreateLogger();
    }
}