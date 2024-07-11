using System.Runtime.CompilerServices;
using Logging.Common;
using Logging.Interface;
using Serilog.Core;
using Serilog.Events;

namespace Logging;

public partial class Log : ILog
{
    #region Warning

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData WarningLine(
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Warning, messageTemplate, sourceFilePath, memberName, sourceLineNumber);

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Warning, ex, messageTemplate, sourceFilePath, memberName, sourceLineNumber);

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) => Write(LogEventLevel.Warning, messageTemplate, sourceFilePath, memberName, sourceLineNumber, propertyValue);

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Warning,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Warning,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T0, T1, T2, T3>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Warning,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T0, T1, T2, T3, T4>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Warning,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3,
            propertyValue4
        );

    [MessageTemplateFormatMethod("messageTemplate")]
    public LogMetaData Warning<T0, T1, T2, T3, T4, T5>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3,
        T4 propertyValue4,
        T5 propertyValue5,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    ) =>
        Write(
            LogEventLevel.Warning,
            messageTemplate,
            sourceFilePath,
            memberName,
            sourceLineNumber,
            propertyValue0,
            propertyValue1,
            propertyValue2,
            propertyValue3,
            propertyValue4,
            propertyValue5
        );

    #endregion
}
