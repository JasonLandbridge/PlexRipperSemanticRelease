﻿namespace Application.Contracts;

public class PlexApiClientProgress
{
    #region Properties

    public int RetryAttemptIndex { get; set; }

    public int RetryAttemptCount { get; set; }

    public int TimeToNextRetry { get; set; }

    public int StatusCode { get; set; }

    public bool ConnectionSuccessful { get; set; }

    public bool Completed { get; set; }

    public string Message { get; set; }

    public string ErrorMessage { get; set; }

    #endregion
}