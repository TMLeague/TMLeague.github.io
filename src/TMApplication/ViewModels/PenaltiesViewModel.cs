namespace TMApplication.ViewModels;

public class PenaltiesViewModel : List<PenaltyViewModel>
{

}

public class PenaltyViewModel
{
    public PenaltyViewModel(string player, string gameName, double points, string details, bool isValid)
    {
        Player = player;
        GameName = gameName;
        Points = points;
        Details = details;
    }

    public string Player { get; set; }
    public string GameName { get; set; }
    public double Points { get; set; }
    public string Details { get; set; }
    public bool IsValid { get; set; } = true;
}