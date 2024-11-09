using BattleGame.Gaming.GameGetting;
using BattleGame.PlayerSystem;

namespace BattleGame.UIConsole {

    public class RoomUIConsole : RoomServer.IUser, RoomClient.IUser {

        public Room Master { get; set; }

        RoomServer RoomServer.IUser.Master { set => Master = value; }
        RoomClient RoomClient.IUser.Master { set => Master = value; }

        public RoomUIConsole(Room master) {
            Master = master;
        }

        void RoomServer.IUser.Start() {
            Console.Clear();
            AskOperations();
        }

        void RoomClient.IUser.Start() {
            Console.Clear();
        }

        private void AskOperations() {
            Console.Clear();
            Console.SetCursorPosition(0, Master.MaxPlayer + 6);
            OnUpdated();
            while(true) AskOperation();
        }

        public void AskOperation() {
            string[] op = ConsoleUI.Ask("> ").Split();
            switch(op[0]) {
            case "s":
            case "start": {
                Start();
                break;
            }
            case "k":
            case "kick": {
                Remove(op);
                break;
            }
            case "rf":
            case "refresh": {
                OnUpdated();
                break;
            }
            case "": {
                break;
            }
            default: {
                ConsoleUI.PrintLine("%+cUnknown command.");
                break;
            }
            }
        }

        private void Start() {
            if(Master.PlayerList.Count == 0) {
                ConsoleUI.PrintLine("%+cYour room needs at lease 2 players.");
                return;
            }
            var master = Master as RoomServer;
            master?.Ready();
        }

        private void Remove(string[] op) {
            int count = Master.PlayerList.Count;
            var valid = new ConvertChainStart<string[]>(op)
                .Check((op) => op.Length == 2, "kick [INDEX]\nINDEX\tIndex of the player you want to kick.")
                .Convert((op) => op[1])
                .TryConvert<string, int>(int.TryParse, "%+cArgument should be an integer.")
                .Check((ind) => ind >= 0 && ind <= count, "%+cInvalid player index.")
                .Check((ind) => ind != 0, "%+cYou can't kick yourself.")
                .TryGetValue(out int index);
            if(!valid) return;
            var master = Master as RoomServer;
            master?.RemovePlayer(index - 1);
        }

        public Pair<int> GetBattlers() {
            int count = Master.PlayerList.Count;
            Pair<int> battlers = new(
                ConsoleUIConvertChain.Ask("Battler 1 index: ")
                    .TryConvert<string, int>(int.TryParse, "%+cPlease enter an integer")
                    .Check((ind) => ind >= 0 && ind <= count, "%+cPlayer doesn't exist.")
                    .Convert((ind) => ind == 0 ? count : ind - 1)
                    .GetValue(),
                ConsoleUIConvertChain.Ask("Battler 2 index: ")
                    .TryConvert<string, int>(int.TryParse, "%+cPlease enter an integer")
                    .Check((ind) => ind >= 0 && ind <= count, "%+cPlayer doesn't exist.")
                    .Convert((ind) => ind == 0 ? count : ind - 1)
                    .GetValue()
            );
            if(battlers.here == battlers.there) {
                ConsoleUI.PrintLine("%+cOne can't play with himself.");
                return GetBattlers();
            }
            return battlers;
        }

        public bool GetIfContinue() {
            return true;
        }

        public string GetGameName() {
            var games = GameContainer.GameList.Keys.ToArray();
            for(int i = 0; i < games.Length; i++) {
                ConsoleUI.PrintLine($"%+a{i + 1}. {games[i]}");
            }
            return ConsoleUIConvertChain.Ask("Game index: ")
                .TryConvert<string, int>(int.TryParse, "%+cPlease enter an integer.")
                .Check((ind) => ind > 0 && ind <= games.Length, "%+cIndex doesn't exist.")
                .Convert((ind) => games[ind - 1])
                .GetValue();
        }

        public void OnUpdated() {
            int curPosLeft = Console.CursorLeft;
            int curPosTop = Console.CursorTop;
            for(int i = 0; i < Master.MaxPlayer + 6; i++)
                ConsoleUI.PrintIntoLineAt(i);
            ConsoleUI.PrintIntoLineAt(0, $"%+0%-f{Master.Info.name}");
            ConsoleUI.PrintIntoLineAt(1, $"%+7{Master.Info.description}");
            ConsoleUI.PrintIntoLineAt(2, $"Player Count: {Master.PlayerList.Count + 1}/{Master.MaxPlayer + 1}");
            ConsoleUI.PrintIntoLineAt(4, $"%+e0. {Master.Host.name}");
            for(int i = 0; i < Master.MaxPlayer; i++) {
                if(i < Master.PlayerList.Count) {
                    Room.PlayerStat playerStat = Master.PlayerList[i];
                    ConsoleUI.PrintIntoLineAt(i + 5, $"%+{(playerStat.joined ? 'a' : '2')}{i + 1}. {playerStat.player.name}");
                } else {
                    ConsoleUI.PrintIntoLineAt(i + 5, "%+7-");
                }
            }
            Console.SetCursorPosition(curPosLeft, curPosTop);
        }

        public bool CheckAccept() {
            return true;
        }

    }

}
