using BattleGame.Gaming;
using BattleGame.Gaming.GameGetting;
using BattleGame.Messaging;
using System.Net;
using System.Net.Sockets;

namespace BattleGame.PlayerSystem {

    public class RoomClient : Room {

        public class ConnectionFailedException : Exception { }
        public class RoomFullException : ConnectionFailedException { }
        public class PlayerRefusedException : ConnectionFailedException { }
        public class UnrecognisedSignalException : NotImplementedException { }

        public interface IUser {
            public RoomClient Master { set; }
            public void Start();
            public bool CheckAccept();
            public void OnUpdated();
        }

        public IUser UI { get; }

        public int SelfIndex { get; private set; }

        private readonly StreamMessager messager;

        public RoomClient(
            IUser ui,
            IPEndPoint ep,
            Player self
        ) :
            base(self) {
            UI = ui;
            UI.Master = this;
            TcpClient socket = new();
            socket.Connect(ep);
            messager = new StreamMessager(socket.GetStream());
            messager.SendObj(base.self);
            PlayerList = messager.GetObj<List<PlayerStat>>();
            SelfIndex = messager.GetObj<int>();
            Info = messager.GetObj<Information>();
            MaxPlayer = messager.GetObj<int>();
            Stat = messager.GetObj<Status>();
            Host = messager.GetObj<Player>();
            if(PlayerList.Count > MaxPlayer) {
                socket.Close();
                throw new RoomFullException();
            }
            bool accept = UI.CheckAccept();
            messager.SendObj(accept);
            if(!accept) {
                socket.Close();
                throw new PlayerRefusedException();
            }
        }

        public void Lauch() {
            UI.Start();
            while(RecieveSignal()) ;
            Ready();
        }

        private bool RecieveSignal() {
            RoomServer.Signal signal = messager.GetObj<RoomServer.Signal>();
            switch(signal) {
            case RoomServer.Signal.Update: {
                PlayerList = messager.GetObj<List<PlayerStat>>();
                SelfIndex = messager.GetObj<int>();
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
            string gameName = messager.GetString();
            Pair<int> battlers = messager.GetObj<Pair<int>>();
            bool reversed;
            if(reversed = battlers.there == SelfIndex)
                battlers = battlers.Swapped();
            if(battlers.here == SelfIndex) {
                GetGameRunnerBattler(gameName, battlers, reversed).Run();
            } else {
                GetGameRunnerSpectator(gameName, battlers, reversed).Run();
            }
            return messager.GetObj<bool>();
        }

        private IGameRunner GetGameRunnerBattler(string gameName, Pair<int> battlers, bool reversed) {
            Pair<Player> players = battlers.ConvertAll(GetPlayer);
            return GameContainer.GameList[gameName].GetGameRunnerBattler(messager, reversed, players);
        }

        private IGameRunner GetGameRunnerSpectator(string gameName, Pair<int> battlers, bool reversed) {
            MultiTunnelMessager mtMessager = new(messager, 2);
            Pair<Listener> listeners = new(mtMessager[0], mtMessager[1]);
            Pair<Player> players = battlers.ConvertAll(GetPlayer);
            return GameContainer.GameList[gameName].GetGameRunnerSpectator(listeners, reversed, players);
        }

    }

}
