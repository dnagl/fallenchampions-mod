using System;
using System.Collections.Generic;
using System.Linq;
using FallenChampions.ApiClient;
using FallenChampions.Model;
using TowerFall;
using EventLog = TowerFall.EventLog;
using GameMode = FallenChampions.Model.GameMode;
using GameStats = FallenChampions.Model.GameStats;
using Session = TowerFall.Session;
using VersusRoundResults = TowerFall.VersusRoundResults;

namespace FallenChampions.Game
{
    public class GameStatsInterceptor
    {
        private GameStats _currentGameStats = null;
        
        public void OnVersusRoundResults(On.TowerFall.VersusRoundResults.orig_ctor orig, VersusRoundResults self,
            Session session, List<EventLog> events)
        {
            orig(self, session, events);

            var matchStats = session.MatchStats;

            if (!matchStats.Any(x => x.GotWin))
                return;

            var playerStats = GetPlayerStats(matchStats);
            var playerWithHighestKills = playerStats.OrderByDescending(x => x.Kills).First();
            var winningPlayerIdx = playerStats.FindIndex(x => x.Color == playerWithHighestKills.Color);
            var winningPlayerColor = playerStats[winningPlayerIdx].Color;
            
            var gameStats = new GameStats
            {
                GameId = _currentGameStats.GameId,
                Mode = _currentGameStats.Mode,
                StartDate = _currentGameStats.StartDate,
                EndDate = DateTime.Now,
                ArrowsShot = matchStats.Sum(x => x.ArrowsShot),
                ArrowsCaught = matchStats.Sum(x => x.ArrowsShot),
                ShieldsBroken = matchStats.Sum(x => x.ShieldsBroken),
                Kills = matchStats.Sum(x => (double)x.Kills.Total),
                Deaths = matchStats.Sum(x => (double)x.Deaths.Total),
                Jumps = matchStats.Sum(x => x.Jumps),
                Dodges = matchStats.Sum(x => x.Dodges),
                Variants = GetVariants(session),
                PlayerStats = GetPlayerStats(matchStats),
            };
            
            var winnerIdx = gameStats.PlayerStats.FindIndex(x => x.Color == winningPlayerColor);
            
            if (gameStats.PlayerStats.Count(x => x.Kills == gameStats.PlayerStats[winnerIdx].Kills) > 1)
                return; // Is a draw and will go to overtime
            
            gameStats.PlayerStats[winnerIdx].GotWin = true;
            gameStats.PlayerStats = gameStats.PlayerStats.Where(PlayerParticipated).ToList();
            
            var client = new GameStatClient();
            client.SendStats(gameStats);
        }
        
        public void VersusStartOnctor(On.TowerFall.VersusStart.orig_ctor orig, TowerFall.VersusStart self, Session session)
        {
            _currentGameStats = new GameStats
            {
                GameId = Guid.NewGuid().ToString(),
                Mode = GetGameMode(session),
                StartDate = DateTime.Now,
            };
            orig(self, session);
        }
        
        private bool PlayerParticipated(PlayerStats playerStats) =>
            playerStats.Kills > 0 || playerStats.Deaths > 0;

        private List<string> GetVariants(Session session) =>
            session.MatchSettings.Variants.Variants.Where(x => x.AllTrue).Select(x => x.Title).ToList();
        
        private List<PlayerStats> GetPlayerStats(MatchStats[] matchStats)
        {
            return new List<PlayerStats>
            {
                new PlayerStats
                {
                    Color = PlayerColor.Green,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Green),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Green)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Blue,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Blue),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Blue)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Pink,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Pink),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Pink)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Orange,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Orange),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Orange)
                },
                new PlayerStats
                {
                    Color = PlayerColor.White,
                    Deaths = matchStats.Sum(x => (double)x.Kills.White),
                    Kills = matchStats.Sum(x => (double)x.Deaths.White)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Yellow,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Yellow),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Yellow)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Cyan,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Cyan),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Cyan)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Purple,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Purple),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Purple)
                },
                new PlayerStats
                {
                    Color = PlayerColor.Red,
                    Deaths = matchStats.Sum(x => (double)x.Kills.Red),
                    Kills = matchStats.Sum(x => (double)x.Deaths.Red)
                }
            };
        }
        
        private GameMode GetGameMode(Session session)
        {
            switch (session.MatchSettings.Mode)
            {
                case Modes.Quest:
                    return GameMode.Quest;
                case Modes.DarkWorld:
                    return GameMode.DarkWorld;
                case Modes.Trials:
                    return GameMode.Trials;
                case Modes.LastManStanding:
                    return GameMode.LastManStanding;
                case Modes.HeadHunters:
                    return GameMode.HeadHunters;
                case Modes.TeamDeathmatch:
                    return GameMode.TeamDeathmatch;
                case Modes.Warlord:
                    return GameMode.Warlord;
                case Modes.EditorTest:
                    return GameMode.EditorTest;
                case Modes.EditorPreview:
                    return GameMode.EditorPreview;
                case Modes.LevelTest:
                    return GameMode.LevelTest;
                case Modes.Custom:
                    return GameMode.Custom;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}