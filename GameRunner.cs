using BattleGame.Messager;
using BattleGame.TurnBasedGame.RPS;

namespace BattleGame.TurnBasedGame {

	public static class GameRunner {

		public enum GameType {
			RPS
		}

		public interface IBattlerUIGetter {
			public GameRunnerBattler<RPSGame.GameState, RPSGame.Gesture>.IUser GetRpsBattlerUI();
		}

		public interface ISpectatorUIGetter {
			public GameRunnerSpectator<RPSGame.GameState, RPSGame.Gesture>.IUser GetRpsSpectatorUI();
		}

		public static IGameRunner GetGameRunnerBattler(
			IBattlerUIGetter uiGetter, GameType type, IMessager messager
		) => type switch {
			GameType.RPS =>
				new GameRunnerBattler<RPSGame.GameState, RPSGame.Gesture>(
					uiGetter.GetRpsBattlerUI(), new RPSGame(), messager
				),
			_ => throw new NotImplementedException()
		};

		public static IGameRunner GetGameRunnerSpectator(
			ISpectatorUIGetter uiGetter, GameType type, Joint<IListener> listeners
		) => type switch {
			GameType.RPS =>
				new GameRunnerSpectator<RPSGame.GameState, RPSGame.Gesture>(
					uiGetter.GetRpsSpectatorUI(), new RPSGame(), listeners
				),
			_ => throw new NotImplementedException()
		};
	}

	public interface IGameRunner {
		public void Run();
	}

	public abstract class GameRunner<TState, TOperation> : IGameRunner {

		public class InfoMismatchException : Exception { }
		public class StateDifferentException : Exception { }

		public IGame<TState, TOperation> Game { get; protected set; }

		public GameRunner() {
			Game = default!;
		}

		public void Run() {
			Start();
			while(!Game.Operate(GetOperations()))
				OnOperated();
		}

		protected abstract Joint<TOperation> GetOperations();
		protected abstract void OnOperated();
		protected abstract void Start();
	}

	public class GameRunnerBattler<TState, TOperation> :
		GameRunner<TState, TOperation> {

		public interface IUser {
			public Task<TOperation> GetOperationAsync();
			public void OnStarted();
			public void OnNewRound();
			public void OnGotOperation(TOperation operation);
			public void OnOperated();
		}

		public IUser UI { get; }

		protected IMessager messager;

		public GameRunnerBattler(
			IUser _ui,
			IGame<TState, TOperation> _game,
			IMessager _messager
		) {
			UI = _ui; Game = _game; messager = _messager;
		}

		protected override void Start() {
			if(!messager.OperateExchange(Game.Info, (a, b) => a == b))
				throw new InfoMismatchException();
			UI.OnStarted();
		}

		protected override Joint<TOperation> GetOperations() {
			UI.OnNewRound();
			Task<TOperation> taskHere = GetHitherOperationAsync();
			Task<TOperation> taskThere = GetThitherOperationAsync();
			TOperation hitherOperation = taskHere.Result;
			TOperation thitherOperation = taskThere.Result;
			UI.OnGotOperation(thitherOperation);
			return new(hitherOperation, thitherOperation);
		}

		private async Task<TOperation> GetHitherOperationAsync() {
			TOperation operation = await UI.GetOperationAsync();
			messager.Send(operation);
			return operation;
		}

		private async Task<TOperation> GetThitherOperationAsync() {
			return await messager.GetAsync<TOperation>();
		}

		protected override void OnOperated() {
			if(!Game.SyncAtThere(messager.Exchange(Game.State)))
				throw new StateDifferentException();
			UI.OnOperated();
		}
	}

	public class GameRunnerSpectator<TState, TOperation> :
		GameRunner<TState, TOperation> {

		public interface IUser {
			public void OnStarted();
			public void OnNewRound();
			public void OnGotOperation0(TOperation operation);
			public void OnGotOperation1(TOperation operation);
			public void OnOperated();
		}

		public IUser UI { get; }

		private Joint<IListener> listeners;

		public GameRunnerSpectator(
			IUser _ui,
			IGame<TState, TOperation> _game,
			Joint<IListener> _listeners
		) {
			UI = _ui; Game = _game; listeners = _listeners;
		}

		public GameRunnerSpectator(
			IUser _user,
			IGame<TState, TOperation> _game,
			IMessager _messager0,
			IMessager _messager1
		) :
			this(_user, _game, new(_messager0, _messager1)) { }

		protected override void Start() {
			if(listeners.here.Get<string>() != listeners.there.Get<string>())
				throw new InfoMismatchException();
			UI.OnStarted();
		}

		protected override Joint<TOperation> GetOperations() {
			UI.OnNewRound();
			Task<TOperation> task0 = GetOperationAsync(listeners.here, UI.OnGotOperation0);
			Task<TOperation> task1 = GetOperationAsync(listeners.there, UI.OnGotOperation1);
			return new(task0.Result, task1.Result);
		}

		private static async Task<TOperation> GetOperationAsync(
			IListener messager, Action<TOperation>? onGot
		) {
			TOperation res = await messager.GetAsync<TOperation>();
			onGot?.Invoke(res);
			return res;
		}

		protected override void OnOperated() {
			if(!(
				Game.SyncAtHere(listeners.here.Get<TState>()) &&
				Game.SyncAtThere(listeners.there.Get<TState>())
			))
				throw new StateDifferentException();
			UI.OnOperated();
		}
	}
}
