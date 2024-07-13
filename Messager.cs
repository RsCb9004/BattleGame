using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace BattleGame.Messager {

	public interface IListener {
		public T? GetNullable<T>();
		public T Get<T>();
		public Task<T?> GetNullableAsync<T>();
		public Task<T> GetAsync<T>();
	}

	public interface IMessager : IListener {
		public void Send<T>(T message);
		public T? ExchangeNullabe<T>(T message);
		public T Exchange<T>(T message);
		public TRes OperateExchange<TRes, T>(T message, Func<T, T, TRes> operate);
	}

	public class JsonMessager<TBytesMessager> : IMessager
		where TBytesMessager : IBytesMessager {

		public TBytesMessager BytesMessager { get; }

		public JsonMessager(TBytesMessager _bytesMessager) {
			BytesMessager = _bytesMessager;
		}

		public T? GetNullable<T>() {
			string json = Encoding.UTF8.GetString(BytesMessager.GetBytes());
			return JsonConvert.DeserializeObject<T>(json);
		}

		public T Get<T>() =>
			GetNullable<T>() ?? throw new NullReferenceException();

		public async Task<T?> GetNullableAsync<T>() {
			string json = Encoding.UTF8.GetString(await BytesMessager.GetBytesAsync());
			return JsonConvert.DeserializeObject<T>(json);
		}

		public async Task<T> GetAsync<T>() =>
			await GetNullableAsync<T>() ?? throw new NullReferenceException();

		public void Send<T>(T message) {
			byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
			BytesMessager.SendBytes(json);
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
