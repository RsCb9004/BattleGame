using BattleGame.Messager;
using BattleGame.TurnBasedGame;
using System.Net;
using System.Net.Sockets;

namespace BattleGame {

	public struct Player {
		public string name;
		public Player(string _name) {
			name = _name;
		}
	}

	public abstract class Room {

		public struct Information {
			public string name;
			public string description;
		}

		public enum Status {
			NotReady, Ready
		}

		public class PlayerStat {

			public Player player;
			public bool joined;

			public PlayerStat() { }

			public PlayerStat(
				Player _player,
				bool _joined = false
			) {
				player = _player;
				joined = _joined;
			}

		}

		public Information Info { get; protected set; }

		public int MaxPlayer { get; protected set; }
		public Status Stat { get; protected set; }

		public Player Host { get; protected set; }
		public List<PlayerStat> PlayerList { get; protected set; }

		protected Player self;

		protected Room(Player _self) {
			Stat = Status.NotReady;
			PlayerList = [];
			self = _self;
		}

	}

	public class RoomServer : Room {

		public interface IUser {
			public RoomServer Master { set; }
			public Joint<int> GetBattlers();
			public bool GetIfContinue();
			public GameRunner.GameType GetGameType();
			public GameRunner.IBattlerUIGetter GetBattlerUI();
			public GameRunner.ISpectatorUIGetter GetSpectatorUI();
			public void OnUpdated();
		}

		private struct SocketWithMessager {
			public TcpClient socket;
			public JsonMessager<StreamBytesMessager> messager;
			public SocketWithMessager(TcpClient _socket) {
				socket = _socket; messager = new(socket.GetStream());
			}
		}

		public enum Signal {
			Update, Ready, Kick
		}

		public IUser UI { get; }

		private readonly TcpListener server;
		private readonly List<SocketWithMessager> clients;

		private readonly CancellationTokenSource acceptPlayersCtSource;

		public RoomServer(IUser _ui, IPEndPoint ep, int _maxPkayer, Player _self, Information _info) :
			base(_self) {
			UI = _ui;
			UI.Master = this;
			MaxPlayer = _maxPkayer;
			Host = self;
			Info = _info;
			server = new(ep);
			clients = [];
			acceptPlayersCtSource = new();
		}

		public void Lauch() {
			_ = StartAcceptPlayersAsync(acceptPlayersCtSource.Token);
		}

		private async Task StartAcceptPlayersAsync(CancellationToken ct) {
			server.Start();
			while(true) {
				TcpClient newPlayer = await server.AcceptTcpClientAsync(ct);
				_ = AcceptPlayerAsync(newPlayer);
				ct.ThrowIfCancellationRequested();
			}
		}

		private async Task AcceptPlayerAsync(TcpClient socket) {
			SocketWithMessager sm = new(socket);
			Player player = await sm.messager.GetAsync<Player>();
			PlayerStat playerStat = new(player);
			int index = AddPlayer(playerStat, sm);
			sm.messager.Send(PlayerList);
			sm.messager.Send(index);
			sm.messager.Send(Info);
			sm.messager.Send(MaxPlayer);
			sm.messager.Send(Stat);
			sm.messager.Send(Host);
			if(PlayerList.Count > MaxPlayer) {
				RemovePlayer(index); return;
			}
			bool accept = await sm.messager.GetAsync<bool>();
			if(!accept) {
				RemovePlayer(index); return;
			}
			playerStat.joined = true;
			Update();
		}

		private int AddPlayer(PlayerStat playerStat, SocketWithMessager sm) {
			int index = PlayerList.Count;
			PlayerList.Add(playerStat);
			clients.Insert(index, sm);
			Update();
			return index;
		}

		public void RemovePlayer(int index) {
			try {
				clients[index].messager.Send(Signal.Kick);
			} catch { }
			clients[index].socket?.Close();
			PlayerList.RemoveAt(index);
			clients.RemoveAt(index);
			Update();
		}

		private void Update() {
			UI.OnUpdated();
			for(int i = 0; i < clients.Count; i++) {
				if(!PlayerList[i].joined) continue;
				clients[i].messager?.Send(Signal.Update);
				clients[i].messager?.Send(PlayerList);
				clients[i].messager?.Send(i);
			}
		}

		public void Ready() {
			Stat = Status.Ready;
			acceptPlayersCtSource.Cancel();
			for(int i = 0; i < PlayerList.Count; i++) {
				if(!PlayerList[i].joined)
					RemovePlayer(i);
			}
			Update();
			clients.ForEach((x) => x.messager.Send(Signal.Ready));
			while(GameStart()) ;
		}

		private bool GameStart() {
			GameRunner.GameType gameType = UI.GetGameType();
			Joint<int> battlers = UI.GetBattlers();
			for(int i = 0; i < clients.Count; i++) {
				clients[i].messager.Send(gameType);
				clients[i].messager.Send(battlers.Contains(i));
			}
			int self = PlayerList.Count;
			if(battlers.Contains(self)) {
				int opponent = battlers.here == self ? battlers.there : battlers.here;
				StartAsBattler(gameType, opponent);
			} else {
				StartAsSpectator(gameType, battlers);
			}
			bool ifContinue = UI.GetIfContinue();
			clients.ForEach((x) => x.messager.Send(ifContinue));
			return ifContinue;
		}

		private void StartAsBattler(GameRunner.GameType gameType, int opponent) {
			List<Joint<IMessager>> spectatorMessagers = [];
			for(int i = 0; i < PlayerList.Count; i++) {
				if(i == opponent) continue;
				MTBytesMessager messager = new(clients[i].messager.BytesMessager, 2);
				Joint<JsonMessager<MTBytesMessager.Tunnel>> joint = new(messager[0], messager[1]);
				spectatorMessagers.Add(joint.ConvertAll<IMessager>((x) => x));
			}
			ServerMessager serverMessager = new([.. spectatorMessagers], clients[opponent].messager);
			GameRunner.GetGameRunnerBattler(UI.GetBattlerUI(), gameType, serverMessager).Run();
		}

		private void StartAsSpectator(GameRunner.GameType gameType, Joint<int> battlers) {
			Joint<List<IMessager>> spectatorMessagers = new([], []);
			for(int i = 0; i < PlayerList.Count; i++) {
				if(battlers.Contains(i)) continue;
				MTBytesMessager messager = new(clients[i].messager.BytesMessager, 2);
				Joint<JsonMessager<MTBytesMessager.Tunnel>> joint = new(messager[0], messager[1]);
				spectatorMessagers.here.Add(joint.here);
				spectatorMessagers.there.Add(joint.there);
			}
			Joint<IListener> serverListeners = new(
				new ServerListener([.. spectatorMessagers.here], clients[battlers.here].messager),
				new ServerListener([.. spectatorMessagers.there], clients[battlers.there].messager)
			);
			GameRunner.GetGameRunnerSpectator(UI.GetSpectatorUI(), gameType, serverListeners).Run();
		}
	}

	public class RoomClient : Room {

		public class ConnectionFailedException : Exception { }
		public class RoomFullException : ConnectionFailedException { }
		public class PlayerRefusedException : ConnectionFailedException { }
		public class UnrecognisedSignalException : NotImplementedException { }

		public interface IUser {
			public RoomClient Master { set; }
			public bool CheckAccept();
			public void OnUpdated();
			public GameRunner.IBattlerUIGetter GetBattlerUI();
			public GameRunner.ISpectatorUIGetter GetSpectatorUI();
		}

		public IUser UI { get; }

		private PlayerStat selfStat;
		private readonly JsonMessager<StreamBytesMessager> messager;

		public RoomClient(IUser _ui, IPEndPoint ep, Player _self) :
			base(_self) {
			UI = _ui;
			UI.Master = this;
			TcpClient socket = new();
			socket.Connect(ep);
			messager = new JsonMessager<StreamBytesMessager>(socket.GetStream());
			messager.Send(self);
			PlayerList = messager.Get<List<PlayerStat>>();
			int index = messager.Get<int>();
			Info = messager.Get<Information>();
			MaxPlayer = messager.Get<int>();
			Stat = messager.Get<Status>();
			Host = messager.Get<Player>();
			selfStat = PlayerList[index];
			if(PlayerList.Count > MaxPlayer) {
				socket.Close();
				throw new RoomFullException();
			}
			bool accept = UI.CheckAccept();
			messager.Send(accept);
			if(!accept) {
				socket.Close();
				throw new PlayerRefusedException();
			}
		}

		public void Lauch() {
			while(RecieveSignal()) ;
			Ready();
		}

		private bool RecieveSignal() {
			RoomServer.Signal signal = messager.Get<RoomServer.Signal>();
			switch(signal) {
			case RoomServer.Signal.Update: {
				PlayerList = messager.Get<List<PlayerStat>>();
				int index = messager.Get<int>();
				selfStat = PlayerList[index];
				UI.OnUpdated();
				return true;
			}
			case RoomServer.Signal.Ready: {
				return false;
			}
			default: {
				throw new UnrecognisedSignalException();
			}
			}
		}

		private void Ready() {
			Stat = Status.Ready;
			while(GameStart()) ;
		}

		private bool GameStart() {
			GameRunner.GameType gameType = messager.Get<GameRunner.GameType>();
			bool battler = messager.Get<bool>();
			if(battler) {
				StartAsBattler(gameType);
			} else {
				StartAsSpectator(gameType);
			}
			return messager.Get<bool>();
		}

		private void StartAsBattler(GameRunner.GameType gameType) {
			GameRunner.GetGameRunnerBattler(UI.GetBattlerUI(), gameType, messager).Run();
		}

		private void StartAsSpectator(GameRunner.GameType gameType) {
			MTBytesMessager mtMessager = new(messager.BytesMessager, 2);
			Joint<JsonMessager<MTBytesMessager.Tunnel>> joint = new(mtMessager[0], mtMessager[1]);
			Joint<IListener> jointListener = joint.ConvertAll<IListener>((x) => x);
			GameRunner.GetGameRunnerSpectator(UI.GetSpectatorUI(), gameType, jointListener).Run();
		}

	}

}
