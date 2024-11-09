namespace BattleGame.PlayerSystem {

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
                Player player,
                bool joined = false
            ) {
                this.player = player;
                this.joined = joined;
            }

        }

        public Information Info { get; protected set; }

        public int MaxPlayer { get; protected set; }
        public Status Stat { get; protected set; }

        public Player Host { get; protected set; }
        public List<PlayerStat> PlayerList { get; protected set; }

        protected Player self;

        protected Room(Player self) {
            Stat = Status.NotReady;
            PlayerList = [];
            this.self = self;
        }

        public Player GetPlayer(int index) {
            if(index == PlayerList.Count) return Host;
            return PlayerList[index].player;
        }

    }

}
