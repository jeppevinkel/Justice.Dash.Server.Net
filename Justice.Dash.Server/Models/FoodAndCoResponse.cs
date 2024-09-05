using System.Text.Json.Serialization;

namespace Justice.Dash.Server.Models;

[JsonSerializable(typeof(FoodAndCoResponse))]
public class FoodAndCoResponse
{
    public int WeekNumber { get; set; }
    public DateTime FirstDateOfWeek { get; set; }
    public List<Day> Days { get; set; } = [];

    public class MenuItem
    {
        public required string Type { get; set; }
        public required string Menu { get; set; }
        public required string FriendlyUrl { get; set; }
        public required string Image { get; set; }
    }
    
    public class Day
    {
        public string DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public List<MenuItem> Menus { get; set; } = [];
    }
}