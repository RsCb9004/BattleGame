using BattleGame.Messaging;

namespace BattleGame.Gaming {

    public class GameRunnerSpectator<TOperation, TResult> : GameRunner<TOperation, TResult>
        where TResult : ITurnResult {

        public interface IUser {
            public void OnStarted();
            public void OnNewRound();
            public void OnGotOperation0(TOperation operation);
            public void OnGotOperation1(TOperation operation);
            public void OnOperated(TResult result);
        }

        public IUser UI { get; }

        private Pair<Listener> listeners;

        public GameRunnerSpectator(
            IUser ui,
            Game<TOperation, TResult> game,
            Pair<Listener> listeners
        ) {
            UI = ui; Game = game; this.listeners = listeners;
        }

        public GameRunnerSpectator(
            IUser ui,
            Game<TOperation, TResult> game,
            Messager messager0,
            Messager messager1
        ) :
            this(ui, game, new(messager0, messager1)) { }

        protected override void Start() {
            Pair<string> infos = listeners.ConvertAll((messager) => messager.GetString());
            if(infos.here != infos.there)
                throw new InfoMismatchException();
            UI.OnStarted();
        }

        protected override Pair<TOperation> GetOperations() {
            UI.OnNewRound();
            Task<TOperation> task0 = GetOperationAsync(listeners.here, UI.OnGotOperation0);
            Task<TOperation> task1 = GetOperationAsync(listeners.there, UI.OnGotOperation1);
            return new(task0.Result, task1.Result);
        }

        private static async Task<TOperation> GetOperationAsync(
            Listener messager, Action<TOperation>? onGot
        ) {
            TOperation res = await messager.GetObjAsync<TOperation>();
            onGot?.Invoke(res);
            return res;
        }

        protected override void OnOperated(TResult result) {
            Pair<byte[]> stateHashes = listeners.ConvertAll((messager) => messager.Get());
            if(!Enumerable.SequenceEqual(stateHashes.here, stateHashes.there))
                throw new StateDifferentException();
            UI.OnOperated(result);
        }

    }

}
