namespace BattleGame.TurnBasedGame.RPS {

	public class RPSGame : IGame<RPSGame.GameState, RPSGame.Gesture> {

		public enum Gesture {
			Rock, Paper, Scissors
		}

		public enum RoundResult {
			Tie = 0, Win = 1, Lose = -1
		}

		public struct GameState {
			public int round;
			public Joint<int> score;
		}

		private const string DefaultInfo = "RPS";

		public string Info { get; }
		public GameState State { get => state; }

		private GameState state;

		public RoundResult CurResult { get; private set; }

		public RPSGame() {
			Info = DefaultInfo;
			state = new();
		}

		public bool Operate(Joint<Gesture> operation) {
			state.round++;
			if(operation.here == operation.there) {
				CurResult = RoundResult.Tie;
			} else if((operation.here - operation.there + 3) % 3 == 1) {
				CurResult = RoundResult.Win;
				state.score.here++;
			} else {
				CurResult = RoundResult.Lose;
				state.score.there++;
			}
			return false;
		}

		bool IGame<GameState, Gesture>.SyncAtHere(GameState thatState) {
			if(thatState.round != state.round) return false;
			if(Equals(thatState.score, state.score)) return false;
			return true;
		}

		bool IGame<GameState, Gesture>.SyncAtThere(GameState thatState) {
			if(thatState.round != state.round) return false;
			if(Equals(thatState.score, state.score.Swapped())) return false;
			return true;
		}
	}
}
