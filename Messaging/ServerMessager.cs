namespace BattleGame.Messaging {

    public class ServerMessager : Messager {

        public Messager MessageTarget { get; }
        public Pair<Messager>[] ForwardTargets { get; }

        public ServerMessager(Messager messageTarget, Pair<Messager>[] fowardTargets) {
            MessageTarget = messageTarget; ForwardTargets = fowardTargets;
        }

        public override byte[] Get() {
            byte[] message = MessageTarget.Get();
            foreach(Pair<Messager> forwardTarget in ForwardTargets)
                forwardTarget.there.Send(message);
            return message;
        }

        public override async Task<byte[]> GetAsync() {
            byte[] message = await MessageTarget.GetAsync();
            foreach(Pair<Messager> forwardTarget in ForwardTargets)
                forwardTarget.there.Send(message);
            return message;
        }

        public override void Send(byte[] message) {
            MessageTarget.Send(message);
            foreach(Pair<Messager> forwardTarget in ForwardTargets)
                forwardTarget.here.Send(message);
        }

    }

}
