using BattleGame.Gaming.GameGetting;
using BattleGame.UIConsole;
using System.Reflection;
using Newtonsoft.Json;

namespace BattleGame {

    internal static class Program {

        public static void Main() {
            AssemblyGameGetter.AssignGame(Assembly.LoadFile(Path.GetFullPath("RPS.dll")));
            MainUIConsole.StartRoom();
        }

    }

}
