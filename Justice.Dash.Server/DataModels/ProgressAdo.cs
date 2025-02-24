namespace Justice.Dash.Server.DataModels;

public class ProgressAdo : BaseDataModel
{
    public int CompletedItems { get; set; }
    public int TotalItems { get; set; }
    public double PercentageCompletion => TotalItems == 0 ? 0 : (double)CompletedItems / TotalItems * 100;
}