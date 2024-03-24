using RsCb.Messager;

namespace RsCb.TurnBasedGame {

	internal class GameRunner<TOperation, THash, TResult> {

		public class InfoMismatchException : Exception { }
		public class StateDifferentException : Exception { }

		public delegate TOperation GetOperation();

		public IGame<TOperation, THash, TResult> Game { get; private set; }
		protected GetOperation input;
		protected IMessager messager;

		public (
			Action? onStart,
			Action? onNewRound,
			Action? onSendOperation,
			Action<TOperation>? onGetOperation,
			Action? onOperate
		) Actions;

		public GameRunner(
			IGame<TOperation, THash, TResult> _game,
			GetOperation _input,
			IMessager _messager
		) {
			Game = _game; input = _input; messager = _messager;
			string thitherInfo = messager.Exchange(Game.Info);
			if(!Game.CheckInfo(thitherInfo))
				throw new InfoMismatchException();
		}

		public TResult Run() {
			Actions.onStart?.Invoke();
			while(!GameLoop()) ;
			return Game.Result;
		}

		private bool GameLoop() {
			Actions.onNewRound?.Invoke();
			TOperation localOperation = input.Invoke();
			TOperation opponentOperation = messager.Exchange(localOperation, Actions.onSendOperation);
			Actions.onGetOperation?.Invoke(opponentOperation);
			bool res = Game.Operate(new(localOperation, opponentOperation));
			Actions.onOperate?.Invoke();
			THash hitherHash = Game.GetHash();
			THash thitherHash = messager.Exchange(hitherHash);
			if(!Game.CheckHash(thitherHash))
				throw new StateDifferentException();
			return res;
		}
	}
}
