using System.Text;

namespace BattleGame.Messager {

	public interface IBytesMessager {
		public byte[] GetBytes();
		public Task<byte[]> GetBytesAsync();
		public void SendBytes(byte[] message);
	}

	public static class StreamReadByte {

		public static byte ReadSingleByte(this Stream stream) {
			byte[] bytes = new byte[1];
			stream.Read(bytes, 0, 1);
			return bytes[0];
		}

		public static async Task<byte> ReadSingleByteAsync(this Stream stream) {
			byte[] bytes = new byte[1];
			await stream.ReadAsync(bytes.AsMemory(0, 1));
			return bytes[0];
		}

	}

	public class StreamBytesMessager : IBytesMessager {

		public Stream Stream { get; }

		public StreamBytesMessager(Stream _stream) {
			Stream = _stream;
		}

		public static implicit operator StreamBytesMessager(
			Stream stream
		) =>
			new(stream);

		public static implicit operator JsonMessager<StreamBytesMessager>(
			StreamBytesMessager messager
		) =>
			new(messager);

		public void SendBytes(byte[] message) {
			Stream.WriteByte((byte)message.Length);
			Stream.Write(message);
		}

		public byte[] GetBytes() {
			int len = Stream.ReadSingleByte();
			byte[] res = new byte[len];
			int cur = 0;
			while(cur < len) {
				cur += Stream.Read(res, cur, len - cur);
			}
			return res;
		}

		public async Task<byte[]> GetBytesAsync() {
			int len = await Stream.ReadSingleByteAsync();
			byte[] res = new byte[len];
			int cur = 0;
			while(cur < len) {
				cur += Stream.Read(res, cur, len - cur);
			}
			return res;
		}
	}

	public class MTBytesMessager {

		public class Tunnel : IBytesMessager {

			private readonly Func<byte[]> get;
			private readonly Func<Task<byte[]>> getAsync;
			private readonly Action<byte[]> send;

			public Tunnel(
				Func<byte[]> _get,
				Func<Task<byte[]>> _getAsync,
				Action<byte[]> _send
			) {
				get = _get; getAsync = _getAsync; send = _send;
			}

			public static implicit operator JsonMessager<Tunnel>(Tunnel tunnel) =>
				new(tunnel);

			public byte[] GetBytes() => get.Invoke();
			public async Task<byte[]> GetBytesAsync() => await getAsync.Invoke();
			public void SendBytes(byte[] message) => send.Invoke(message);
		}

		public int TunnelCount { get; }

		public IBytesMessager BaseMessager { get; }

		private readonly Queue<byte[]>[] messageQues;
		private readonly Tunnel[] tunnels;

		public MTBytesMessager(IBytesMessager _mtMessager, int _tunnelCount = 1) {
			BaseMessager = _mtMessager; TunnelCount = _tunnelCount;
			messageQues = new Queue<byte[]>[TunnelCount];
			tunnels = new Tunnel[TunnelCount];
			for(int i = 0; i < TunnelCount; i++) {
				messageQues[i] = new();
				tunnels[i] = new(
					() => GetBytes(i),
					() => GetBytesAsync(i),
					(message) => SendBytes(i, message)
				);
			}
		}

		public Tunnel this[int index] { get => tunnels[index]; }

		private void GetBytes() {
			byte[] bytes = BaseMessager.GetBytes();
			int index = bytes[0];
			byte[] message = bytes[1..bytes.Length];
			messageQues[index].Enqueue(message);
		}

		private byte[] GetBytes(int index) {
			while(messageQues[index].Count == 0)
				GetBytes();
			return messageQues[index].Dequeue();
		}

		private async Task GetBytesAsync() {
			byte[] bytes = await BaseMessager.GetBytesAsync();
			int index = bytes[0];
			byte[] message = bytes[1..bytes.Length];
			messageQues[index].Enqueue(message);
		}

		private async Task<byte[]> GetBytesAsync(int index) {
			while(messageQues[index].Count == 0)
				await GetBytesAsync();
			return messageQues[index].Dequeue();
		}

		private void SendBytes(int index, byte[] message) {
			byte[] bytes = new byte[message.Length + 1];
			bytes[0] = (byte)index;
			message.CopyTo(bytes, 1);
			BaseMessager.SendBytes(bytes);
		}
	}
}
