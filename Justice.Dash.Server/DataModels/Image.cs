namespace Justice.Dash.Server.DataModels;

public class Image : BaseDataModel
{
    public required string Path { get; set; }
    public required string Prompt { get; set; }
    public string? RevisedPrompt { get; set; }
}