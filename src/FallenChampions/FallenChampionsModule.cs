using FallenChampions.Game;
using FortRise;
using On.TowerFall;
using Session = TowerFall.Session;

namespace FallenChampions
{
    [Fort("dev.dnagl.fallenChampions", "FallenChampions")]
    public class FallenChampionsModule : FortModule
    {
        private readonly GameStatsInterceptor _gameStatsInterceptor;

        public FallenChampionsModule()
        {
            _gameStatsInterceptor = new GameStatsInterceptor();
        }

        public override void Load()
        {
            VersusRoundResults.ctor += _gameStatsInterceptor.OnVersusRoundResults;
            On.TowerFall.VersusStart.ctor += _gameStatsInterceptor.VersusStartOnctor;
        }

        public override void Unload()
        {
            VersusRoundResults.ctor -= _gameStatsInterceptor.OnVersusRoundResults;
        }
    }
}