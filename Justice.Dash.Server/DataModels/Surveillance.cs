using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Justice.Dash.Server.DataModels;

public class Surveillance : BaseDataModel
{
    public required SurveillanceType Type { get; set; }
    public required int Week { get; set; }
    public required int Year { get; set; } = DateTime.Now.Year;
    public required string Responsible { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SurveillanceType
{
    [EnumMember(Value = "MDM")]
    MDM = 0,
    [EnumMember(Value = "EDI")]
    EDI = 1
}