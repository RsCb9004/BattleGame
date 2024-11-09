namespace BattleGame.Messaging {
    
    public class ServerListener : Listener {

        public Listener ListenTarget { get; }
        public Messager[] FowardTargets { get; }

        public ServerListener(Messager listenTarget, Messager[] forwardTargets) {
            ListenTarget = listenTarget; FowardTargets = forwardTargets;
        }

        public override byte[] Get() {
            byte[] res = ListenTarget.Get();
            foreach(Messager spectator in FowardTargets)
                spectator.Send(res);
            return res;
        }

        public override async Task<byte[]> GetAsync() {
            byte[] res = await ListenTarget.GetAsync();
            foreach(Messager spectator in FowardTargets)
                spectator.Send(res);
            return res;
        }

    }
}
