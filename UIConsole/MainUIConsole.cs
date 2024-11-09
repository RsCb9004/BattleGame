using BattleGame.PlayerSystem;
using System.Net;

namespace BattleGame.UIConsole {

    public static class MainUIConsole {

        public static void StartRoom() =>
            ConsoleUIConvertChain.Ask("Enter 'c' to create a room; enter 'j' to Join a room. > ")
                .Convert<Action?>((op) => op switch {
                    "c" => CreateRoom,
                    "j" => JoinRoom,
                    _ => null
                })
                .Check((act) => act != null, "%+cPlease enter 'c' or 'j'.")
                .GetValue()!.Invoke();

        private static void CreateRoom() {
            IPEndPoint ip = ConsoleUIConvertChain.Ask("IP Address: ")
                .TryConvert<string, IPEndPoint?>(IPEndPoint.TryParse, "%+cWrong URL format. Please try again.")
                .GetValue() ?? throw new NullReferenceException();
            string name = ConsoleUI.Ask("Your Name: ");
            Room.Information info = new() {
                name = ConsoleUI.Ask("Room Name: "),
                description = ConsoleUI.Ask("Room Description: ")
            };
            int maxPlayer = ConsoleUIConvertChain.Ask("Max Player: ")
                .TryConvert<string, int>(int.TryParse, "%+cPlease enter an integer.")
                .Check((x) => x > 0, "%+cPlease enter an positive integer.")
                .GetValue();
            RoomServer room = new(new RoomUIConsole(null!), ip, new() { name = name }, maxPlayer, info);
            room.Lauch();
        }

        private static void JoinRoom() {
            IPEndPoint ip = ConsoleUIConvertChain.Ask("IP Address: ")
                .TryConvert<string, IPEndPoint?>(IPEndPoint.TryParse, "%+cWrong URL format. Please try again.")
                .GetValue() ?? throw new NullReferenceException();
            string name = ConsoleUI.Ask("Your Name: ");
            RoomClient room = new(new RoomUIConsole(null!), ip, new(name));
            room.Lauch();
        }

    }

}
