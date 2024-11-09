using BattleGame;
using BattleGame.Gaming;
using BattleGame.PlayerSystem;

namespace RPS {

    public class RPSUIConsole : GameRunnerBattler<Gesture, Result>.IUser, GameRunnerSpectator<Gesture, Result>.IUser {

        public RPSGame Game { get; }
        public Pair<Player> Players { get; }

        public RPSUIConsole(RPSGame game, Pair<Player> players) {
            Game = game;
            Players = players;
        }

        public void OnStarted() {
            Console.Clear();
            ConsoleUI.PrintLine($"%-f%+0RPS%= %+b{Players.here.name} %=vs. %+d{Players.there.name}");
        }

        public async Task<Gesture> GetOperationAsync()
            => await Task.Run(GetOperation);

        private Gesture GetOperation() {
            var res = (Gesture)ConsoleUIConvertChain.Ask("Input your gesture (0: Rock, 1: Paper, 2: Scissors): ")
                .TryConvert<string, int>(int.TryParse, "%+cPlease input 1, 2 or 3.")
                .GetValue();
            ConsoleUI.PrintLine("Waiting for opponent's gesture...");
            return res;
        }

        public void OnGotOperation(Gesture operation) { }

        public void OnGotOperation0(Gesture operation)
            => ConsoleUI.PrintLine($"%+b{Players.here.name} has chose {operation}.");

        public void OnGotOperation1(Gesture operation)
            => ConsoleUI.PrintLine($"%+d{Players.there.name} has chose {operation}.");

        public void OnNewRound() {
            ConsoleUI.PrintIntoLineAt(Console.WindowHeight - 1);
            ConsoleUI.PrintLine();
            ConsoleUI.PrintLine($"%+0%-fRound {Game.RoundCount} | {Game.Score.here}:{Game.Score.there}");
        }

        public void OnOperated(Result result) {
            switch(result.result) {
            case Result.RPSResult.Tie:
                ConsoleUI.PrintLine("%+0%-7Tie!");
                break;
            case Result.RPSResult.Win:
                ConsoleUI.PrintLine($"%+0%-b{Players.here.name} Wins!");
                break;
            case Result.RPSResult.Lose:
                ConsoleUI.PrintLine($"%+0%-d{Players.there.name} Wins!");
                break;
            }
        }

    }

}
