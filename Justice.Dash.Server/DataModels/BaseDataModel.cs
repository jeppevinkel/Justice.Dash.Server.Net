using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Justice.Dash.Server.DataModels;

public abstract class BaseDataModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonIgnore]
    public Guid Id { get; set; } = Guid.NewGuid();
}