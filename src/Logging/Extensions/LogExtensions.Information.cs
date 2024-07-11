using Logging.Common;
using Serilog.Core;
using Serilog.Events;

namespace Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogMetaData Information<T0>(this LogMetaData logMetaData, string messageTemplate, T0 propertyValue0)
    {
        logMetaData.Update(LogEventLevel.Information, messageTemplate, propertyValue0).Write();
        return logMetaData;
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogMetaData Information<T0, T1>(
        this LogMetaData logMetaData,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1
    )
    {
        logMetaData.Update(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1).Write();
        return logMetaData;
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogMetaData Information<T0, T1, T2>(
        this LogMetaData logMetaData,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2
    )
    {
        logMetaData
            .Update(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1, propertyValue2)
            .Write();
        return logMetaData;
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static LogMetaData Information<T0, T1, T2, T3>(
        this LogMetaData logMetaData,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        T3 propertyValue3
    )
    {
        logMetaData
            .Update(
                LogEventLevel.Information,
                messageTemplate,
                propertyValue0,
                propertyValue1,
                propertyValue2,
                propertyValue3
            )
            .Write();
        return logMetaData;
    }
}
