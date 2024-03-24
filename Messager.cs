using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace RsCb.Messager {

	internal interface IMessager {

		public void Send<T>(T message);
		public T? GetNullable<T>();

		public T Get<T>() =>
			GetNullable<T>() ?? throw new NullReferenceException();

		public T? ExchangeNullabe<T>(T message, Action? onSend = null) {
			Send(message);
			onSend?.Invoke();
			return GetNullable<T>();
		}
		public T Exchange<T>(T message, Action? onSend = null) =>
			ExchangeNullabe(message, onSend) ?? throw new NullReferenceException();
	}

	internal abstract class TcpMessager : IMessager {

		const int DefaultBufferSize = 1048576;

		public static IPEndPoint IpPort2Ep(string ip, int port) =>
			new(IPAddress.Parse(ip), port);

		protected Socket socket;

		private readonly byte[] buffer;

		protected TcpMessager(int bufferSize = DefaultBufferSize) {
			socket = null!;
			buffer = new byte[bufferSize];
		}

		public void Send<T>(T message) {
			byte[] json = JsonSerializer.SerializeToUtf8Bytes(message);
			socket.Send(json);
		}

		public T? GetNullable<T>() {
			int len = socket.Receive(buffer);
			ReadOnlySpan<byte> json = new(buffer, 0, len);
			return JsonSerializer.Deserialize<T>(json);
		}
	}

	internal class ServerTcpMessager : TcpMessager {

		private readonly Socket serverSocket;

		public ServerTcpMessager(IPEndPoint serverEp, Action? onBind = null) {
			serverSocket = new(SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(serverEp);
			onBind?.Invoke();
			serverSocket.Listen();
			socket = serverSocket.Accept();
		}

		public ServerTcpMessager(string ip, int port, Action? onBind = null) :
			this(IpPort2Ep(ip, port), onBind) { }
	}

	internal class ClientTcpMessager : TcpMessager {

		public ClientTcpMessager(IPEndPoint serverEp) {
			socket = new(SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(serverEp);
		}

		public ClientTcpMessager(string ip, int port) :
			this(IpPort2Ep(ip, port)) { }
	}
}
