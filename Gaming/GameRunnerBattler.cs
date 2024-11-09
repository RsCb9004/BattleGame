using BattleGame.Messaging;

namespace BattleGame.Gaming {

    public class GameRunnerBattler<TOperation, TResult> : GameRunner<TOperation, TResult>
        where TResult : ITurnResult {

        public interface IUser {
            public Task<TOperation> GetOperationAsync();
            public void OnStarted();
            public void OnNewRound();
            public void OnGotOperation(TOperation operation);
            public void OnOperated(TResult result);
        }

        public IUser UI { get; }

        protected Messager messager;

        public GameRunnerBattler(
            IUser ui,
            Game<TOperation, TResult> game,
            Messager messager
        ) {
            UI = ui; Game = game; this.messager = messager;
        }

        protected override void Start() {
            if(!messager.OperateExchangeString(Game.Info, Equals))
                throw new InfoMismatchException();
            UI.OnStarted();
        }

        protected override Pair<TOperation> GetOperations() {
            UI.OnNewRound();
            Task<TOperation> taskHere = GetHitherOperationAsync();
            Task<TOperation> taskThere = GetThitherOperationAsync();
            TOperation hitherOperation = taskHere.Result;
            TOperation thitherOperation = taskThere.Result;
            return new(hitherOperation, thitherOperation);
        }

        private async Task<TOperation> GetHitherOperationAsync() {
            TOperation operation = await UI.GetOperationAsync();
            messager.SendObj(operation);
            return operation;
        }

        private async Task<TOperation> GetThitherOperationAsync() {
            TOperation res = await messager.GetObjAsync<TOperation>();
            UI.OnGotOperation(res);
            return res;
        }

        protected override void OnOperated(TResult result) {
            if(!messager.OperateExchange(Game.GetStateHash(), Enumerable.SequenceEqual))
                throw new StateDifferentException();
            UI.OnOperated(result);
        }

    }

}
