namespace Justice.Dash.Server.DataModels;

public class Photo : BaseDataModel
{
    public required string Path { get; set; }
    public required long ImageUpdateDate { get; set; }
    public required long AlbumAddDate { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required string Uid { get; set; }
}