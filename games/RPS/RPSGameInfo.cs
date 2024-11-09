using BattleGame.Gaming.GameGetting;

namespace RPS {

    public class RPSGameInfo : AssemblyGameGetter.IAssemblyGameInfo {

        public string GameId => "assembly.rps@battle-game";
        public Type OperationType => typeof(Gesture);
        public Type ResultType => typeof(Result);
        public Type GameType => typeof(RPSGame);
        public Type BattlerUIType => typeof(RPSUIConsole);
        public Type SpectatorUIType => typeof(RPSUIConsole);

    }

}
