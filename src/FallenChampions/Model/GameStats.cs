using System;
using System.Collections.Generic;

namespace FallenChampions.Model
{
    public class GameStats
    {
        public string GameId { get; set; }
        public GameMode Mode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long ArrowsShot { get; set; }
        public long ArrowsCaught { get; set; }
        public long ArrowsCollected { get; set; }
        public long OwnArrowsCaught { get; set; }
        public long ArrowsStolen { get; set; }
        public long ShieldsBroken { get; set; }
        public double Kills { get; set; }
        public double Deaths { get; set; }
        public long Jumps { get; set; }
        public long Dodges { get; set; }
        public List<string> Variants { get; set; }
        public List<PlayerStats> PlayerStats { get; set; }
        
    }

    public enum GameMode
    {
        Quest,
        DarkWorld,
        Trials,
        LastManStanding,
        HeadHunters,
        TeamDeathmatch,
        Warlord,
        EditorTest,
        EditorPreview,
        LevelTest,
        Custom
    }
}