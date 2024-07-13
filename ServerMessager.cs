using BattleGame.TurnBasedGame;

namespace BattleGame.Messager {

	public class ServerListener : IListener {

		public IMessager[] Spectators { get; }
		public IMessager Battler { get; }

		public ServerListener(IMessager[] _spectators, IMessager _battler) {
			Spectators = _spectators; Battler = _battler;
		}

		public T? GetNullable<T>() {
			T? res = Battler.GetNullable<T>();
			foreach(IMessager spectator in Spectators)
				spectator.Send(res);
			return res;
		}

		public T Get<T>() =>
			GetNullable<T>() ?? throw new NullReferenceException();

		public async Task<T?> GetNullableAsync<T>() {
			T? res = await Battler.GetNullableAsync<T>();
			foreach(IMessager spectator in Spectators)
				spectator.Send(res);
			return res;
		}

		public async Task<T> GetAsync<T>() =>
			await GetNullableAsync<T>() ?? throw new NullReferenceException();
	}

	public class ServerMessager : IMessager {

		public Joint<IMessager>[] Spectators { get; }
		public IMessager Battler { get; }

		public ServerMessager(Joint<IMessager>[] _spectators, IMessager _battler) {
			Spectators = _spectators; Battler = _battler;
		}

		public T? GetNullable<T>() {
			T? res = Battler.GetNullable<T>();
			foreach(Joint<IMessager> spectator in Spectators)
				spectator.there.Send(res);
			return res;
		}

		public T Get<T>() =>
			GetNullable<T>() ?? throw new NullReferenceException();

		public async Task<T?> GetNullableAsync<T>() {
			T? res = await Battler.GetNullableAsync<T>();
			foreach(Joint<IMessager> spectator in Spectators)
				spectator.there.Send(res);
			return res;
		}

		public async Task<T> GetAsync<T>() =>
			await GetNullableAsync<T>() ?? throw new NullReferenceException();

		public void Send<T>(T message) {
			Battler.Send(message);
			foreach(Joint<IMessager> spectator in Spectators)
				spectator.here.Send(message);
		}

		public T? ExchangeNullabe<T>(T message) {
			Send(message);
			return GetNullable<T>();
		}

		public T Exchange<T>(T message) =>
			ExchangeNullabe(message) ?? throw new NullReferenceException();

		public TRes OperateExchange<TRes, T>(T message, Func<T, T, TRes> operate) =>
			operate.Invoke(message, Exchange(message));
	}
}
