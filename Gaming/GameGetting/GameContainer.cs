using BattleGame.Messaging;
using BattleGame.PlayerSystem;

namespace BattleGame.Gaming.GameGetting {

    public static class GameContainer {

        public interface IGame {
            string GameId { get; }
            IGameRunner GetGameRunnerBattler(Messager messager, bool reversed, Pair<Player> players);
            IGameRunner GetGameRunnerSpectator(Pair<Listener> listeners, bool reversed, Pair<Player> players);
        }

        public static Dictionary<string, IGame> GameList { get; } = [];

        public static void Add(IGame game) => GameList[game.GameId] = game;

    }

}
