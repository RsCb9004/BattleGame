namespace BattleGame.TurnBasedGame {

	public interface IGame<TState, TOperation> {

		public string Info { get; }
		public TState State { get; }

		public bool Operate(Joint<TOperation> operation);
		public bool SyncAtHere(TState state);
		public bool SyncAtThere(TState state);
	}
}
