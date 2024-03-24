namespace RsCb.TurnBasedGame.RPS {

	internal class RPSGame : IGame<RPSGame.Gesture, bool, bool> {

		public enum Gesture {
			Rock, Paper, Scissors
		}

		public enum RoundResult {
			Tie = 0, Win = 1, Lose = -1
		}

		private const string DefaultInfo = "RPS";

		public string Info { get; }
		public bool Result { get; }

		public int Round { get; private set; }
		public Joint<int> State { get => state; }
		private Joint<int> state;

		public RoundResult CurResult { get; private set; }

		public RPSGame() {
			Info = DefaultInfo;
			Round = 0;
			state = new(0, 0);
		}

		public bool CheckInfo(string info) => info == DefaultInfo;

		public bool Operate(Joint<Gesture> operation) {
			Round++;
			if(operation.here == operation.there) {
				CurResult = RoundResult.Tie;
			} else if((operation.here - operation.there + 3) % 3 == 1) {
				CurResult = RoundResult.Win;
				state.here++;
			} else {
				CurResult = RoundResult.Lose;
				state.there++;
			}
			return false;
		}

		public bool GetHash() {
			return true;
		}

		public bool CheckHash(bool hash) {
			return true;
		}
	}
}
