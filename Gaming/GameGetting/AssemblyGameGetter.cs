using BattleGame.Messaging;
using BattleGame.PlayerSystem;
using System.Reflection;

namespace BattleGame.Gaming.GameGetting {

    public static class AssemblyGameGetter {

        public class GameLoadException : Exception { }

        public interface IAssemblyGameInfo {
            string GameId { get; }
            Type OperationType { get; }
            Type ResultType { get; }
            Type GameType { get; }
            Type BattlerUIType { get; }
            Type SpectatorUIType { get; }
        }

        public class AssemblyGame : GameContainer.IGame {

            private readonly IAssemblyGameInfo info;

            public AssemblyGame(IAssemblyGameInfo info) {
                this.info = info;
            }

            public string GameId => info.GameId;

            public IGameRunner GetGameRunnerBattler(Messager messager, bool reversed, Pair<Player> players) {
                var game = Activator.CreateInstance(info.GameType, [reversed]);
                var ui = Activator.CreateInstance(info.BattlerUIType, [game, players]);
                var gameRunnerType = typeof(GameRunnerBattler<,>).MakeGenericType(info.OperationType, info.ResultType);
                return Activator.CreateInstance(gameRunnerType, [ui, game, messager]) as IGameRunner ??
                    throw new GameLoadException();
            }

            public IGameRunner GetGameRunnerSpectator(Pair<Listener> listeners, bool reversed, Pair<Player> players) {
                var game = Activator.CreateInstance(info.GameType, [reversed]);
                var ui = Activator.CreateInstance(info.SpectatorUIType, [game, players]);
                var gameRunnerType = typeof(GameRunnerSpectator<,>).MakeGenericType(info.OperationType, info.ResultType);
                return Activator.CreateInstance(gameRunnerType, [ui, game, listeners]) as IGameRunner ??
                    throw new GameLoadException();
            }
        }

        public static void AssignGame(Assembly assembly) {
            var games = assembly.GetTypes()
                .Where((type) => type.IsAssignableTo(typeof(IAssemblyGameInfo)))
                .Select((type) => Activator.CreateInstance(type) as IAssemblyGameInfo)
                .Where((info) => info != null)
                .Select((info) => new AssemblyGame(info!))
                .ToArray();
            Array.ForEach(games, GameContainer.Add);
        }

    }

}
