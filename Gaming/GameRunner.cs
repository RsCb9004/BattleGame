namespace BattleGame.Gaming {

    public interface IGameRunner {
        public void Run();
    }

    public abstract class GameRunner<TOperation, TResult> : IGameRunner
        where TResult : ITurnResult {

        public class InfoMismatchException : Exception { }
        public class StateDifferentException : Exception { }

        public Game<TOperation, TResult> Game { get; protected set; }

        public GameRunner() {
            Game = default!;
        }

        public void Run() {
            Start();
            while(true) {
                TResult result = Game.Operate(GetOperations());
                OnOperated(result);
                if(result.End) break;
            }
        }

        protected abstract Pair<TOperation> GetOperations();
        protected abstract void OnOperated(TResult result);
        protected abstract void Start();

    }

}
