using BattleGame;
using BattleGame.Gaming;

namespace RPS {

    public enum Gesture {
        Rock, Paper, Scissors
    }

    public struct Result : ITurnResult {
        public enum RPSResult {
            Tie, Win, Lose
        }
        public readonly bool End => false;
        public RPSResult result;
        public Result(RPSResult result) {
            this.result = result;
        }
    }

    public class RPSGame : Game<Gesture, Result> {

        public override string Info => "assembly.rps@battle-game.1.0";

        public int RoundCount { get; private set; }
        public Pair<int> Score => score;
        private Pair<int> score;

        public RPSGame(bool reversed) : base(reversed) {
            RoundCount = 1;
            score = new(0, 0);
        }

        public override Result Operate(Pair<Gesture> operation) {
            var result = ((operation.here - operation.there + 3) % 3) switch {
                0 => Result.RPSResult.Tie,
                1 => Result.RPSResult.Win,
                2 => Result.RPSResult.Lose,
                _ => throw new NotImplementedException()
            };
            switch(result) {
            case Result.RPSResult.Win:
                score.here++;
                break;
            case Result.RPSResult.Lose:
                score.there++;
                break;
            }
            RoundCount++;
            return new(result);
        }

    }

}
