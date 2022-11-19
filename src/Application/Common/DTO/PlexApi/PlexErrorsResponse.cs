using Newtonsoft.Json;

namespace PlexRipper.Application;

public class PlexErrorsResponse
{
    [JsonProperty(Required = Required.Always)]
    public List<PlexError> Errors { get; set; }
}

public class PlexError : Error
{
    [JsonProperty(Required = Required.Always)]
    public int Code { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Status { get; set; }
}