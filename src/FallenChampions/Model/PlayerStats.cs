namespace FallenChampions.Model
{
    public class PlayerStats
    {
        
        public PlayerColor Color { get; set; }
        public double Kills { get; set; }
        public double Deaths { get; set; }
        public bool GotWin { get; set; }
    }

    public enum PlayerColor
    {
        Green,
        Blue,
        Pink,
        Orange,
        White,
        Yellow,
        Cyan,
        Purple,
        Red
    }
}