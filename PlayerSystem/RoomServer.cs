using BattleGame.Gaming;
using BattleGame.Gaming.GameGetting;
using BattleGame.Messaging;
using System.Net;
using System.Net.Sockets;

namespace BattleGame.PlayerSystem {

    public class RoomServer : Room {

        public interface IUser {
            public RoomServer Master { set; }
            public void Start();
            public string GetGameName();
            public Pair<int> GetBattlers();
            public bool GetIfContinue();
            public void OnUpdated();
        }

        private struct SocketWithMessager {
            public TcpClient socket;
            public StreamMessager messager;
            public SocketWithMessager(TcpClient socket) {
                this.socket = socket; messager = new(this.socket.GetStream());
            }
        }

        public enum Signal {
            Update, Ready, Kick
        }

        public IUser UI { get; }

        private readonly TcpListener server;
        private readonly List<SocketWithMessager> clients;

        private readonly CancellationTokenSource acceptPlayersCtSource;

        public RoomServer(
            IUser ui,
            IPEndPoint ep,
            Player self,
            int maxPlayer,
            Information info
        ) :
            base(self) {
            UI = ui;
            UI.Master = this;
            MaxPlayer = maxPlayer;
            Host = base.self;
            Info = info;
            server = new(ep);
            clients = [];
            acceptPlayersCtSource = new();
        }

        public void Lauch() {
            _ = StartAcceptPlayersAsync(acceptPlayersCtSource.Token);
            UI.Start();
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
            Player player = await sm.messager.GetObjAsync<Player>();
            PlayerStat playerStat = new(player);
            int index = AddPlayer(playerStat, sm);
            sm.messager.SendObj(PlayerList);
            sm.messager.SendObj(index);
            sm.messager.SendObj(Info);
            sm.messager.SendObj(MaxPlayer);
            sm.messager.SendObj(Stat);
            sm.messager.SendObj(Host);
            if(PlayerList.Count > MaxPlayer) {
                RemovePlayer(index); return;
            }
            bool accept = await sm.messager.GetObjAsync<bool>();
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
                clients[index].messager.SendObj(Signal.Kick);
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
                clients[i].messager?.SendObj(Signal.Update);
                clients[i].messager?.SendObj(PlayerList);
                clients[i].messager?.SendObj(i);
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
            clients.ForEach((x) => x.messager.SendObj(Signal.Ready));
            while(GameStart()) ;
        }

        private bool GameStart() {
            string gameName = UI.GetGameName();
            Pair<int> battlers = UI.GetBattlers();
            int selfIndex = PlayerList.Count;
            if(battlers.there == selfIndex)
                battlers = battlers.Swapped();
            for(int i = 0; i < clients.Count; i++) {
                clients[i].messager.SendString(gameName);
                clients[i].messager.SendObj(battlers);
            }
            if(battlers.here == selfIndex) {
                GetGameRunnerBattler(gameName, battlers).Run();
            } else {
                GetGameRunnerSpectator(gameName, battlers).Run();
            }
            bool ifContinue = UI.GetIfContinue();
            clients.ForEach((x) => x.messager.SendObj(ifContinue));
            return ifContinue;
        }

        private IGameRunner GetGameRunnerBattler(string gameName, Pair<int> battlers) {
            int opponent = battlers.there;
            List<Pair<Messager>> spectatorMessagers = [];
            for(int i = 0; i < PlayerList.Count; i++) {
                if(i == opponent) continue;
                MultiTunnelMessager messager = new(clients[i].messager, 2);
                spectatorMessagers.Add(new(messager[0], messager[1]));
            }
            ServerMessager serverMessager = new(clients[opponent].messager, [.. spectatorMessagers]);
            Pair<Player> players = battlers.ConvertAll(GetPlayer);
            return GameContainer.GameList[gameName].GetGameRunnerBattler(serverMessager, false, players);
        }

        private IGameRunner GetGameRunnerSpectator(string gameName, Pair<int> battlers) {
            Pair<List<Messager>> spectatorMessagers = new([], []);
            for(int i = 0; i < PlayerList.Count; i++) {
                if(battlers.Contains(i)) {
                    (battlers.here == i ? spectatorMessagers.there : spectatorMessagers.here)
                        .Add(clients[i].messager);
                    continue;
                }
                MultiTunnelMessager messager = new(clients[i].messager, 2);
                spectatorMessagers.ForEach<Messager>(new(messager[0], messager[1]), (list, elem) => list.Add(elem));
            }
            Pair<Listener> serverListeners = spectatorMessagers.Combine(battlers,
                (messager, battler) => new ServerListener(clients[battler].messager, [.. messager]) as Listener
            );
            Pair<Player> players = battlers.ConvertAll(GetPlayer);
            return GameContainer.GameList[gameName].GetGameRunnerSpectator(serverListeners, false, players);
        }

    }

}
