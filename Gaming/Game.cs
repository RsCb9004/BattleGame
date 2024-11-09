namespace BattleGame.Gaming {

    public interface ITurnResult {
        public bool End { get; }
    }

    public abstract class Game<TOperation, TResult>
        where TResult : ITurnResult {

        public abstract string Info { get; }

        public bool Reversed { get; }

        public Game(bool reversed) {
            Reversed = reversed;
        }

        public abstract TResult Operate(Pair<TOperation> operation);

        public virtual byte[] GetStateHash() => [];

    }
}
